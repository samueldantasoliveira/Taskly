public class UpdateTeamDto
{
    public long? Version { get; set; }
    public Guid? OwnerId { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}
