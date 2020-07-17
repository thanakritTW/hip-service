namespace In.ProjectEKA.HipServiceTest.Discovery
{
    using System;
    using FluentAssertions;
    using HipService.Discovery;
    using HipLibrary.Patient.Model;
    using Xunit;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using OpenMrsPatient = Hl7.Fhir.Model.Patient;
    using OpenMrsPatientName = Hl7.Fhir.Model.HumanName;
    using OpenMrsGender = Hl7.Fhir.Model.AdministrativeGender;

    public class PatientExtensionsTest
    {
        [Fact]
        private async void ToHipPatient_GivenOpenMrsPatientWithMultipleNames_UsesTextValueFromFirstNameInCollection()
        {
            const string patientName = "Patient name";

            var openMrsPatient = new OpenMrsPatient() {
                Name = new List<OpenMrsPatientName>{  new OpenMrsPatientName() { Text = patientName }, new OpenMrsPatientName() { Text = "a second name" } },
                Gender = OpenMrsGender.Female,
                BirthDate = "1981"
            };

            var hipPatient = openMrsPatient.ToHipPatient();

            openMrsPatient.Name.Count().Should().Be(2);
            hipPatient.Name.Should().Be(patientName);
        }

        [Theory]
        [InlineData(OpenMrsGender.Male, Gender.M)]
        [InlineData(OpenMrsGender.Female, Gender.F)]
        [InlineData(OpenMrsGender.Other, Gender.O)]
        [InlineData(OpenMrsGender.Unknown, Gender.U)]
        [InlineData(null, null)]
        private async void ToHipPatient_GivenOpenMrsPatient_GenderIsMappedCorrectly(OpenMrsGender? sourceOpenMrsGender, Gender? expectedHipGender)
        {

            var openMrsPatient = new OpenMrsPatient() {
                Name = new List<OpenMrsPatientName>{  new OpenMrsPatientName() { Text = "Patient name" } },
                Gender = sourceOpenMrsGender,
                BirthDate = "1981"
            };

            var hipPatient = openMrsPatient.ToHipPatient();
            
            hipPatient.Gender.Should().Be(expectedHipGender);

        }


        [Theory]
        [InlineData("1981", (UInt16)1981)]
        [InlineData("1973-06", (UInt16)1973)]
        [InlineData("1905-08-23", (UInt16)1905)]
        [InlineData(null, null)]
        private async void ToHipPatient_GivenOpenMrsPatient_YearOfBirthIsCalculatedFromBirthDate(string sourceBirthDate, ushort? expectedYearOfBirth)
        {
            // The hl7 date format is YYYY, YYYY-MM, or YYYY-MM-DD, e.g. 2018, 1973-06, or 1905-08-23. 
            // https://www.hl7.org/fhir/datatypes.html#date

            const string patientName = "Patient name";

            var openMrsPatient = new OpenMrsPatient() {
                Name = new List<OpenMrsPatientName>{  new OpenMrsPatientName() { Text = patientName } },
                Gender = OpenMrsGender.Female,
                BirthDate = sourceBirthDate
            };

            var hipPatient = openMrsPatient.ToHipPatient();

            hipPatient.YearOfBirth.Should().Be(expectedYearOfBirth);
        }
    }
}