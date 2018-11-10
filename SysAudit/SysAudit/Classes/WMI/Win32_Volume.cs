namespace SysAudit.Classes.WMI
{
    public sealed class Win32_Volume
    {
        public bool BootVolume { get; set; }
        public ulong Capacity { get; set; }
        public string Caption { get; set; }
        public bool Compressed { get; set; }
        public string DeviceID { get; set; }
        public bool DirtyBitSet { get; set; }
        public string DriveLetter { get; set; }
        public uint DriveType { get; set; }
        public string FileSystem { get; set; }
        public ulong FreeSpace { get; set; }
        public bool IndexingEnabled { get; set; }
        public string Label { get; set; }
        public uint MaximumFileNameLength { get; set; }
        public string Name { get; set; }
        public bool PageFilePresent { get; set; }
        public bool QuotasEnabled { get; set; }
        public bool QuotasIncomplete { get; set; }
        public bool QuotasRebuilding { get; set; }
        public ulong SerialNumber { get; set; }
        public bool SupportsDiskQuotas { get; set; }
        public bool SupportsFileBasedCompression { get; set; }
        public string SystemName { get; set; }
        public bool SystemVolume { get; set; }
    }
}