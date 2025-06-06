using System.ComponentModel.DataAnnotations;

namespace daSSH.Models;

public class InstanceApiModel {
    [StringLength(30, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z0-9_ ]+$")]
    public required string Name { get; set; }
    [Range(2000, ushort.MaxValue)]
    public ushort? Port { get; set; }
    public bool? PublicPort { get; set; }
}
