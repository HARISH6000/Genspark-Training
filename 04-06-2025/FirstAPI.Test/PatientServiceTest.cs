using System;
using System.Threading.Tasks;
using AutoMapper;
using FirstAPI.Interfaces;
using FirstAPI.Models;
using FirstAPI.Models.DTOs.DoctorSpecialities;
using FirstAPI.Services;
using Moq;
using NUnit.Framework;

namespace FirstAPI.Test
{
    public class PatientServiceTest
    {
        private Mock<IRepository<int, Patient>> _patientRepositoryMock;
        private Mock<IRepository<string, User>> _userRepositoryMock;
        private Mock<IOtherContextFunctionities> _otherContextFunctionitiesMock;
        private Mock<IEncryptionService> _encryptionServiceMock;
        private Mock<IMapper> _mapperMock;
        private PatientService _patientService;

        [SetUp]
        public void Setup()
        {
            _patientRepositoryMock = new Mock<IRepository<int, Patient>>();
            _userRepositoryMock = new Mock<IRepository<string, User>>();
            _otherContextFunctionitiesMock = new Mock<IOtherContextFunctionities>();
            _encryptionServiceMock = new Mock<IEncryptionService>();
            _mapperMock = new Mock<IMapper>();

            _patientService = new PatientService(
                _patientRepositoryMock.Object,
                _userRepositoryMock.Object,
                _otherContextFunctionitiesMock.Object,
                _encryptionServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Test]
        public async Task AddPatient()
        {
            // Arrange
            var patientDto = new PatientAddRequestDto
            {
                Password = "plainPassword",
                // Add other properties as needed
            };

            var user = new User();
            var encryptedModel = new EncryptModel
            {
                EncryptedData = System.Text.Encoding.UTF8.GetBytes("encryptedPwd"),
                HashKey = System.Text.Encoding.UTF8.GetBytes("hashKey")
            };

            var patientEntity = new Patient { Id = 1 };

            _mapperMock.Setup(m => m.Map<PatientAddRequestDto, User>(patientDto)).Returns(user);
            _encryptionServiceMock.Setup(e => e.EncryptData(It.IsAny<EncryptModel>())).ReturnsAsync(encryptedModel);
            _userRepositoryMock.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync(user);
            _patientRepositoryMock.Setup(r => r.Add(It.IsAny<Patient>())).ReturnsAsync(patientEntity);

            // Act
            var result = await _patientService.AddPatient(patientDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(patientEntity.Id, result.Id);

            _mapperMock.Verify(m => m.Map<PatientAddRequestDto, User>(patientDto), Times.Once);
            _encryptionServiceMock.Verify(e => e.EncryptData(It.Is<EncryptModel>(em => em.Data == patientDto.Password)), Times.Once);
            _userRepositoryMock.Verify(r => r.Add(It.Is<User>(u =>
                u.Password == encryptedModel.EncryptedData &&
                u.HashKey == encryptedModel.HashKey &&
                u.Role == "Patient"
            )), Times.Once);
            _patientRepositoryMock.Verify(r => r.Add(It.IsAny<Patient>()), Times.Once);
        }

        
    }
}
