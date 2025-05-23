using System;
using System.IO;
using System.Text.Json;

public class FileManager : IDisposable
{
    private static FileManager _instance;
    private FileStream _fileStream;
    private readonly string _filePath;

    private FileManager(string filePath)
    {
        _filePath = filePath;
        _fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        Console.WriteLine($"File {_filePath} opened.");
    }

    public static FileManager GetInstance(string filePath)
    {
        if (_instance == null)
        {
            _instance = new FileManager(filePath);
        }
        return _instance;
    }

    public FileStream GetFileStream() => _fileStream;

    public void Dispose()
    {
        if (_fileStream != null)
        {
            _fileStream.Close();
            _fileStream.Dispose();
            Console.WriteLine($"File {_filePath} closed.");
            _instance = null;
        }
    }
}

public interface IFileOperation
{
    void Execute(FileStream fileStream);
}

public static class FileOperationFactory
{
    public static IFileOperation CreateFileOperation(string operationType, string data = null)
    {
        return operationType.ToLower() switch
        {
            "textread" => new TextReadOperation(),
            "textwrite" => new TextWriteOperation(data),
            _ => throw new ArgumentException("Unsupported operation type.")
        };
    }
}

public class TextReadOperation : IFileOperation
{
    public void Execute(FileStream fileStream)
    {
        fileStream.Seek(0, SeekOrigin.Begin);
        using (var reader = new StreamReader(fileStream, leaveOpen: true))
        {
            string content = reader.ReadToEnd();
            Console.WriteLine($"Read from text file: {content}");
        }
    }
}

public class TextWriteOperation : IFileOperation
{
    private readonly string _data;

    public TextWriteOperation(string data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public void Execute(FileStream fileStream)
    {
        fileStream.Seek(0, SeekOrigin.Begin);
        using (var writer = new StreamWriter(fileStream, leaveOpen: true))
        {
            writer.Write(_data);
            writer.Flush();
            Console.WriteLine($"Wrote to text file: {_data}");
        }
    }
}


public class FileHandler
{
    public static void Run()
    {
        Console.WriteLine("1. Write\n2.Read\nEnter Choice:");
        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice) || (choice != 1 && choice != 2))
        {
            Console.WriteLine("Invalid input. Enter 1 or 2:");
        }

        string op = choice == 1 ? "textwrite" : "textread";
        string data = "Hello, Text File!";

        using (var fileManager = FileManager.GetInstance("data.txt"))
        {
            IFileOperation operation = FileOperationFactory.CreateFileOperation(op, data);
            operation.Execute(fileManager.GetFileStream());
        }
    }
}

