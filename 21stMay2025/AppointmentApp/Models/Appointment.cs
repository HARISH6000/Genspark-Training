using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int PatientAge { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; } = string.Empty;

        public Appointment()
        {
            PatientName = string.Empty;
            Reason = string.Empty;
        }

        public Appointment(int id, string patientName, int patientAge, DateTime appointmentDate, string reason)
        {
            Id = id;
            PatientName = patientName;
            PatientAge = patientAge;
            AppointmentDate = appointmentDate;
            Reason = reason;
        }

        public override string ToString()
        {
            return $"Appointment ID: {Id}\nPatient Name: {PatientName}\nPatient Age: {PatientAge}\nDate and Time: {AppointmentDate}\nReason: {Reason}";
        }

        public void TakeAppointmentDetailsFromUser()
        {
            Console.WriteLine("\nPlease enter the patient name");
            PatientName = Console.ReadLine() ?? "";
            Console.WriteLine("Please enter the patient age");
            int age;
            while (!int.TryParse(Console.ReadLine(), out age) || age <= 0)
            {
                Console.WriteLine("Invalid entry for age. Please enter a valid patient age");
            }
            PatientAge = age;
            Console.WriteLine("Please enter the date and time (YYYY-MM-DD HH:MM AM/PM)");
            DateTime dateTime;
            while (!DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd hh:mm tt",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dateTime))
            {
                Console.WriteLine("Invalid date and time format. Please enter in YYYY-MM-DD hh:MM AM/PM format");
            }
            AppointmentDate = dateTime;

            Console.WriteLine("Please enter the reason");
            Reason = Console.ReadLine() ?? "";
        }

    }
}