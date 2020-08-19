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

        public static IEnumerable<object[]> GetPatientVisitsWithNoObservation()
        {
            var PatientVisitsWithoutVisits = File.ReadAllText("../../../OpenMrs/sampleData/EmptyData.json");
            var PatientVisitsWithoutEncounters = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithoutEncounters.json");
            var PatientVisitsWithoutObservation = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithoutObservation.json");
            var PatientVisitsWithoutVisitType = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitWithoutVisitType.json");


            yield return new object[] { PatientVisitsWithoutVisits };
            yield return new object[] { PatientVisitsWithoutEncounters };
            yield return new object[] { PatientVisitsWithoutObservation };
            yield return new object[] { PatientVisitsWithoutVisitType };

        }

        [Theory]
        [MemberData(nameof(GetPatientVisitsWithNoObservation))]
        public async Task LoadObservationsForVisits_ShouldReturnEmptyList_WhenNoObservationFound(string patientVisits)
        {
            //Given
            var patientReferenceNumber = "123";
            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";

            openMrsClientReturnsVisits(path, patientVisits);

            //When
            var observations = await dataFlowRepository.LoadObservationsForVisits(patientReferenceNumber, "OPD");

            //Then
            Assert.Empty(observations);
        }
        [Fact]
        public async Task LoadObservationVisits_ShouldReturnEmptyList_WhenVisitTypeIsIncorect()
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";
            var PatientVisitsWithObservations = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitWithObservations.json");
            openMrsClientReturnsVisits(path, PatientVisitsWithObservations);

            var observations = await dataFlowRepository.LoadObservationsForVisits(patientReferenceNumber, "");

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
        public static IEnumerable<object[]> GetPatientVisitsWithNoDiagnosis()
        {
            var PatientVisitsWithoutVisitType = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitWithoutVisitType.json");
            var PatientVisitsWithoutDiagnosis = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithoutDiagnosis.json");
            var PatientVisitsWithoutVisits = File.ReadAllText("../../../OpenMrs/sampleData/EmptyData.json");
            var PatientVisitsWithoutEncounters = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithoutEncounters.json");
            var PatientVisitsWithoutObservation = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithoutObservation.json");

            yield return new object[] { PatientVisitsWithoutVisitType };
            yield return new object[] { PatientVisitsWithoutDiagnosis };
            yield return new object[] { PatientVisitsWithoutVisits };
            yield return new object[] { PatientVisitsWithoutEncounters };
            yield return new object[] { PatientVisitsWithoutObservation };
        }

        [Theory]
        [MemberData(nameof(GetPatientVisitsWithNoDiagnosis))]
        public async Task LoadDiagnosticReportForVisits_ShouldReturnEmptyList_WhenNoDiagnosisFound(string patientVisits)
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";
            openMrsClientReturnsVisits(path, patientVisits);

            var diagnosis = await dataFlowRepository.LoadDiagnosticReportForVisits(patientReferenceNumber, "OPD");

            //Then
            Assert.Empty(diagnosis);
        }

        [Fact]
        public async Task LoadDiagnosticReportForVisits_ShouldReturnEmptyList_WhenVisitTypeIsIncorect()
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";
            var PatientVisitsWithDiagnosis = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithDiagnosis.json");
            openMrsClientReturnsVisits(path, PatientVisitsWithDiagnosis);

            var diagnosis = await dataFlowRepository.LoadDiagnosticReportForVisits(patientReferenceNumber, "");

            //Then
            Assert.Empty(diagnosis);
        }

        [Fact]
        public void LoadDiagnosticReportForVisits_ShouldReturnError_WhenNoPatientReferenceNumber()
        {
            //Given
            var patientReferenceNumber = string.Empty;

            //When
            Func<Task> loadDiagnosticReportForVisits = async () =>
            {
                await dataFlowRepository.LoadDiagnosticReportForVisits(patientReferenceNumber, "OPD");
            };

            //Then
            loadDiagnosticReportForVisits.Should().Throw<OpenMrsFormatException>();
        }

        [Fact]
        public async Task LoadMedicationForVisits_ShouldReturnVisitMedication()
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

        public static IEnumerable<object[]> GetPatientVisitsWithNoOrders()
        {
            var PatientVisitsWithoutVisitType = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitWithoutVisitType.json");
            var PatientVisitsWithoutOrder = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithoutOrders.json");
            var PatientVisitsWithoutVisits = File.ReadAllText("../../../OpenMrs/sampleData/EmptyData.json");
            var PatientVisitsWithoutEncounters = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithoutEncounters.json");

            yield return new object[] { PatientVisitsWithoutVisitType };
            yield return new object[] { PatientVisitsWithoutOrder };
            yield return new object[] { PatientVisitsWithoutVisits };
            yield return new object[] { PatientVisitsWithoutEncounters };

        }
        [Theory]
        [MemberData(nameof(GetPatientVisitsWithNoOrders))]
        public async Task LoadMedicationForVisits_ShouldReturnEmptyList_WhenNoOrdersFound(string patientVisits)
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";

            openMrsClientReturnsVisits(path, patientVisits);

            //When
            var medications = await dataFlowRepository.LoadMedicationForVisits(patientReferenceNumber, "OPD");

            //Then
            Assert.Empty(medications);
        }
        [Fact]
        public async Task LoadMedicationForVisits_ShouldReturnEmptyList_WhenVisitTypeIsIncorect()
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";
            var PatientVisitsWithMedication = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitsWithMedication.json");
            openMrsClientReturnsVisits(path, PatientVisitsWithMedication);

            var medications = await dataFlowRepository.LoadObservationsForVisits(patientReferenceNumber, "");

            //Then
            Assert.Empty(medications);

        }
        [Fact]
        public void LoadMedicationForVisits_ShouldReturnError_WhenNoPatientReferenceNumber()
        {
            //Given
            var patientReferenceNumber = string.Empty;

            //When
            Func<Task> loadMedicationForVisits = async () =>
            {
                await dataFlowRepository.LoadMedicationForVisits(patientReferenceNumber, "OPD");
            };

            //Then
            loadMedicationForVisits.Should().Throw<OpenMrsFormatException>();
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
        [Fact]
        public void LoadConditionsForVisit_ShouldReturnError_WhenNoPatientReferenceNumber()
        {
            //Given
            var patientReferenceNumber = string.Empty;

            //When
            Func<Task> loadConditionsForVisit = async () =>
             {
                 await dataFlowRepository.LoadConditionsForVisit(patientReferenceNumber);
             };

            //Then
            loadConditionsForVisit.Should().Throw<OpenMrsFormatException>();
        }
        [Fact]
        public async Task LoadConditionForVisits_ShouldReturnVisitCodedCondition()
        {
            //Given
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.EMRAPI.OnConditionPath}?patientUuid={patientReferenceNumber}";
            var PatientVisitsWithCondition = File.ReadAllText("../../../OpenMrs/sampleData/PatientVisitWithCodedCondition.json");


            openMrsClientReturnsVisits(path, PatientVisitsWithCondition);

            var conditions = await dataFlowRepository.LoadConditionsForVisit(patientReferenceNumber);

            //Then
            var firstCondition = conditions[0];
            firstCondition.ConditionNonCoded.Should().Be(null);
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