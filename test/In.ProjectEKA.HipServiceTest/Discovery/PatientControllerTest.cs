namespace In.ProjectEKA.HipServiceTest.Discovery
{
    using System;
    using System.Linq;
    using System.Net;
    using Hangfire;
    using In.ProjectEKA.HipService.Gateway;
    using In.ProjectEKA.HipService.Gateway.Model;
    using System.Collections.Generic;
    using HipLibrary.Patient.Model;
    using In.ProjectEKA.HipService.Discovery;
    using Moq;
    using Xunit;
    using System.Net.Http.Headers;
    using Common.TestServer;
    using Builder;
    using FluentAssertions;
    using Hangfire.Common;
    using Hangfire.States;
    using Microsoft.AspNetCore.Hosting;

    public class PatientControllerTest
    {
        private readonly Mock<IPatientDiscovery> patientDiscoveryMock;
        private readonly CareContextDiscoveryController careContextDiscoveryController;
        private readonly Dictionary<string, GatewayDiscoveryRepresentation> responsesSentToGateway;
        private readonly Dictionary<string, Job> backgroundJobs;
        private DiscoveryRequestPayloadBuilder discoveryRequestBuilder;

        public PatientControllerTest()
        {
            discoveryRequestBuilder = new DiscoveryRequestPayloadBuilder();
            
            patientDiscoveryMock = new Mock<IPatientDiscovery>();
            var gatewayClientMock = new Mock<IGatewayClient>();
            var backgroundJobClientMock = new Mock<IBackgroundJobClient>();
            
            responsesSentToGateway = new Dictionary<string, GatewayDiscoveryRepresentation>();
            backgroundJobs = new Dictionary<string, Job>();

            careContextDiscoveryController = new CareContextDiscoveryController(patientDiscoveryMock.Object, gatewayClientMock.Object, backgroundJobClientMock.Object);

            SetupGatewayClientToSaveAllSentDiscoveryIntoThisList(gatewayClientMock, responsesSentToGateway);
            SetupBackgroundJobClientToSaveAllCreatedJobsIntoThisList(backgroundJobClientMock, backgroundJobs);
        }
        
        private static User Krunal = User.Krunal;
        private static User JohnDoe = User.JohnDoe;

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
                .BuildSerializedFormat();

            var response =
                await _client.PostAsync(
                    "v1/care-contexts/discover",
                    requestContent);

            response.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public async void ShouldSendStatusCode200IfPatientFound()
        {
            //Given
            User patient = Krunal;

            GivenAPatientStartedANewDiscoveryRequest(Krunal, out DiscoveryRequest discoveryRequest);
            AndThisPatientMatchASingleRegisteredPatient(Krunal, new []{"name", "gender"}, out DiscoveryRepresentation discoveryRepresentation);

            //When
            await careContextDiscoveryController.GetPatientCareContext(discoveryRequest);

            //Then
            responsesSentToGateway.Should().ContainKey(discoveryRequest.TransactionId);

            GatewayDiscoveryRepresentation actualResponse = responsesSentToGateway[discoveryRequest.TransactionId];

            actualResponse.Patient.ReferenceNumber.Should().Be(patient.Id);
            actualResponse.Patient.Display.Should().Be(patient.Name);
            actualResponse.Patient.CareContexts.Count().Should().Be(patient.CareContexts.Count());
            foreach (CareContextRepresentation careContext in patient.CareContexts)
            {
                actualResponse.Patient.CareContexts.Should().ContainEquivalentOf(careContext);
            }
            actualResponse.Patient.MatchedBy.Count().Should().Be(discoveryRepresentation.Patient.MatchedBy.Count());
            foreach (var matchedFieldName in discoveryRepresentation.Patient.MatchedBy)
            {
                actualResponse.Patient.MatchedBy.Should().ContainEquivalentOf(matchedFieldName);
            }

            actualResponse.TransactionId.Should().Be(discoveryRequest.TransactionId);

            actualResponse.Resp.RequestId.Should().Be(discoveryRequest.RequestId);
            actualResponse.Resp.StatusCode.Should().Be(HttpStatusCode.OK);
            actualResponse.Resp.Message.Should().Be("Patient record with one or more care contexts found");

            actualResponse.Error.Should().BeNull();
        }

        [Fact]
        public async void ShouldSendStatusCode404WhenNoPatientWasFound()
        {
            //Given
            GivenAPatientStartedANewDiscoveryRequest(JohnDoe, out DiscoveryRequest discoveryRequest);
            AndThePatientDoesNotMatchAnyRegisteredPatient(out ErrorRepresentation errorRepresentation);

            //When
            await careContextDiscoveryController.GetPatientCareContext(discoveryRequest);

            //Then
            responsesSentToGateway.Should().ContainKey(discoveryRequest.TransactionId);

            GatewayDiscoveryRepresentation actualResponse = responsesSentToGateway[discoveryRequest.TransactionId];

            actualResponse.Patient.Should().BeNull();

            actualResponse.TransactionId.Should().Be(discoveryRequest.TransactionId);

            actualResponse.Resp.RequestId.Should().Be(discoveryRequest.RequestId);
            actualResponse.Resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            actualResponse.Resp.Message.Should().Be("No Matching Record Found or More than one Record Found");

            actualResponse.Error.Code.Should().Be(ErrorCode.NoPatientFound);
            actualResponse.Error.Message.Should().Be(errorRepresentation.Error.Message);
        }

        [Fact]
        public async void ShouldSendStatusCode404WhenMultiplePatientWereFound()
        {
            //Given
            GivenAPatientStartedANewDiscoveryRequest(JohnDoe, out DiscoveryRequest discoveryRequest);
            AndThePatientMatchMultipleRegisteredPatient(out ErrorRepresentation errorRepresentation);

            //When
            await careContextDiscoveryController.GetPatientCareContext(discoveryRequest);

            //Then
            responsesSentToGateway.Should().ContainKey(discoveryRequest.TransactionId);
            var actualResponse = responsesSentToGateway[discoveryRequest.TransactionId];

            actualResponse.Patient.Should().BeNull();

            actualResponse.TransactionId.Should().Be(discoveryRequest.TransactionId);

            actualResponse.Resp.RequestId.Should().Be(discoveryRequest.RequestId);
            actualResponse.Resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
            actualResponse.Resp.Message.Should().Be("No Matching Record Found or More than one Record Found");

            actualResponse.Error.Code.Should().Be(ErrorCode.MultiplePatientsFound);
            actualResponse.Error.Message.Should().Be(errorRepresentation.Error.Message);
        }

        [Fact]
        public async void ShouldSendStatusCode500WhenBahmniIsDownOrAnExternalSystem()
        {
            //Given
            GivenAPatientStartedANewDiscoveryRequest(Krunal, out DiscoveryRequest discoveryRequest);
            ButTheDataSourceIsNotReachable();

            //When
            await careContextDiscoveryController.GetPatientCareContext(discoveryRequest);

            //Then
            responsesSentToGateway.Should().ContainKey(discoveryRequest.TransactionId);
            var actualResponse = responsesSentToGateway[discoveryRequest.TransactionId];
            actualResponse.Patient.Should().BeNull();

            actualResponse.TransactionId.Should().Be(discoveryRequest.TransactionId);

            actualResponse.Resp.RequestId.Should().Be(discoveryRequest.RequestId);
            actualResponse.Resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            actualResponse.Resp.Message.Should().Be("Unreachable external service");

            actualResponse.Error.Code.Should().Be(ErrorCode.ServerInternalError);
            actualResponse.Error.Message.Should().Be("Unreachable external service");
        }

        [Fact]
        public async void ShouldAddTheDiscoveryTaskToTheBackgroundJobList()
        {
            //Given
            GivenAPatientStartedANewDiscoveryRequest(JohnDoe, out DiscoveryRequest discoveryRequest);

            //When
            careContextDiscoveryController.DiscoverPatientCareContexts(discoveryRequest);

            //Then
            backgroundJobs.Should().ContainKey("GetPatientCareContext");
            ((DiscoveryRequest)backgroundJobs["GetPatientCareContext"].Args.First()).Should().BeSameAs(discoveryRequest);
        }

        private void GivenAPatientStartedANewDiscoveryRequest(User user, out DiscoveryRequest discoveryRequest)
        {
            discoveryRequest = discoveryRequestBuilder
                .FromUser(user)
                .WithRequestId("aRequestId")
                .WithTransactionId("aTransactionId")
                .RequestedOn(new DateTime(2020, 06, 14))
                .Build();
        }

        private void AndThisPatientMatchASingleRegisteredPatient(User patient, IEnumerable<string> matchBy, out DiscoveryRepresentation discoveryRepresentation)
        {
            var discovery = new DiscoveryRepresentation(
                new PatientEnquiryRepresentation(
                    patient.Id,
                    patient.Name,
                    patient.CareContexts,
                    matchBy
                )
            );

            patientDiscoveryMock
                .Setup(patientDiscovery => patientDiscovery.PatientFor(It.IsAny<DiscoveryRequest>()))
                .Returns(async () => (discovery, null));

            discoveryRepresentation = discovery;
        }

        private void AndThePatientDoesNotMatchAnyRegisteredPatient(out ErrorRepresentation errorRepresentation)
        {
            var error = new ErrorRepresentation(new Error(ErrorCode.NoPatientFound, "unusedMessage"));

            patientDiscoveryMock
                .Setup(patientDiscovery => patientDiscovery.PatientFor(It.IsAny<DiscoveryRequest>()))
                .Returns(async () => (null, error));

            errorRepresentation = new ErrorRepresentation(new Error(ErrorCode.NoPatientFound, "unusedMessage"));
        }

        private void AndThePatientMatchMultipleRegisteredPatient(out ErrorRepresentation errorRepresentation)
        {
            var error = new ErrorRepresentation(new Error(ErrorCode.MultiplePatientsFound, "unusedMessage"));
            patientDiscoveryMock
                .Setup(patientDiscovery => patientDiscovery.PatientFor(It.IsAny<DiscoveryRequest>()))
                .Returns(async () => (null, error));

            errorRepresentation = error;
        }

        private void ButTheDataSourceIsNotReachable()
        {
            patientDiscoveryMock
                .Setup(patientDiscovery => patientDiscovery.PatientFor(It.IsAny<DiscoveryRequest>()))
                .Returns(async () => throw new Exception("Exception coming from tests"));
        }

        private static void SetupGatewayClientToSaveAllSentDiscoveryIntoThisList(Mock<IGatewayClient> gatewayClientMock,
            Dictionary<string, GatewayDiscoveryRepresentation> responsesSentToGateway)
        {
            gatewayClientMock
                .Setup(gatewayClient => gatewayClient.SendDataToGateway(
                    It.IsAny<string>(), It.IsAny<GatewayDiscoveryRepresentation>(), It.IsAny<string>())
                )
                .Callback<string, GatewayDiscoveryRepresentation, string>((urlPath, response, cmSuffix) =>
                {
                    responsesSentToGateway.TryAdd(response.TransactionId, response);
                });
        }

        private void SetupBackgroundJobClientToSaveAllCreatedJobsIntoThisList(
            Mock<IBackgroundJobClient> backgroundJobClientMock,
            Dictionary<string, Job> backgroundJobs)
        {
            backgroundJobClientMock
                .Setup(s => s.Create(It.IsAny<Job>(), It.IsAny<IState>()))
                .Callback<Job, IState>((job, state) => { backgroundJobs.Add(job.Method.Name, job); });
        }
    }
}