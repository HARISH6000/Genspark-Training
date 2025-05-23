public class User
{
    public string Username { get; }
    public string Role { get; }

    public User(string username, string role)
    {
        Username = username;
        Role = role;
    }
}