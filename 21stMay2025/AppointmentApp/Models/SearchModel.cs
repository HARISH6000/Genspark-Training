using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentApp.Models
{
    public class SearchModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public Range<int>? Age { get; set; }
        public DateTime? AppointmentDate { get; set; }

        public override string ToString()
        {
            return "Id : " + Id + "\nName : " + Name + "\nAge : " + Age + "\nAppointment Date : " + AppointmentDate;
        }

    }
    public class Range<T>
    {
        public T? MinVal { get; set; }
        public T? MaxVal { get; set; }
        public override string ToString()
        {
            return "MinVal : " + MinVal + "\nMaxVal : " + MaxVal;
        }
    }
}