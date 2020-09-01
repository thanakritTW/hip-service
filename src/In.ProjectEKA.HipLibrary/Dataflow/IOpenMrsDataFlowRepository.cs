using System.Threading.Tasks;

namespace In.ProjectEKA.HipLibrary.DataFlow
{
    public interface IOpenMrsDataFlowRepository
    {
        Task<string> GetMedicationsForVisits(string patientId, string linkedCareContextVisitType);
    }
}