using FirstAPI.Models;
using FirstAPI.Models.DTOs;

namespace FirstAPI.Interfaces
{
    public interface IAppointmnetService
    {
        public Task<Appointmnet> AddAppointmnet(AppointmnetDTO appointmnet);

        public Task<Appointmnet> CancelAppointmnet(string appointmentNumber);
    }
}