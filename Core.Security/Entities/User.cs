using Core.Persistence.Entities;
using Core.Security.Enums;
using Core.Security.JWT;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Security.Entities;

public class User<TId> : Entity<TId>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public byte[] PasswordSalt { get; set; }
    public byte[] PasswordHash { get; set; }
    public bool Status { get; set; }
    public AuthenticatorType AuthenticatorType { get; set; }


    [NotMapped]
    public virtual ICollection<RefreshToken<TId, TId>> RefreshTokens { get; set; }

    [NotMapped]
    public virtual ICollection<UserOperationClaim<TId, TId, TId>> UserOperationClaims { get; set; }


    public User()
    {
        RefreshTokens = new List<RefreshToken<TId, TId>>();
        UserOperationClaims = new List<UserOperationClaim<TId, TId, TId>>();

        Email = string.Empty;
        PasswordHash = Array.Empty<byte>();
        PasswordSalt = Array.Empty<byte>();
    }

    public User(string firstName, string lastName, string email, byte[] passwordSalt, byte[] passwordHash,
                bool status, AuthenticatorType authenticatorType)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordSalt = passwordSalt;
        PasswordHash = passwordHash;
        Status = status;
        AuthenticatorType = authenticatorType;
    }

    public User(TId id, string firstName, string lastName, string email, byte[] passwordSalt, byte[] passwordHash,
                bool status, AuthenticatorType authenticatorType) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordSalt = passwordSalt;
        PasswordHash = passwordHash;
        Status = status;
        AuthenticatorType = authenticatorType;
    }
}