using System;

class Program
{
    static void Main(string[] args)
    {
        //FileHandler.Run(); // Implemented Singleton and Factory design pattern.
        AdapterExample.Run(); // Adapter

        Console.WriteLine("All tasks completed. Press Enter to exit.");
        Console.ReadLine();
    }
}