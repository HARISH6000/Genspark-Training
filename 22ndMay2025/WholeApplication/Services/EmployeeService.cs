using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholeApplication.Interfaces;
using WholeApplication.Models;

namespace WholeApplication.Services
{
    public class EmployeeService : IEmployeeService
    {
        IRepositor<int, Employee> _employeeRepository;
        public EmployeeService(IRepositor<int, Employee> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        /**
        * Adds a new employee to the repository.
        *
        * @param {Employee} employee - The employee object to be added.
        * @returns {number} - The ID of the newly added employee, or -1 if the employee could not be added.
        * @throws {Error} - If there is an error adding the employee.
        */
        public int AddEmployee(Employee employee)
        {
            try
            {
                var result = _employeeRepository.Add(employee);
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

        /**
        * Searches for employees based on the provided search criteria.
        *
        * @param {SearchModel} searchModel - An object containing the search parameters (Id, Name, Age, Salary).
        * @returns {List<Employee>?} - A list of employees matching the search criteria, or null if no employees are found or an error occurs.
        * @throws {Error} - If there is an error during the search operation.
        */
        public List<Employee>? SearchEmployee(SearchModel searchModel)
        {
            try
            {
                var employees = _employeeRepository.GetAll();
                employees = SearchById(employees, searchModel.Id);
                employees = SearchByName(employees, searchModel.Name);
                employees = SeachByAge(employees, searchModel.Age);
                employees = SearchBySalary(employees, searchModel.Salary);
                if (employees != null && employees.Count > 0)
                    return employees.ToList(); ;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        /**
        * Filters a collection of employees by their salary within a specified range.
        *
        * @param {ICollection<Employee>} employees - The collection of employees to filter.
        * @param {Range<double>?} salary - The minimum and maximum salary values for filtering.
        * @returns {ICollection<Employee>} - A filtered collection of employees whose salaries fall within the specified range.
        */
        private ICollection<Employee> SearchBySalary(ICollection<Employee> employees, Range<double>? salary)
        {
            if (salary == null || employees.Count == 0 || employees == null)
            {
                return employees;
            }
            else
            {
                return employees.Where(e => e.Salary >= salary.MinVal && e.Salary <= salary.MaxVal).ToList();
            }
        }

        /**
        * Filters a collection of employees by their age within a specified range.
        *
        * @param {ICollection<Employee>} employees - The collection of employees to filter.
        * @param {Range<int>?} age - The minimum and maximum age values for filtering.
        * @returns {ICollection<Employee>} - A filtered collection of employees whose ages fall within the specified range.
        */
        private ICollection<Employee> SeachByAge(ICollection<Employee> employees, Range<int>? age)
        {
            if (age == null || employees.Count == 0 || employees == null)
            {
                return employees;
            }
            else
            {
                return employees.Where(e => e.Age >= age.MinVal && e.Age <= age.MaxVal).ToList();
            }
        }

        /**
        * Filters a collection of employees by their name, performing a case-insensitive search.
        *
        * @param {ICollection<Employee>} employees - The collection of employees to filter.
        * @param {string?} name - The name or part of the name to search for.
        * @returns {ICollection<Employee>} - A filtered collection of employees whose names contain the specified string.
        */
        private ICollection<Employee> SearchByName(ICollection<Employee> employees, string? name)
        {
            if (name == null || employees == null || employees.Count == 0)
            {
                return employees;
            }
            else
            {
                return employees.Where(e => e.Name.ToLower().Contains(name.ToLower())).ToList();
            }
        }

        /**
        * Filters a collection of employees by their ID.
        *
        * @param {ICollection<Employee>} employees - The collection of employees to filter.
        * @param {int?} id - The ID of the employee to search for.
        * @returns {ICollection<Employee>} - A filtered collection of employees that match the specified ID.
        */
        private ICollection<Employee> SearchById(ICollection<Employee> employees, int? id)
        {
            if (id == null || employees == null || employees.Count == 0)
            {
                return employees;
            }
            else
            {
                return employees.Where(e => e.Id == id).ToList();
            }
        }
    }
}