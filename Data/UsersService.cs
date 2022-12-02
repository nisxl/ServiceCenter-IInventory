using System.Text.Json;

namespace Todo.Data;

public static class UsersService
{
    private const string _seedUsername = "admin";
    private const string _seedPassword = "admin";

    private static void _saveAll(List<User> users)
    {
        string _appDataDirectoryPath = Utils.GetAppDirectoryPath();
        string _appUsersFilePath = Utils.GetAppUsersFilePath();

        if (!Directory.Exists(_appDataDirectoryPath))
        {
            Directory.CreateDirectory(_appDataDirectoryPath);
        }

        var json = JsonSerializer.Serialize(users);
        File.WriteAllText(_appUsersFilePath, json);
    }

    public static List<User> GetAll()
    {
        string _appUsersFilePath = Utils.GetAppUsersFilePath();
        if (!File.Exists(_appUsersFilePath))
        {
            return new List<User>();
        }

        var json = File.ReadAllText(_appUsersFilePath);

        return JsonSerializer.Deserialize<List<User>>(json);
    }

    public static List<User> Create(Guid userId, string username, string password, Role role)
    {
        List<User> users = GetAll();
        bool usernameExists = users.Any(x => x.Username == username);

        if (usernameExists)
        {
            throw new Exception("Username already exists.");
        }

        users.Add(
            new User
            {
                Username = username,
                PasswordHash = Utils.HashSecret(password),
                Role = role,
                CreatedBy = userId
            }
        );
        _saveAll(users);
        return users;
    }

    public static void SeedUsers()
    {
        var users = GetAll().FirstOrDefault(x => x.Role == Role.Admin);

        if (users == null)
        {
            Create(Guid.Empty, _seedUsername, _seedPassword, Role.Admin);
        }
    }

    public static User GetById(Guid id)
    {
        List<User> users = GetAll();
        return users.FirstOrDefault(x => x.Id == id);
    }

    public static List<User> Delete(Guid id)
    {
        List<User> users = GetAll();
        User user = users.FirstOrDefault(x => x.Id == id);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        users.Remove(user);
        _saveAll(users);

        return users;
    }

    public static User Login(string username, string password)
    {
        var loginErrorMessage = "Invalid username or password.";
        List<User> users = GetAll();
        User user = users.FirstOrDefault(x => x.Username == username);

        if (user == null)
        {
            throw new Exception(loginErrorMessage);
        }

        bool passwordIsValid = Utils.VerifyHash(password, user.PasswordHash);

        if (!passwordIsValid)
        {
            throw new Exception(loginErrorMessage);
        }

        return user;
    }

    public static User ChangePassword(Guid id, string currentPassword, string newPassword)
    {
        List<User> users = GetAll();
        User user = users.FirstOrDefault(x => x.Id == id);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        bool passwordIsValid = Utils.VerifyHash(currentPassword, user.PasswordHash);

        if (!passwordIsValid)
        {
            throw new Exception("Incorrect current password.");
        }

        user.PasswordHash = Utils.HashSecret(newPassword);
        _saveAll(users);

        return user;
    }
}
