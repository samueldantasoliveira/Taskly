namespace Rivulus.Application.Results;

public static class UserErrors
{
    public readonly static Error NotFound =
        Error.Create("User.NotFound", "User not found");

    public readonly static Error InvalidName =
        Error.Create("User.InvalidName", "Name is invalid");

    public readonly static Error InvalidPassword =
        Error.Create("User.InvalidPassword", "Password must contain between 6 and 128 characters and cannot be blank.");
    public readonly static Error InvalidEmail =
        Error.Create("User.InvalidEmail", "Email is invalid");
    public readonly static Error InvalidAvatar =
        Error.Create("User.InvalidAvatar", "Avatar is invalid");
    public readonly static Error EmailAlreadyExists =
        Error.Create("User.EmailAlreadyExists", "Email already exists");
    public readonly static Error InvalidCredentials =
        Error.Create("User.InvalidCredentials","User Credentials are Invalid");

}
