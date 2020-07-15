namespace In.ProjectEKA.HipServiceTest.Discovery
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Hangfire;
    using HipLibrary.Matcher;
    using HipLibrary.Patient;
    using HipLibrary.Patient.Model;
    using HipService.Discovery;
    using HipService.Link;
    using In.ProjectEKA.HipService.Gateway;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Newtonsoft.Json;
    using Xunit;

    public class PatientControllerTest
    {
        private readonly Mock<IDiscoveryRequestRepository> discoveryRequestRepository = new Mock<IDiscoveryRequestRepository>();

        private readonly Mock<ILinkPatientRepository> linkPatientRepository = new Mock<ILinkPatientRepository>();

        private readonly Mock<IMatchingRepository> matchingRepository = new Mock<IMatchingRepository>();

        private readonly Mock<IPatientRepository> patientRepository = new Mock<IPatientRepository>();

        private readonly Mock<IBackgroundJobClient> backgroundJobClient = new Mock<IBackgroundJobClient>();

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
            string _patientGender;

            public DiscoveryRequestPayloadBuilder WithRequestId()
            {
                _requestId = "\"requestId\": \"3fa85f64 - 5717 - 4562 - b3fc - 2c963f66afa6\"";
                return this;
            }
            public DiscoveryRequestPayloadBuilder WithTransactionId()
            {
                _transactionId = "\"transactionId\": \"4fa85f64 - 5717 - 4562 - b3fc - 2c963f66afa6\"";
                return this;
            }
            public DiscoveryRequestPayloadBuilder WithPatientId()
            {
                _patientId = "\"id\": \"<patient-id>@<consent-manager-id>\"";
                return this;
            }
            public DiscoveryRequestPayloadBuilder WithPatientName()
            {
                _patientName = "\"name\": \"chandler bing\"";
                return this;
            }
            public DiscoveryRequestPayloadBuilder WithPatientGender()
            {
                _patientGender = "\"gender\": \"M\"";
                return this;
            }

            private string AddWithCommaIfNonEmpty(string text)
            {
                if (string.IsNullOrEmpty(text)) return text;

                return text + ",";
            }

            private bool IsAnyStringNonEmpty(params string[] texts) => texts.Any(text => !string.IsNullOrEmpty(text));

            public StringContent Build()
            {
                return new StringContent(
                    $@"{{
                        {AddWithCommaIfNonEmpty(_requestId)}
                        {AddWithCommaIfNonEmpty(_transactionId)}
                        {(IsAnyStringNonEmpty(_patientId, _patientName, _patientGender)
                            ? $"\"patient\": {{ " +
                                    $"{AddWithCommaIfNonEmpty(_patientId)}" +
                                    $"{AddWithCommaIfNonEmpty(_patientName)} " +
                                    $"{AddWithCommaIfNonEmpty(_patientGender)} " +
                                $"}}"
                            : "")}
                    }}",
                    Encoding.UTF8,
                    "application/json");
            }
        }

        [Fact]
        private async void DiscoverPatientCareContexts_ReturnsBadRequestResult_WhenModelIsInvalid()
        {
            var _server = new Microsoft.AspNetCore.TestHost.TestServer(new WebHostBuilder().UseStartup<TestStartup>());
            var _client = _server.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            var requestContent = new DiscoveryRequestPayloadBuilder()
                .WithRequestId()
                .WithTransactionId()
                .WithPatientId()
                .WithPatientName()
                .WithPatientGender()
                .Build();

            var response =
                await _client.PostAsync(
                    "v1/care-contexts/discover",
                    requestContent);

            Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        private async void DiscoverPatientCareContexts_ReturnsBadRequestResult_WhenModelStateIsInvalid(){

        var patientDiscovery = new PatientDiscovery(
            matchingRepository.Object,
            discoveryRequestRepository.Object,
            linkPatientRepository.Object,
            patientRepository.Object);

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(handlerMock.Object);
        var gatewayConfiguration = new GatewayConfiguration {Url = "http://someUrl"};
        var gatewayClient = new GatewayClient(httpClient, gatewayConfiguration);
        var controller = new CareContextDiscoveryController(patientDiscovery, gatewayClient, backgroundJobClient.Object);
        controller.ModelState.AddModelError("TransactionId", "Required");

        const string transactionId = "transactionId";
        const string requestId = "requestId";
        var timestamp = DateTime.MinValue;
        var patientEnquiry = new PatientEnquiry( "id",null, null, "name", Gender.F, 1);
        var discoveryRequest = new DiscoveryRequest(patientEnquiry, requestId, transactionId, timestamp);
        var result = controller.DiscoverPatientCareContexts(discoveryRequest);

        Assert.IsType<BadRequestObjectResult>(result);

        }

        [Fact]        
        private async void DiscoverPatientCareContexts_ReturnsBadRequestResult_IfNameAndGenderBothEmpty()
        {
            var patientDiscovery = new PatientDiscovery(
            matchingRepository.Object,
            discoveryRequestRepository.Object,
            linkPatientRepository.Object,
            patientRepository.Object);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(handlerMock.Object);
            var gatewayConfiguration = new GatewayConfiguration {Url = "http://someUrl"};
            var gatewayClient = new GatewayClient(httpClient, gatewayConfiguration);
            var controller = new CareContextDiscoveryController(patientDiscovery, gatewayClient, backgroundJobClient.Object);

            const string transactionId = "transactionId";
            const string requestId = "requestId";
            var timestamp = DateTime.MinValue;

            const string name = null;
            Gender? gender = null;

            var patientEnquiry = new PatientEnquiry( "id", null, null, name, gender, 1);
            var discoveryRequest = new DiscoveryRequest(patientEnquiry, requestId, transactionId, timestamp);

            var result = controller.DiscoverPatientCareContexts(discoveryRequest);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]        
        private async void DiscoverPatientCareContexts_ReturnsBadRequestResult_IfPatientIdNotProvided()
        {
            var patientDiscovery = new PatientDiscovery(
            matchingRepository.Object,
            discoveryRequestRepository.Object,
            linkPatientRepository.Object,
            patientRepository.Object);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(handlerMock.Object);
            var gatewayConfiguration = new GatewayConfiguration {Url = "http://someUrl"};
            var gatewayClient = new GatewayClient(httpClient, gatewayConfiguration);
            var controller = new CareContextDiscoveryController(patientDiscovery, gatewayClient, backgroundJobClient.Object);

            const string transactionId = "transactionId";
            const string requestId = "requestId";
            var timestamp = DateTime.MinValue;

            const string patientId = "";

            var patientEnquiry = new PatientEnquiry(patientId, null, null, "name", Gender.F, 1);
            var discoveryRequest = new DiscoveryRequest(patientEnquiry, requestId, transactionId, timestamp);

            var result = controller.DiscoverPatientCareContexts(discoveryRequest);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}