namespace SysAudit.Classes.AD
{
    public sealed class user
    { 
        /*            Ldap-Display-Name                        Common-Name
          -----------------------------------------------------------------------------
        */
        public string accountExpires { get; set; }          // Account-Expires
        public string adminCount { get; set; }              // Admin-Count
        public string adminDisplayName { get; set; }        // Admin-Display-Name
        public string allowedAttributes { get; set; }       // Allowed-Attributes
        public string badPasswordTime { get; set; }         // Bad-Password-Time
        public string badPwdCount { get; set; }             // Bad-Pwd-Count
        public string streetAddress { get; set; }           // Address
        public string canonicalName { get; set; }           // Canonical-Name
        public string cn { get; set; }                      // Common-Name
        public string company { get; set; }                 // Company
        public string carLicense { get; set; }              // carLicense
        public string controlAccessRights { get; set; }     // Control-Access-Rights
        public string createTimeStamp { get; set; }         // Create-Time-Stamp
        public string defaultClassStore { get; set; }       // Default-Class-Store
        public string department { get; set; }              // Department
        public string departmentNumber { get; set; }        // departmentNumber
        public string description { get; set; }             // Description
        public string desktopProfile { get; set; }          // Desktop-Profile
        public string displayName { get; set; }             // Display-Name
        public string displayNamePrintable { get; set; }    // Display-Name-Printable
        public string division { get; set; }                // Division
        public string mail { get; set; }                    // E-mail-Addresses
        public string employeeID { get; set; }              // Employee-ID
        public string employeeNumber { get; set; }          // Employee-Number
        public string employeeType { get; set; }            // Employee-Type
        public byte groupMembershipSAM { get; set; }        // Group-Membership-SAM
        public string groupPriority { get; set; }           // Group-Priority
        public string homeDirectory { get; set; }           // Home-Directory
        public string homeDrive { get; set; }               // Home-Drive
        public string initials { get; set; }                // Initials
        public string isPrivilegeHolder { get; set; }       // Is-Privilege-Holder
        public string lastLogoff { get; set; }              // Last-Logoff
        public string lastLogon { get; set; }               // Last-Logon
        public string lastLogonTimestamp { get; set; }      // Last-Logon-Timestamp
        public string lockoutTime { get; set; }             // Lockout-Time
        public string logonCount { get; set; }              // Logon-Count
        public byte logonHours { get; set; }                // Logon-Hours
        public string managedObjects { get; set; }          // Managed-Objects
        public string manager { get; set; }                 // Manager
        public string masteredBy { get; set; }              // Mastered-By
        public string maxStorage { get; set; }              // Max-Storage
        public string modifyTimeStamp { get; set; }         // Modify-Time-Stamp
        public string distinguishedName { get; set; }       // Obj-Dist-Name
        public string objectCategory { get; set; }          // Object-Category
        public string objectClass { get; set; }             // Object-Class
        public string objectGUID { get; set; }              // Object-Guid
        public string objectVersion { get; set; }           // Object-Version
        public string operatorCount { get; set; }           // Operator-Count
        public string ou { get; set; }                      // Organizational-Unit-Name
        public string o { get; set; }                       // Organization-Name
        public string otherLoginWorkstations { get; set; }  // Other-Login-Workstations
        public string otherMailbox { get; set; }            // Other-Mailbox
        public string middleName { get; set; }              // Other-Name
        public string otherWellKnownObjects { get; set; }   // Other-Well-Known-Objects
        public string personalTitle { get; set; }           // Personal-Title
        public string mobile { get; set; }                  // Phone-Mobile-Primary
        public string otherMobile { get; set; }             // Phone-Mobile-Other
        public string postalAddress { get; set; }           // Postal-Address
        public string postalCode { get; set; }              // Postal-Code
        public string profilePath { get; set; }             // Profile-Path
        public string pwdLastSet { get; set; }              // Pwd-Last-Set
        public string name { get; set; }                    // RDN (Relative Distinguished Name)
        public byte registeredAddress { get; set; }         // Registered-Address
        public string roomNumber { get; set; }              // roomNumber
        public string sAMAccountName { get; set; }          // SAM-Account-Name
        public string sAMAccountType { get; set; }          // SAM-Account-Type
        public string scriptPath { get; set; }              // Script-Path
        public string servicePrincipalName { get; set; }    // Service-Principal-Name
        public string sn { get; set; }                      // Surname
        public string street { get; set; }                  // Street-Address
        public string telephoneNumber { get; set; }         // Telephone-Number
        public string terminalServer { get; set; }          // Terminal-Server
        public string uid { get; set; }                     // uid
        public string userAccountControl { get; set; }      // User-Account-Control
        public string userParameters { get; set; }          // User-Parameters
        public string userPrincipalName { get; set; }       // User-Principal-Name
        public string userSharedFolder { get; set; }        // User-Shared-Folder
        public string userSharedFolderOther { get; set; }   // User-Shared-Folder-Other
        public string userWorkstations { get; set; }        // User-Workstations
        public string wbemPath { get; set; }                // Wbem-Path
        public string whenChanged { get; set; }             // When-Changed
        public string whenCreated { get; set; }             // When-Created
    }
}