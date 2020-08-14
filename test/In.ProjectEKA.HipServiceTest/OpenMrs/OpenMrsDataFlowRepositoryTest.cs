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
            Func<Task> loadObservationsForVisits = async () => {
                await dataFlowRepository.LoadObservationsForVisits(patientReferenceNumber, "OPD");
            };

            //Then
            loadObservationsForVisits.Should().Throw<OpenMrsFormatException>();
        }

private const string PatientVisitsSampleWithoutVisitType=@"{
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