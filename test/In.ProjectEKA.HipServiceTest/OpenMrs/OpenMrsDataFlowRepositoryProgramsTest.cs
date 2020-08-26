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
    public class OpenMrsDataFlowRepositoryProgramsTest
    {
        private readonly Mock<IOpenMrsClient> openmrsClientMock;
        private readonly OpenMrsDataFlowRepository dataFlowRepository;

        public OpenMrsDataFlowRepositoryProgramsTest()
        {
          openmrsClientMock = new Mock<IOpenMrsClient>();
          dataFlowRepository = new OpenMrsDataFlowRepository(openmrsClientMock.Object);
        }

        [Fact]
        public async Task LoadObservationsForPrograms_ShouldReturnListOfObservations()
        {
            //Given
            string programEnrollmentUuid = "12345678-1234-1234-1234-123456789ABC";

            var path = $"{Endpoints.OpenMrs.OnProgramObservations}{programEnrollmentUuid}";

            var patientProgramsWithObservationsResponse =
                    File.ReadAllText("../../../OpenMrs/sampleData/PatientProgramsWithObservations.json");
            SetupOpenMrsClient(path, patientProgramsWithObservationsResponse);
            var expectedObservations = new Dictionary<string, string> {
                { "15d8c427-9f31-4049-9509-07ab7b599f1b", "Mental Health: Treatment complete, test drug, test chief complaint, test diagnosis" }
                , { "3b319244-4946-44f9-a271-4df830968b93", "Mental Health, Disposition: Treatment complete" }
                , { "f4c20284-3cfb-45d2-8ba5-ac23bfe7299f", "Mental Health, Drugs Provided: test drug" }
                , { "549d33ad-ff78-404c-8021-19a16a6f539b", "Mental Health, Chief Complaint: test chief complaint" }
                , { "995597e2-9daf-45d6-b382-1a7f950c2179", "Mental Health, Diagnosis: test diagnosis" }
            };
            foreach (var obsUuid in expectedObservations.Keys)
            {
                SetupOpenMrsClient(
                    $"{Endpoints.OpenMrs.OnObs}/{obsUuid}",
                    File.ReadAllText($"../../../OpenMrs/sampleData/Observations/Obs_{obsUuid}.json")
                );
            }

            //When
            var observations = await dataFlowRepository.LoadObservationsForPrograms(programEnrollmentUuid);

            //Then
            observations.Should().NotBeNullOrEmpty();
            observations.Should().HaveCount(expectedObservations.Count);
            foreach (var observation in observations)
            {
                expectedObservations.Values.Should().Contain(observation.Display);
            }
            foreach (var expectedObservation in expectedObservations)
            {
                observations.Select(o => o.Display).Should().Contain(expectedObservation.Value);
            }
        }

        [Fact]
        public void LoadObservationsForProgramsWhenObservationsHaveNoUuid_ShouldThrowError()
        {
            //Given
            string programEnrollmentUuid = "12345678-1234-1234-1234-123456789ABC";

            var path = $"{Endpoints.OpenMrs.OnProgramObservations}{programEnrollmentUuid}";

            var patientProgramsWithObservationsResponse =
                    File.ReadAllText("../../../OpenMrs/sampleData/PatientProgramsWithObservationsHavingNoUuid.json");
            SetupOpenMrsClient(path, patientProgramsWithObservationsResponse);

            //When
            Func<Task> loadObservations = async () =>
                await dataFlowRepository.LoadObservationsForPrograms(programEnrollmentUuid);

            //Then
            loadObservations.Should().Throw<OpenMrsFormatException>();
        }


        [Fact]
        public void LoadObservationsForProgramsWhenSpecificObservationHaveNoUuidAndDisplay_ShouldThrowError()
        {
            //Given
            string programEnrollmentUuid = "12345678-1234-1234-1234-123456789ABC";

            var path = $"{Endpoints.OpenMrs.OnProgramObservations}{programEnrollmentUuid}";

            var patientProgramsWithObservationsResponse =
                    File.ReadAllText("../../../OpenMrs/sampleData/PatientProgramsWithOneObservation.json");
            SetupOpenMrsClient(path, patientProgramsWithObservationsResponse);
            var obsUuid = "15d8c427-9f31-4049-9509-07ab7b599f1b";
            SetupOpenMrsClient(
                $"{Endpoints.OpenMrs.OnObs}/{obsUuid}",
                File.ReadAllText($"../../../OpenMrs/sampleData/Observations/Obs_{obsUuid}_without_uuid_and_display.json")
            );

            //When
            Func<Task> loadObservations = async () =>
                await dataFlowRepository.LoadObservationsForPrograms(programEnrollmentUuid);

            //Then
            loadObservations.Should().Throw<OpenMrsFormatException>();
        }

        private void SetupOpenMrsClient(string path, string response)
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
