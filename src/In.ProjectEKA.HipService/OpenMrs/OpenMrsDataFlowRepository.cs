
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using In.ProjectEKA.HipLibrary.Patient;
using In.ProjectEKA.HipLibrary.Patient.Model;

namespace In.ProjectEKA.HipService.OpenMrs
{
     public static class VisitProperties
    {
        public const string Results = "results";
        public const string VisitType = "visitType";
        public const string Encounters = "encounters";
        public const string Observations = "obs";
    }

    public static class VisitTypeProperties
    {
        public const string Display = "display";
    }

    public static class ObservationProperties
    {
        public const string Display = "display";
        public const string Uuid = "uuid";
    }

    public static class ConditionProperties
    {
        public const string Conditions = "conditions";
        public const string Concept = "concept";
        public const string OnSetDate = "onSetDate";
        public const string Uuid = "uuid";
        public const string ConditionNonCoded = "conditionNonCoded";
        public const string Status = "status";
    }

    public static class ConceptProperties
    {
        public const string Name = "name";
    }

    public class OpenMrsDataFlowRepository : IOpenMrsDataFlowRepository
    {
        private readonly IOpenMrsClient openMrsClient;
        public OpenMrsDataFlowRepository(IOpenMrsClient openMrsClient)
        {
            this.openMrsClient = openMrsClient;
        }

        public async Task<List<Observation>> LoadObservationsForVisits(string patientReferenceNumber, string visitTypeDisplay)
        {
            var observations = new List<Observation>();

            JsonElement root = await getRootElementOfResult(patientReferenceNumber);

            var results = getResults(root);
            var encountersMatchingVisitType = getEncounters(results, visitTypeDisplay);
            encountersMatchingVisitType.ForEach(e => {
                if (e.GetArrayLength() != 0) {
                    for (int j = 0; j < e.GetArrayLength(); j++)
                        {
                            var obs = e[j].GetProperty(VisitProperties.Observations);
                            if (obs.GetArrayLength() != 0)
                            {
                                for (int k = 0; k < obs.GetArrayLength(); k++)
                                {
                                    observations.Add(
                                        new Observation(
                                            obs[k].GetProperty(ObservationProperties.Uuid).GetString(),
                                            obs[k].GetProperty(ObservationProperties.Display).GetString()
                                        )
                                    );
                                }
                            }

                        }
                }
            });

            return observations;
        }
        public async Task<List<Diagnosis>> LoadDiagnosticReportForVisits(string patientReferenceNumber, string visitTypeDisplay)
        {
            var diagnosis = new List<Diagnosis>();
            string diagnosisVisit = "Visit Diagnoses";

            JsonElement root = await getRootElementOfResult(patientReferenceNumber);

            var results = getResults(root);
            var encountersMatchingVisitType = getEncounters(results, visitTypeDisplay);
            encountersMatchingVisitType.ForEach(e => {
                if (e.GetArrayLength() != 0)
                    {
                        for (int j = 0; j < e.GetArrayLength(); j++)
                        {
                            var obs = e[j].GetProperty("obs");
                            {
                                if (obs.GetArrayLength() != 0)
                                {
                                    for (int k = 0; k < obs.GetArrayLength(); k++)
                                    {
                                        if (obs[k].TryGetProperty("display", out var obsDisplay) && obsDisplay.GetString().Contains(diagnosisVisit))
                                        {
                                            diagnosis.Add(new Diagnosis(obs[k].GetProperty("uuid").ToString(), obs[k].GetProperty("display").ToString()));
                                        }
                                    }
                                }
                            }

                        }
                    }
            });
            
            return diagnosis;
        }
        public async Task<List<Medication>> LoadMedicationForVisits(string patientReferenceNumber, string visitTypeDisplay)
        {
            var medications = new List<Medication>();

            JsonElement root = await getRootElementOfResult(patientReferenceNumber);

            var results = getResults(root);
            var encountersMatchingVisitType = getEncounters(results, visitTypeDisplay);
            encountersMatchingVisitType.ForEach(e => {
                if (e.GetArrayLength() != 0)
                    {
                        for (int j = 0; j < e.GetArrayLength(); j++)
                        {
                            var orders = e[j].GetProperty("orders");
                            {
                                if (orders.GetArrayLength() != 0)
                                {
                                    for (int k = 0; k < orders.GetArrayLength(); k++)
                                    {
                                        medications.Add(new Medication(orders[k].GetProperty("uuid").ToString(), orders[k].GetProperty("display").ToString(), orders[k].GetProperty("type").ToString()));
                                    }
                                }
                            }

                        }
                    }
            });
            
            return medications;
        }

