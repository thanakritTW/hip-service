namespace In.ProjectEKA.HipLibrary.Patient
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Model;

  public interface IOpenMrsDataFlowRepository
  {
    Task<List<Observation>> LoadObservationsForVisits(string patientReferenceNumber, string visitType);

  }
}
