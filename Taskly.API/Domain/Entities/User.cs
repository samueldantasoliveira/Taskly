using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Taskly.Domain.Entities
{
    public class User
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; set; }

        // Stores the password hash along with all parameters needed for verification:
        // algorithm$iterations$saltBase64$hashBase64
        public string PasswordHash { get; private set; }
        public User(string name, string passwordHash)
        {
            Id = Guid.NewGuid();
            Name = name;
            PasswordHash = passwordHash;
            IsActive = true;
        }
    }
}
