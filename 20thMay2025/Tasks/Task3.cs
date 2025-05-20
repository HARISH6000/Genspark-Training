//Medium and Hard questions from doc
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq; 

public class Employee : IComparable<Employee>
{
    int id, age;
    string name;
    double salary;

    public Employee(int id, int age, string name, double salary)
    {
        this.id = id;
        this.age = age;
        this.name = name;
        this.salary = salary;
    }

    public override string ToString()
    {
        return $"Employee ID : {id}\nName : {name}\nAge : {age}\nSalary : {salary:F2}";
    }

    public int Id { get => id; set => id = value; }
    public int Age { get => age; set => age = value; }
    public string Name { get => name; set => name = value; }
    public double Salary { get => salary; set => salary = value; }

    public int CompareTo(Employee? other)
    {
        if (other == null) return 1;
        return this.salary.CompareTo(other.salary);
    }
}


public class Task3
{
    static private Dictionary<int, Employee> employeesById=new Dictionary<int, Employee>();

    public static void AddEmployee()
    {
        int empId;
        bool isDuplicateId;

        do
        {
            empId = TaskHelper.getValidIntInput("Please enter the employee ID:");
            isDuplicateId = employeesById.ContainsKey(empId);
            if (isDuplicateId)
            {
                Console.WriteLine($"Error: Employee with ID {empId} already exists. Please enter a unique ID.");
            }
        } while (isDuplicateId);

        string name = TaskHelper.getValidString("Please enter the employee name:");
        int age = TaskHelper.getValidIntInput("Please enter the employee age:");
        double salary = TaskHelper.getValidDoubleInput("Please enter the employee salary:");

        Employee newEmployee = new Employee(empId, age, name, salary);

        employeesById.Add(empId, newEmployee);
        Console.WriteLine($"Employee {newEmployee.Name} added successfully!");
    }

    public static void DisplayAllEmployees()
    {
        if (employeesById.Count == 0)
        {
            Console.WriteLine("\nNo employees registered yet.");
            return;
        }

        Console.WriteLine("\n--- All Employees ---");
        foreach (var entry in employeesById)
        {
            Console.WriteLine(entry.Value);
            Console.WriteLine("---------------------");
        }
    }


    public static List<Employee> GetAllEmployeesAsList()
    {
        return employeesById.Values.ToList();
    }

    public static void SortEmployeesBySalaryAndDisplay()
    {
        if (employeesById.Count == 0)
        {
            Console.WriteLine("\nNo employees to sort.");
            return;
        }

        List<Employee> sortedEmployees = employeesById.Values.ToList();
        sortedEmployees.Sort();

        Console.WriteLine("\n--- Employees Sorted by Salary (Ascending) ---");
        foreach (Employee emp in sortedEmployees)
        {
            Console.WriteLine(emp);
            Console.WriteLine("--------------------------------------------");
        }
    }

    public static void FindAndPrintEmployeeDetailsById()
    {
        int searchId = TaskHelper.getValidIntInput("Enter the Employee ID to find: ");
        if (!employeesById.TryGetValue(searchId, out Employee? foundEmployee))
        {
            Console.WriteLine($"\nEmployee with ID {searchId} not found.");
        }

        if (foundEmployee != null)
        {
            Console.WriteLine("\n--- Employee Found ---");
            Console.WriteLine(foundEmployee);
        }
        else
        {
            Console.WriteLine($"\nEmployee with ID {searchId} not found.");
        }
    }

    public static void FindAndUpdateEmployeeDetailsById()
    {
        int searchId = TaskHelper.getValidIntInput("Enter the Employee ID to find: ");
        if (!employeesById.TryGetValue(searchId, out Employee? foundEmployee))
        {
            Console.WriteLine($"\nEmployee with ID {searchId} not found.");
        }

        if (foundEmployee != null)
        {
            Console.WriteLine("\n--- Employee Found ---");
            Console.WriteLine(foundEmployee);
            Console.WriteLine("\n---Update Employee ---");
            foundEmployee.Name = TaskHelper.getValidString("Please enter the employee name:");
            foundEmployee.Age = TaskHelper.getValidIntInput("Please enter the employee age:");
            foundEmployee.Salary = TaskHelper.getValidDoubleInput("Please enter the employee salary:");

        }
        else
        {
            Console.WriteLine($"\nEmployee with ID {searchId} not found.");
        }
    }

