using FirstAPI.Contexts;
using FirstAPI.Models;
using FirstAPI.Repositories;
using FirstAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace FirstAPI.Test
{
    public class PatientRepositoryTests
    {
        private ClinicContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ClinicContext>()
                .UseInMemoryDatabase("PatientTestDb")
                .Options;
            _context = new ClinicContext(options);
        }

        [Test]
        public async Task AddPatient()
        {
            // Arrange
            var patient = new Patient { Name = "John Doe", Age = 30, Email = "john.doe@example.com", Phone = "1234567890" };
            IRepository<int, Patient> _patientRepository = new PatinetRepository(_context);

            // Act
            var result = await _patientRepository.Add(patient);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.Name, Is.EqualTo("John Doe"));
        }

        [Test]
        public async Task GetPatient()
        {
            // Arrange
            var patient = new Patient { Name = "Jane Doe", Age = 25, Email = "jane.doe@example.com", Phone = "0987654321" };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            IRepository<int, Patient> _patientRepository = new PatinetRepository(_context);

            // Act
            var result = await _patientRepository.Get(patient.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(patient.Id));
            Assert.That(result.Name, Is.EqualTo("Jane Doe"));
        }

        [Test]
        public async Task GetAllPatients()
        {
            // Arrange
            var patient1 = new Patient { Name = "Patient1", Age = 30, Email = "patient1@example.com", Phone = "1111111111" };
            var patient2 = new Patient { Name = "Patient2", Age = 40, Email = "patient2@example.com", Phone = "2222222222" };
            _context.Patients.AddRange(patient1, patient2);
            await _context.SaveChangesAsync();
            IRepository<int, Patient> _patientRepository = new PatinetRepository(_context);

            // Act
            var result = await _patientRepository.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.Any(p => p.Name == "Patient1"), Is.True);
            Assert.That(result.Any(p => p.Name == "Patient2"), Is.True);
        }

        [Test]
        public async Task UpdatePatient()
        {
            // Arrange
            var patient = new Patient { Name = "Initial Name", Age = 35, Email = "initial@example.com", Phone = "3333333333" };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            IRepository<int, Patient> _patientRepository = new PatinetRepository(_context);

            var updatedPatient = new Patient { Id = patient.Id, Name = "Updated Name", Age = 40, Email = "updated@example.com", Phone = "4444444444" };

            // Act
            var result = await _patientRepository.Update(patient.Id, updatedPatient);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Updated Name"));
            Assert.That(result.Age, Is.EqualTo(40));
            Assert.That(result.Email, Is.EqualTo("updated@example.com"));
        }

        [Test]
        public async Task DeletePatient()
        {
            // Arrange
            var patient = new Patient { Name = "ToDelete", Age = 45, Email = "delete@example.com", Phone = "5555555555" };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            IRepository<int, Patient> _patientRepository = new PatinetRepository(_context);

            // Act
            var result = await _patientRepository.Delete(patient.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(patient.Id));
            Assert.ThrowsAsync<Exception>(async () => await _patientRepository.Get(patient.Id));
        }

        [TearDown]
        public void TearDown()
        {
            _context.Patients.RemoveRange(_context.Patients);
            _context.SaveChanges();
            _context.Dispose();
        }
    }
}
