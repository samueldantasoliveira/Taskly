using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Taskly.Domain.Exceptions;

namespace Taskly.Domain.Entities
{
    public class Team
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        [BsonRepresentation(BsonType.String)]
        public Guid OwnerId {get; private set; }

        [BsonElement("UserIds")]
        [BsonRepresentation(BsonType.String)]
        private List<Guid> _userIds = new();
        public IReadOnlyCollection<Guid> UserIds => _userIds;

        

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public Team(string name, Guid ownerId)
        {
            Id = Guid.NewGuid();
            OwnerId = ownerId;
            _userIds.Add(ownerId);
            Name = name;
            IsActive = true;
            var now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;
        }
        public void Update(string? name, bool? isActive)
        {
            if (name != null)
                Name = name;

            if (isActive != null)
                IsActive = isActive.Value;

            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveMember(Guid userId)
        {
            if (userId == OwnerId)
            {
                throw new OwnerCannotBeRemovedException(
                    "The team owner cannot be removed."
                );
            }
            if (!UserIds.Contains(userId))
            {
                throw new UserNotMemberException(
                    "Cannot remove a user who is not a member of the team."
                );
            }
                
            _userIds.Remove(userId);
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddMember(Guid userId)
        {

            if (UserIds.Contains(userId))
            {
                throw new UserAlreadyMemberException(
                    "Cannot add a user who is already a member of the team."
                );
            }
                
            _userIds.Add(userId);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
