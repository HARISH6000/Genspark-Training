/*

Single Responsibility Principle (SRP):
A class should have only one reason to change, meaning it should encapsulate only one responsibility or functionality.

*/

/*

public class PaymentProcessor
{
    public void ProcessPayment(string payment)
    {
        // Process the order
        Console.WriteLine("Processing payment: " + payment);

        // Log to a file
        File.AppendAllText("log.txt", $"Payment {payment} processed at {DateTime.Now}\n");
    }
}

*/

public static class Logger
{
    public static void Log(string message)
    {
        File.AppendAllText("log.txt", $"{message} at {DateTime.Now}\n");
    }
}

public class PaymentProcessor
{

    public void ProcessPayment(string payment)
    {
        Console.WriteLine("Processing payment: " + payment);
        Logger.Log($"Payment of {payment} processed");
    }
}


class SingleResponsibility
{
    public static void Run()
    {
        PaymentProcessor processor = new PaymentProcessor();
        processor.ProcessPayment("Rs 1500 from Alice to Bob");
    }
}