using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using In.ProjectEKA.HipService.OpenMrs;
using Moq;
using Xunit;
using System.IO;
using System.Collections.Generic;
namespace In.ProjectEKA.HipServiceTest.OpenMrs
{

    [Collection("DataFlowRepository Tests")]
    public class OpenMrsDataFlowRepositoryTest
    {
        private readonly Mock<IOpenMrsClient> openmrsClientMock;
        private readonly OpenMrsDataFlowRepository dataFlowRepository;

        public OpenMrsDataFlowRepositoryTest()
        {
            openmrsClientMock = new Mock<IOpenMrsClient>();
            dataFlowRepository = new OpenMrsDataFlowRepository(openmrsClientMock.Object);
        }

        [Fact]
        public async Task LoadObservationsForVisits_ShouldReturnListOfObservations()
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";

            var PatientVisitsWithObservations = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitWithObservations.json");
            openMrsClientReturnsVisits(path, PatientVisitsWithObservations);

            //When
            var observations = await dataFlowRepository.LoadObservationsForVisits(patientReferenceNumber, "OPD");

            //Then
            var firstObservation = observations[0];
            firstObservation.Display.Should().Be("Location of diagnosis: India");
        }

        public static IEnumerable<object[]> GetPatientVisitsWithNoObservation(int numTests)
        {
            var PatientVisitsWithoutVisits = File.ReadAllText("../../../OpenMrs/sampleData/EmptyData.json");
            var PatientVisitsWithoutEncounters = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithoutEncounters.json");
            var PatientVisitsWithoutObservation = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithoutObservation.json");
            var PatientVisitsWithoutVisitType = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitWithoutVisitType.json");

            var sampleData = new List<object[]>
            {
          new object[]{PatientVisitsWithoutVisits},
          new object[]{PatientVisitsWithoutEncounters},
          new object[]{PatientVisitsWithoutObservation},
          new object[]{PatientVisitsWithoutVisitType}
          };
            return sampleData.Take(numTests);
        }

        [Theory]
        [MemberData(nameof(GetPatientVisitsWithNoObservation), parameters: 4)]
        public async Task LoadObservationsForVisits_ShouldReturnEmptyList_WhenNoObservationFound(string PatientVisits)
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";

            openMrsClientReturnsVisits(path, PatientVisits);

            //When
            var observations = await dataFlowRepository.LoadObservationsForVisits(patientReferenceNumber, "OPD");

            //Then
            Assert.Empty(observations);
        }

        [Fact]
        public void LoadObservationsForVisits_ShouldReturnError_WhenNoPatientReferenceNumber()
        {
            //Given
            var patientReferenceNumber = string.Empty;

            //When
            Func<Task> loadObservationsForVisits = async () =>
            {
                await dataFlowRepository.LoadObservationsForVisits(patientReferenceNumber, "OPD");
            };

            //Then
            loadObservationsForVisits.Should().Throw<OpenMrsFormatException>();
        }
        [Fact]
        public async Task LoadDiagnosticReportForVisits_ShouldReturnVisitDiagnoses()
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";
            var PatientVisitsWithDiagnosis = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithDiagnosis.json");

            openMrsClientReturnsVisits(path, PatientVisitsWithDiagnosis);

            //When
            var diagnosis = await dataFlowRepository.LoadDiagnosticReportForVisits(patientReferenceNumber, "OPD");

            //Then
            var firstDiagnosis = diagnosis[0];
            firstDiagnosis.Display.Should().Be("Visit Diagnoses: Primary, Confirmed, Hypertension, unspec., a5e9f749-4bd5-43b4-b5e5-886f0eccc09f, false");
        }
        public static IEnumerable<object[]> GetPatientVisitsWithNoDiagnosis(int numTests)
        {
            var PatientVisitsWithoutVisitType = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitWithoutVisitType.json");
            var PatientVisitsWithoutDiagnosis = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitWithoutVisitType.json");
            var sampleData = new List<object[]>
            {
              new object[] {PatientVisitsWithoutVisitType},
              new object[] {PatientVisitsWithoutDiagnosis}
            };
            return sampleData.Take(numTests);
        }
        [Theory]
        [MemberData(nameof(GetPatientVisitsWithNoDiagnosis), parameters: 2)]
        public async Task LoadDiagnosticReportForVisits_ShouldReturnEmptyList_WhenNoDiagnosisFound(string PatientVisits)
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";
            openMrsClientReturnsVisits(path, PatientVisits);

            var diagnosis = await dataFlowRepository.LoadDiagnosticReportForVisits(patientReferenceNumber, "OPD");

            //Then
            Assert.Empty(diagnosis);
        }

        [Fact]
        public async Task LoadMedicationForVisits_ShouldReturnVisitmedication()
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";
            var PatientVisitsWithMedication = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithMedication.json");
            openMrsClientReturnsVisits(path, PatientVisitsWithMedication);

            //When
            var medication = await dataFlowRepository.LoadMedicationForVisits(patientReferenceNumber, "OPD");

            //Then
            var firstOrder = medication[0];
            firstOrder.Display.Should().Be("(NEW) Losartan 50mg: null");
        }
        [Fact]
        public async Task LoadMedicationForVisits_ShouldReturnEmptyList_WhenNoOrdersFound()
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";
            var PatientVisitsWithoutOrder = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithoutOrders.json");

            openMrsClientReturnsVisits(path, PatientVisitsWithoutOrder);

            //When
            var medications = await dataFlowRepository.LoadMedicationForVisits(patientReferenceNumber, "OPD");

            //Then
            Assert.Empty(medications);
        }
        [Fact]
        public async Task LoadConditionForVisits_ShouldReturnVisitCondition()
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.EMRAPI.OnConditionPath}?patientUuid={patientReferenceNumber}";
            var PatientVisitsWithCondition = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithCondition.json");


            openMrsClientReturnsVisits(path, PatientVisitsWithCondition);

            var conditions = await dataFlowRepository.LoadConditionsForVisit(patientReferenceNumber);

            //Then
            var firstCondition = conditions[0];
            firstCondition.ConditionNonCoded.Should().Be("Former smoker");
        }
        [Fact]
        public async Task LoadConditionForVisits__ShouldReturnEmptyList_WhenNoConditionsFound()
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.EMRAPI.OnConditionPath}?patientUuid={patientReferenceNumber}";
            var PatientVisitsNoCondition = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitWithNoCondition.json");
            openMrsClientReturnsVisits(path, PatientVisitsNoCondition);

            var conditions = await dataFlowRepository.LoadConditionsForVisit(patientReferenceNumber);

            //Then
            Assert.Empty(conditions);
        }


        private void openMrsClientReturnsVisits(string path, string response)
        {
            openmrsClientMock
                .Setup(x => x.GetAsync(path))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(response)
                })
                .Verifiable();
        }
    }
}