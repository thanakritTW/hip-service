namespace In.ProjectEKA.HipServiceTest.Discovery
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Hangfire;
    using HipLibrary.Matcher;
    using HipLibrary.Patient;
    using HipService.Discovery;
    using HipService.Link;
    using In.ProjectEKA.HipLibrary.Patient.Model;
    using In.ProjectEKA.HipService.Gateway;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Xunit;

    public class PatientControllerTest
    {
        [Theory]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.Accepted, "RequestId")]
        [InlineData(HttpStatusCode.Accepted, "RequestId", "PatientGender")]
        [InlineData(HttpStatusCode.Accepted, "RequestId", "PatientName")]
        [InlineData(HttpStatusCode.Accepted, "PatientName")]
        [InlineData(HttpStatusCode.Accepted, "PatientGender")]
        [InlineData(HttpStatusCode.BadRequest, "PatientName", "PatientGender")]
        [InlineData(HttpStatusCode.BadRequest, "TransactionId")]
        [InlineData(HttpStatusCode.BadRequest, "PatientId")]
        private async void DiscoverPatientCareContexts_ReturnsExpectedStatusCode_WhenRequestIsSentWithParameters(
            HttpStatusCode expectedStatusCode, params string[] missingRequestParameters)
        {
            var _server = new Microsoft.AspNetCore.TestHost.TestServer(new WebHostBuilder().UseStartup<TestStartup>());
            var _client = _server.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            var requestContent = new DiscoveryRequestPayloadBuilder()
                .WithMissingParameters(missingRequestParameters)
                .Build();

            var response =
                await _client.PostAsync(
                    "v1/care-contexts/discover",
                    requestContent);

            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        class TestStartup
        {
            public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
            {
                public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                    ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
                    : base(options, logger, encoder, clock)
                {
                }

                protected override Task<AuthenticateResult> HandleAuthenticateAsync()
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
                    var identity = new ClaimsIdentity(claims, "Test");
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, "Test");

                    var result = AuthenticateResult.Success(ticket);

                    return Task.FromResult(result);
                }
            }

            public class AuthenticatedTestRequestMiddleware
            {
                private readonly RequestDelegate _next;

                public AuthenticatedTestRequestMiddleware(RequestDelegate next)
                {
                    _next = next;
                }

                public async Task Invoke(HttpContext context)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
                    var identity = new ClaimsIdentity(claims, "Test");
                    var principal = new ClaimsPrincipal(identity);
                    context.User = principal;

                    await _next(context);
                }
            }

            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                app
                    .UseRouting()
                    .UseAuthentication()
                    .UseAuthorization()
                    .UseEndpoints(endpoints => { endpoints.MapControllers(); })
                    .UseMiddleware<AuthenticatedTestRequestMiddleware>();
            }

            public void ConfigureServices(IServiceCollection services)
            {
                Mock<IDiscoveryRequestRepository> discoveryRequestRepository = new Mock<IDiscoveryRequestRepository>();
                Mock<ILinkPatientRepository> linkPatientRepository = new Mock<ILinkPatientRepository>();
                Mock<IMatchingRepository> matchingRepository = new Mock<IMatchingRepository>();
                Mock<IPatientRepository> patientRepository = new Mock<IPatientRepository>();
                Mock<IBackgroundJobClient> backgroundJobClient = new Mock<IBackgroundJobClient>();

                var patientDiscovery = new PatientDiscovery(
                    matchingRepository.Object,
                    discoveryRequestRepository.Object,
                    linkPatientRepository.Object,
                    patientRepository.Object);

                var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
                var httpClient = new HttpClient(handlerMock.Object);
                var gatewayConfiguration = new GatewayConfiguration { Url = "http://someUrl" };
                var gatewayClient = new GatewayClient(httpClient, gatewayConfiguration);

                services
                    .AddScoped(provider => patientDiscovery)
                    .AddScoped(provider => gatewayClient)
                    .AddScoped(provider => backgroundJobClient.Object)
                    .AddRouting(options => options.LowercaseUrls = true)
                    .AddControllers()
                    .AddNewtonsoftJson(
                        options => { options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; })
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        options.JsonSerializerOptions.IgnoreNullValues = true;
                    })
                    .Services
                        .AddAuthentication("Test")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            }
        }

        class DiscoveryRequestPayloadBuilder
        {
            string _requestId;
            string _transactionId;
            string _patientId;
            string _patientName;
            Gender? _patientGender;

            public DiscoveryRequestPayloadBuilder WithRequestId()
            {
                _requestId = "3fa85f64 - 5717 - 4562 - b3fc - 2c963f66afa6";
                return this;
            }
            public DiscoveryRequestPayloadBuilder WithTransactionId()
            {
                _transactionId = "4fa85f64 - 5717 - 4562 - b3fc - 2c963f66afa6";
                return this;
            }
            public DiscoveryRequestPayloadBuilder WithPatientId()
            {
                _patientId = "<patient-id>@<consent-manager-id>";
                return this;
            }
            public DiscoveryRequestPayloadBuilder WithPatientName()
            {
                _patientName = "chandler bing";
                return this;
            }
            public DiscoveryRequestPayloadBuilder WithPatientGender()
            {
                _patientGender = Gender.M;
                return this;
            }

            public DiscoveryRequestPayloadBuilder WithMissingParameters(string[] requestParametersToSet)
            {
                WithRequestId();
                WithTransactionId();
                WithPatientId();
                WithPatientName();
                WithPatientGender();

                requestParametersToSet.ToList().ForEach(p =>
                {
                    switch (p)
                    {
                        case "RequestId": { _requestId = null; break; }
                        case "TransactionId": { _transactionId = null; break; }
                        case "PatientId": { _patientId = null; break; }
                        case "PatientName": { _patientName = null; break; }
                        case "PatientGender": { _patientGender = null; break; }
                        default: throw new ArgumentException("Invalid request parameter name in test", nameof(p));
                    }
                });

                return this;
            }

            public StringContent Build()
            {
                var requestObject = new DiscoveryRequest(
                    new PatientEnquiry(
                        _patientId, verifiedIdentifiers: null, unverifiedIdentifiers: null,
                        _patientName, _patientGender, yearOfBirth: null),
                    _requestId,
                    _transactionId,
                    DateTime.Now);
                var json = JsonConvert.SerializeObject(requestObject, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                });

                return new StringContent(
                    json,
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);
            }
        }
    }
}