    public static void FindEmployeesByName()
    {
        string searchName = TaskHelper.getValidString("Enter the employee name to search for: ");

        var foundEmployees = employeesById.Values
                                .Where(emp => emp.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase))
                                .ToList();

        if (foundEmployees.Any())
        {
            Console.WriteLine($"\n--- Employees with Name '{searchName}' ---");
            foreach (var emp in foundEmployees)
            {
                Console.WriteLine(emp);
                Console.WriteLine("------------------------------------------");
            }
        }
        else
        {
            Console.WriteLine($"\nNo employees found with the name '{searchName}'.");
        }
    }

    public static void DeleteEmployee()
    {

        int empIdToDelete = TaskHelper.getValidIntInput("Enter the ID of the employee to delete: ");

        if (employeesById.Remove(empIdToDelete))
        {
            Console.WriteLine($"\nEmployee with ID {empIdToDelete} deleted successfully!");
        }
        else
        {
            Console.WriteLine($"\nEmployee with ID {empIdToDelete} not found. No deletion performed.");
        }
    }

    public static void FindEmployeesElderThanGivenEmployee()
    {
        int referenceAge = TaskHelper.getValidIntInput("Please enter the reference age:");


        var elderEmployees = employeesById.Values
                                .Where(emp => emp.Age > referenceAge)
                                .ToList();

        if (elderEmployees.Any())
        {
            Console.WriteLine($"\n--- Employees Elder Than {referenceAge} Years Old ---");
            foreach (var emp in elderEmployees)
            {
                Console.WriteLine(emp);
                Console.WriteLine("-------------------------------------------------");
            }
        }
        else
        {
            Console.WriteLine($"\nNo employees found who are elder than {referenceAge} years old.");
        }
    }

    public static void Run()
    {
        // int num = TaskHelper.getValidIntInput("Number of employees to add:");
        // for (int i = 0; i < num; i++)
        // {
        //     AddEmployee();
        // }

        // SortEmployeesBySalaryAndDisplay();

        // FindAndPrintEmployeeDetailsById();

        // FindEmployeesByName();

        // FindEmployeesElderThanGivenEmployee();

        bool exitApp = false;

        while (!exitApp)
        {
            Console.WriteLine("\n----------------------------------");
            Console.WriteLine("         MAIN MENU                ");
            Console.WriteLine("----------------------------------");
            Console.WriteLine("1. Add New Employee");
            Console.WriteLine("2. Display All Employee Details");
            Console.WriteLine("3. Modify Employee Details (by ID)");
            Console.WriteLine("4. Find Employee Details (by ID)");
            Console.WriteLine("5. Delete Employee (by ID)");
            Console.WriteLine("6. Sort and Display Employees by Salary");
            Console.WriteLine("7. Find Employees by Name"); 
            Console.WriteLine("8. Find Employees Elder Than a Given Age");
            Console.WriteLine("0. Exit Application");
            Console.WriteLine("----------------------------------");

            int choice = TaskHelper.getValidIntInput("Enter your choice: ");

            switch (choice)
            {
                case 1:
                    AddEmployee();
                    break;
                case 2:
                    DisplayAllEmployees();
                    break;
                case 3:
                    FindAndUpdateEmployeeDetailsById();
                    break;
                case 4:
                    FindAndPrintEmployeeDetailsById();
                    break;
                case 5:
                    DeleteEmployee();
                    break;
                case 6:
                    SortEmployeesBySalaryAndDisplay();
                    break;
                case 7:
                    FindEmployeesByName();
                    break;
                case 8:
                    FindEmployeesElderThanGivenEmployee();
                    break;
                case 0:
                    exitApp = true;
                    Console.WriteLine("\nExiting Employee Management System.");
                    break;
                default:
                    Console.WriteLine("\nInvalid choice. Please enter a number from the menu.");
                    break;
            }
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}