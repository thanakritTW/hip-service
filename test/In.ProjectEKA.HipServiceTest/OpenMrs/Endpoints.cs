namespace In.ProjectEKA.HipServiceTest.OpenMrs
{
  public static class Endpoints
  {
    public static class OpenMrs
    {
      public const string OnProgramEnrollmentPath = "ws/rest/v1/bahmniprogramenrollment";
      public const string OnVisitPath = "ws/rest/v1/visit";
      public const string OnPatientPath = "ws/rest/v1/patient";
      public const string OnProgramObservations = "ws/rest/v1/bahmnicore/sql?q=emrapi.sqlSearch.programObservations&program_enrollment_uuid=";
            public const string OnObs = "ws/rest/v1/obs";
    }

    public static class Fhir
    {
      public const string OnPatientPath = "ws/fhir2/Patient";
    }
    public static class EMRAPI
    {
      public const string OnConditionPath = "ws/rest/emrapi/conditionhistory";
    }
  }
}
