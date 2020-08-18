
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

            var results = root.GetProperty("results");
            for (int i = 0; i < results.GetArrayLength(); i++)
            {
                var visitType = results[i].GetProperty("visitType");

                if (visitType.TryGetProperty("display", out var display) && display.GetString() == visitTypeDisplay)
                {
                    var encounters = results[i].GetProperty("encounters");
                    if (encounters.GetArrayLength() != 0)
                    {
                        for (int j = 0; j < encounters.GetArrayLength(); j++)
                        {
                            var obs = encounters[j].GetProperty("obs");
                            if (obs.GetArrayLength() != 0)
                            {
                                for (int k = 0; k < obs.GetArrayLength(); k++)
                                {
                                    observations.Add(new Observation(obs[k].GetProperty("uuid").ToString(), obs[k].GetProperty("display").ToString()));
                                }
                            }

                        }
                    }
                }
            }


            return observations;
        }
        public async Task<List<Diagnosis>> LoadDiagnosticReportForVisits(string patientReferenceNumber, string visitTypeDisplay)
        {
            var diagnosis = new List<Diagnosis>();
            string diagnosisVisit = "Visit Diagnoses";

            JsonElement root = await getRootElementOfResult(patientReferenceNumber);

            var results = root.GetProperty("results");
            for (int i = 0; i < results.GetArrayLength(); i++)
            {
                var visitType = results[i].GetProperty("visitType");

                if (visitType.TryGetProperty("display", out var display) && display.GetString() == visitTypeDisplay)
                {
                    var encounters = results[i].GetProperty("encounters");
                    {
                        if (encounters.GetArrayLength() != 0)
                        {
                            for (int j = 0; j < encounters.GetArrayLength(); j++)
                            {
                                var obs = encounters[j].GetProperty("obs");
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
                    }
                }
            }
            return diagnosis;
        }
        public async Task<List<Medication>> LoadMedicationForVisits(string patientReferenceNumber, string visitTypeDisplay)
        {
            var medications = new List<Medication>();

            JsonElement root = await getRootElementOfResult(patientReferenceNumber);

            var results = root.GetProperty("results");
            for (int i = 0; i < results.GetArrayLength(); i++)
            {
                var visitType = results[i].GetProperty("visitType");

                if (visitType.TryGetProperty("display", out var display) && display.GetString() == visitTypeDisplay)
                {
                    var encounters = results[i].GetProperty("encounters");
                    {
                        if (encounters.GetArrayLength() != 0)
                        {
                            for (int j = 0; j < encounters.GetArrayLength(); j++)
                            {
                                var orders = encounters[j].GetProperty("orders");
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
                    }
                }
            }
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
                var condition = results[i].GetProperty("conditions");
                for (int j = 0; j < condition.GetArrayLength(); j++)
                {
                    var concept = condition[j].GetProperty("concept");
                    conditions.Add(new Condition(condition[j].GetProperty("uuid").ToString(),
                    new Concept(concept.GetProperty("uuid").ToString(), concept.GetProperty("name").ToString()),
                    condition[j].GetProperty("conditionNonCoded").ToString(),
                    condition[j].GetProperty("status").ToString()));
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
    }
}