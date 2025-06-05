using System.Threading.Tasks;
using AutoMapper;
using FirstAPI.Interfaces;
using FirstAPI.Misc;
using FirstAPI.Models;
using FirstAPI.Models.DTOs;


namespace FirstAPI.Services
{
    public class AppointmnetService : IAppointmnetService
    {
        AppointmnetMapper appointmnetMapper;
        private readonly IRepository<string, Appointmnet> _appointmnetRepository;

        public AppointmnetService(IRepository<string, Appointmnet> appointmnetRepository,
                            IMapper mapper)
        {
            appointmnetMapper = new AppointmnetMapper();
            _appointmnetRepository = appointmnetRepository;
        }

        public async Task<Appointmnet> AddAppointmnet(AppointmnetDTO appointmnet)
        {
            try
            {
                var newAppointmnet = appointmnetMapper.MapAppointmnetDTOAppointmnet(appointmnet);
                newAppointmnet.Status = "Scheduled";
                newAppointmnet.AppointmnetNumber = Guid.NewGuid().ToString();
                newAppointmnet = await _appointmnetRepository.Add(newAppointmnet);
                if (newAppointmnet == null)
                    throw new Exception("Could not add appointmnet");
                return newAppointmnet;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
        
        public async Task<Appointmnet> CancelAppointmnet(string appointmnetNumber)
        {
            try
            {
                var appointmnet = await _appointmnetRepository.Get(appointmnetNumber);
                appointmnet.Status = "Cancelled";
                appointmnet = await _appointmnetRepository.Update(appointmnetNumber,appointmnet);
                if (appointmnet == null)
                    throw new Exception("Could not add appointmnet");
                return appointmnet;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            
        }
    }
}