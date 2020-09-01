using System.Linq;
using System.Threading.Tasks;
using In.ProjectEKA.HipLibrary.DataFlow;
using In.ProjectEKA.HipLibrary.Patient;
using In.ProjectEKA.HipLibrary.Patient.Model;
using Optional;
using Bundle = Hl7.Fhir.Model.Bundle;
using MedicationRequest = Hl7.Fhir.Model.MedicationRequest;
using Medication = Hl7.Fhir.Model.Medication;
using static Hl7.Fhir.Model.Bundle;

namespace In.ProjectEKA.HipService.DataFlow
{
    public class CollectDataflowBundle : ICollect
    {
        private IOpenMrsDataFlowRepository _openMrsDataFlowRepository;

        public CollectDataflowBundle(IOpenMrsDataFlowRepository openMrsDataFlowRepository)
        {
            _openMrsDataFlowRepository = openMrsDataFlowRepository;
        }

        public async Task<Option<Entries>> CollectData(HipLibrary.Patient.Model.DataRequest dataRequest)
        {
            var bundles = dataRequest
                .CareContexts
                .Select(async cc =>
                {
                    var medications =
                        await _openMrsDataFlowRepository.GetMedicationsForVisits(cc.PatientReference, cc.CareContextReference);
                    var medicationRequest = new MedicationRequest();
                    medicationRequest.Children.Append(new Medication());
                    var entryComponent = new EntryComponent();
                    entryComponent.Resource = medicationRequest;
                    var bundle = new Bundle();
                    bundle.Type = BundleType.Collection;
                    bundle.Id = "bundle-1";
                    bundle.Entry.Add(entryComponent);
                    return new CareBundle(cc.CareContextReference, bundle);
                })
                .ToList();

            var carebundles = await Task.WhenAll(bundles);

            return Option.Some(new Entries(carebundles));
        }
    }
}