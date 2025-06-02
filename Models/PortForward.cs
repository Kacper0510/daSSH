namespace daSSH.Models;

public class PortForward {
    public int PortForwardID { get; set; }
    public required Instance Instance { get; set; }
    public ushort Port { get; set; }
    public bool Public { get; set; }
}
