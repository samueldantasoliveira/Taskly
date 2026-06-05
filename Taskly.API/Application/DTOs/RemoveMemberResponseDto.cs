public class RemoveMemberResponseDto
{
    public Guid TeamId { get; set; }

    public Guid UserId { get; set; }

    public DateTime RemovedAt { get; set; }
}