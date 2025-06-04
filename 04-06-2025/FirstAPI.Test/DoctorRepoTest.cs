using FirstAPI.Contexts;
using FirstAPI.Models;
using FirstAPI.Repositories;
using FirstAPI.Interfaces;
using Microsoft.EntityFrameworkCore;



namespace FirstAPI.Test;

public class Tests
{
    private ClinicContext _context;
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ClinicContext>()
                            .UseInMemoryDatabase("TestDb")
                            .Options;
        _context = new ClinicContext(options);

        // Clear the database before each test
        _context.Doctors.RemoveRange(_context.Doctors);
        _context.SaveChanges();
    }


    [Test]
    public async Task UpdateDoctorTest()
    {
        // Arrange
        var doctor = new Doctor { Name = "Initial", YearsOfExperience = 3, Email = "doctor@example.com" };
        IRepository<int, Doctor> _doctorRepository = new DoctorRepository(_context);
        var addedDoctor = await _doctorRepository.Add(doctor);

        var updatedDoctor = new Doctor { Id = addedDoctor.Id, Name = "Updated", YearsOfExperience = 5, Email = "updated@example.com" };

        // Act
        var result = await _doctorRepository.Update(addedDoctor.Id, updatedDoctor);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Updated"));
        Assert.That(result.YearsOfExperience, Is.EqualTo(5));
    }

    [Test]
    public async Task DeleteDoctorTest()
    {
        // Arrange
        var doctor = new Doctor { Name = "ToDelete", YearsOfExperience = 4, Email = "delete@example.com" };
        IRepository<int, Doctor> _doctorRepository = new DoctorRepository(_context);
        var addedDoctor = await _doctorRepository.Add(doctor);

        // Act
        var deletedDoctor = await _doctorRepository.Delete(addedDoctor.Id);

        // Assert
        Assert.That(deletedDoctor, Is.Not.Null);
        Assert.That(deletedDoctor.Id, Is.EqualTo(addedDoctor.Id));
        Assert.ThrowsAsync<Exception>(async () => await _doctorRepository.Get(addedDoctor.Id));
    }

    [Test]
    public async Task GetAllDoctorsTest()
    {
        // Arrange
        var doctor1 = new Doctor { Name = "Doctor1", YearsOfExperience = 2, Email = "doc1@example.com" };
        var doctor2 = new Doctor { Name = "Doctor2", YearsOfExperience = 3, Email = "doc2@example.com" };

        IRepository<int, Doctor> _doctorRepository = new DoctorRepository(_context);
        await _doctorRepository.Add(doctor1);
        await _doctorRepository.Add(doctor2);

        // Act
        var result = await _doctorRepository.GetAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.Any(d => d.Name == "Doctor1"), Is.True);
        Assert.That(result.Any(d => d.Name == "Doctor2"), Is.True);
    }

    [Test]
    public async Task AddDoctorTest()
    {
        //arrange
        var email = " test1@gmail.com";
        var password = System.Text.Encoding.UTF8.GetBytes("test123");
        var key = Guid.NewGuid().ToByteArray();
        var user = new User
        {
            Username = email,
            Password = password,
            HashKey = key,
            Role = "Doctor"
        };
        _context.Add(user);
        await _context.SaveChangesAsync();
        var doctor = new Doctor
        {
            Name = "test",
            YearsOfExperience = 2,
            Email = email
        };
        IRepository<int, Doctor> _doctorRepository = new DoctorRepository(_context);
        //action
        var result = await _doctorRepository.Add(doctor);
        //assert
        Assert.That(result, Is.Not.Null, "Doctor IS not addeed");
        Assert.That(result.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task Get()
    {
        // Arrange
        var _doctorRepository = new DoctorRepository(_context);
        var doctor = new Doctor { Name = "Doctor1", YearsOfExperience = 5, Email = "doctor1@example.com" };
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _doctorRepository.Get(doctor.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Doctor1"));
        Assert.That(result.Email, Is.EqualTo("doctor1@example.com"));
    }

    [TearDown]
    public void TearDown()
    {
        _context.Doctors.RemoveRange(_context.Doctors);
        _context.SaveChanges();
        _context.Dispose();
    }

}