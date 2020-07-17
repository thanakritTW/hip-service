namespace In.ProjectEKA.HipService.Discovery
{
    using System.Linq;
    using System;
    using System.Threading.Tasks;
    using HipLibrary.Matcher;
    using HipLibrary.Patient.Model;
    using DiscoveryRequest = HipLibrary.Patient.Model.DiscoveryRequest;

    public class OpenMrsPatientMatchingRepository : IMatchingRepository
    {
        private readonly IPatientDal _patientDal;
        public OpenMrsPatientMatchingRepository(IPatientDal patientDal)
        {
            _patientDal = patientDal;
        }

        public async Task<IQueryable<Patient>> Where(DiscoveryRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var result =
                await _patientDal.LoadPatientsAsync(
                    request.Patient?.Name,
                    request.Patient?.Gender.ToOpenMrsGender(),
                    request.Patient?.YearOfBirth);

            return result
                    .Select(p => p.ToHipPatient())
                    .ToList()
                    .AsQueryable();
        }
    }

    public static class PatientExtensions
    {
        public static Patient ToHipPatient(this Hl7.Fhir.Model.Patient openMrsPatient)
        {

            return new Patient()
            {
                Name = openMrsPatient.Name.First().Text,
                Gender = openMrsPatient.Gender.HasValue ? (Gender)((int)openMrsPatient.Gender) : (Gender?)null,
                YearOfBirth = (ushort?)openMrsPatient.BirthDateElement?.ToDateTimeOffset()?.Year
           };
        }

        public static Hl7.Fhir.Model.AdministrativeGender? ToOpenMrsGender(this Gender? gender)
        {
            return gender.HasValue ? (Hl7.Fhir.Model.AdministrativeGender)((int)gender) : (Hl7.Fhir.Model.AdministrativeGender?)null;
        }
    }
}