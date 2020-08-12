namespace In.ProjectEKA.HipLibrary.Patient.Model
{
    public class Observation
    {
        public string ReferenceNumber { get; }

        public string Display { get; }

        public Observation(string referenceNumber, string display)
        {
            ReferenceNumber = referenceNumber;
            Display = display;
        }
    }
}