        public async Task<List<Condition>> LoadConditionsForVisit(string patientReferenceNumber)
        {
            var conditions = new List<Condition>();
            var path = DataFlowPathConstants.OnConditionPath;
            var query = HttpUtility.ParseQueryString(string.Empty);
            var observations = new List<Observation>();
            if (!string.IsNullOrEmpty(patientReferenceNumber))
            {
                query["patientUuid"] = patientReferenceNumber;
            }
            else
            {
                throw new OpenMrsFormatException();
            }
            if (query.ToString() != "")
            {
                path = $"{path}?{query}";
            }

            var response = await openMrsClient.GetAsync(path);
            var content = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(content);
            var results = jsonDoc.RootElement;

            for (int i = 0; i < results.GetArrayLength(); i++)
            {
                var condition = results[i].GetProperty(ConditionProperties.Conditions);
                for (int j = 0; j < condition.GetArrayLength(); j++)
                {
                    var concept = condition[j].GetProperty(ConditionProperties.Concept);
                    var onSetDateMilliseconds = condition[j].GetProperty(ConditionProperties.OnSetDate).GetInt64();
                    DateTime onSetDate = DateTimeOffset.FromUnixTimeMilliseconds(onSetDateMilliseconds).UtcDateTime;

                    conditions.Add(new Condition(
                        condition[j].GetProperty(ConditionProperties.Uuid).GetString(),
                        new Concept(concept.GetProperty(ConditionProperties.Uuid).GetString(), concept.GetProperty(ConceptProperties.Name).GetString()),
                        condition[j].GetProperty(ConditionProperties.ConditionNonCoded).GetString(),
                        condition[j].GetProperty(ConditionProperties.Status).GetString(),
                        onSetDate
                    ));
                }
            }

            return conditions;
        }

        private JsonElement getResults(JsonElement root)
        {
            return root.GetProperty(VisitProperties.results);
        }

        private List<JsonElement> getEncounters(JsonElement results, string visitTypeDisplay)
        {
            var encountersMatchingVisitType = new List<JsonElement>();
            for (int i = 0; i < results.GetArrayLength(); i++)
            {
                var visitType = results[i].GetProperty(VisitProperties.visitType);
                if (visitType.TryGetProperty(VisitTypeProperties.display, out var display) && display.GetString() == visitTypeDisplay)
                {
                    encountersMatchingVisitType.Add(results[i].GetProperty(VisitProperties.encounters));
                }
            }
            return encountersMatchingVisitType;
        }

        private async Task<JsonElement> getRootElementOfResult(string patientReferenceNumber)
        {
            var path = DataFlowPathConstants.OnVisitPath;
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(patientReferenceNumber))
            {
                query["patient"] = patientReferenceNumber;
                query["v"] = "full";
            }
            else
            {
                throw new OpenMrsFormatException();
            }
            if (query.ToString() != "")
            {
                path = $"{path}?{query}";
            }

            var response = await openMrsClient.GetAsync(path);
            var content = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;
            return root;
        }

        private JsonElement getResults(JsonElement root)
        {
            return root.GetProperty(VisitProperties.Results);
        }

        private List<JsonElement> getEncounters(JsonElement results, string visitTypeDisplay)
        {
            var encountersMatchingVisitType = new List<JsonElement>();
            for (int i = 0; i < results.GetArrayLength(); i++)
            {
                var visitType = results[i].GetProperty(VisitProperties.VisitType);
                if (visitType.TryGetProperty(VisitTypeProperties.Display, out var display) && display.GetString() == visitTypeDisplay)
                {
                    encountersMatchingVisitType.Add(results[i].GetProperty(VisitProperties.Encounters));
                }
            }
            return encountersMatchingVisitType;
        }
    }
}
