using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstAPI.Exceptions;
using FirstAPI.Models;
using FirstAPI.Interfaces;

namespace FirstAPI.Repositories
{
    public class AppointmentRepository : Repository<int, Appointment>
    {
        public AppointmentRepository() : base()
        {
        }
        public override ICollection<Appointment> GetAll()
        {
            if(_items.Count == 0)
            {
                throw new CollectionEmptyException("No appointments found");
            }
            return _items;
        }

        public override Appointment GetById(int id)
        {
            var appointment = _items.FirstOrDefault(e => e.Id == id);
            if(appointment == null)
            {
                throw new KeyNotFoundException("Appointment not found");
            }
            return appointment;
        }

        protected override int GenerateID()
        {
            if(_items.Count == 0)
            {
                return 101;
            }
            else
            {
                return _items.Max(e => e.Id) + 1; 
            }
        }
    }
   
}