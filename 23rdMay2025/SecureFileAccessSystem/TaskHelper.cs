using System;

public class TaskHelper
{
    public static int getValidIntInput(string s = "Enter a number: ")
    {
        do
        {
            Console.Write(s);
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Invalid Input. Try again!");
                continue;
            }
            if (!int.TryParse(input, out int number))
            {
                Console.WriteLine("Invalid Input. Try again!");
                continue;
            }
            return number;
        } while (true);
    }

    public static double getValidDoubleInput(string s = "Enter a double: ")
    {
        do
        {
            Console.Write(s);
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Invalid Input. Try again!");
                continue;
            }
            if (!double.TryParse(input, out double number))
            {
                Console.WriteLine("Invalid Input. Try again!");
                continue;
            }
            return number;
        } while (true);
    }
    public static string getValidString(string s = "Enter a String: ")
    {
        do
        {
            Console.Write(s);
            string? output = Console.ReadLine();
            if (string.IsNullOrEmpty(output))
            {
                Console.WriteLine("Invalid Input. Try again...");
                continue;
            }
            return output;
        } while (true);
    }
}