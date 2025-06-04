using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NUnit.Framework;
using FirstAPI.Interfaces;
using FirstAPI.Models;
using FirstAPI.Models.DTOs.DoctorSpecialities;
using FirstAPI.Services;
using FirstAPI.Misc;

namespace FirstAPI.Tests.Services
{
    [TestFixture]
    public class DoctorServiceTests
    {
        private Mock<IRepository<int, Doctor>> _doctorRepositoryMock;
        private Mock<IRepository<int, Speciality>> _specialityRepositoryMock;
        private Mock<IRepository<int, DoctorSpeciality>> _doctorSpecialityRepositoryMock;
        private Mock<IRepository<string, User>> _userRepositoryMock;
        private Mock<IOtherContextFunctionities> _otherContextFunctionitiesMock;
        private Mock<IEncryptionService> _encryptionServiceMock;
        private Mock<IMapper> _mapperMock;

        private DoctorService _doctorService;

        [SetUp]
        public void Setup()
        {
            _doctorRepositoryMock = new Mock<IRepository<int, Doctor>>();
            _specialityRepositoryMock = new Mock<IRepository<int, Speciality>>();
            _doctorSpecialityRepositoryMock = new Mock<IRepository<int, DoctorSpeciality>>();
            _userRepositoryMock = new Mock<IRepository<string, User>>();
            _otherContextFunctionitiesMock = new Mock<IOtherContextFunctionities>();
            _encryptionServiceMock = new Mock<IEncryptionService>();
            _mapperMock = new Mock<IMapper>();

            _doctorService = new DoctorService(
                _doctorRepositoryMock.Object,
                _specialityRepositoryMock.Object,
                _doctorSpecialityRepositoryMock.Object,
                _userRepositoryMock.Object,
                _otherContextFunctionitiesMock.Object,
                _encryptionServiceMock.Object,
                _mapperMock.Object);
        }

        [Test]
        public async Task AddDoctor()
        {
            // Arrange
            var doctorDto = new DoctorAddRequestDto
            {
                Password = "plainPassword",
                Specialities = new List<SpecialityAddRequestDto>
                {
                    new SpecialityAddRequestDto { Name = "Cardiology" }
                }
            };

            var user = new User();
            var encryptedModel = new EncryptModel
            {
                EncryptedData = System.Text.Encoding.UTF8.GetBytes("encryptedPwd"),
                HashKey = System.Text.Encoding.UTF8.GetBytes("hashKey")
            };

            var doctorEntity = new Doctor { Id = 1 };
            var speciality = new Speciality { Id = 10, Name = "Cardiology" };
            var doctorSpeciality = new DoctorSpeciality();

            _mapperMock.Setup(m => m.Map<DoctorAddRequestDto, User>(doctorDto)).Returns(user);
            _encryptionServiceMock.Setup(e => e.EncryptData(It.IsAny<EncryptModel>())).ReturnsAsync(encryptedModel);
            _userRepositoryMock.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync(user);
            _doctorRepositoryMock.Setup(r => r.Add(It.IsAny<Doctor>())).ReturnsAsync(doctorEntity);
            _specialityRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Speciality>());
            _specialityRepositoryMock.Setup(r => r.Add(It.IsAny<Speciality>())).ReturnsAsync(speciality);
            _doctorSpecialityRepositoryMock.Setup(r => r.Add(It.IsAny<DoctorSpeciality>())).ReturnsAsync(doctorSpeciality);

            // Act
            var result = await _doctorService.AddDoctor(doctorDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(doctorEntity.Id, result.Id);

            _mapperMock.Verify(m => m.Map<DoctorAddRequestDto, User>(doctorDto), Times.Once);
            _encryptionServiceMock.Verify(e => e.EncryptData(It.Is<EncryptModel>(em => em.Data == doctorDto.Password)), Times.Once);
            _userRepositoryMock.Verify(r => r.Add(It.Is<User>(u => u.Password == encryptedModel.EncryptedData && u.HashKey == encryptedModel.HashKey && u.Role == "Doctor")), Times.Once);
            _doctorRepositoryMock.Verify(r => r.Add(It.IsAny<Doctor>()), Times.Once);
            _specialityRepositoryMock.Verify(r => r.Add(It.IsAny<Speciality>()), Times.Once);
            _doctorSpecialityRepositoryMock.Verify(r => r.Add(It.IsAny<DoctorSpeciality>()), Times.Once);
        }

        [Test]
        public async Task GetDoctorById()
        {
            // Arrange
            int doctorId = 5;
            var doctor = new Doctor { Id = doctorId };

            _doctorRepositoryMock.Setup(r => r.Get(doctorId)).ReturnsAsync(doctor);

            // Act
            var result = await _doctorService.GetDoctorById(doctorId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(doctorId, result.Id);
            _doctorRepositoryMock.Verify(r => r.Get(doctorId), Times.Once);
        }

        [Test]
        public void GetDoctorByIdThrowException()
        {
            // Arrange
            int doctorId = 5;
            _doctorRepositoryMock.Setup(r => r.Get(doctorId)).ReturnsAsync((Doctor)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _doctorService.GetDoctorById(doctorId));
            Assert.AreEqual("Could not get doctor", ex.Message);
        }
    }
}
