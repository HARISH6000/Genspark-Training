using FirstAPI.Contexts;
using FirstAPI.Models;
using FirstAPI.Repositories;
using FirstAPI.Interfaces;
using FirstAPI.Services;
using FirstAPI.Models.DTOs;
using FirstAPI.Models.DTOs.DoctorSpecialities;
using FirstAPI.Misc;
using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;

namespace FirstAPI.Test;

public class DoctorServiceTest
{
    private ClinicContext _context;
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ClinicContext>()
                            .UseInMemoryDatabase("TestDb")
                            .Options;
        _context = new ClinicContext(options);
    }
    [TestCase("General")]
    public async Task TestGetDoctorBySpeciality(string speciality)
    {
        Mock<DoctorRepository> doctorRepositoryMock = new Mock<DoctorRepository>(_context);
        Mock<SpecialityRepository> specialityRepositoryMock = new(_context);
        Mock<DoctorSpecialityRepository> doctorSpecialityRepositoryMock = new(_context);
        Mock<UserRepository> userRepositoryMock = new(_context);
        Mock<OtherFuncinalitiesImplementation> otherContextFunctionitiesMock = new(_context);
        Mock<EncryptionService> encryptionServiceMock = new();
        Mock<IMapper> mapperMock = new();

        otherContextFunctionitiesMock.Setup(ocf => ocf.GetDoctorsBySpeciality(It.IsAny<string>()))
                                    .ReturnsAsync((string specilaity) => new List<DoctorsBySpecialityResponseDto>{
                                   new DoctorsBySpecialityResponseDto
                                        {
                                            Dname = "test",
                                            Yoe = 2,
                                            Id=1
                                        }
                            });
        IDoctorService doctorService = new DoctorService(doctorRepositoryMock.Object,
                                                        specialityRepositoryMock.Object,
                                                        doctorSpecialityRepositoryMock.Object,
                                                        userRepositoryMock.Object,
                                                        otherContextFunctionitiesMock.Object,
                                                        encryptionServiceMock.Object,
                                                        mapperMock.Object);


        //Assert.That(doctorService, Is.Not.Null);
        //Action
        var result = await doctorService.GetDoctorsBySpeciality(speciality);
        //Assert
        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task TestAddDoctor()
    {
        // Arrange
        var doctorDto = new DoctorAddRequestDto
        {
            Name = "Dr. John",
            Password = "password123",
            Specialities = new List<int> { 1, 2 }
        };

        var user = new User
        {
            Id = 1,
            Name = "Dr. John",
            Password = "encryptedPassword",
            Role = "Doctor",
            HashKey = "hashKey"
        };

        var doctor = new Doctor
        {
            Id = 1,
            Name = "Dr. John"
        };

        var encryptedData = new EncryptModel
        {
            EncryptedData = "encryptedPassword",
            HashKey = "hashKey"
        };

        var speciality = new DoctorSpeciality
        {
            DoctorId = 1,
            SpecialityId = 1
        };

        var encryptionServiceMock = new Mock<IEncryptionService>();
        encryptionServiceMock
            .Setup(es => es.EncryptData(It.IsAny<EncryptModel>()))
            .ReturnsAsync(encryptedData);

        var userRepositoryMock = new Mock<IRepository<string, User>>();
        userRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<User>()))
            .ReturnsAsync(user);

        var doctorRepositoryMock = new Mock<IRepository<int, Doctor>>();
        doctorRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<Doctor>()))
            .ReturnsAsync(doctor);

        var doctorSpecialityRepositoryMock = new Mock<IRepository<int, DoctorSpeciality>>();
        doctorSpecialityRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<DoctorSpeciality>()))
            .ReturnsAsync(speciality);

        var mapperMock = new Mock<IMapper>();
        mapperMock
            .Setup(mapper => mapper.Map<DoctorAddRequestDto, User>(It.IsAny<DoctorAddRequestDto>()))
            .Returns(user);
        mapperMock
            .Setup(mapper => mapper.Map<DoctorAddRequestDto, Doctor>(It.IsAny<DoctorAddRequestDto>()))
            .Returns(doctor);

        var specialityMapperMock = new Mock<ISpecialityMapper>();
        specialityMapperMock
            .Setup(sm => sm.MapDoctorSpecility(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(speciality);

        var doctorService = new DoctorService(
            doctorRepositoryMock.Object,
            Mock.Of<IRepository<int, Speciality>>(), // No interaction tested for Speciality
            doctorSpecialityRepositoryMock.Object,
            userRepositoryMock.Object,
            Mock.Of<IOtherContextFunctionities>(), // No interaction tested
            encryptionServiceMock.Object,
            mapperMock.Object
        );

        // Act
        var result = await doctorService.AddDoctor(doctorDto);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(doctor.Name, result.Name);
        encryptionServiceMock.Verify(es => es.EncryptData(It.IsAny<EncryptModel>()), Times.Once);
        userRepositoryMock.Verify(repo => repo.Add(It.IsAny<User>()), Times.Once);
        doctorRepositoryMock.Verify(repo => repo.Add(It.IsAny<Doctor>()), Times.Once);
        doctorSpecialityRepositoryMock.Verify(repo => repo.Add(It.IsAny<DoctorSpeciality>()), Times.Exactly(doctorDto.Specialities.Count));
    }



    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }


}