using Core.Application.Authorization.Models;
using Microsoft.AspNetCore.Authorization;

namespace Core.Application.Authorization.Services;

public class AuthorizationPolicies
{
    public const string ViewOrders = "ViewOrdersPolicy";
    public const string ManageOrders = "ManageOrdersPolicy";

    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        options.AddPolicy(ViewOrders, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(GeneralOperationClaims.Admin, GeneralOperationClaims.Manager);
            policy.RequireClaim("Permission", "ViewOrders");
        });

        options.AddPolicy(ManageOrders, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(GeneralOperationClaims.Admin);
            policy.RequireClaim("Permission", "ManageOrders");
        });
    }
}