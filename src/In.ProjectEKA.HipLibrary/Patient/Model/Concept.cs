namespace In.ProjectEKA.HipLibrary.Patient.Model
{
  public class Concept
  {
    public string ReferenceNumber { get; }

    public string Name { get; }

    public Concept(string referenceNumber, string name)
    {
      ReferenceNumber = referenceNumber;
      Name = name;
    }
}
