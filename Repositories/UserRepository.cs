using DigitalLibraryApi.Models;

namespace DigitalLibraryApi.Repositories
{
    public class UserRepository
    {
        public static readonly List<User> Users = new()
        {
            new User { Id = 1, Username = "alice", Email = "alice@example.com" },
            new User { Id = 2, Username = "bob", Email = "bob@example.com" },
            new User { Id = 3, Username = "charlie", Email = "charlie@example.com" }
        };
    }
}
