using DemoSwimmApp.Models;

namespace DemoSwimmApp.Services
{
    public interface IUserService
    {
        User? AuthenticateUser(string username, string password);
        User? GetUserById(int id);
        User? GetUserByUsername(string username);
        bool RegisterUser(User user);
        List<User> GetAllUsers();
    }

    public class UserService : IUserService
    {
        private static List<User> _users = new List<User>
        {
            new User { Id = 1, Username = "admin", Password = "admin123", Email = "admin@example.com", FullName = "Администратор" },
            new User { Id = 2, Username = "user", Password = "user123", Email = "user@example.com", FullName = "Пользователь" },
            new User { Id = 3, Username = "test", Password = "test123", Email = "test@example.com", FullName = "Тестовый пользователь" }
        };
        
        private static int _nextId = 4;

        public User? AuthenticateUser(string username, string password)
        {
            return _users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
                u.Password == password);
        }

        public User? GetUserById(int id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public User? GetUserByUsername(string username)
        {
            return _users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public bool RegisterUser(User user)
        {
            if (GetUserByUsername(user.Username) != null)
            {
                return false;
            }

            user.Id = _nextId++;
            user.CreatedAt = DateTime.UtcNow;
            _users.Add(user);
            return true;
        }

        public List<User> GetAllUsers()
        {
            return _users.ToList();
        }
    }
}