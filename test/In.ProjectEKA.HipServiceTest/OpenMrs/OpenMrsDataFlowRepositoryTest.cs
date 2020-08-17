using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using In.ProjectEKA.HipService.OpenMrs;
using Moq;
using Xunit;
using System;

namespace In.ProjectEKA.HipServiceTest.OpenMrs
{

    [Collection("DataFlowRepository Tests")]
    public class OpenMrsDataFlowRepositoryTest
    {
        [Fact]
        public async Task LoadObservationsForVisits_ShouldReturnListOfObservations()
        {
            //Given
            var openmrsClientMock = new Mock<IOpenMrsClient>();
            var dataFlowRepository = new OpenMrsDataFlowRepository(openmrsClientMock.Object);
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";

            openmrsClientMock
                .Setup(x => x.GetAsync(path))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(PatientVisitsSample)
                })
                .Verifiable();

            //When
            var observations = await dataFlowRepository.LoadObservationsForVisits(patientReferenceNumber, "OPD");

            //Then
            var firstObservation = observations[0];
            firstObservation.Display.Should().Be("Location of diagnosis: India");
        }

        [Theory]
        [InlineData(PatientVisitsSampleWithoutVisits)]
        [InlineData(PatientVisitsSampleWithoutEncounters)]
        [InlineData(PatientVisitsSampleWithoutObs)]
        [InlineData(PatientVisitsSampleWithoutVisitType)]
        public async Task LoadObservationsForVisits_ShouldReturnEmptyList_WhenNoObservationFound(string sampleData)
        {
            //Given
            var openmrsClientMock = new Mock<IOpenMrsClient>();
            var dataFlowRepository = new OpenMrsDataFlowRepository(openmrsClientMock.Object);
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";

            openmrsClientMock
                .Setup(x => x.GetAsync(path))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(sampleData)
                })
                .Verifiable();

            //When
            var observations = await dataFlowRepository.LoadObservationsForVisits(patientReferenceNumber, "OPD");

            //Then
            Assert.Empty(observations);
        }

        [Fact]
        public void LoadObservationsForVisits_ShouldReturnError_WhenNoPatientReferenceNumber()
        {
            //Given
            var openmrsClientMock = new Mock<IOpenMrsClient>();
            var dataFlowRepository = new OpenMrsDataFlowRepository(openmrsClientMock.Object);
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
            var openmrsClientMock = new Mock<IOpenMrsClient>();
            var dataFlowRepository = new OpenMrsDataFlowRepository(openmrsClientMock.Object);
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";

            openmrsClientMock
                .Setup(x => x.GetAsync(path))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(PatientVisitsSampleWithDiagnosis)
                })
                .Verifiable();

            //When
            var diagnosis = await dataFlowRepository.LoadDiagnosticReportForVisits(patientReferenceNumber, "OPD");

            //Then
            var firstDiagnosis = diagnosis[0];
            firstDiagnosis.Display.Should().Be("Visit Diagnoses: Primary, Confirmed, Hypertension, unspec., a5e9f749-4bd5-43b4-b5e5-886f0eccc09f, false");
        }
        [Theory]
        [InlineData(PatientVisitsSampleWithoutVisitType)]
        [InlineData(sampleDataWithoutDiagnosis)]
        public async Task LoadDiagnosticReportForVisits_ShouldReturnEmptyList_WhenNoDiagnosisFound(string sampleData)
        {
            //Given
            var openmrsClientMock = new Mock<IOpenMrsClient>();
            var dataFlowRepository = new OpenMrsDataFlowRepository(openmrsClientMock.Object);
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";

            openmrsClientMock
                .Setup(x => x.GetAsync(path))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(sampleData)
                })
                .Verifiable();

            //When
            var diagnosis = await dataFlowRepository.LoadDiagnosticReportForVisits(patientReferenceNumber, "OPD");

            //Then
            Assert.Empty(diagnosis);
        }

        [Fact]
        public async Task LoadMedicationForVisits_ShouldReturnVisitmedication()
        {
            //Given
            var openmrsClientMock = new Mock<IOpenMrsClient>();
            var dataFlowRepository = new OpenMrsDataFlowRepository(openmrsClientMock.Object);
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";

            openmrsClientMock
                .Setup(x => x.GetAsync(path))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(PatientVisitsSampleWithMedication)
                })
                .Verifiable();

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
            var openmrsClientMock = new Mock<IOpenMrsClient>();
            var dataFlowRepository = new OpenMrsDataFlowRepository(openmrsClientMock.Object);
            var patientReferenceNumber = "123";

            var path = $"{Endpoints.OpenMrs.OnVisitPath}?patient={patientReferenceNumber}&v=full";

            openmrsClientMock
                .Setup(x => x.GetAsync(path))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(sampleDataWithoutOrders)
                })
                .Verifiable();

            //When
            var medications = await dataFlowRepository.LoadMedicationForVisits(patientReferenceNumber, "OPD");

            //Then
            Assert.Empty(medications);
        }
        private const string sampleDataWithoutOrders = @"
{
    ""results"": [
{
    ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
    ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
    ""patient"": {
        ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
        ""display"": ""GAN203009 - Test Hypertension"",
        ""links"": [
            {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
            }
        ]
    },
    ""visitType"": {
        ""uuid"": ""c22a5000-3f10-11e4-adec-0800271c1b75"",
        ""display"": ""OPD"",
        ""links"": [
            {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visittype/c22a5000-3f10-11e4-adec-0800271c1b75""
            }
        ]
    },
    ""indication"": null,
    ""location"": {
        ""uuid"": ""c1e42932-3f10-11e4-adec-0800271c1b75"",
        ""display"": ""Ganiyari"",
        ""links"": [
            {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/c1e42932-3f10-11e4-adec-0800271c1b75""
            }
        ]
    },
    ""startDatetime"": ""2017-05-05T15:07:26.000+0800"",
    ""stopDatetime"": ""2017-05-18T14:42:47.000+0800"",
    ""encounters"": [
        {
            ""uuid"": ""ac369187-d04a-4aa2-bd8e-7c2e95e80c84"",
            ""display"": ""Consultation 05/18/2017"",
            ""encounterDatetime"": ""2017-05-18T14:42:47.000+0800"",
            ""patient"": {
                ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
                ""display"": ""GAN203009 - Test Hypertension"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
                    }
                ]
            },
            ""location"": {
                ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
                ""display"": ""General Ward"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
                    }
                ]
            },
            ""form"": null,
            ""encounterType"": {
                ""uuid"": ""81852aee-3f10-11e4-adec-0800271c1b75"",
                ""display"": ""Consultation"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81852aee-3f10-11e4-adec-0800271c1b75""
                    }
                ]
            },
            ""obs"": [
                {
                    ""uuid"": ""51b2349f-316d-4366-9163-f00f6699ad15"",
                    ""display"": ""Vitals: true, 90.0, Sitting, 130.0, false"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/51b2349f-316d-4366-9163-f00f6699ad15""
                        }
                    ]
                }
            ],
            ""orders"": [],
            ""voided"": false,
            ""visit"": {
                ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
                ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
                    }
                ]
            },
            ""encounterProviders"": [
                {
                    ""uuid"": ""70e0e7f9-2a75-4be9-bbf8-1037ff6a4832"",
                    ""display"": ""Super Man: Unknown"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/ac369187-d04a-4aa2-bd8e-7c2e95e80c84/encounterprovider/70e0e7f9-2a75-4be9-bbf8-1037ff6a4832""
                        }
                    ]
                }
            ],
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/ac369187-d04a-4aa2-bd8e-7c2e95e80c84""
                },
                {
                    ""rel"": ""full"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/ac369187-d04a-4aa2-bd8e-7c2e95e80c84?v=full""
                }
            ],
            ""resourceVersion"": ""1.9""
        },
        {
            ""uuid"": ""a8dd660f-e582-4a7a-b167-6a934697bac3"",
            ""display"": ""LAB_RESULT 05/05/2017"",
            ""encounterDatetime"": ""2017-05-05T16:36:36.000+0800"",
            ""patient"": {
                ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
                ""display"": ""GAN203009 - Test Hypertension"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
                    }
                ]
            },
            ""location"": {
                ""uuid"": ""c58e12ed-3f12-11e4-adec-0800271c1b75"",
                ""display"": ""OPD-1"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/c58e12ed-3f12-11e4-adec-0800271c1b75""
                    }
                ]
            },
            ""form"": null,
            ""encounterType"": {
                ""uuid"": ""82024e00-3f10-11e4-adec-0800271c1b75"",
                ""display"": ""LAB_RESULT"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/82024e00-3f10-11e4-adec-0800271c1b75""
                    }
                ]
            },
            ""obs"": [
                {
                    ""uuid"": ""ce574095-df86-485b-8e33-e2e69872c453"",
                    ""display"": ""LDL: "",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/ce574095-df86-485b-8e33-e2e69872c453""
                        }
                    ]
                },
                {
                    ""uuid"": ""ac481b5f-966f-47cd-b084-67d967b3c35e"",
                    ""display"": ""Kidney Function: "",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/ac481b5f-966f-47cd-b084-67d967b3c35e""
                        }
                    ]
                }
            ],
            ""orders"": [],
            ""voided"": false,
            ""visit"": {
                ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
                ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
                    }
                ]
            },
            ""encounterProviders"": [
                {
                    ""uuid"": ""d6508bc8-a808-48bf-8777-3fb9d42055df"",
                    ""display"": ""labsystem system: Unknown"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/a8dd660f-e582-4a7a-b167-6a934697bac3/encounterprovider/d6508bc8-a808-48bf-8777-3fb9d42055df""
                        }
                    ]
                }
            ],
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/a8dd660f-e582-4a7a-b167-6a934697bac3""
                },
                {
                    ""rel"": ""full"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/a8dd660f-e582-4a7a-b167-6a934697bac3?v=full""
                }
            ],
            ""resourceVersion"": ""1.9""
        },
        {
            ""uuid"": ""15da42b1-a535-4330-b228-8a6530c89cb9"",
            ""display"": ""Consultation 05/05/2017"",
            ""encounterDatetime"": ""2017-05-05T16:36:36.000+0800"",
            ""patient"": {
                ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
                ""display"": ""GAN203009 - Test Hypertension"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
                    }
                ]
            },
            ""location"": {
                ""uuid"": ""c58e12ed-3f12-11e4-adec-0800271c1b75"",
                ""display"": ""OPD-1"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/c58e12ed-3f12-11e4-adec-0800271c1b75""
                    }
                ]
            },
            ""form"": null,
            ""encounterType"": {
                ""uuid"": ""81852aee-3f10-11e4-adec-0800271c1b75"",
                ""display"": ""Consultation"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81852aee-3f10-11e4-adec-0800271c1b75""
                    }
                ]
            },
            ""obs"": [
                {
                    ""uuid"": ""3be53600-7934-41ba-a043-daa04021fc61"",
                    ""display"": ""History and Examination: false, blood spot in the eye, 4320.0"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/3be53600-7934-41ba-a043-daa04021fc61""
                        }
                    ]
                },
                {
                    ""uuid"": ""da7e38f7-57a2-46b4-a604-7c7dfe1c5519"",
                    ""display"": ""Hypertension, Intake: false, false, false, No Previous Diagnosis, 2017-05-05"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/da7e38f7-57a2-46b4-a604-7c7dfe1c5519""
                        }
                    ]
                },
                {
                    ""uuid"": ""a5e9f749-4bd5-43b4-b5e5-886f0eccc09f"",
                    ""display"": ""Visit Diagnoses: Primary, Confirmed, Hypertension, unspec., a5e9f749-4bd5-43b4-b5e5-886f0eccc09f, false"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/a5e9f749-4bd5-43b4-b5e5-886f0eccc09f""
                        }
                    ]
                },
                {
                    ""uuid"": ""9208c68e-bee7-4169-9c37-c6278e69c21b"",
                    ""display"": ""Vitals: 97.0, false, 86.0, true, true, 22.0, 100.0, true, 160.0, true, Sitting, 98.6, false"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/9208c68e-bee7-4169-9c37-c6278e69c21b""
                        }
                    ]
                }
            ],
            ""orders"": [],
            ""voided"": false,
            ""visit"": {
                ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
                ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
                    }
                ]
            },
            ""encounterProviders"": [
                {
                    ""uuid"": ""acfe506b-8ef2-4365-8921-f3093ad082e3"",
                    ""display"": ""Super Man: Unknown"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/15da42b1-a535-4330-b228-8a6530c89cb9/encounterprovider/acfe506b-8ef2-4365-8921-f3093ad082e3""
                        }
                    ]
                }
            ],
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/15da42b1-a535-4330-b228-8a6530c89cb9""
                },
                {
                    ""rel"": ""full"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/15da42b1-a535-4330-b228-8a6530c89cb9?v=full""
                }
            ],
            ""resourceVersion"": ""1.9""
        },
        {
            ""uuid"": ""93df5a9e-1507-4022-93a4-ffbdd16d46f3"",
            ""display"": ""REG 05/05/2017"",
            ""encounterDatetime"": ""2017-05-05T15:07:45.000+0800"",
            ""patient"": {
                ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
                ""display"": ""GAN203009 - Test Hypertension"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
                    }
                ]
            },
            ""location"": {
                ""uuid"": ""bb0e512e-d225-11e4-9c67-080027b662ec"",
                ""display"": ""Labour Ward"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/bb0e512e-d225-11e4-9c67-080027b662ec""
                    }
                ]
            },
            ""form"": null,
            ""encounterType"": {
                ""uuid"": ""81888515-3f10-11e4-adec-0800271c1b75"",
                ""display"": ""REG"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81888515-3f10-11e4-adec-0800271c1b75""
                    }
                ]
            },
            ""obs"": [
                {
                    ""uuid"": ""11057e47-f631-47ea-abb1-840404aeb67f"",
                    ""display"": ""BMI Data: false, 21.33"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/11057e47-f631-47ea-abb1-840404aeb67f""
                        }
                    ]
                },
                {
                    ""uuid"": ""a14732d9-36c5-47f3-8272-4dd573faeeba"",
                    ""display"": ""Nutritional Values: "",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/a14732d9-36c5-47f3-8272-4dd573faeeba""
                        }
                    ]
                },
                {
                    ""uuid"": ""ff132a9c-35c6-4102-9778-9139466a593f"",
                    ""display"": ""Fee Information: 10.0"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/ff132a9c-35c6-4102-9778-9139466a593f""
                        }
                    ]
                },
                {
                    ""uuid"": ""af8db28c-a16d-49dc-a4be-cc283e881e4d"",
                    ""display"": ""BMI Status Data: Normal, false"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/af8db28c-a16d-49dc-a4be-cc283e881e4d""
                        }
                    ]
                }
            ],
            ""orders"": [],
            ""voided"": false,
            ""visit"": {
                ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
                ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
                    }
                ]
            },
            ""encounterProviders"": [
                {
                    ""uuid"": ""7d81380b-b443-4288-b5a7-9041387e28b5"",
                    ""display"": ""Super Man: Unknown"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/93df5a9e-1507-4022-93a4-ffbdd16d46f3/encounterprovider/7d81380b-b443-4288-b5a7-9041387e28b5""
                        }
                    ]
                }
            ],
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/93df5a9e-1507-4022-93a4-ffbdd16d46f3""
                },
                {
                    ""rel"": ""full"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/93df5a9e-1507-4022-93a4-ffbdd16d46f3?v=full""
                }
            ],
            ""resourceVersion"": ""1.9""
        }
    ],
    ""attributes"": [
        {
            ""display"": ""Visit Status: OPD"",
            ""uuid"": ""1ccc20ef-7e7d-44c1-a2a1-99a2302a436e"",
            ""attributeType"": {
                ""uuid"": ""ff25b0f3-e276-11e4-900f-080027b662ec"",
                ""display"": ""Visit Status"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visitattributetype/ff25b0f3-e276-11e4-900f-080027b662ec""
                    }
                ]
            },
            ""value"": ""OPD"",
            ""voided"": false,
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215/attribute/1ccc20ef-7e7d-44c1-a2a1-99a2302a436e""
                },
                {
                    ""rel"": ""full"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215/attribute/1ccc20ef-7e7d-44c1-a2a1-99a2302a436e?v=full""
                }
            ],
            ""resourceVersion"": ""1.9""
        }
    ],
    ""voided"": false,
    ""auditInfo"": {
        ""creator"": {
            ""uuid"": ""c1c21e11-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""superman"",
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/c1c21e11-3f10-11e4-adec-0800271c1b75""
                }
            ]
        },
        ""dateCreated"": ""2017-05-05T15:07:26.000+0800"",
        ""changedBy"": {
            ""uuid"": ""A4F30A1B-5EB9-11DF-A648-37A07F9C90FB"",
            ""display"": ""daemon"",
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/A4F30A1B-5EB9-11DF-A648-37A07F9C90FB""
                }
            ]
        },
        ""dateChanged"": ""2017-05-31T23:59:59.000+0800""
    },
    ""links"": [
        {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
        }
    ],
    ""resourceVersion"": ""1.9""
}
]
}
";
        private const string PatientVisitsSampleWithMedication = @"{
  ""results"": [
    {
      ""uuid"": ""f341f996-f714-4763-9d19-6287cb609758"",
      ""display"": ""OPD @ General Ward - 08/17/2020 12:21 PM"",
      ""patient"": {
        ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
        ""display"": ""GAN203009 - Test Hypertension"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
          }
        ]
      },
      ""visitType"": {
        ""uuid"": ""c22a5000-3f10-11e4-adec-0800271c1b75"",
        ""display"": ""OPD"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visittype/c22a5000-3f10-11e4-adec-0800271c1b75""
          }
        ]
      },
      ""indication"": null,
      ""location"": {
        ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
        ""display"": ""General Ward"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
          }
        ]
      },
      ""startDatetime"": ""2020-08-17T12:21:49.000+0800"",
      ""stopDatetime"": null,
      ""encounters"": [
        {
          ""uuid"": ""023a324e-539a-46e9-9d16-c6313393c04c"",
          ""display"": ""Consultation 08/17/2020"",
          ""encounterDatetime"": ""2020-08-17T12:23:42.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
            ""display"": ""General Ward"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81852aee-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""Consultation"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81852aee-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [],
          ""orders"": [
            {
              ""uuid"": ""e25e271d-76c7-4ffe-a205-87f509b06282"",
              ""display"": ""(NEW) Losartan 50mg: null"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/e25e271d-76c7-4ffe-a205-87f509b06282""
                }
              ],
              ""type"": ""drugorder""
            },
            {
              ""uuid"": ""d9470260-09f2-422a-b239-8e9e6bb4b309"",
              ""display"": ""(DISCONTINUE) Losartan 50mg"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/d9470260-09f2-422a-b239-8e9e6bb4b309""
                }
              ],
              ""type"": ""drugorder""
            },
            {
              ""uuid"": ""bf556c5b-92e4-49b6-9014-340f75a7930e"",
              ""display"": ""(NEW) Losartan 50mg: null"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/bf556c5b-92e4-49b6-9014-340f75a7930e""
                }
              ],
              ""type"": ""drugorder""
            }
          ],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""f341f996-f714-4763-9d19-6287cb609758"",
            ""display"": ""OPD @ General Ward - 08/17/2020 12:21 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""ffbe55f5-eb92-4107-bf8d-8734e419026e"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/023a324e-539a-46e9-9d16-c6313393c04c/encounterprovider/ffbe55f5-eb92-4107-bf8d-8734e419026e""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/023a324e-539a-46e9-9d16-c6313393c04c""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/023a324e-539a-46e9-9d16-c6313393c04c?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        },
        {
          ""uuid"": ""efa793f4-1469-4e46-a137-f7d59a73e629"",
          ""display"": ""REG 08/17/2020"",
          ""encounterDatetime"": ""2020-08-17T12:21:54.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
            ""display"": ""General Ward"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81888515-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""REG"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81888515-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [
            {
              ""uuid"": ""c085f35e-cc01-48d8-b00a-dbe64704f606"",
              ""display"": ""Fee Information: 10.0"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/c085f35e-cc01-48d8-b00a-dbe64704f606""
                }
              ]
            }
          ],
          ""orders"": [],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""f341f996-f714-4763-9d19-6287cb609758"",
            ""display"": ""OPD @ General Ward - 08/17/2020 12:21 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""6ef333f3-bb59-489a-b44d-3a6b4b1cfd3c"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/efa793f4-1469-4e46-a137-f7d59a73e629/encounterprovider/6ef333f3-bb59-489a-b44d-3a6b4b1cfd3c""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/efa793f4-1469-4e46-a137-f7d59a73e629""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/efa793f4-1469-4e46-a137-f7d59a73e629?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        }
      ],
      ""attributes"": [
        {
          ""display"": ""Visit Status: OPD"",
          ""uuid"": ""57e83405-8131-4efe-8e99-617ec8b254a0"",
          ""attributeType"": {
            ""uuid"": ""ff25b0f3-e276-11e4-900f-080027b662ec"",
            ""display"": ""Visit Status"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visitattributetype/ff25b0f3-e276-11e4-900f-080027b662ec""
              }
            ]
          },
          ""value"": ""OPD"",
          ""voided"": false,
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758/attribute/57e83405-8131-4efe-8e99-617ec8b254a0""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758/attribute/57e83405-8131-4efe-8e99-617ec8b254a0?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        }
      ],
      ""voided"": false,
      ""auditInfo"": {
        ""creator"": {
          ""uuid"": ""c1c21e11-3f10-11e4-adec-0800271c1b75"",
          ""display"": ""superman"",
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/c1c21e11-3f10-11e4-adec-0800271c1b75""
            }
          ]
        },
        ""dateCreated"": ""2020-08-17T12:21:49.000+0800"",
        ""changedBy"": null,
        ""dateChanged"": null
      },
      ""links"": [
        {
          ""rel"": ""self"",
          ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758""
        }
      ],
      ""resourceVersion"": ""1.9""
    },
    {
      ""uuid"": ""fb788e13-092d-4773-b7db-065fe8c4eb6d"",
      ""display"": ""OPD @ Registration Desk - 06/07/2017 05:12 PM"",
      ""patient"": {
        ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
        ""display"": ""GAN203009 - Test Hypertension"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
          }
        ]
      },
      ""visitType"": {
        ""uuid"": ""c22a5000-3f10-11e4-adec-0800271c1b75"",
        ""display"": ""OPD"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visittype/c22a5000-3f10-11e4-adec-0800271c1b75""
          }
        ]
      },
      ""indication"": null,
      ""location"": {
        ""uuid"": ""c5854fd7-3f12-11e4-adec-0800271c1b75"",
        ""display"": ""Registration Desk"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/c5854fd7-3f12-11e4-adec-0800271c1b75""
          }
        ]
      },
      ""startDatetime"": ""2017-06-07T17:12:12.000+0800"",
      ""stopDatetime"": ""2017-06-07T17:12:12.000+0800"",
      ""encounters"": [],
      ""attributes"": [],
      ""voided"": false,
      ""auditInfo"": {
        ""creator"": {
          ""uuid"": ""c1c21e11-3f10-11e4-adec-0800271c1b75"",
          ""display"": ""superman"",
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/c1c21e11-3f10-11e4-adec-0800271c1b75""
            }
          ]
        },
        ""dateCreated"": ""2017-06-07T17:12:12.000+0800"",
        ""changedBy"": {
          ""uuid"": ""A4F30A1B-5EB9-11DF-A648-37A07F9C90FB"",
          ""display"": ""daemon"",
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/A4F30A1B-5EB9-11DF-A648-37A07F9C90FB""
            }
          ]
        },
        ""dateChanged"": ""2020-08-16T23:59:59.000+0800""
      },
      ""links"": [
        {
          ""rel"": ""self"",
          ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/fb788e13-092d-4773-b7db-065fe8c4eb6d""
        }
      ],
      ""resourceVersion"": ""1.9""
    },
    {
      ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
      ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
      ""patient"": {
        ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
        ""display"": ""GAN203009 - Test Hypertension"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
          }
        ]
      },
      ""visitType"": {
        ""uuid"": ""c22a5000-3f10-11e4-adec-0800271c1b75"",
        ""display"": ""OPD"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visittype/c22a5000-3f10-11e4-adec-0800271c1b75""
          }
        ]
      },
      ""indication"": null,
      ""location"": {
        ""uuid"": ""c1e42932-3f10-11e4-adec-0800271c1b75"",
        ""display"": ""Ganiyari"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/c1e42932-3f10-11e4-adec-0800271c1b75""
          }
        ]
      },
      ""startDatetime"": ""2017-05-05T15:07:26.000+0800"",
      ""stopDatetime"": ""2017-05-18T14:42:47.000+0800"",
      ""encounters"": [
        {
          ""uuid"": ""ac369187-d04a-4aa2-bd8e-7c2e95e80c84"",
          ""display"": ""Consultation 05/18/2017"",
          ""encounterDatetime"": ""2017-05-18T14:42:47.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
            ""display"": ""General Ward"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81852aee-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""Consultation"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81852aee-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [
            {
              ""uuid"": ""51b2349f-316d-4366-9163-f00f6699ad15"",
              ""display"": ""Vitals: true, 90.0, Sitting, 130.0, false"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/51b2349f-316d-4366-9163-f00f6699ad15""
                }
              ]
            }
          ],
          ""orders"": [],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
            ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""70e0e7f9-2a75-4be9-bbf8-1037ff6a4832"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/ac369187-d04a-4aa2-bd8e-7c2e95e80c84/encounterprovider/70e0e7f9-2a75-4be9-bbf8-1037ff6a4832""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/ac369187-d04a-4aa2-bd8e-7c2e95e80c84""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/ac369187-d04a-4aa2-bd8e-7c2e95e80c84?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        },
        {
          ""uuid"": ""a8dd660f-e582-4a7a-b167-6a934697bac3"",
          ""display"": ""LAB_RESULT 05/05/2017"",
          ""encounterDatetime"": ""2017-05-05T16:36:36.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""c58e12ed-3f12-11e4-adec-0800271c1b75"",
            ""display"": ""OPD-1"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/c58e12ed-3f12-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""82024e00-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""LAB_RESULT"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/82024e00-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [
            {
              ""uuid"": ""ce574095-df86-485b-8e33-e2e69872c453"",
              ""display"": ""LDL: "",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/ce574095-df86-485b-8e33-e2e69872c453""
                }
              ]
            },
            {
              ""uuid"": ""ac481b5f-966f-47cd-b084-67d967b3c35e"",
              ""display"": ""Kidney Function: "",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/ac481b5f-966f-47cd-b084-67d967b3c35e""
                }
              ]
            }
          ],
          ""orders"": [],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
            ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""d6508bc8-a808-48bf-8777-3fb9d42055df"",
              ""display"": ""labsystem system: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/a8dd660f-e582-4a7a-b167-6a934697bac3/encounterprovider/d6508bc8-a808-48bf-8777-3fb9d42055df""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/a8dd660f-e582-4a7a-b167-6a934697bac3""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/a8dd660f-e582-4a7a-b167-6a934697bac3?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        },
        {
          ""uuid"": ""15da42b1-a535-4330-b228-8a6530c89cb9"",
          ""display"": ""Consultation 05/05/2017"",
          ""encounterDatetime"": ""2017-05-05T16:36:36.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""c58e12ed-3f12-11e4-adec-0800271c1b75"",
            ""display"": ""OPD-1"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/c58e12ed-3f12-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81852aee-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""Consultation"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81852aee-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [
            {
              ""uuid"": ""3be53600-7934-41ba-a043-daa04021fc61"",
              ""display"": ""History and Examination: false, blood spot in the eye, 4320.0"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/3be53600-7934-41ba-a043-daa04021fc61""
                }
              ]
            },
            {
              ""uuid"": ""da7e38f7-57a2-46b4-a604-7c7dfe1c5519"",
              ""display"": ""Hypertension, Intake: false, false, false, No Previous Diagnosis, 2017-05-05"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/da7e38f7-57a2-46b4-a604-7c7dfe1c5519""
                }
              ]
            },
            {
              ""uuid"": ""a5e9f749-4bd5-43b4-b5e5-886f0eccc09f"",
              ""display"": ""Visit Diagnoses: Primary, Confirmed, Hypertension, unspec., a5e9f749-4bd5-43b4-b5e5-886f0eccc09f, false"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/a5e9f749-4bd5-43b4-b5e5-886f0eccc09f""
                }
              ]
            },
            {
              ""uuid"": ""9208c68e-bee7-4169-9c37-c6278e69c21b"",
              ""display"": ""Vitals: 97.0, false, 86.0, true, true, 22.0, 100.0, true, 160.0, true, Sitting, 98.6, false"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/9208c68e-bee7-4169-9c37-c6278e69c21b""
                }
              ]
            }
          ],
          ""orders"": [
            {
              ""uuid"": ""3d340d6d-3a3f-4e38-81dd-e734e84f3216"",
              ""display"": ""Kidney Function"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/3d340d6d-3a3f-4e38-81dd-e734e84f3216""
                }
              ],
              ""type"": ""order""
            },
            {
              ""uuid"": ""f3420428-93c9-4ab9-99ad-504643d9ff1b"",
              ""display"": ""(NEW) Losartan 50mg: null"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/f3420428-93c9-4ab9-99ad-504643d9ff1b""
                }
              ],
              ""type"": ""drugorder""
            },
            {
              ""uuid"": ""40585130-0976-4b94-876e-67a9e3aaaf07"",
              ""display"": ""LDL"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/40585130-0976-4b94-876e-67a9e3aaaf07""
                }
              ],
              ""type"": ""order""
            }
          ],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
            ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""acfe506b-8ef2-4365-8921-f3093ad082e3"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/15da42b1-a535-4330-b228-8a6530c89cb9/encounterprovider/acfe506b-8ef2-4365-8921-f3093ad082e3""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/15da42b1-a535-4330-b228-8a6530c89cb9""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/15da42b1-a535-4330-b228-8a6530c89cb9?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        },
        {
          ""uuid"": ""93df5a9e-1507-4022-93a4-ffbdd16d46f3"",
          ""display"": ""REG 05/05/2017"",
          ""encounterDatetime"": ""2017-05-05T15:07:45.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""bb0e512e-d225-11e4-9c67-080027b662ec"",
            ""display"": ""Labour Ward"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/bb0e512e-d225-11e4-9c67-080027b662ec""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81888515-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""REG"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81888515-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [
            {
              ""uuid"": ""11057e47-f631-47ea-abb1-840404aeb67f"",
              ""display"": ""BMI Data: false, 21.33"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/11057e47-f631-47ea-abb1-840404aeb67f""
                }
              ]
            },
            {
              ""uuid"": ""a14732d9-36c5-47f3-8272-4dd573faeeba"",
              ""display"": ""Nutritional Values: "",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/a14732d9-36c5-47f3-8272-4dd573faeeba""
                }
              ]
            },
            {
              ""uuid"": ""ff132a9c-35c6-4102-9778-9139466a593f"",
              ""display"": ""Fee Information: 10.0"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/ff132a9c-35c6-4102-9778-9139466a593f""
                }
              ]
            },
            {
              ""uuid"": ""af8db28c-a16d-49dc-a4be-cc283e881e4d"",
              ""display"": ""BMI Status Data: Normal, false"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/af8db28c-a16d-49dc-a4be-cc283e881e4d""
                }
              ]
            }
          ],
          ""orders"": [],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
            ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""7d81380b-b443-4288-b5a7-9041387e28b5"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/93df5a9e-1507-4022-93a4-ffbdd16d46f3/encounterprovider/7d81380b-b443-4288-b5a7-9041387e28b5""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/93df5a9e-1507-4022-93a4-ffbdd16d46f3""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/93df5a9e-1507-4022-93a4-ffbdd16d46f3?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        }
      ],
      ""attributes"": [
        {
          ""display"": ""Visit Status: OPD"",
          ""uuid"": ""1ccc20ef-7e7d-44c1-a2a1-99a2302a436e"",
          ""attributeType"": {
            ""uuid"": ""ff25b0f3-e276-11e4-900f-080027b662ec"",
            ""display"": ""Visit Status"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visitattributetype/ff25b0f3-e276-11e4-900f-080027b662ec""
              }
            ]
          },
          ""value"": ""OPD"",
          ""voided"": false,
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215/attribute/1ccc20ef-7e7d-44c1-a2a1-99a2302a436e""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215/attribute/1ccc20ef-7e7d-44c1-a2a1-99a2302a436e?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        }
      ],
      ""voided"": false,
      ""auditInfo"": {
        ""creator"": {
          ""uuid"": ""c1c21e11-3f10-11e4-adec-0800271c1b75"",
          ""display"": ""superman"",
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/c1c21e11-3f10-11e4-adec-0800271c1b75""
            }
          ]
        },
        ""dateCreated"": ""2017-05-05T15:07:26.000+0800"",
        ""changedBy"": {
          ""uuid"": ""A4F30A1B-5EB9-11DF-A648-37A07F9C90FB"",
          ""display"": ""daemon"",
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/A4F30A1B-5EB9-11DF-A648-37A07F9C90FB""
            }
          ]
        },
        ""dateChanged"": ""2017-05-31T23:59:59.000+0800""
      },
      ""links"": [
        {
          ""rel"": ""self"",
          ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
        }
      ],
      ""resourceVersion"": ""1.9""
    }
  ]
}";
        private const string sampleDataWithoutDiagnosis = @"{
  ""results"": [
    {
      ""uuid"": ""f341f996-f714-4763-9d19-6287cb609758"",
      ""display"": ""OPD @ General Ward - 08/17/2020 12:21 PM"",
      ""patient"": {
        ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
        ""display"": ""GAN203009 - Test Hypertension"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
          }
        ]
      },
      ""visitType"": {
        ""uuid"": ""c22a5000-3f10-11e4-adec-0800271c1b75"",
        ""display"": ""OPD"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visittype/c22a5000-3f10-11e4-adec-0800271c1b75""
          }
        ]
      },
      ""indication"": null,
      ""location"": {
        ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
        ""display"": ""General Ward"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
          }
        ]
      },
      ""startDatetime"": ""2020-08-17T12:21:49.000+0800"",
      ""stopDatetime"": null,
      ""encounters"": [
        {
          ""uuid"": ""023a324e-539a-46e9-9d16-c6313393c04c"",
          ""display"": ""Consultation 08/17/2020"",
          ""encounterDatetime"": ""2020-08-17T12:23:42.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
            ""display"": ""General Ward"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81852aee-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""Consultation"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81852aee-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [],
          ""orders"": [
            {
              ""uuid"": ""e25e271d-76c7-4ffe-a205-87f509b06282"",
              ""display"": ""(NEW) Losartan 50mg: null"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/e25e271d-76c7-4ffe-a205-87f509b06282""
                }
              ],
              ""type"": ""drugorder""
            },
            {
              ""uuid"": ""d9470260-09f2-422a-b239-8e9e6bb4b309"",
              ""display"": ""(DISCONTINUE) Losartan 50mg"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/d9470260-09f2-422a-b239-8e9e6bb4b309""
                }
              ],
              ""type"": ""drugorder""
            },
            {
              ""uuid"": ""bf556c5b-92e4-49b6-9014-340f75a7930e"",
              ""display"": ""(NEW) Losartan 50mg: null"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/bf556c5b-92e4-49b6-9014-340f75a7930e""
                }
              ],
              ""type"": ""drugorder""
            }
          ],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""f341f996-f714-4763-9d19-6287cb609758"",
            ""display"": ""OPD @ General Ward - 08/17/2020 12:21 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""ffbe55f5-eb92-4107-bf8d-8734e419026e"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/023a324e-539a-46e9-9d16-c6313393c04c/encounterprovider/ffbe55f5-eb92-4107-bf8d-8734e419026e""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/023a324e-539a-46e9-9d16-c6313393c04c""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/023a324e-539a-46e9-9d16-c6313393c04c?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        },
        {
          ""uuid"": ""efa793f4-1469-4e46-a137-f7d59a73e629"",
          ""display"": ""REG 08/17/2020"",
          ""encounterDatetime"": ""2020-08-17T12:21:54.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
            ""display"": ""General Ward"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81888515-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""REG"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81888515-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [
            {
              ""uuid"": ""c085f35e-cc01-48d8-b00a-dbe64704f606"",
              ""display"": ""Fee Information: 10.0"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/c085f35e-cc01-48d8-b00a-dbe64704f606""
                }
              ]
            }
          ],
          ""orders"": [],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""f341f996-f714-4763-9d19-6287cb609758"",
            ""display"": ""OPD @ General Ward - 08/17/2020 12:21 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""6ef333f3-bb59-489a-b44d-3a6b4b1cfd3c"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/efa793f4-1469-4e46-a137-f7d59a73e629/encounterprovider/6ef333f3-bb59-489a-b44d-3a6b4b1cfd3c""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/efa793f4-1469-4e46-a137-f7d59a73e629""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/efa793f4-1469-4e46-a137-f7d59a73e629?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        }
      ],
      ""attributes"": [
        {
          ""display"": ""Visit Status: OPD"",
          ""uuid"": ""57e83405-8131-4efe-8e99-617ec8b254a0"",
          ""attributeType"": {
            ""uuid"": ""ff25b0f3-e276-11e4-900f-080027b662ec"",
            ""display"": ""Visit Status"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visitattributetype/ff25b0f3-e276-11e4-900f-080027b662ec""
              }
            ]
          },
          ""value"": ""OPD"",
          ""voided"": false,
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758/attribute/57e83405-8131-4efe-8e99-617ec8b254a0""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758/attribute/57e83405-8131-4efe-8e99-617ec8b254a0?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        }
      ],
      ""voided"": false,
      ""auditInfo"": {
        ""creator"": {
          ""uuid"": ""c1c21e11-3f10-11e4-adec-0800271c1b75"",
          ""display"": ""superman"",
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/c1c21e11-3f10-11e4-adec-0800271c1b75""
            }
          ]
        },
        ""dateCreated"": ""2020-08-17T12:21:49.000+0800"",
        ""changedBy"": null,
        ""dateChanged"": null
      },
      ""links"": [
        {
          ""rel"": ""self"",
          ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758""
        }
      ],
      ""resourceVersion"": ""1.9""
    }
  ]
}
";
        private const string PatientVisitsSampleWithDiagnosis = @"{
  ""results"": [
    {
      ""uuid"": ""f341f996-f714-4763-9d19-6287cb609758"",
      ""display"": ""OPD @ General Ward - 08/17/2020 12:21 PM"",
      ""patient"": {
        ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
        ""display"": ""GAN203009 - Test Hypertension"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
          }
        ]
      },
      ""visitType"": {
        ""uuid"": ""c22a5000-3f10-11e4-adec-0800271c1b75"",
        ""display"": ""OPD"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visittype/c22a5000-3f10-11e4-adec-0800271c1b75""
          }
        ]
      },
      ""indication"": null,
      ""location"": {
        ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
        ""display"": ""General Ward"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
          }
        ]
      },
      ""startDatetime"": ""2020-08-17T12:21:49.000+0800"",
      ""stopDatetime"": null,
      ""encounters"": [
        {
          ""uuid"": ""023a324e-539a-46e9-9d16-c6313393c04c"",
          ""display"": ""Consultation 08/17/2020"",
          ""encounterDatetime"": ""2020-08-17T12:23:42.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
            ""display"": ""General Ward"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81852aee-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""Consultation"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81852aee-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [],
          ""orders"": [
            {
              ""uuid"": ""e25e271d-76c7-4ffe-a205-87f509b06282"",
              ""display"": ""(NEW) Losartan 50mg: null"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/e25e271d-76c7-4ffe-a205-87f509b06282""
                }
              ],
              ""type"": ""drugorder""
            },
            {
              ""uuid"": ""d9470260-09f2-422a-b239-8e9e6bb4b309"",
              ""display"": ""(DISCONTINUE) Losartan 50mg"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/d9470260-09f2-422a-b239-8e9e6bb4b309""
                }
              ],
              ""type"": ""drugorder""
            },
            {
              ""uuid"": ""bf556c5b-92e4-49b6-9014-340f75a7930e"",
              ""display"": ""(NEW) Losartan 50mg: null"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/bf556c5b-92e4-49b6-9014-340f75a7930e""
                }
              ],
              ""type"": ""drugorder""
            }
          ],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""f341f996-f714-4763-9d19-6287cb609758"",
            ""display"": ""OPD @ General Ward - 08/17/2020 12:21 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""ffbe55f5-eb92-4107-bf8d-8734e419026e"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/023a324e-539a-46e9-9d16-c6313393c04c/encounterprovider/ffbe55f5-eb92-4107-bf8d-8734e419026e""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/023a324e-539a-46e9-9d16-c6313393c04c""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/023a324e-539a-46e9-9d16-c6313393c04c?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        },
        {
          ""uuid"": ""efa793f4-1469-4e46-a137-f7d59a73e629"",
          ""display"": ""REG 08/17/2020"",
          ""encounterDatetime"": ""2020-08-17T12:21:54.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
            ""display"": ""General Ward"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81888515-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""REG"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81888515-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [
            {
              ""uuid"": ""c085f35e-cc01-48d8-b00a-dbe64704f606"",
              ""display"": ""Fee Information: 10.0"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/c085f35e-cc01-48d8-b00a-dbe64704f606""
                }
              ]
            }
          ],
          ""orders"": [],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""f341f996-f714-4763-9d19-6287cb609758"",
            ""display"": ""OPD @ General Ward - 08/17/2020 12:21 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""6ef333f3-bb59-489a-b44d-3a6b4b1cfd3c"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/efa793f4-1469-4e46-a137-f7d59a73e629/encounterprovider/6ef333f3-bb59-489a-b44d-3a6b4b1cfd3c""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/efa793f4-1469-4e46-a137-f7d59a73e629""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/efa793f4-1469-4e46-a137-f7d59a73e629?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        }
      ],
      ""attributes"": [
        {
          ""display"": ""Visit Status: OPD"",
          ""uuid"": ""57e83405-8131-4efe-8e99-617ec8b254a0"",
          ""attributeType"": {
            ""uuid"": ""ff25b0f3-e276-11e4-900f-080027b662ec"",
            ""display"": ""Visit Status"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visitattributetype/ff25b0f3-e276-11e4-900f-080027b662ec""
              }
            ]
          },
          ""value"": ""OPD"",
          ""voided"": false,
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758/attribute/57e83405-8131-4efe-8e99-617ec8b254a0""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758/attribute/57e83405-8131-4efe-8e99-617ec8b254a0?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        }
      ],
      ""voided"": false,
      ""auditInfo"": {
        ""creator"": {
          ""uuid"": ""c1c21e11-3f10-11e4-adec-0800271c1b75"",
          ""display"": ""superman"",
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/c1c21e11-3f10-11e4-adec-0800271c1b75""
            }
          ]
        },
        ""dateCreated"": ""2020-08-17T12:21:49.000+0800"",
        ""changedBy"": null,
        ""dateChanged"": null
      },
      ""links"": [
        {
          ""rel"": ""self"",
          ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/f341f996-f714-4763-9d19-6287cb609758""
        }
      ],
      ""resourceVersion"": ""1.9""
    },
    {
      ""uuid"": ""fb788e13-092d-4773-b7db-065fe8c4eb6d"",
      ""display"": ""OPD @ Registration Desk - 06/07/2017 05:12 PM"",
      ""patient"": {
        ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
        ""display"": ""GAN203009 - Test Hypertension"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
          }
        ]
      },
      ""visitType"": {
        ""uuid"": ""c22a5000-3f10-11e4-adec-0800271c1b75"",
        ""display"": ""OPD"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visittype/c22a5000-3f10-11e4-adec-0800271c1b75""
          }
        ]
      },
      ""indication"": null,
      ""location"": {
        ""uuid"": ""c5854fd7-3f12-11e4-adec-0800271c1b75"",
        ""display"": ""Registration Desk"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/c5854fd7-3f12-11e4-adec-0800271c1b75""
          }
        ]
      },
      ""startDatetime"": ""2017-06-07T17:12:12.000+0800"",
      ""stopDatetime"": ""2017-06-07T17:12:12.000+0800"",
      ""encounters"": [],
      ""attributes"": [],
      ""voided"": false,
      ""auditInfo"": {
        ""creator"": {
          ""uuid"": ""c1c21e11-3f10-11e4-adec-0800271c1b75"",
          ""display"": ""superman"",
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/c1c21e11-3f10-11e4-adec-0800271c1b75""
            }
          ]
        },
        ""dateCreated"": ""2017-06-07T17:12:12.000+0800"",
        ""changedBy"": {
          ""uuid"": ""A4F30A1B-5EB9-11DF-A648-37A07F9C90FB"",
          ""display"": ""daemon"",
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/A4F30A1B-5EB9-11DF-A648-37A07F9C90FB""
            }
          ]
        },
        ""dateChanged"": ""2020-08-16T23:59:59.000+0800""
      },
      ""links"": [
        {
          ""rel"": ""self"",
          ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/fb788e13-092d-4773-b7db-065fe8c4eb6d""
        }
      ],
      ""resourceVersion"": ""1.9""
    },
    {
      ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
      ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
      ""patient"": {
        ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
        ""display"": ""GAN203009 - Test Hypertension"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
          }
        ]
      },
      ""visitType"": {
        ""uuid"": ""c22a5000-3f10-11e4-adec-0800271c1b75"",
        ""display"": ""OPD"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visittype/c22a5000-3f10-11e4-adec-0800271c1b75""
          }
        ]
      },
      ""indication"": null,
      ""location"": {
        ""uuid"": ""c1e42932-3f10-11e4-adec-0800271c1b75"",
        ""display"": ""Ganiyari"",
        ""links"": [
          {
            ""rel"": ""self"",
            ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/c1e42932-3f10-11e4-adec-0800271c1b75""
          }
        ]
      },
      ""startDatetime"": ""2017-05-05T15:07:26.000+0800"",
      ""stopDatetime"": ""2017-05-18T14:42:47.000+0800"",
      ""encounters"": [
        {
          ""uuid"": ""ac369187-d04a-4aa2-bd8e-7c2e95e80c84"",
          ""display"": ""Consultation 05/18/2017"",
          ""encounterDatetime"": ""2017-05-18T14:42:47.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""baf7bd38-d225-11e4-9c67-080027b662ec"",
            ""display"": ""General Ward"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/baf7bd38-d225-11e4-9c67-080027b662ec""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81852aee-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""Consultation"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81852aee-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [
            {
              ""uuid"": ""51b2349f-316d-4366-9163-f00f6699ad15"",
              ""display"": ""Vitals: true, 90.0, Sitting, 130.0, false"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/51b2349f-316d-4366-9163-f00f6699ad15""
                }
              ]
            }
          ],
          ""orders"": [],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
            ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""70e0e7f9-2a75-4be9-bbf8-1037ff6a4832"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/ac369187-d04a-4aa2-bd8e-7c2e95e80c84/encounterprovider/70e0e7f9-2a75-4be9-bbf8-1037ff6a4832""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/ac369187-d04a-4aa2-bd8e-7c2e95e80c84""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/ac369187-d04a-4aa2-bd8e-7c2e95e80c84?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        },
        {
          ""uuid"": ""a8dd660f-e582-4a7a-b167-6a934697bac3"",
          ""display"": ""LAB_RESULT 05/05/2017"",
          ""encounterDatetime"": ""2017-05-05T16:36:36.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""c58e12ed-3f12-11e4-adec-0800271c1b75"",
            ""display"": ""OPD-1"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/c58e12ed-3f12-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""82024e00-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""LAB_RESULT"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/82024e00-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [
            {
              ""uuid"": ""ce574095-df86-485b-8e33-e2e69872c453"",
              ""display"": ""LDL: "",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/ce574095-df86-485b-8e33-e2e69872c453""
                }
              ]
            },
            {
              ""uuid"": ""ac481b5f-966f-47cd-b084-67d967b3c35e"",
              ""display"": ""Kidney Function: "",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/ac481b5f-966f-47cd-b084-67d967b3c35e""
                }
              ]
            }
          ],
          ""orders"": [],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
            ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""d6508bc8-a808-48bf-8777-3fb9d42055df"",
              ""display"": ""labsystem system: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/a8dd660f-e582-4a7a-b167-6a934697bac3/encounterprovider/d6508bc8-a808-48bf-8777-3fb9d42055df""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/a8dd660f-e582-4a7a-b167-6a934697bac3""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/a8dd660f-e582-4a7a-b167-6a934697bac3?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        },
        {
          ""uuid"": ""15da42b1-a535-4330-b228-8a6530c89cb9"",
          ""display"": ""Consultation 05/05/2017"",
          ""encounterDatetime"": ""2017-05-05T16:36:36.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""c58e12ed-3f12-11e4-adec-0800271c1b75"",
            ""display"": ""OPD-1"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/c58e12ed-3f12-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81852aee-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""Consultation"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81852aee-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [
            {
              ""uuid"": ""3be53600-7934-41ba-a043-daa04021fc61"",
              ""display"": ""History and Examination: false, blood spot in the eye, 4320.0"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/3be53600-7934-41ba-a043-daa04021fc61""
                }
              ]
            },
            {
              ""uuid"": ""da7e38f7-57a2-46b4-a604-7c7dfe1c5519"",
              ""display"": ""Hypertension, Intake: false, false, false, No Previous Diagnosis, 2017-05-05"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/da7e38f7-57a2-46b4-a604-7c7dfe1c5519""
                }
              ]
            },
            {
              ""uuid"": ""a5e9f749-4bd5-43b4-b5e5-886f0eccc09f"",
              ""display"": ""Visit Diagnoses: Primary, Confirmed, Hypertension, unspec., a5e9f749-4bd5-43b4-b5e5-886f0eccc09f, false"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/a5e9f749-4bd5-43b4-b5e5-886f0eccc09f""
                }
              ]
            },
            {
              ""uuid"": ""9208c68e-bee7-4169-9c37-c6278e69c21b"",
              ""display"": ""Vitals: 97.0, false, 86.0, true, true, 22.0, 100.0, true, 160.0, true, Sitting, 98.6, false"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/9208c68e-bee7-4169-9c37-c6278e69c21b""
                }
              ]
            }
          ],
          ""orders"": [
            {
              ""uuid"": ""3d340d6d-3a3f-4e38-81dd-e734e84f3216"",
              ""display"": ""Kidney Function"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/3d340d6d-3a3f-4e38-81dd-e734e84f3216""
                }
              ],
              ""type"": ""order""
            },
            {
              ""uuid"": ""f3420428-93c9-4ab9-99ad-504643d9ff1b"",
              ""display"": ""(NEW) Losartan 50mg: null"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/f3420428-93c9-4ab9-99ad-504643d9ff1b""
                }
              ],
              ""type"": ""drugorder""
            },
            {
              ""uuid"": ""40585130-0976-4b94-876e-67a9e3aaaf07"",
              ""display"": ""LDL"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/order/40585130-0976-4b94-876e-67a9e3aaaf07""
                }
              ],
              ""type"": ""order""
            }
          ],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
            ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""acfe506b-8ef2-4365-8921-f3093ad082e3"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/15da42b1-a535-4330-b228-8a6530c89cb9/encounterprovider/acfe506b-8ef2-4365-8921-f3093ad082e3""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/15da42b1-a535-4330-b228-8a6530c89cb9""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/15da42b1-a535-4330-b228-8a6530c89cb9?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        },
        {
          ""uuid"": ""93df5a9e-1507-4022-93a4-ffbdd16d46f3"",
          ""display"": ""REG 05/05/2017"",
          ""encounterDatetime"": ""2017-05-05T15:07:45.000+0800"",
          ""patient"": {
            ""uuid"": ""3ae1ee52-e9b2-4934-876d-30711c0e3e2f"",
            ""display"": ""GAN203009 - Test Hypertension"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/patient/3ae1ee52-e9b2-4934-876d-30711c0e3e2f""
              }
            ]
          },
          ""location"": {
            ""uuid"": ""bb0e512e-d225-11e4-9c67-080027b662ec"",
            ""display"": ""Labour Ward"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/location/bb0e512e-d225-11e4-9c67-080027b662ec""
              }
            ]
          },
          ""form"": null,
          ""encounterType"": {
            ""uuid"": ""81888515-3f10-11e4-adec-0800271c1b75"",
            ""display"": ""REG"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encountertype/81888515-3f10-11e4-adec-0800271c1b75""
              }
            ]
          },
          ""obs"": [
            {
              ""uuid"": ""11057e47-f631-47ea-abb1-840404aeb67f"",
              ""display"": ""BMI Data: false, 21.33"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/11057e47-f631-47ea-abb1-840404aeb67f""
                }
              ]
            },
            {
              ""uuid"": ""a14732d9-36c5-47f3-8272-4dd573faeeba"",
              ""display"": ""Nutritional Values: "",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/a14732d9-36c5-47f3-8272-4dd573faeeba""
                }
              ]
            },
            {
              ""uuid"": ""ff132a9c-35c6-4102-9778-9139466a593f"",
              ""display"": ""Fee Information: 10.0"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/ff132a9c-35c6-4102-9778-9139466a593f""
                }
              ]
            },
            {
              ""uuid"": ""af8db28c-a16d-49dc-a4be-cc283e881e4d"",
              ""display"": ""BMI Status Data: Normal, false"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/obs/af8db28c-a16d-49dc-a4be-cc283e881e4d""
                }
              ]
            }
          ],
          ""orders"": [],
          ""voided"": false,
          ""visit"": {
            ""uuid"": ""76642da9-5a2b-455e-ac30-312de339e215"",
            ""display"": ""OPD @ Ganiyari - 05/05/2017 03:07 PM"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
              }
            ]
          },
          ""encounterProviders"": [
            {
              ""uuid"": ""7d81380b-b443-4288-b5a7-9041387e28b5"",
              ""display"": ""Super Man: Unknown"",
              ""links"": [
                {
                  ""rel"": ""self"",
                  ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/93df5a9e-1507-4022-93a4-ffbdd16d46f3/encounterprovider/7d81380b-b443-4288-b5a7-9041387e28b5""
                }
              ]
            }
          ],
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/93df5a9e-1507-4022-93a4-ffbdd16d46f3""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/encounter/93df5a9e-1507-4022-93a4-ffbdd16d46f3?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        }
      ],
      ""attributes"": [
        {
          ""display"": ""Visit Status: OPD"",
          ""uuid"": ""1ccc20ef-7e7d-44c1-a2a1-99a2302a436e"",
          ""attributeType"": {
            ""uuid"": ""ff25b0f3-e276-11e4-900f-080027b662ec"",
            ""display"": ""Visit Status"",
            ""links"": [
              {
                ""rel"": ""self"",
                ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visitattributetype/ff25b0f3-e276-11e4-900f-080027b662ec""
              }
            ]
          },
          ""value"": ""OPD"",
          ""voided"": false,
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215/attribute/1ccc20ef-7e7d-44c1-a2a1-99a2302a436e""
            },
            {
              ""rel"": ""full"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215/attribute/1ccc20ef-7e7d-44c1-a2a1-99a2302a436e?v=full""
            }
          ],
          ""resourceVersion"": ""1.9""
        }
      ],
      ""voided"": false,
      ""auditInfo"": {
        ""creator"": {
          ""uuid"": ""c1c21e11-3f10-11e4-adec-0800271c1b75"",
          ""display"": ""superman"",
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/c1c21e11-3f10-11e4-adec-0800271c1b75""
            }
          ]
        },
        ""dateCreated"": ""2017-05-05T15:07:26.000+0800"",
        ""changedBy"": {
          ""uuid"": ""A4F30A1B-5EB9-11DF-A648-37A07F9C90FB"",
          ""display"": ""daemon"",
          ""links"": [
            {
              ""rel"": ""self"",
              ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/user/A4F30A1B-5EB9-11DF-A648-37A07F9C90FB""
            }
          ]
        },
        ""dateChanged"": ""2017-05-31T23:59:59.000+0800""
      },
      ""links"": [
        {
          ""rel"": ""self"",
          ""uri"": ""http://localhost:8050/openmrs/ws/rest/v1/visit/76642da9-5a2b-455e-ac30-312de339e215""
        }
      ],
      ""resourceVersion"": ""1.9""
    }
  ]
}";

        private const string PatientVisitsSampleWithoutVisitType = @"{
  ""results"": [
      {
          ""uuid"": ""823618e9-f403-4b41-8b83-7caba33403e7"",
          ""display"": ""OPD @ Odisha - 08/04/2020 06:58 AM"",
          ""patient"": {
              ""uuid"": ""9da1a756-aa62-46fd-b56d-e35a6d2f2b30"",
              ""display"": ""OD100013 - Patient test one"",
              ""links"": [
                  {
                      ""rel"": ""self"",
                      ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/patient/9da1a756-aa62-46fd-b56d-e35a6d2f2b30""
                  }
              ]
          },
          ""visitType"": {},
          ""indication"": null,
          ""location"": {
              ""uuid"": ""8d6c993e-c2cc-11de-8d13-0010c6dffd0f"",
              ""display"": ""Odisha"",
              ""links"": [
                  {
                      ""rel"": ""self"",
                      ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/location/8d6c993e-c2cc-11de-8d13-0010c6dffd0f""
                  }
              ]
          },
          ""startDatetime"": ""2020-08-04T06:58:45.000+0530"",
          ""stopDatetime"": ""2020-08-05T11:30:08.000+0530"",
          ""resourceVersion"": ""1.9""
      }
  ]
}";
        private const string PatientVisitsSampleWithoutObs = @"{
    ""results"": [
        {
            ""uuid"": ""ff633a82-43ac-45b0-94d8-3a06d379c0ab"",
            ""display"": ""OPD @ Odisha - 08/11/2020 11:25 AM"",
            ""patient"": {
                ""uuid"": ""9da1a756-aa62-46fd-b56d-e35a6d2f2b30"",
                ""display"": ""OD100013 - Patient test one"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/patient/9da1a756-aa62-46fd-b56d-e35a6d2f2b30""
                    }
                ]
            },
            ""visitType"": {
                ""uuid"": ""96c49059-9af1-4f63-a8be-7554984fda02"",
                ""display"": ""OPD"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visittype/96c49059-9af1-4f63-a8be-7554984fda02""
                    }
                ]
            },
            ""indication"": null,
            ""location"": {
                ""uuid"": ""8d6c993e-c2cc-11de-8d13-0010c6dffd0f"",
                ""display"": ""Odisha"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/location/8d6c993e-c2cc-11de-8d13-0010c6dffd0f""
                    }
                ]
            },
            ""startDatetime"": ""2020-08-11T11:25:16.000+0530"",
            ""stopDatetime"": ""2020-08-11T11:26:40.000+0530"",
            ""encounters"": [
                {
                    ""uuid"": ""e3b9cdcf-9f42-489f-b465-eb382923cd22"",
                    ""display"": ""Consultation 08/11/2020"",
                    ""encounterDatetime"": ""2020-08-11T11:26:40.000+0530"",
                    ""patient"": {
                        ""uuid"": ""9da1a756-aa62-46fd-b56d-e35a6d2f2b30"",
                        ""display"": ""OD100013 - Patient test one"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/patient/9da1a756-aa62-46fd-b56d-e35a6d2f2b30""
                            }
                        ]
                    },
                    ""location"": {
                        ""uuid"": ""6b8e123c-8522-4f71-8f80-0a893439bbce"",
                        ""display"": ""OPD"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/location/6b8e123c-8522-4f71-8f80-0a893439bbce""
                            }
                        ]
                    },
                    ""form"": null,
                    ""encounterType"": {
                        ""uuid"": ""7e10b3cb-e42f-11e5-8c3e-08002715d519"",
                        ""display"": ""Consultation"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encountertype/7e10b3cb-e42f-11e5-8c3e-08002715d519""
                            }
                        ]
                    },
                    ""obs"": [],
                    ""orders"": [],
                    ""voided"": false,
                    ""visit"": {
                        ""uuid"": ""ff633a82-43ac-45b0-94d8-3a06d379c0ab"",
                        ""display"": ""OPD @ Odisha - 08/11/2020 11:25 AM"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/ff633a82-43ac-45b0-94d8-3a06d379c0ab""
                            }
                        ]
                    },
                    ""encounterProviders"": [
                        {
                            ""uuid"": ""3bf6e3d0-31ab-41ee-aa8c-118c99f48d5f"",
                            ""display"": ""super man: Unknown"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encounter/e3b9cdcf-9f42-489f-b465-eb382923cd22/encounterprovider/3bf6e3d0-31ab-41ee-aa8c-118c99f48d5f""
                                }
                            ]
                        }
                    ],
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encounter/e3b9cdcf-9f42-489f-b465-eb382923cd22""
                        },
                        {
                            ""rel"": ""full"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encounter/e3b9cdcf-9f42-489f-b465-eb382923cd22?v=full""
                        }
                    ],
                    ""resourceVersion"": ""1.9""
                }
            ],
            ""attributes"": [
                {
                    ""display"": ""Visit Status: OPD"",
                    ""uuid"": ""b5f18330-a072-4894-bae0-4407e2c4548d"",
                    ""attributeType"": {
                        ""uuid"": ""7f7bd8d8-e42f-11e5-8c3e-08002715d519"",
                        ""display"": ""Visit Status"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visitattributetype/7f7bd8d8-e42f-11e5-8c3e-08002715d519""
                            }
                        ]
                    },
                    ""value"": ""OPD"",
                    ""voided"": false,
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/ff633a82-43ac-45b0-94d8-3a06d379c0ab/attribute/b5f18330-a072-4894-bae0-4407e2c4548d""
                        },
                        {
                            ""rel"": ""full"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/ff633a82-43ac-45b0-94d8-3a06d379c0ab/attribute/b5f18330-a072-4894-bae0-4407e2c4548d?v=full""
                        }
                    ],
                    ""resourceVersion"": ""1.9""
                }
            ],
            ""voided"": false,
            ""auditInfo"": {
                ""creator"": {
                    ""uuid"": ""ed6cab31-46c9-4d78-b814-e79ddc406379"",
                    ""display"": ""superman"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/user/ed6cab31-46c9-4d78-b814-e79ddc406379""
                        }
                    ]
                },
                ""dateCreated"": ""2020-08-11T11:25:16.000+0530"",
                ""changedBy"": {
                    ""uuid"": ""A4F30A1B-5EB9-11DF-A648-37A07F9C90FB"",
                    ""display"": ""daemon"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/user/A4F30A1B-5EB9-11DF-A648-37A07F9C90FB""
                        }
                    ]
                },
                ""dateChanged"": ""2020-08-11T23:59:59.000+0530""
            },
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/ff633a82-43ac-45b0-94d8-3a06d379c0ab""
                }
            ],
            ""resourceVersion"": ""1.9""
        }
    ]
}";
        private const string PatientVisitsSampleWithoutEncounters = @"{
    ""results"": [
        {
            ""uuid"": ""ff633a82-43ac-45b0-94d8-3a06d379c0ab"",
            ""display"": ""OPD @ Odisha - 08/11/2020 11:25 AM"",
            ""patient"": {
                ""uuid"": ""9da1a756-aa62-46fd-b56d-e35a6d2f2b30"",
                ""display"": ""OD100013 - Patient test one"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/patient/9da1a756-aa62-46fd-b56d-e35a6d2f2b30""
                    }
                ]
            },
            ""visitType"": {
                ""uuid"": ""96c49059-9af1-4f63-a8be-7554984fda02"",
                ""display"": ""OPD"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visittype/96c49059-9af1-4f63-a8be-7554984fda02""
                    }
                ]
            },
            ""indication"": null,
            ""location"": {
                ""uuid"": ""8d6c993e-c2cc-11de-8d13-0010c6dffd0f"",
                ""display"": ""Odisha"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/location/8d6c993e-c2cc-11de-8d13-0010c6dffd0f""
                    }
                ]
            },
            ""startDatetime"": ""2020-08-11T11:25:16.000+0530"",
            ""stopDatetime"": null,
            ""encounters"": [],
            ""attributes"": [
                {
                    ""display"": ""Visit Status: OPD"",
                    ""uuid"": ""b5f18330-a072-4894-bae0-4407e2c4548d"",
                    ""attributeType"": {
                        ""uuid"": ""7f7bd8d8-e42f-11e5-8c3e-08002715d519"",
                        ""display"": ""Visit Status"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visitattributetype/7f7bd8d8-e42f-11e5-8c3e-08002715d519""
                            }
                        ]
                    },
                    ""value"": ""OPD"",
                    ""voided"": false,
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/ff633a82-43ac-45b0-94d8-3a06d379c0ab/attribute/b5f18330-a072-4894-bae0-4407e2c4548d""
                        },
                        {
                            ""rel"": ""full"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/ff633a82-43ac-45b0-94d8-3a06d379c0ab/attribute/b5f18330-a072-4894-bae0-4407e2c4548d?v=full""
                        }
                    ],
                    ""resourceVersion"": ""1.9""
                }
            ],
            ""voided"": false,
            ""auditInfo"": {
                ""creator"": {
                    ""uuid"": ""ed6cab31-46c9-4d78-b814-e79ddc406379"",
                    ""display"": ""superman"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/user/ed6cab31-46c9-4d78-b814-e79ddc406379""
                        }
                    ]
                },
                ""dateCreated"": ""2020-08-11T11:25:16.000+0530"",
                ""changedBy"": null,
                ""dateChanged"": null
            },
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/ff633a82-43ac-45b0-94d8-3a06d379c0ab""
                }
            ],
            ""resourceVersion"": ""1.9""
        }
    ]
}";
        private const string PatientVisitsSampleWithoutVisits = @"{
            ""results"":[]
            }";
        private const string PatientVisitsSample = @"{
    ""results"": [
        {
            ""uuid"": ""ff633a82-43ac-45b0-94d8-3a06d379c0ab"",
            ""display"": ""OPD @ Odisha - 08/11/2020 11:25 AM"",
            ""patient"": {
                ""uuid"": ""9da1a756-aa62-46fd-b56d-e35a6d2f2b30"",
                ""display"": ""OD100013 - Patient test one"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/patient/9da1a756-aa62-46fd-b56d-e35a6d2f2b30""
                    }
                ]
            },
            ""visitType"": {
                ""uuid"": ""96c49059-9af1-4f63-a8be-7554984fda02"",
                ""display"": ""OPD"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visittype/96c49059-9af1-4f63-a8be-7554984fda02""
                    }
                ]
            },
            ""indication"": null,
            ""location"": {
                ""uuid"": ""8d6c993e-c2cc-11de-8d13-0010c6dffd0f"",
                ""display"": ""Odisha"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/location/8d6c993e-c2cc-11de-8d13-0010c6dffd0f""
                    }
                ]
            },
            ""startDatetime"": ""2020-08-11T11:25:16.000+0530"",
            ""stopDatetime"": null,
            ""encounters"": [
                {
                    ""uuid"": ""e3b9cdcf-9f42-489f-b465-eb382923cd22"",
                    ""display"": ""Consultation 08/11/2020"",
                    ""encounterDatetime"": ""2020-08-11T11:26:40.000+0530"",
                    ""patient"": {
                        ""uuid"": ""9da1a756-aa62-46fd-b56d-e35a6d2f2b30"",
                        ""display"": ""OD100013 - Patient test one"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/patient/9da1a756-aa62-46fd-b56d-e35a6d2f2b30""
                            }
                        ]
                    },
                    ""location"": {
                        ""uuid"": ""6b8e123c-8522-4f71-8f80-0a893439bbce"",
                        ""display"": ""OPD"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/location/6b8e123c-8522-4f71-8f80-0a893439bbce""
                            }
                        ]
                    },
                    ""form"": null,
                    ""encounterType"": {
                        ""uuid"": ""7e10b3cb-e42f-11e5-8c3e-08002715d519"",
                        ""display"": ""Consultation"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encountertype/7e10b3cb-e42f-11e5-8c3e-08002715d519""
                            }
                        ]
                    },
                    ""obs"": [
                        {
                            ""uuid"": ""217f8ddf-899a-4923-aa60-df85ca789537"",
                            ""display"": ""Location of diagnosis: India"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/obs/217f8ddf-899a-4923-aa60-df85ca789537""
                                }
                            ]
                        },
                        {
                            ""uuid"": ""93f9e066-3ce4-4960-9d88-40689ddb10b9"",
                            ""display"": ""AI, Date of Admission: 2020-08-11"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/obs/93f9e066-3ce4-4960-9d88-40689ddb10b9""
                                }
                            ]
                        },
                        {
                            ""uuid"": ""00665ef4-5ca7-45ec-8abf-856b5f17965a"",
                            ""display"": ""AI, Admission to hospital: Yes"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/obs/00665ef4-5ca7-45ec-8abf-856b5f17965a""
                                }
                            ]
                        },
                        {
                            ""uuid"": ""16d0e32f-d823-404c-a23a-92371ab3dd1a"",
                            ""display"": ""AI, Date of isolation: 2020-08-11"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/obs/16d0e32f-d823-404c-a23a-92371ab3dd1a""
                                }
                            ]
                        },
                        {
                            ""uuid"": ""3419c52b-64c1-40d2-a190-399f16c2301a"",
                            ""display"": ""AI, History of mechanical ventilation: Yes"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/obs/3419c52b-64c1-40d2-a190-399f16c2301a""
                                }
                            ]
                        },
                        {
                            ""uuid"": ""d92577de-acfb-45e4-ba79-b9b873b52414"",
                            ""display"": ""AI, Patient symptoms: Fever"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/obs/d92577de-acfb-45e4-ba79-b9b873b52414""
                                }
                            ]
                        },
                        {
                            ""uuid"": ""16f27286-efb2-4b97-ad59-69d07839ead0"",
                            ""display"": ""AI, Current Health Problems: Diabetes"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/obs/16f27286-efb2-4b97-ad59-69d07839ead0""
                                }
                            ]
                        },
                        {
                            ""uuid"": ""4db941ef-1da1-4d11-9e58-cb19f061dce9"",
                            ""display"": ""AI, Patient health status: Recovered"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/obs/4db941ef-1da1-4d11-9e58-cb19f061dce9""
                                }
                            ]
                        },
                        {
                            ""uuid"": ""7137e02f-1f4c-4bad-8935-a84a47b41261"",
                            ""display"": ""AI, Date of onset of symptoms: 2020-08-11"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/obs/7137e02f-1f4c-4bad-8935-a84a47b41261""
                                }
                            ]
                        }
                    ],
                    ""orders"": [],
                    ""voided"": false,
                    ""visit"": {
                        ""uuid"": ""ff633a82-43ac-45b0-94d8-3a06d379c0ab"",
                        ""display"": ""OPD @ Odisha - 08/11/2020 11:25 AM"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/ff633a82-43ac-45b0-94d8-3a06d379c0ab""
                            }
                        ]
                    },
                    ""encounterProviders"": [
                        {
                            ""uuid"": ""3bf6e3d0-31ab-41ee-aa8c-118c99f48d5f"",
                            ""display"": ""super man: Unknown"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encounter/e3b9cdcf-9f42-489f-b465-eb382923cd22/encounterprovider/3bf6e3d0-31ab-41ee-aa8c-118c99f48d5f""
                                }
                            ]
                        }
                    ],
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encounter/e3b9cdcf-9f42-489f-b465-eb382923cd22""
                        },
                        {
                            ""rel"": ""full"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encounter/e3b9cdcf-9f42-489f-b465-eb382923cd22?v=full""
                        }
                    ],
                    ""resourceVersion"": ""1.9""
                }
            ],
            ""attributes"": [
                {
                    ""display"": ""Visit Status: OPD"",
                    ""uuid"": ""b5f18330-a072-4894-bae0-4407e2c4548d"",
                    ""attributeType"": {
                        ""uuid"": ""7f7bd8d8-e42f-11e5-8c3e-08002715d519"",
                        ""display"": ""Visit Status"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visitattributetype/7f7bd8d8-e42f-11e5-8c3e-08002715d519""
                            }
                        ]
                    },
                    ""value"": ""OPD"",
                    ""voided"": false,
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/ff633a82-43ac-45b0-94d8-3a06d379c0ab/attribute/b5f18330-a072-4894-bae0-4407e2c4548d""
                        },
                        {
                            ""rel"": ""full"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/ff633a82-43ac-45b0-94d8-3a06d379c0ab/attribute/b5f18330-a072-4894-bae0-4407e2c4548d?v=full""
                        }
                    ],
                    ""resourceVersion"": ""1.9""
                }
            ],
            ""voided"": false,
            ""auditInfo"": {
                ""creator"": {
                    ""uuid"": ""ed6cab31-46c9-4d78-b814-e79ddc406379"",
                    ""display"": ""superman"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/user/ed6cab31-46c9-4d78-b814-e79ddc406379""
                        }
                    ]
                },
                ""dateCreated"": ""2020-08-11T11:25:16.000+0530"",
                ""changedBy"": null,
                ""dateChanged"": null
            },
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/ff633a82-43ac-45b0-94d8-3a06d379c0ab""
                }
            ],
            ""resourceVersion"": ""1.9""
        },
        {
            ""uuid"": ""efc4c75c-4165-4942-836b-48649d9f7da2"",
            ""display"": ""OPD @ Odisha - 08/05/2020 05:15 PM"",
            ""patient"": {
                ""uuid"": ""9da1a756-aa62-46fd-b56d-e35a6d2f2b30"",
                ""display"": ""OD100013 - Patient test one"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/patient/9da1a756-aa62-46fd-b56d-e35a6d2f2b30""
                    }
                ]
            },
            ""visitType"": {
                ""uuid"": ""96c49059-9af1-4f63-a8be-7554984fda02"",
                ""display"": ""OPD"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visittype/96c49059-9af1-4f63-a8be-7554984fda02""
                    }
                ]
            },
            ""indication"": null,
            ""location"": {
                ""uuid"": ""8d6c993e-c2cc-11de-8d13-0010c6dffd0f"",
                ""display"": ""Odisha"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/location/8d6c993e-c2cc-11de-8d13-0010c6dffd0f""
                    }
                ]
            },
            ""startDatetime"": ""2020-08-05T17:15:36.000+0530"",
            ""stopDatetime"": ""2020-08-05T17:15:36.000+0530"",
            ""encounters"": [],
            ""attributes"": [],
            ""voided"": false,
            ""auditInfo"": {
                ""creator"": {
                    ""uuid"": ""ed6cab31-46c9-4d78-b814-e79ddc406379"",
                    ""display"": ""superman"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/user/ed6cab31-46c9-4d78-b814-e79ddc406379""
                        }
                    ]
                },
                ""dateCreated"": ""2020-08-05T17:15:36.000+0530"",
                ""changedBy"": {
                    ""uuid"": ""A4F30A1B-5EB9-11DF-A648-37A07F9C90FB"",
                    ""display"": ""daemon"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/user/A4F30A1B-5EB9-11DF-A648-37A07F9C90FB""
                        }
                    ]
                },
                ""dateChanged"": ""2020-08-06T23:59:59.000+0530""
            },
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/efc4c75c-4165-4942-836b-48649d9f7da2""
                }
            ],
            ""resourceVersion"": ""1.9""
        },
        {
            ""uuid"": ""823618e9-f403-4b41-8b83-7caba33403e7"",
            ""display"": ""OPD @ Odisha - 08/04/2020 06:58 AM"",
            ""patient"": {
                ""uuid"": ""9da1a756-aa62-46fd-b56d-e35a6d2f2b30"",
                ""display"": ""OD100013 - Patient test one"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/patient/9da1a756-aa62-46fd-b56d-e35a6d2f2b30""
                    }
                ]
            },
            ""visitType"": {
                ""uuid"": ""96c49059-9af1-4f63-a8be-7554984fda02"",
                ""display"": ""OPD"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visittype/96c49059-9af1-4f63-a8be-7554984fda02""
                    }
                ]
            },
            ""indication"": null,
            ""location"": {
                ""uuid"": ""8d6c993e-c2cc-11de-8d13-0010c6dffd0f"",
                ""display"": ""Odisha"",
                ""links"": [
                    {
                        ""rel"": ""self"",
                        ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/location/8d6c993e-c2cc-11de-8d13-0010c6dffd0f""
                    }
                ]
            },
            ""startDatetime"": ""2020-08-04T06:58:45.000+0530"",
            ""stopDatetime"": ""2020-08-05T11:30:08.000+0530"",
            ""encounters"": [
                {
                    ""uuid"": ""abf986aa-04d1-419c-a222-b9073b307574"",
                    ""display"": ""Consultation 08/04/2020"",
                    ""encounterDatetime"": ""2020-08-04T14:39:08.000+0530"",
                    ""patient"": {
                        ""uuid"": ""9da1a756-aa62-46fd-b56d-e35a6d2f2b30"",
                        ""display"": ""OD100013 - Patient test one"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/patient/9da1a756-aa62-46fd-b56d-e35a6d2f2b30""
                            }
                        ]
                    },
                    ""location"": {
                        ""uuid"": ""6b8e123c-8522-4f71-8f80-0a893439bbce"",
                        ""display"": ""OPD"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/location/6b8e123c-8522-4f71-8f80-0a893439bbce""
                            }
                        ]
                    },
                    ""form"": null,
                    ""encounterType"": {
                        ""uuid"": ""7e10b3cb-e42f-11e5-8c3e-08002715d519"",
                        ""display"": ""Consultation"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encountertype/7e10b3cb-e42f-11e5-8c3e-08002715d519""
                            }
                        ]
                    },
                    ""obs"": [],
                    ""orders"": [
                        {
                            ""uuid"": ""db47c1a5-2d1a-426b-8b3d-02f585ecdc20"",
                            ""display"": ""(NEW) DrugOther: null"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/order/db47c1a5-2d1a-426b-8b3d-02f585ecdc20""
                                }
                            ],
                            ""type"": ""drugorder""
                        }
                    ],
                    ""voided"": false,
                    ""visit"": {
                        ""uuid"": ""823618e9-f403-4b41-8b83-7caba33403e7"",
                        ""display"": ""OPD @ Odisha - 08/04/2020 06:58 AM"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/823618e9-f403-4b41-8b83-7caba33403e7""
                            }
                        ]
                    },
                    ""encounterProviders"": [
                        {
                            ""uuid"": ""a960f3fa-6cf9-45c0-a5e9-b63fd3e1b9e0"",
                            ""display"": ""super man: Unknown"",
                            ""links"": [
                                {
                                    ""rel"": ""self"",
                                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encounter/abf986aa-04d1-419c-a222-b9073b307574/encounterprovider/a960f3fa-6cf9-45c0-a5e9-b63fd3e1b9e0""
                                }
                            ]
                        }
                    ],
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encounter/abf986aa-04d1-419c-a222-b9073b307574""
                        },
                        {
                            ""rel"": ""full"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/encounter/abf986aa-04d1-419c-a222-b9073b307574?v=full""
                        }
                    ],
                    ""resourceVersion"": ""1.9""
                }
            ],
            ""attributes"": [
                {
                    ""display"": ""Visit Status: OPD"",
                    ""uuid"": ""1553d1c3-fdbb-47fc-89a5-bc8c4abc1525"",
                    ""attributeType"": {
                        ""uuid"": ""7f7bd8d8-e42f-11e5-8c3e-08002715d519"",
                        ""display"": ""Visit Status"",
                        ""links"": [
                            {
                                ""rel"": ""self"",
                                ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visitattributetype/7f7bd8d8-e42f-11e5-8c3e-08002715d519""
                            }
                        ]
                    },
                    ""value"": ""OPD"",
                    ""voided"": false,
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/823618e9-f403-4b41-8b83-7caba33403e7/attribute/1553d1c3-fdbb-47fc-89a5-bc8c4abc1525""
                        },
                        {
                            ""rel"": ""full"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/823618e9-f403-4b41-8b83-7caba33403e7/attribute/1553d1c3-fdbb-47fc-89a5-bc8c4abc1525?v=full""
                        }
                    ],
                    ""resourceVersion"": ""1.9""
                }
            ],
            ""voided"": false,
            ""auditInfo"": {
                ""creator"": {
                    ""uuid"": ""ed6cab31-46c9-4d78-b814-e79ddc406379"",
                    ""display"": ""superman"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/user/ed6cab31-46c9-4d78-b814-e79ddc406379""
                        }
                    ]
                },
                ""dateCreated"": ""2020-08-04T06:58:45.000+0530"",
                ""changedBy"": {
                    ""uuid"": ""ed6cab31-46c9-4d78-b814-e79ddc406379"",
                    ""display"": ""superman"",
                    ""links"": [
                        {
                            ""rel"": ""self"",
                            ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/user/ed6cab31-46c9-4d78-b814-e79ddc406379""
                        }
                    ]
                },
                ""dateChanged"": ""2020-08-05T11:30:08.000+0530""
            },
            ""links"": [
                {
                    ""rel"": ""self"",
                    ""uri"": ""http://bahmni-0.92.bahmni-covid19.in/openmrs/ws/rest/v1/visit/823618e9-f403-4b41-8b83-7caba33403e7""
                }
            ],
            ""resourceVersion"": ""1.9""
        }
    ]
}";
    }
}