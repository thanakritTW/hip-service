namespace In.ProjectEKA.HipLibrary.Patient.Model
{
    public class Medication
    {
        public string ReferenceNumber { get; }

        public string Display { get; }

        public string Type { get; }

        public Medication(string referenceNumber, string display, string type)
        {
            ReferenceNumber = referenceNumber;
            Display = display;
            Type = type;
        }
    }
}
