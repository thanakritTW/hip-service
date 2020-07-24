using System.Collections.Generic;
using In.ProjectEKA.HipLibrary.Patient.Model;

namespace In.ProjectEKA.HipServiceTest.Discovery.Builder
{
    public class User
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Gender Gender { get; set; }
        public ushort YearOfBirth { get; set; }
        public string PhoneNumber { get; set; } = null;
        public IEnumerable<CareContextRepresentation> CareContexts { get; set; }

        public static readonly User Krunal = new User
        {
            Name = "Krunal Patel",
            Id = "RVH1111",
            Gender = Gender.M,
            YearOfBirth = 1976,
            CareContexts = new List<CareContextRepresentation> {
                new CareContextRepresentation("NCP1111", "National Cancer program"),
                new CareContextRepresentation("NCP1111", "National Cancer program - Episode 2")
            }
        };
        public static readonly User JohnDoe = new User
        {
            Name = "John Doe",
            Id = "1234",
            Gender = Gender.M,
            YearOfBirth = 1994,
            CareContexts = null,
            PhoneNumber = "11111111111"
        };

        public static readonly User Linda = new User
        {
            Name = "Linda",
            Id = "5678",
            Gender = Gender.F,
            YearOfBirth = 1972,
            CareContexts = null,
            PhoneNumber = "99999999999"
        };


    }
}