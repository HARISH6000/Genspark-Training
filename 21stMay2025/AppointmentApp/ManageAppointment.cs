using System;
using System.Collections.Generic;
using AppointmentApp.Interfaces;
using AppointmentApp.Models;

namespace AppointmentApp
{
    public class ManageAppointment
    {
        private readonly IAppointmentService _appointmentService;

        public ManageAppointment(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public void Start()
        {
            while (true)
            {
                PrintMenu();
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) return; 

                if (!int.TryParse(input, out int option) || option < 1 || option > 2)
                {
                    Console.WriteLine("Invalid option. Please enter 1 or 2, or press Enter to exit.");
                    continue;
                }

                switch (option)
                {
                    case 1:
                        AddAppointment();
                        break;
                    case 2:
                        SearchAppointments();
                        break;
                }
            }
        }

        private void PrintMenu()
        {
            Console.WriteLine("\n--- Appointment Management System ---");
            Console.WriteLine("1. Add Appointment");
            Console.WriteLine("2. Search Appointments");
            Console.WriteLine("Press Enter to exit");
        }

        private void AddAppointment()
        {
            Appointment appointment = new Appointment();
            appointment.TakeAppointmentDetailsFromUser();
            int id = _appointmentService.AddAppointment(appointment);
            Console.WriteLine($"\nAppointment added successfully. ID: {id}");
        }

        private void SearchAppointments()
        {
            SearchModel searchModel = BuildSearchModel();
            var appointments = _appointmentService.SearchAppointments(searchModel);

            Console.WriteLine("\n--- Search Criteria ---");
            Console.WriteLine(searchModel.ToString() ?? "No criteria specified");
            Console.WriteLine("--------------------------");

            if (appointments == null || !appointments.Any())
            {
                Console.WriteLine("No appointments found matching the criteria.");
                return;
            }

            Console.WriteLine("\n--- Matching Appointments ---");
            PrintAppointments(appointments);
        }

        private void PrintAppointments(List<Appointment> appointments)
        {
            foreach (var appointment in appointments)
            {
                Console.WriteLine(appointment);
            }
        }

        private SearchModel BuildSearchModel()
        {
            SearchModel searchModel = new SearchModel();
            Console.WriteLine("\n--- Search Options ---");
            Console.WriteLine("Press Enter to skip any search criterion.\n");

            
            Console.Write("Search by Appointment ID (enter ID or skip): ");
            string? idInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(idInput))
            {
                if (int.TryParse(idInput, out int id) && id > 0)
                {
                    searchModel.Id = id;
                    return searchModel;
                }
                else
                {
                    Console.WriteLine("Invalid ID. Skipping ID search.");
                }
            }

            Console.Write("Search by Patient Name (enter name or skip): ");
            string? nameInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nameInput))
            {
                searchModel.Name = nameInput;
            }

            Console.Write("Search by Age Range? Enter minimum age (or skip): ");
            string? minAgeInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(minAgeInput))
            {
                if (int.TryParse(minAgeInput, out int minAge) && minAge > 0)
                {
                    searchModel.Age = new Range<int> { MinVal = minAge };
                    Console.Write("Enter maximum age (or skip): ");
                    string? maxAgeInput = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(maxAgeInput))
                    {
                        if (int.TryParse(maxAgeInput, out int maxAge) && maxAge >= minAge)
                        {
                            searchModel.Age.MaxVal = maxAge;
                        }
                        else
                        {
                            Console.WriteLine("Invalid maximum age. Skipping age range search.");
                            searchModel.Age = null;
                        }
                    }
                    else
                    {
                        searchModel.Age.MaxVal = 150;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid minimum age. Skipping age range search.");
                }
            }

            Console.Write("Search by Appointment Date (YYYY-MM-DD, or skip): ");
            string? dateInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(dateInput))
            {
                if (DateTime.TryParseExact(dateInput, "yyyy-MM-dd", 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out DateTime searchDate))
                {
                    searchModel.AppointmentDate = searchDate;
                }
                else
                {
                    Console.WriteLine("Invalid date format. Skipping date search.");
                }
            }

            return searchModel;
        }
    }
}