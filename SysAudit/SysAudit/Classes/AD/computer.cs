namespace SysAudit.Classes.AD
{
    public sealed class computer
    {
        /*            Ldap-Display-Name                            Common-Name
          ------------------------------------------------------------------------------------------
        */
        public string adminDescription { get; set; }            // Admin-Description
        public string adminDisplayName { get; set; }            // Admin-Display-Name
        public string allowedAttributes { get; set; }           // Allowed-Attributes
        public string canonicalName { get; set; }               // Canonical-Name
        public string catalogs { get; set; }                    // Catalogs
        public string cn { get; set; }                          // Common-Name
        public string defaultLocalPolicyObject { get; set; }    // Default-Local-Policy-Object
        public string description { get; set; }                 // Description
        public string displayName { get; set; }                 // Display-Name
        public string displayNamePrintable { get; set; }        // Display-Name-Printable
        public string dNSHostName { get; set; }                 // DNS-Host-Name
        public string extensionName { get; set; }               // Extension-Name
        public int flags { get; set; }                          // Flags
        public int instanctType { get; set; }                   // Instance-Type
        public string isCriticalSystemObject { get; set; }      // Is-Critical-System-Object
        public string isDeleted { get; set; }                   // Is-Deleted
        public string isPrivilegeHolder { get; set; }           // Is-Privilege-Holder
        public string lastKnownParent { get; set; }             // Last-Known-Parent
        public string managedBy { get; set; }                   // Managed-By
        public string managedObjects { get; set; }              // Managed-Objects
        public string masteredBy { get; set; }                  // Mastered-By
        public string netbootGUID { get; set; }                 // Netboot-GUID
        public string networkAddress { get; set; }              // Network-Address
        public string nTSecurityDescriptor { get; set; }        // NT-Security-Descriptor
        public string localPolicyFlags { get; set; }            // Local-Policy-Flags
        public string location { get; set; }                    // Location
        public int machineRole { get; set; }                    // Machine-Role
        public string modifyTimeStamp { get; set; }             // Modify-Time-Stamp
        public string distinguishedName { get; set; }           // Obj-Dist-Name
        public string objectCategory { get; set; }              // Object-Category
        public string objectClass { get; set; }                 // Object-Class
        public string objectGUID { get; set; }                  // Object-Guid
        public string objectVersion { get; set; }               // Object-Version
        public string operatingSystem { get; set; }             // Operating-System
        public string operatingSystemHotfix { get; set; }       // Operating-System-Hotfix
        public string operatingSystemServicePack { get; set; }  // Operating-System-Service-Pack
        public string operatingSystemVersion { get; set; }      // Operating-System-Version
        public string o { get; set; }                           // Organization-Name
        public string physicalLocationObject { get; set; }      // Physical-Location-Object
        public string policyReplicationFlags { get; set; }      // Policy-Replication-Flags
        public string possibleInferiors { get; set; }           // Possible-Inferiors
        public string proxiedObjectName { get; set; }           // Proxied-Object-Name
        public string proxyAddresses { get; set; }              // Proxy-Addresses
        public string queryPolicyBL { get; set; }               // Query-Policy-BL
        public string name { get; set; }                        // RDN (Relative Distinguished Name)
        public string revision { get; set; }                    // Revision
        public byte siteGUID { get; set; }                      // Site-GUID
        public int systemFlags { get; set; }                    // System-Flags
        public int volumeCount { get; set; }                    // Volume-Count
        public string wbemPath { get; set; }                    // Wbem-Path
        public string wellKnownObjects { get; set; }            // Well-Known-Objects
        public string whenChanged { get; set; }                 // When-Changed
        public string whenCreated { get; set; }                 // When-Created
    }
}
