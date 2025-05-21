using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentApp.Interfaces;
using AppointmentApp.Models;

namespace AppointmentApp.Services
{
    public class AppointmentService : IAppointmentService
    {
        IRepositor<int, Appointment> _appointmentRepository;
        public AppointmentService(IRepositor<int, Appointment> appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }
        public int AddAppointment(Appointment appointment)
        {
            try
            {
                var result = _appointmentRepository.Add(appointment);
                if (result != null)
                {
                    return result.Id;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return -1;
        }

        public List<Appointment>? SearchAppointments(SearchModel searchModel)
        {
            try
            {
                var appointments = _appointmentRepository.GetAll();
                appointments = SearchById(appointments, searchModel.Id);
                appointments = SearchByName(appointments, searchModel.Name);
                appointments = SearchByAge(appointments, searchModel.Age);
                appointments = SearchByDate(appointments, searchModel.AppointmentDate);
                if(appointments != null && appointments.Count > 0)
                    return appointments.ToList(); ;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        private ICollection<Appointment> SearchByDate(ICollection<Appointment> appointments, DateTime? date)
        {
            if(date == null || appointments==null|| appointments.Count ==0)
            {
                return appointments;
            }
            else
            {
                return appointments.Where(a => a.AppointmentDate.Date == date).ToList();
            }
        }

        private ICollection<Appointment> SearchByAge(ICollection<Appointment> appointments, Range<int>? age)
        {
            if(age == null || appointments == null || appointments.Count == 0)
            {
                return appointments;
            }
            else
            {
                return appointments.Where(a => a.PatientAge >= age.MinVal && a.PatientAge <= age.MaxVal).ToList();
            }
        }

        private ICollection<Appointment> SearchByName(ICollection<Appointment> appointments, string? name)
        {
            if(name == null ||  appointments == null || appointments.Count == 0 )
            {
                return appointments;
            }
            else
            {
                return appointments.Where(a => a.PatientName.ToLower().Contains(name.ToLower())).ToList();
            }
        }

        private ICollection<Appointment> SearchById(ICollection<Appointment> appointments, int? id)
        {
            if (id == null || appointments == null || appointments.Count == 0)
            {
                return appointments;
            }
            else
            {
                return appointments.Where(a => a.Id == id).ToList();
            }
        }
    }
}