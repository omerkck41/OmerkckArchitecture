using Core.Application.Authorization.Models;
using Microsoft.AspNetCore.Authorization;

namespace Core.Application.Authorization.Services;

public class AuthorizationPolicies
{
    public const string ViewUsersPolicy = "ViewUsersPolicy";
    public const string ManageUsersPolicy = "ManageUsersPolicy";
    public const string ViewOperationClaimsPolicy = "ViewOperationClaimsPolicy";
    public const string ManageOperationClaimsPolicy = "ManageOperationClaimsPolicy";
    public const string ViewDataPolicy = "ViewDataPolicy";
    public const string ManageDataPolicy = "ManageDataPolicy";

    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        // Ortak policy tanımlama metodu kullanılarak politikalar ekleniyor.
        AddPolicy(options, ViewUsersPolicy, [GeneralOperationClaims.Admin, GeneralOperationClaims.Manager], "ViewUsers");
        AddPolicy(options, ManageUsersPolicy, [GeneralOperationClaims.Admin, GeneralOperationClaims.Manager], "ManageUsers");
        AddPolicy(options, ViewOperationClaimsPolicy, [GeneralOperationClaims.Admin, GeneralOperationClaims.Manager], "ViewOperationClaims");
        AddPolicy(options, ManageOperationClaimsPolicy, [GeneralOperationClaims.Admin, GeneralOperationClaims.Manager], "ManageOperationClaims");
        AddPolicy(options, ViewDataPolicy, [GeneralOperationClaims.Admin, GeneralOperationClaims.Manager], "ViewData");
        AddPolicy(options, ManageDataPolicy, [GeneralOperationClaims.Admin, GeneralOperationClaims.Manager], "ManageData");

    }

    // Ortak policy ekleme metodu
    private static void AddPolicy(AuthorizationOptions options, string policyName, string[] roles, string permission)
    {
        options.AddPolicy(policyName, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(roles);
            policy.RequireClaim("Permission", permission);
        });
    }
}