namespace SysAudit.Classes.WMI
{
    public sealed class Win32_PhysicalMemory
    {
        public string BankLabel { get; set; }
        public ulong Capacity { get; set; }
        public string Caption { get; set; }
        public uint DataWidth { get; set; }
        public string Description { get; set; }
        public string DeviceLocator { get; set; }
        public ushort FormFactor { get; set; }
        public string Manufacturer { get; set; }
        public ushort MemoryType { get; set; }
        public string Name { get; set; }
        public string PartNumber { get; set; }
        public string SerialNumber { get; set; }
        public uint Speed { get; set; }
        public string Tag { get; set; }
        public uint TotalWidth { get; set; }
        public uint TypeDetail { get; set; }
    }
}