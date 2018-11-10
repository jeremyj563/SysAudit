using System;
using SysAudit.Attributes;

namespace SysAudit.Classes.Domain
{
    public sealed class AuditRecord
    { 
        /*                    Domain Property Name                         Source
          -----------------------------------------------------------------------
        */
        [Order] public DateTime TimeStamp { get; set; }                 // SYSTEM
        [Order] public string UserName { get; set; }                    // SYSTEM
        [Order] public string CommonName { get; set; }                  // AD
        [Order] public string DisplayName { get; set; }                 // AD
        [Order] public string Description { get; set; }                 // AD
        [Order] public string Department { get; set; }                  // AD
        [Order] public string EmailAddress { get; set; }                // AD
        [Order] public string PhoneNumber { get; set; }                 // AD
        [Order] public string LogonServer { get; set; }                 // SYSTEM
        [Order] public string UserDomain { get; set; }                  // SYSTEM
        [Order] public DateTime UserWhenChanged { get; set; }           // AD
        [Order] public DateTime UserWhenCreated { get; set; }           // AD
        [Order] public string ComputerName { get; set; }                // SYSTEM
        [Order] public string ComputerDomain { get; set; }              // WMI
        [Order] public string OsVersion { get; set; }                   // WMI
        [Order] public string SystemType { get; set; }                  // WMI
        [Order] public string NetworkCard { get; set; }                 // WMI
        [Order] public string IpAddress { get; set; }                   // WMI
        [Order] public string SubnetMask { get; set; }                  // WMI
        [Order] public string DefaultGateway { get; set; }              // WMI
        [Order] public string MacAddress { get; set; }                  // WMI
        [Order] public string NetworkSpeed { get; set; }                // WMI
        [Order] public string NetworkType { get; set; }                 // WMI
        [Order] public string DhcpServer { get; set; }                  // WMI
        [Order] public string DnsServer { get; set; }                   // WMI
        [Order] public string Cpu { get; set; }                         // WMI
        [Order] public string Memory { get; set; }                      // WMI
        [Order] public DateTime BootTime { get; set; }                  // WMI
        [Order] public string Volumes { get; set; }                     // WMI
        [Order] public string FreeSpace { get; set; }                   // WMI
        [Order] public DateTime ComputerWhenChanged { get; set; }       // AD
        [Order] public DateTime ComputerWhenCreated { get; set; }       // AD
    }
}
