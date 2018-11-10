namespace SysAudit.Classes.WMI
{
    public sealed class Win32_OperatingSystem
    {
        public string BootDevice { get; set; }
        public string BuildNumber { get; set; }
        public string BuildType { get; set; }
        public string Caption { get; set; }
        public string CSName { get; set; }
        public uint EncryptionLevel { get; set; }
        public string InstallDate { get; set; }
        public string LastBootUpTime { get; set; }
        public string LocalDateTime { get; set; }
        public string Manufacturer { get; set; }
        public uint NumberOfUsers { get; set; }
        public string OSArchitecture { get; set; }
        public uint OSType { get; set; }
        public bool Primary { get; set; }
        public uint ProductType { get; set; }
        public string SerialNumber { get; set; }
        public string Status { get; set; }
        public string SystemDevice { get; set; }
        public string SystemDirectory { get; set; }
        public string SystemDrive { get; set; }
        public string Version { get; set; }
        public string WindowsDirectory { get; set; }
    }
}