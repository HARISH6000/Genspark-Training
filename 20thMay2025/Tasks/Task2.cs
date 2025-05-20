//Easy question from doc
using System;
using System.Collections.Generic;
using System.Net;

class EmployeePromotion
{
    private List<string> promotionEligibilityOrder;

    public EmployeePromotion()
    {
        promotionEligibilityOrder = new List<string>();
    }

    public void GetEmployeeNamesInPromotionOrder()
    {
        Console.WriteLine("Please enter the employee names in the order of their eligibility for promotion (Please enter blank to stop)");
        string? employeeName;
        do
        {
            employeeName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(employeeName))
            {
                promotionEligibilityOrder.Add(employeeName);
            }
        } while (!string.IsNullOrWhiteSpace(employeeName));
    }

    public void DisplayPromotionOrder()
    {
        if (promotionEligibilityOrder.Count == 0)
        {
            Console.WriteLine("\nNo employee names were entered for promotion eligibility.");
            return;
        }

        Console.WriteLine("\nEmployee Promotion Order:");
        for (int i = 0; i < promotionEligibilityOrder.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {promotionEligibilityOrder[i]}");
        }
        Console.WriteLine("--------------------------------");
    }

    public int findPositionInPromotionList(string empName)
    {
        return promotionEligibilityOrder.IndexOf(empName);
    }

    public void promoteEveryone()
    {
        promotionEligibilityOrder.Sort();
        Console.WriteLine("\nPromoted employee list: ");
        for (int i = 0; i < promotionEligibilityOrder.Count; i++)
        {
            Console.WriteLine($"{promotionEligibilityOrder[i]}");
        }
        promotionEligibilityOrder.Clear();
    }

    public void OptimizeMemoryUsage()
    {
        Console.WriteLine($"\nThe current size of the collection is {promotionEligibilityOrder.Capacity}");

        promotionEligibilityOrder.TrimExcess();

        Console.WriteLine($"The size after removing the extra space is {promotionEligibilityOrder.Capacity}");
    }
}

class Task2
{
    static EmployeePromotion employeePromotion = new EmployeePromotion();
    public static void Run()
    {
        Console.WriteLine("Task 2");

        employeePromotion.GetEmployeeNamesInPromotionOrder();
        employeePromotion.DisplayPromotionOrder();

        string name = TaskHelper.getValidString("Please enter the name of the employee to check promotion position\n");
        int pos = employeePromotion.findPositionInPromotionList(name) + 1;
        Console.WriteLine($"“{name}” is in the position {pos} for promotion. ");

        employeePromotion.OptimizeMemoryUsage();

        employeePromotion.promoteEveryone();

        
    }
}