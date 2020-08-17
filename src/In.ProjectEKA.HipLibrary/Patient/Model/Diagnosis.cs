namespace In.ProjectEKA.HipLibrary.Patient.Model
{
    public class Diagnosis
    {
        public string ReferenceNumber { get; }

        public string Display { get; }

        public Diagnosis(string referenceNumber, string display)
        {
            ReferenceNumber = referenceNumber;
            Display = display;
        }
    }
}
