using System.ComponentModel.DataAnnotations;

namespace daSSH.Models;

public class Instance {
    public int InstanceID { get; set; }
    [StringLength(30, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z0-9_ ]+$")]
    public required string Name { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public required User Owner { get; set; }
    public PortForward? PortForward { get; set; }

    public ICollection<User> SharedUsers { get; set; } = [];
    public ICollection<InstanceFile> Files { get; set; } = [];
}
