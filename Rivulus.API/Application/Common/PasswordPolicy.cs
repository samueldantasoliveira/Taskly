namespace Rivulus.Application;

public static class PasswordPolicy
{
    public static bool IsValid(string? password) =>
        !string.IsNullOrWhiteSpace(password) && password.Length is >= 6 and <= 128;
}
