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
    public class PatientRepository : Repository<int, Patient>
    {
        public PatientRepository() : base()
        {
        }
        public override ICollection<Patient> GetAll()
        {
            if(_items.Count == 0)
            {
                throw new CollectionEmptyException("No patients found");
            }
            return _items;
        }

        public override Patient GetById(int id)
        {
            var Patient = _items.FirstOrDefault(e => e.Id == id);
            if(Patient == null)
            {
                throw new KeyNotFoundException("Patient not found");
            }
            return Patient;
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