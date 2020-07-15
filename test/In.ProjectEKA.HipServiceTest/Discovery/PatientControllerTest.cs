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

    public class PatientControllerTest
    {
        private readonly Mock<IDiscoveryRequestRepository> discoveryRequestRepository = new Mock<IDiscoveryRequestRepository>();

        private readonly Mock<ILinkPatientRepository> linkPatientRepository = new Mock<ILinkPatientRepository>();

        private readonly Mock<IMatchingRepository> matchingRepository = new Mock<IMatchingRepository>();

        private readonly Mock<IPatientRepository> patientRepository = new Mock<IPatientRepository>();

        private readonly Mock<IBackgroundJobClient> backgroundJobClient = new Mock<IBackgroundJobClient>();

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