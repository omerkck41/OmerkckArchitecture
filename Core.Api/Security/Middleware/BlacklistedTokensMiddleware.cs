namespace Core.Api.Security.Middleware;

public class BlacklistedTokensMiddleware
{
    //private readonly RequestDelegate _next;
    //private readonly TokenBlacklistDbContext _dbContext;

    //public BlacklistedTokensMiddleware(RequestDelegate next, TokenBlacklistDbContext dbContext)
    //{
    //    _next = next;
    //    _dbContext = dbContext;
    //}

    //public async Task Invoke(HttpContext context)
    //{
    //    var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    //    if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
    //    {
    //        var token = authorizationHeader.Substring("Bearer ".Length).Trim();
    //        var isBlacklisted = await _dbContext.TokenBlacklist.AnyAsync(t => t.Token == token);
    //        if (isBlacklisted)
    //        {
    //            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    //            await context.Response.WriteAsync("Token has been revoked.");
    //            return;
    //        }
    //    }

    //    await _next(context);
    //}
}
/*
 public class TokenBlacklistDbContext : DbContext
    {
        public TokenBlacklistDbContext(DbContextOptions<TokenBlacklistDbContext> options) : base(options) { }
        public DbSet<TokenBlacklistEntry> TokenBlacklist { get; set; }
    }

    public class TokenBlacklistEntry
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime RevokedAt { get; set; }
    }
 */