using System;
using System.IO;

namespace SecureFileAccess
{

    class SecureAccess
    {
        public static void Run()
        {
            string path = "SensitiveFile.txt";

            Console.WriteLine();
            string ch = "y";
            
            while (ch.ToLower() == "y")
            {
                string role = TaskHelper.getValidString("Role(Admin/Guest/User):");
                if (role != "Admin" && role != "Guest" && role != "User") continue;
                string name = TaskHelper.getValidString("Name:");
                var user = new User(name, role);
                var proxy = new ProxyFile(path, user);
                Console.WriteLine($"User: {user.Username} | Role: {user.Role}");
                proxy.Read();

                Console.WriteLine();
                ch = TaskHelper.getValidString("Enter 'y' to continue...\n");
            }
        }
    }
}
