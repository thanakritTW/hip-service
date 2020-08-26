
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using In.ProjectEKA.HipLibrary.Patient;
using In.ProjectEKA.HipLibrary.Patient.Model;
using In.ProjectEKA.HipService.OpenMrs.Exceptions;

namespace In.ProjectEKA.HipService.OpenMrs
{
     public static class VisitProperties
    {
        public const string results = "results";
        public const string visitType = "visitType";
        public const string encounters = "encounters";
        public const string observations = "obs";
    }

    public static class VisitTypeProperties
    {
        public const string display = "display";
    }

    public static class ObservationProperties
    {
        public const string display = "display";
        public const string uuid = "uuid";
    }

    public static class ConditionProperties
    {
        public const string conditions = "conditions";
        public const string concept = "concept";
        public const string onSetDate = "onSetDate";
        public const string uuid = "uuid";
        public const string conditionNonCoded = "conditionNonCoded";
        public const string status = "status";
    }

    public static class ConceptProperties
    {
        public const string name = "name";
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
                            var obs = e[j].GetProperty(VisitProperties.observations);
                            if (obs.GetArrayLength() != 0)
                            {
                                for (int k = 0; k < obs.GetArrayLength(); k++)
                                {
                                    observations.Add(
                                        new Observation(
                                            obs[k].GetProperty(ObservationProperties.uuid).GetString(),
                                            obs[k].GetProperty(ObservationProperties.display).GetString()
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
                var condition = results[i].GetProperty(ConditionProperties.conditions);
                for (int j = 0; j < condition.GetArrayLength(); j++)
                {
                    var concept = condition[j].GetProperty(ConditionProperties.concept);
                    var onSetDateMilliseconds = condition[j].GetProperty(ConditionProperties.onSetDate).GetInt64();
                    DateTime onSetDate = DateTimeOffset.FromUnixTimeMilliseconds(onSetDateMilliseconds).UtcDateTime;

                    conditions.Add(new Condition(
                        condition[j].GetProperty(ConditionProperties.uuid).GetString(),
                        new Concept(concept.GetProperty(ConditionProperties.uuid).GetString(), concept.GetProperty(ConceptProperties.name).GetString()),
                        condition[j].GetProperty(ConditionProperties.conditionNonCoded).GetString(),
                        condition[j].GetProperty(ConditionProperties.status).GetString(),
                        onSetDate
                    ));
                }
            }

            return conditions;
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

        public async Task<IEnumerable<Observation>> LoadObservationsForPrograms(string programEnrollementUuid)
        {
            var path = DataFlowPathConstants.OnCustomQueryPath;
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(programEnrollementUuid))
            {
                query["q"] = "emrapi.sqlSearch.programObservations";
                query["program_enrollment_uuid"] = programEnrollementUuid;
            }
            else
            {
                throw new OpenMrsFormatException();
            }
            path = $"{path}?{query}";

            var response = await openMrsClient.GetAsync(path);
            if (!response.IsSuccessStatusCode)
                throw new OpenMrsResponseException($"Non successful http response {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(content);
            var observationsArray = jsonDoc.RootElement;

            var observationUuids = ParseJsonArrayToObservationUuids(observationsArray);
            var observations = LoadObservations(observationUuids);

            return await observations;
        }

        private async Task<IEnumerable<Observation>> LoadObservations(IEnumerable<string> observationUuids)
        {
            var observations = observationUuids.Select(async o =>
            {
                var response = await openMrsClient.GetAsync($"{DataFlowPathConstants.OnObsPath}/{o}");
                var content = await response.Content.ReadAsStringAsync();

                var jsonDoc = JsonDocument.Parse(content);
                var observation = jsonDoc.RootElement;

                return ParseObservation(observation);
            });

            return await Task.WhenAll(observations);
        }

        private IEnumerable<string> ParseJsonArrayToObservationUuids(JsonElement observationsArray)
        {
            foreach (var observation in observationsArray.EnumerateArray())
            {
                yield return ParseObservationUuid(observation);
            }
        }

        private string ParseObservationUuid(JsonElement observationObject)
        {
            try
            {
                return observationObject.GetProperty("uuid").GetString();
            }
            catch (KeyNotFoundException ex)
            {
                Logger.Log.Error("Missing uuid key in Observations response. {0}", ex);
                throw new OpenMrsFormatException();
            }
        }

        private Observation ParseObservation(JsonElement observationObject)
        {
            try
            {
                return new Observation(
                    observationObject.GetProperty("uuid").GetString(),
                    observationObject.GetProperty("display").GetString());
            }
            catch (KeyNotFoundException ex)
            {
                Logger.Log.Error("Missing uuid key or display in Observation response. {0}", ex);
                throw new OpenMrsFormatException();
            }
        }
    }
}
