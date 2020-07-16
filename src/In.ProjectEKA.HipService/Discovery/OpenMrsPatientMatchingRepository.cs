namespace In.ProjectEKA.HipService.Discovery
{
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using System.Threading.Tasks;
    using HipLibrary.Matcher;
    using HipLibrary.Patient.Model;
    using DiscoveryRequest = HipLibrary.Patient.Model.DiscoveryRequest;
    using static HipLibrary.Matcher.StrongMatcherFactory;

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

            var result = await _patientDal.LoadPatientsAsync(request.Patient?.Name, request.Patient?.Gender, request.Patient?.YearOfBirth);

            return (from r in result
                    select new Patient()
                    {
                        Name = r.Name.First().Text,
                        Gender = r.Gender.HasValue ? (Gender)((int)r.Gender) : Gender.M,
                        YearOfBirth = (ushort)r.BirthDateElement.ToDateTimeOffset()?.Year
                    }).ToList().AsQueryable();
        }
    }
}