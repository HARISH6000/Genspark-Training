using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WholeApplication.Models
{
    public class SearchModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public Range<int>? Age { get; set; }
        public Range<double>? Salary { get; set; }

        public override string ToString()
        {
            return "Id : " + Id + "\nName : " + Name + "\nAge : " + Age + "\nSalary : " + Salary;
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