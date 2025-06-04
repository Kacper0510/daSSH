namespace daSSH.Models;

public class User {
    public int UserID { get; set; }
    public long DiscordID { get; set; }
    public required string Username { get; set; }
    public required string Avatar { get; set; }
    public required string PublicKey { get; set; }
    public string? APIToken { get; set; }

    public ICollection<Instance> Instances { get; set; } = [];
    public ICollection<Instance> SharedInstances { get; set; } = [];

    public string GenerateNewKeyPair() {
        PublicKey = "pubkey";
        return "privkey";
    }
}
