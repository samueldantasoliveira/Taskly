using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Taskly.Domain.Entities
{
    public class User
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public User(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            IsActive = true;
        }
    }
}
