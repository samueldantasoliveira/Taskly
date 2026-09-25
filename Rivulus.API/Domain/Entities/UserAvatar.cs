namespace Rivulus.Domain.Entities;

public static class UserAvatar
{
    public static readonly IReadOnlySet<string> Keys = new HashSet<string>(StringComparer.Ordinal)
    {
        "capybara", "otter", "frog", "duck",
        "turtle", "fish", "heron", "kingfisher"
    };

    public static bool IsValid(string? key) => key != null && Keys.Contains(key);
}
