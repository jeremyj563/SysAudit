namespace SysAudit.Classes.WMI
{
    public sealed class Win32_Processor
    {
        public uint AddressWidth { get; set; }
        public uint Architecture { get; set; }
        public uint Availability { get; set; }
        public string Caption { get; set; }
        public uint CpuStatus { get; set; }
        public uint CurrentClockSpeed { get; set; }
        public uint CurrentVoltage { get; set; }
        public uint DataWidth { get; set; }
        public string Description { get; set; }
        public string DeviceID { get; set; }
        public string Manufacturer { get; set; }
        public uint MaxClockSpeed { get; set; }
        public string Name { get; set; }
        public uint NumberOfCores { get; set; }
        public bool PowerManagementSupported { get; set; }
        public uint ProcessorType { get; set; }
        public string Status { get; set; }
        public string SystemName { get; set; }
    }
}