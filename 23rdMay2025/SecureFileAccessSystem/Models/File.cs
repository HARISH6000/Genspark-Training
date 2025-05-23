public class File : IFile
{
    private readonly string _filePath;

    public File(string filePath)
    {
        _filePath = filePath;
    }

    public void Read()
    {
        if (System.IO.File.Exists(_filePath))
        {
            string content = System.IO.File.ReadAllText(_filePath);
            Console.WriteLine("[Access Granted] Reading sensitive file content...");
            Console.WriteLine($"File Content: {content}");
        }
        else
        {
            Console.WriteLine("File not found.");
        }
    }
}
