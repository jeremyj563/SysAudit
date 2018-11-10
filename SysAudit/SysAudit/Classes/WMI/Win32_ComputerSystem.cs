namespace SysAudit.Classes.WMI
{
    public sealed class Win32_ComputerSystem
    {
        public string DNSHostName { get; set; }
        public string Domain { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Name { get; set; }
        public uint NumberOfProcessors { get; set; }
        public string PrimaryOwnerName { get; set; }
        public string Status { get; set; }
        public string SystemType { get; set; }
        public string UserName { get; set; }
    }
}
