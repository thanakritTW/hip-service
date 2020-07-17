using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace In.ProjectEKA.HipService.Discovery
{
    public interface IPatientDal
    {
        Task<List<Patient>> LoadPatientsAsync(string name, AdministrativeGender? gender, ushort? yearOfBirth);
    }
}