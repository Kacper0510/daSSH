namespace daSSH.Models;

public class Instance {
    public int InstanceID { get; set; }
    public required string Name { get; set; }
    public required User Owner { get; set; }
    public PortForward? PortForward { get; set; }

    public required ICollection<User> SharedUsers { get; set; }
    public required ICollection<InstanceFile> Files { get; set; }
}
