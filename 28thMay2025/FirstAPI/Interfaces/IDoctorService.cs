using FirstAPI.Models;
using FirstAPI.Models.DTOs.DoctorSpecialities;
namespace FirstAPI.Interfaces
{
    public interface IDoctorService
    {
        public Task<Doctor> GetDoctByName(string name);
        public Task<ICollection<Doctor>> GetDoctorsBySpeciality(string speciality);
        public Task<Doctor> AddDoctor(DoctorAddRequestDto doctor);
    }
}