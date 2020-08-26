using System;

namespace In.ProjectEKA.HipLibrary.Patient.Model
{
    public class Condition
    {
        public string ReferenceNumber { get; }
        public Concept Concept { get; }
        public string ConditionNonCoded { get; }
        public string Status { get; }
        public DateTime OnSetDate { get; }

        public Condition(string referenceNumber, Concept concept, string conditionNonCoded, string status, DateTime onSetDate)
        {
        ReferenceNumber = referenceNumber;
        ConditionNonCoded = conditionNonCoded;
        Status = status;
        Concept = concept;
        OnSetDate = onSetDate;
        }
    }
}
