namespace SysAudit.Classes.WMI
{
    public sealed class Win32_NetworkAdapterConfiguration
    {
        public string Caption { get; set; }
        public string[] DefaultIpGateway { get; set; }
        public string Description { get; set; }
        public bool DHCPEnabled { get; set; }
        public string DHCPServer { get; set; }
        public string DNSDomain { get; set; }
        public string[] DNSDomainSuffixSearchOrder { get; set; }
        public bool DNSEnabledForWINSResolution { get; set; }
        public string DNSHostName { get; set; }
        public string[] DNSServerSearchOrder { get; set; }
        public bool DomainDNSRegistrationEnabled { get; set; }
        public bool FullDNSRegistrationEnabled { get; set; }
        public ushort[] GatewayCostMetric { get; set; }
        public uint Index { get; set; }
        public uint InterfaceIndex { get; set; }
        public string[] IPAddress { get; set; }
        public uint IPConnectionMetric { get; set; }
        public bool IPEnabled { get; set; }
        public bool IPFilterSecurityEnabled { get; set; }
        public string[] IPSubnet { get; set; }
        public string MACAddress { get; set; }
        public string ServiceName { get; set; }
        public string SettingID { get; set; }
        public bool WINSEnableLMHostsLookup { get; set; }
    }
}