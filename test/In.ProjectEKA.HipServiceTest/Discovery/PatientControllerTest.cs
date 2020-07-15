namespace In.ProjectEKA.HipServiceTest.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Http;
    using FluentAssertions;
    using HipLibrary.Matcher;
    using HipLibrary.Patient;
    using HipLibrary.Patient.Model;
    using HipService.Discovery;
    using HipService.Link;
    using HipService.Link.Model;
    using Moq;
    using Optional;
    using Xunit;
    using Match = HipLibrary.Patient.Model.Match;
    using static Builder.TestBuilders;
    using In.ProjectEKA.HipService.Gateway;
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Hosting;
    using In.ProjectEKA.HipService;
    using System.Text;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using System.Text.Encodings.Web;
    using System.Security.Claims;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
    using System.Text.Json;

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
                //public const string TestingCookieAuthentication = "TestCookieAuthentication";
                //public const string TestingHeader = "X-Integration-Testing";
                //public const string TestingHeaderValue = "abcde-12345";

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
                    Console.WriteLine("User set to " + principal);

                    //    if (context.Request.Headers.Keys.Contains(TestingHeader) &&
                    //        context.Request.Headers[TestingHeader].First().Equals(TestingHeaderValue))
                    //    {
                    //        if (context.Request.Headers.Keys.Contains("my-name"))
                    //        {
                    //            var name =
                    //                context.Request.Headers["my-name"].First();
                    //            var id =
                    //                context.Request.Headers.Keys.Contains("my-id")
                    //                    ? context.Request.Headers["my-id"].First() : "";
                    //            ClaimsIdentity claimsIdentity = new ClaimsIdentity(new List<Claim>
                    //{
                    //    new Claim(ClaimTypes.Name, name),
                    //    new Claim(ClaimTypes.NameIdentifier, id),
                    //}, TestingCookieAuthentication);
                    //            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    //            context.User = claimsPrincipal;
                    //        }
                    //    }

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
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { })
                    ;
            }
        }

        [Fact]
        private async void DiscoverPatientCareContexts_ReturnsBadRequestResult_WhenModelIsInvalid()
        {
            var _server = new Microsoft.AspNetCore.TestHost.TestServer(new WebHostBuilder().UseStartup<TestStartup>());
            var _client = _server.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            var response =
                await _client.PostAsync(
                    "v1/care-contexts/discover",
                    new StringContent(
                        @"{
                            ""requestId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
	                        ""transactionId"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
	                        ""patient"": {
                                ""id"": ""<patient-id>@<consent-manager-id>"",
		                        ""name"": ""chandler bing"",
		                        ""gender"": ""M"",
                            }
                        }",
                        Encoding.UTF8,
                        "application/json"));

            Console.WriteLine(_client.BaseAddress);
            Console.WriteLine(response.RequestMessage.RequestUri);
            Console.WriteLine(response.RequestMessage.Headers);
            Console.WriteLine(response);
            Assert.True(response.IsSuccessStatusCode);
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