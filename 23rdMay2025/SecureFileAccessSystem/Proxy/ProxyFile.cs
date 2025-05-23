public class ProxyFile : IFile
{
    private readonly File _realFile;
    private readonly User _user;

    private readonly FileInfo _fileInfo;

    public ProxyFile(string path, User user)
    {
        _realFile = new File(path);
        _user = user;
        _fileInfo = new FileInfo(path);
    }

    public void Read()
    {
        switch (_user.Role)
        {
            case "Admin":
                _realFile.Read();
                break;
            case "User":
                Console.WriteLine("[Access Granted] You have limited access: Metadata only.");
                Console.WriteLine("File Metadata:\n" + $" Name: {_fileInfo.Name}\n" +
                               $"  Path: {_fileInfo.FullName}\n" +
                               $"  Size: {_fileInfo.Length} bytes\n" +
                               $"  Creation Time: {_fileInfo.CreationTime}\n" +
                               $"  Last Write Time: {_fileInfo.LastWriteTime}\n");
                break;
            case "Guest":
                Console.WriteLine("[Access Denied] You do not have permission to read this file.");
                break;
            default:
                Console.WriteLine("[Access Denied] Unknown role.");
                break;
        }
    }
}