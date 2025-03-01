namespace Core.Application.Authorization.Models;

public static class GeneralOperationClaims
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";

    // Add more roles dynamically if needed
    public static readonly List<string> AllRoles = [Admin, Manager, User];


    public const string ViewUsers = "ViewUsers";
    public const string ManageUsers = "ManageUsers";
    public const string ViewOperationClaims = "ViewOperationClaims";
    public const string ManageOperationClaims = "ManageOperationClaims";
    public const string ViewData = "ViewData";
    public const string ManageData = "ManageData";
}