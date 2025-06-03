namespace daSSH.Models;

public class InstanceFile {
    public enum AccessType {
        Private,
        Protected,
        Public,
    }

    public int InstanceFileID { get; set; }
    public required string Name { get; set; }
    public long Size { get; set; }
    public required Instance Instance { get; set; }
    public AccessType Access { get; set; }
}
