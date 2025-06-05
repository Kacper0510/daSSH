using System.ComponentModel.DataAnnotations;

namespace daSSH.Models;

public class PortForward {
    public int PortForwardID { get; set; }
    public required Instance Instance { get; set; }
    [Range(2000, ushort.MaxValue)]
    public ushort Port { get; set; }
    public bool Public { get; set; }
}
