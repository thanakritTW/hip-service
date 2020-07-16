namespace In.ProjectEKA.HipServiceTest.Discovery
{
    using System;
    using FluentAssertions;
    using HipService.Discovery;
    using HipService.Discovery.Database;
    using HipLibrary.Patient.Model;
    using Microsoft.EntityFrameworkCore;
    using Xunit;
    using Moq;
    using System.Collections.Generic;
    using FluentAssertions;
    using System.Linq;
    using System.Threading.Tasks;
    public class OpenMrsPatientMatchingRepositoryTest
    {
        private Mock<IPatientDal> patientDal = new Mock<IPatientDal>();

        public OpenMrsPatientMatchingRepositoryTest()
        {
                patientDal.Setup(e => e.LoadPatientsAsync(
                It.IsAny<string>(),
                It.IsAny<Gender?>(),
                It.IsAny<ushort?>())).Returns( (string name, Gender? gender, ushort? yob) => {
                    var humanName = new Hl7.Fhir.Model.HumanName();
                    humanName.Text = name;

                    return Task.FromResult(new List<Hl7.Fhir.Model.Patient>(){ new Hl7.Fhir.Model.Patient() { Name = new List<Hl7.Fhir.Model.HumanName>{ humanName }, Gender = Hl7.Fhir.Model.AdministrativeGender.Female, BirthDate = "1981" }});
                }
                );
        }

        [Fact]
        private async void PatientDalIsInvokedWithExpectedParameters()
        {
            const string  patientName = "patient name";
            Gender? patientGender = Gender.F;
            ushort?  patientYob = 1981;

            var patientEnquiry = new PatientEnquiry("id", verifiedIdentifiers: null, unverifiedIdentifiers: null, patientName, patientGender, patientYob);
            var request = new DiscoveryRequest(patientEnquiry,"requestId", "transactionId", DateTime.Now);
            var repo = new OpenMrsPatientMatchingRepository(patientDal.Object);
           
            var result = repo.Where(request);

            patientDal.Verify( x => x.LoadPatientsAsync(patientName, patientGender, patientYob), Times.Once);

        }

        [Fact]
        private async void ReturnsAnHIPPatientWithExpectedValues()
        {
            const string  patientName = "patient name";
            Gender? patientGender = Gender.F;
            ushort?  patientYob = 1981;

            var patientEnquiry = new PatientEnquiry("id", verifiedIdentifiers: null, unverifiedIdentifiers: null, patientName, patientGender, patientYob);
            var request = new DiscoveryRequest(patientEnquiry,"requestId", "transactionId", DateTime.Now);
            var repo = new OpenMrsPatientMatchingRepository(patientDal.Object);
           
            var patient = repo.Where(request).Result.Single();

            patient.Name.Should().Be(patientName);
            patient.Gender.Should().Be(patientGender);
            patient.YearOfBirth.Should().Be(patientYob);
        }
    }
}