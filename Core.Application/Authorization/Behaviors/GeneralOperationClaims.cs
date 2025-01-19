namespace Core.Application.Authorization.Behaviors;

public static class GeneralOperationClaims
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";

    // Add more roles dynamically if needed
    public static readonly List<string> AllRoles = [Admin, Manager, User];
}