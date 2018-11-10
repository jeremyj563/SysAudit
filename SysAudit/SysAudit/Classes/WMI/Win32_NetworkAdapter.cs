namespace SysAudit.Classes.WMI
{
    public sealed class Win32_NetworkAdapter
    {
        public string AdapterType { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string DeviceID { get; set; }
        public string GUID { get; set; }
        public uint Index { get; set; }
        public bool Installed { get; set; }
        public uint InterfaceIndex { get; set; }
        public string MACAddress { get; set; }
        public string Manufacturer { get; set; }
        public string Name { get; set; }
        public string NetConnectionID { get; set; }
        public ushort NetConnectionStatus { get; set; }
        public bool NetEnabled { get; set; }
        public bool PhysicalAdapter { get; set; }
        public string PNPDeviceID { get; set; }
        public string ProductName { get; set; }
        public string ServiceName { get; set; }
        public ulong Speed { get; set; }
        public string SystemName { get; set; }
        public string TimeOfLastReset { get; set; }
    }
}