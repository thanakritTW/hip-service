
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using In.ProjectEKA.HipLibrary.Patient;
using In.ProjectEKA.HipLibrary.Patient.Model;
using In.ProjectEKA.HipService.Logger;

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
            var path = DataFlowPathConstants.OnVisitPath;
            var query = HttpUtility.ParseQueryString(string.Empty);
            var observations = new List<Observation>();
            if (!string.IsNullOrEmpty(patientReferenceNumber))
            {
                query["patient"] = patientReferenceNumber;
                query["v"] = "full";
            } else {
                return observations;
            }
            if (query.ToString() != "")
            {
                path = $"{path}?{query}";
            }

            var response = await openMrsClient.GetAsync(path);
            var content = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            var results = root.GetProperty("results");
            for (int i = 0; i < results.GetArrayLength(); i++)
            {
                var visitType = results[i].GetProperty("visitType");
                var display = visitType.GetProperty("display").GetString();
                if (display == visitTypeDisplay)
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


    }
}