using FirstAPI.Models;
using FirstAPI.Models.DTOs;

namespace FirstAPI.Misc
{
    public class AppointmnetMapper
    {
        public Appointmnet? MapAppointmnetDTOAppointmnet(AppointmnetDTO addRequestDto)
        {
            Appointmnet appointmnet = new();
            appointmnet.PatientId = addRequestDto.PatientId;
            appointmnet.DoctorId = addRequestDto.DoctorId;
            appointmnet.AppointmnetDateTime = addRequestDto.AppointmnetDateTime;
            return appointmnet;
        }
    }
}
