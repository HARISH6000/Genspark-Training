using FirstAPI.Models;
using FirstAPI.Models.DTOs.DoctorSpecialities;

namespace FirstAPI.Misc
{
    public class PatientMapper
    {
        public Patient? MapPatientAddRequestPatient(PatientAddRequestDto addRequestDto)
        {
            Patient patient = new();
            patient.Name = addRequestDto.Name;
            patient.Age = addRequestDto.Age;
            patient.Email = addRequestDto.Email;
            patient.Phone = addRequestDto.Phone;
            return patient;
        }
    }
}