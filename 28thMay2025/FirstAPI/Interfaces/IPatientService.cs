using FirstAPI.Models;
using FirstAPI.Models.DTOs.Patient;
namespace FirstAPI.Interfaces
{
    public interface IPatientService
    {
        public Task<Patient> AddPatient(PatientAddRequestDto patient);
    }
}