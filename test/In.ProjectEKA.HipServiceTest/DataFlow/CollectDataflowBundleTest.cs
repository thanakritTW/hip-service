using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using In.ProjectEKA.HipLibrary.Patient.Model;
using In.ProjectEKA.HipLibrary.DataFlow;
using In.ProjectEKA.HipService.DataFlow;
using Moq;
using Xunit;
using Task = System.Threading.Tasks.Task;
using DataRequest = In.ProjectEKA.HipLibrary.Patient.Model.DataRequest;
using Optional.Unsafe;
using System.Linq;
using FluentAssertions;

namespace In.ProjectEKA.HipServiceTest.DataFlow
{
    public class CollectDataflowBundleTest
    {
        private CollectDataflowBundle collectDataflowBundle;

        private Mock<IOpenMrsDataFlowRepository> openMrsDataflowRepository;

        public CollectDataflowBundleTest()
        {
            openMrsDataflowRepository = new Mock<IOpenMrsDataFlowRepository>();

            collectDataflowBundle = new CollectDataflowBundle(openMrsDataflowRepository.Object);
        }

        [Fact]
        public async Task ShouldReturnBundleForDataflowData()
        {
            //arrange
            var linkedCareContextVisitType = "OPD";
            var patientId = "1";
            openMrsDataflowRepository
                .Setup(r => r.GetMedicationsForVisits(patientId, linkedCareContextVisitType))
                .Returns((string patientId, string visitType) => Task.FromResult("test medicine"));
            DataRequest dataRequest = GetDataRequest(patientId, linkedCareContextVisitType);

            //act
            var entries = await collectDataflowBundle.CollectData(dataRequest);

            //assert
            var careBundles = entries.ValueOrDefault()?.CareBundles;
            careBundles.Should().NotBeNull();
            careBundles.Count().Should().Be(1);
            careBundles.First().CareContextReference.Should().Be(linkedCareContextVisitType);
            careBundles.First().BundleForThisCcr.Entry.First().Children.First().GetType().Should().Be(typeof(MedicationRequest));
        }

        private static DataRequest GetDataRequest(string patientId, string careContextId)
        {
            const string consentId = "ConsentId";
            const string consentManagerId = "ConsentManagerId";
            var grantedContexts = new List<GrantedContext>
            {
                new GrantedContext(patientId, careContextId)
            };
            var dateRange = new HipLibrary.Patient.Model.DateRange("2017-12-01T15:43:00.818234", "2021-12-31T15:43:00.818234");
            var hiTypes = new List<HiType>
            {
                HiType.Condition,
                HiType.Observation,
                HiType.DiagnosticReport,
                HiType.MedicationRequest,
                HiType.DocumentReference,
                HiType.Prescription,
                HiType.DischargeSummary,
                HiType.OPConsultation
            };
            var dataRequest = new DataRequest(grantedContexts,
                dateRange,
                "/someUrl",
                hiTypes,
                "someTxnId",
                null,
                consentManagerId,
                consentId,
                "sometext");
            return dataRequest;
        }
    }
}
