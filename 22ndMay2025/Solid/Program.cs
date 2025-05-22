using System;

class Program
{
    static void Main(string[] args)
    {
        //SingleResponsibility.Run(); //For SRP
        DependencyInversion.Run(); // For OCP,ISP,DIP
        //LiskovSubstitutionPrinciple.Run(); //FOr LSP

        Console.WriteLine("All tasks completed. Press Enter to exit.");
        Console.ReadLine();
    }
}