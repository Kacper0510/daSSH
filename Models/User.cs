using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using daSSH.Data;

namespace daSSH.Models;

public class User {
    public int UserID { get; set; }
    public long DiscordID { get; set; }
    public DateOnly CreatedOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [StringLength(32, MinimumLength = 2)]
    [RegularExpression(@"^[a-z0-9._]+$")]
    public required string Username { get; set; }
    public required string Avatar { get; set; }
    public required string PublicKey { get; set; }
    public string? APIToken { get; set; }

    public ICollection<Instance> Instances { get; set; } = [];
    public ICollection<Instance> SharedInstances { get; set; } = [];

    public async Task<string> GenerateNewKeyPair() {
        var (pub, priv) = await SSH.RunKeyGen(UserID);
        PublicKey = pub;
        return priv;
    }

    public void GenerateNewToken() {
        byte[] tokenBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        APIToken = "daSSH-" + Convert.ToBase64String(tokenBytes);
    }
}
