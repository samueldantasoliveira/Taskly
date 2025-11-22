using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Taskly.Domain.Entities
{
    public class User
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public bool IsActive { get; set; }

        // Stores the password hash along with all parameters needed for verification:
        // algorithm$iterations$saltBase64$hashBase64
        public string PasswordHash { get; private set; }
        public User(string name, string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));
            if (!IsValidEmail(email))
                throw new ArgumentException("Invalid Email format.", nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password cannot be empty.", nameof(passwordHash));
            Id = Guid.NewGuid();
            Name = name;
            Email = email.ToLowerInvariant();
            PasswordHash = passwordHash;
            IsActive = true;
        }
        
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        

    }
}
