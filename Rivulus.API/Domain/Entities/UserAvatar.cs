namespace Rivulus.Domain.Entities;

public static class UserAvatar
{
    public static readonly IReadOnlySet<string> Keys = new HashSet<string>(StringComparer.Ordinal)
    {
        "capybara", "otter", "duck",
        "turtle", "fish", "kingfisher"
    };

    public static bool IsValid(string? key) => key != null && Keys.Contains(key);
}
