namespace daSSH.Models;

public class User {
    public int UserID { get; set; }
    public long DiscordID { get; set; }
    public required string PublicKey { get; set; }

    public required ICollection<Instance> Instances { get; set; }
    public required ICollection<Instance> SharedInstances { get; set; }
}
