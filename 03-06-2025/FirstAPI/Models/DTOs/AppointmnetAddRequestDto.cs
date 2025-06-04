using System;

namespace FirstAPI.Models.DTOs
{
    public class AppointmnetDTO
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmnetDateTime { get; set; }
    }
}
