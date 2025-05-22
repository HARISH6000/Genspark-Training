// For OCP, ISP and DIP

/*
Interface Segregation Principle (ISP):
The ILogger interface is small and specific, containing only the Log method, ensuring that implementers are not forced to include unrelated functionality.

Open-Closed Principle (OCP):
The OrderProcessor class is open for extension (new loggers like FileLogger, DatabaseLogger) but closed for modification, as adding a new logger requires no changes to its code.

Dependency Inversion Principle (DIP):
The OrderProcessor depends on the abstraction (ILogger) rather than concrete implementations (FileLogger, DatabaseLogger), enabling flexibility and decoupling.
*/

public class OrderProcessor
{
    private readonly ILogger _logger;

    public OrderProcessor(ILogger logger)
    {
        _logger = logger;
    }

    public void ProcessOrder(string order)
    {
        Console.WriteLine("Processing order: " + order);
        _logger.Log($"Order {order} processed");
    }
}


public interface ILogger
{
    void Log(string message);

}

public class FileLogger : ILogger
{
    public void Log(string message)
    {
        File.AppendAllText("log.txt", $"{message} at {DateTime.Now}\n");
    }
}

public class DatabaseLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"Logging to database: {message}");
    }

}

class DependencyInversion
{
    public static void Run()
    {
        ILogger logger = new FileLogger();
        //ILogger logger = new DatabaseLogger();
        OrderProcessor processor = new OrderProcessor(logger);
        processor.ProcessOrder("Order123");
    }
}