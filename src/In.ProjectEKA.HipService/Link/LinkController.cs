namespace In.ProjectEKA.HipService.Link
{
    using System;
    using System.Threading.Tasks;
    using Discovery;
    using Gateway;
    using Gateway.Model;
    using Hangfire;
    using HipLibrary.Patient.Model;
    using Logger;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("v1/links/link/init")]
    public class LinkController : ControllerBase
    {
        private readonly IDiscoveryRequestRepository discoveryRequestRepository;
        private readonly IBackgroundJobClient backgroundJob;
        private readonly LinkPatient linkPatient;
        private readonly GatewayClient gatewayClient;

        public LinkController(
            IDiscoveryRequestRepository discoveryRequestRepository,
            IBackgroundJobClient backgroundJob,
            LinkPatient linkPatient, GatewayClient gatewayClient)
        {
            this.discoveryRequestRepository = discoveryRequestRepository;
            this.backgroundJob = backgroundJob;
            this.linkPatient = linkPatient;
            this.gatewayClient = gatewayClient;
        }

        [HttpPost]
        public AcceptedResult LinkFor(LinkReferenceRequest request)
        {
            backgroundJob.Enqueue(() => LinkPatient(request));
            return Accepted();
        }

        public async Task LinkPatient(LinkReferenceRequest request)
        {
            var cmUserId = request.Patient.Id;
            var cmSuffix = cmUserId.Substring(
                cmUserId.LastIndexOf("@", StringComparison.Ordinal) + 1);
            var patient = new LinkEnquiry(
                cmSuffix,
                cmUserId,
                request.Patient.ReferenceNumber,
                request.Patient.CareContexts);
            try
            {
                var doesRequestExists = await discoveryRequestRepository.RequestExistsFor(
                    request.TransactionId,
                    request.Patient?.Id,
                    request.Patient?.ReferenceNumber);

                ErrorRepresentation errorRepresentation = null;
                if (!doesRequestExists)
                {
                    errorRepresentation = new ErrorRepresentation(
                        new Error(ErrorCode.DiscoveryRequestNotFound, ErrorMessage.DiscoveryRequestNotFound));
                }

                var patientReferenceRequest =
                    new PatientLinkEnquiry(request.TransactionId, request.RequestId, patient);
                var linkEnquiryRepresentation = new PatientLinkEnquiryRepresentation();

                var (linkReferenceResponse, error) = errorRepresentation != null
                    ? (linkEnquiryRepresentation, errorRepresentation)
                    : await linkPatient.LinkPatients(patientReferenceRequest);

                var response = new GatewayLinkResponse(
                    linkReferenceResponse ?? linkEnquiryRepresentation,
                    error?.Error,
                    new Resp(request.RequestId),
                    request.TransactionId,
                    DateTime.Now.ToUniversalTime(),
                    Guid.NewGuid());

                await gatewayClient.SendDataToGateway(GatewayPathConstants.OnLinkInitPath, response, cmSuffix);
            }
            catch (Exception exception)
            {
                Log.Error(exception, exception.StackTrace);
            }
        }
    }
}