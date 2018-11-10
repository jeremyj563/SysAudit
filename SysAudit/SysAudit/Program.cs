// Application Name: SysAudit
//           Author: Jeremy Johnson
//             Date: 11/07/2018
//      Description: Creates and posts a system audit file to SQL
//
//     Dependencies: Apache log4net (https://www.nuget.org/packages/log4net/)
//
//       Exit Codes: 0 - No errors
//                   01 - Error logging
//                   02 - User failed to provide audit file path
//                   03 - Error creating audit file folder
//                   04 - Error accessing audit file folder
//                   05 - Error writing to audit file
//                   06 - Audit file was not created
//                   07 - Error posting audit file to SQL
//                   08 - User failed to run post as SYSTEM
//                   09 - Error accessing audit file during post
//                   10 - Error parsing audit file during post
//                   11 - Audit file not found during post

using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Management;
using SysAudit.Properties;
using SysAudit.Classes.AD;
using SysAudit.Classes.Domain;
using SysAudit.Classes.WMI;
using SysAudit.Controllers;
using SysAudit.Enums.System;
using SysAudit.Structs.Domain;
using SysAudit.Structs.WMI;
using Microsoft.VisualBasic;
using log4net.Config;
using System.Collections.Generic;

namespace SysAudit
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            XmlConfigurator.Configure(); // Initialize log4net

            // *******************
            // *** HANDLE ARGS ***
            // *******************
            string auditFilePath = null;
            string[] commandLineArgs = Environment.GetCommandLineArgs();

            if (commandLineArgs.Length == 3 && (commandLineArgs[1].ToUpper() == CommandLineOptions.POST | commandLineArgs[1].ToUpper() == CommandLineOptions.AUDIT))
                auditFilePath = commandLineArgs[2];
            else if (commandLineArgs.Length == 2 && (commandLineArgs[1].ToUpper() == CommandLineOptions.POST | commandLineArgs[1].ToUpper() == CommandLineOptions.AUDIT))
                Global.ExitApp(2, "Please specify the audit file location");
            else
                DisplayConsoleHelp();

            Global.LogEvent(string.Format("Currently logged in as: {0}", Environment.UserName), Global.LogEvents.Info, logToLog4Net: false);

            // ***********************
            // *** POST AUDIT FILE ***
            // ***********************
            if (Environment.UserName == "SYSTEM" && commandLineArgs.Length == 3 && commandLineArgs[1].ToUpper() == CommandLineOptions.POST)
            {
                if (PostAuditFile(auditFilePath))
                    Global.LogEvent("Finished posting audit record", Global.LogEvents.Info, logToLog4Net: true);
                else
                    Global.ExitApp(7, string.Format("ERROR: failed to post audit file: {0}", auditFilePath));

                // The audit file has been posted so exit the application
                Global.ExitApp(0, logEvent: Global.LogEvents.Info);
            }

            // *************************
            // *** CREATE AUDIT FILE ***
            // *************************
            if (commandLineArgs.Length == 3 && commandLineArgs[1].ToUpper() == CommandLineOptions.AUDIT)
            {
                if (CreateAuditFile(auditFilePath))
                    Global.LogEvent(string.Format("Generated audit file at: {0}", auditFilePath), Global.LogEvents.Info, logToLog4Net: true);
                else
                    Global.ExitApp(6, string.Format("ERROR: audit file not created: {0}", auditFilePath));

                // The audit file has been created so exit the application
                Global.ExitApp(0, logEvent: Global.LogEvents.Info);
            }

            Global.ExitApp(8, "ERROR: Option -POST requires execution as SYSTEM");
        }

        private static bool CreateAuditFileFolder(string auditFileFolder)
        {
            try
            {
                if (!Directory.Exists(auditFileFolder))
                    Directory.CreateDirectory(auditFileFolder);

                if (Directory.Exists(auditFileFolder))
                    return true;
            }
            catch (Exception ex)
            {
                Global.ExitApp(4, String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }

            return false;
        }

        private static AuditRecord GenerateAuditRecord(string computerName)
        {
            // *****************************************************
            // *** Collect Data from System.Environment (SYSTEM) ***
            // *****************************************************
            DateTime timeStamp = DateTime.Now;
            string userName = Environment.UserName;
            string userDomainName = Environment.UserDomainName;
            string logonServer = Environment.GetEnvironmentVariable(nameof(EnvironmentVariables.LogonServer));

            // ***********************************************
            // *** Collect data from Active Directory (AD) ***
            // ***********************************************
            ControllerAD controllerAD = new ControllerAD(connectionString: Settings.Default.ConnectionStringAD);

            controllerAD.Type = typeof(user);
            var user = controllerAD.GetRecord<user>(userName, attribute: nameof(Enums.AD.Attributes.sAMAccountName));

            controllerAD.Type = typeof(computer);
            var computer = controllerAD.GetRecord<computer>(computerName, attribute: nameof(Enums.AD.Attributes.name));

            // ******************************************************************
            // *** Collect data from Windows Management Instrumentation (WMI) ***
            // ******************************************************************
            ControllerWMI controllerWMI = new ControllerWMI(computerName: computerName, nameSpace: Namespaces.cimv2);

            controllerWMI.Type = typeof(Win32_ComputerSystem);
            var computerSystem = controllerWMI.GetAllRecords<Win32_ComputerSystem>().First();

            controllerWMI.Type = typeof(Win32_Processor);
            var processor = controllerWMI.GetAllRecords<Win32_Processor>().First();

            controllerWMI.Type = typeof(Win32_OperatingSystem);
            var operatingSystem = controllerWMI.GetAllRecords<Win32_OperatingSystem>().First();

            controllerWMI.Type = typeof(Win32_NetworkAdapterConfiguration);
            var listNetworkAdapterConfiguration = controllerWMI.GetAllRecords<Win32_NetworkAdapterConfiguration>();

            controllerWMI.Type = typeof(Win32_NetworkAdapter);
            var listNetworkAdapter = controllerWMI.GetAllRecords<Win32_NetworkAdapter>();

            controllerWMI.Type = typeof(Win32_PhysicalMemory);
            var listPhysicalMemory = controllerWMI.GetAllRecords<Win32_PhysicalMemory>();

            controllerWMI.Type = typeof(Win32_Volume);
            var listVolume = controllerWMI.GetAllRecords<Win32_Volume>();

            // **********************************************
            // *** Build audit record from collected data ***
            // **********************************************
            AuditRecord auditRecord = new AuditRecord();

            auditRecord.ComputerName = computerName;                                                                            // SYSTEM
            auditRecord.TimeStamp = timeStamp;                                                                                  // SYSTEM
            auditRecord.UserName = userName ?? string.Empty;                                                                    // SYSTEM
            auditRecord.UserDomain = userDomainName ?? string.Empty;                                                            // SYSTEM
            auditRecord.LogonServer = string.IsNullOrWhiteSpace(logonServer) ? string.Empty :                                   // SYSTEM
                logonServer.Replace(@"\\", string.Empty);

            auditRecord.CommonName = user == null ? string.Empty : user.cn;                                                     // AD
            auditRecord.DisplayName = user == null ? string.Empty : user.displayName;                                           // AD
            auditRecord.Description = user == null ? string.Empty : user.description;                                           // AD
            auditRecord.Department = user == null ? string.Empty : user.department;                                             // AD
            auditRecord.UserWhenChanged = user == null ? DateTime.MinValue : DateTime.Parse(user.whenChanged);                  // AD
            auditRecord.UserWhenCreated = user == null ? DateTime.MinValue : DateTime.Parse(user.whenCreated);                  // AD
            auditRecord.EmailAddress = user == null ? string.Empty : user.mail;                                                 // AD
            auditRecord.PhoneNumber = user == null ? string.Empty : user.telephoneNumber;                                       // AD
            auditRecord.ComputerWhenChanged = computer == null ? DateTime.MinValue : DateTime.Parse(computer.whenChanged);      // AD
            auditRecord.ComputerWhenCreated = computer == null ? DateTime.MinValue : DateTime.Parse(computer.whenCreated);      // AD

            auditRecord.OsVersion = operatingSystem == null ? string.Empty : operatingSystem.Caption;                           // WMI
            auditRecord.ComputerDomain = computerSystem == null ? string.Empty : computerSystem.Domain;                         // WMI
            auditRecord.SystemType = computerSystem == null ? string.Empty : computerSystem.SystemType;                         // WMI
            auditRecord.Cpu = processor == null ? string.Empty : processor.Caption;                                             // WMI
            auditRecord.BootTime = operatingSystem == null ? DateTime.MinValue :                                                // WMI
                ManagementDateTimeConverter.ToDateTime(operatingSystem.LastBootUpTime);

            auditRecord.NetworkCard = listNetworkAdapterConfiguration == null ? string.Empty :                                  // WMI
                string.Join(" ", listNetworkAdapterConfiguration
                      .Where(n => !string.IsNullOrWhiteSpace(n.Description))
                      .Select(n => n.Description)
                      .Cast<string>()
                      .ToArray());

            auditRecord.IpAddress = listNetworkAdapterConfiguration == null ? string.Empty :                                    // WMI
                string.Join(" ", listNetworkAdapterConfiguration
                      .Where(n => n.IPAddress != null)
                      .Select(n => string.Join(" ", n.IPAddress))
                      .ToArray());

            auditRecord.SubnetMask = listNetworkAdapterConfiguration == null ? string.Empty :                                   // WMI
                string.Join(" ", listNetworkAdapterConfiguration
                      .Where(n => n.IPSubnet != null)
                      .Select(n => string.Join(" ", n.IPSubnet))
                      .ToArray());

            auditRecord.DefaultGateway = listNetworkAdapterConfiguration == null ? string.Empty :                               // WMI
                listNetworkAdapterConfiguration.Where(n => n.DefaultIpGateway != null)
                                               .Select(n => n.DefaultIpGateway)
                                               .First()
                                               .First();

            auditRecord.MacAddress = listNetworkAdapterConfiguration == null ? string.Empty :                                   // WMI
                string.Join(" ", listNetworkAdapterConfiguration
                      .Where(n => !string.IsNullOrWhiteSpace(n.MACAddress))
                      .Select(n => n.MACAddress)
                      .ToArray());

            auditRecord.NetworkSpeed = listNetworkAdapter == null ? string.Empty :                                              // WMI
                string.Join(" ", listNetworkAdapter
                      .Where(n => !n.Speed.Equals(null))
                      .Select(n => string.Format("{0} Mbit", (n.Speed / (double)(1000 * 1000))
                      .ToString()))
                      .ToArray());

            auditRecord.NetworkType = listNetworkAdapter == null ? string.Empty :                                               // WMI
                string.Join(" ", listNetworkAdapter
                      .Where(n => !string.IsNullOrWhiteSpace(n.AdapterType))
                      .Select(n => n.AdapterType)
                      .ToArray());

            auditRecord.DhcpServer = listNetworkAdapterConfiguration == null ? string.Empty :                                   // WMI
                string.Join(" ", listNetworkAdapterConfiguration
                      .Where(n => !string.IsNullOrWhiteSpace(n.DHCPServer))
                      .Select(n => n.DHCPServer)
                      .ToArray());

            auditRecord.DnsServer = listNetworkAdapterConfiguration == null ? string.Empty :                                    // WMI
                string.Join(" ", listNetworkAdapterConfiguration
                      .Where(n => n.DNSServerSearchOrder != null)
                      .Select(n => string.Join(" ", n.DNSServerSearchOrder))
                      .ToArray());

            auditRecord.Memory = listPhysicalMemory == null ? string.Empty :                                                    // WMI
                string.Format("{0} MB", (listPhysicalMemory
                      .Select(n => n.Capacity)
                      .Aggregate((result, item) => result + item) / (double)(1024 * 1024))
                      .ToString());

            auditRecord.Volumes = listVolume == null ? string.Empty :                                                           // WMI
                string.Join(" ", listVolume.Where(v => !string.IsNullOrWhiteSpace(v.DriveLetter))
                      .Select(v => string.Format("{0} {1} GB {2}", v.DriveLetter, (v.Capacity / (double)(1024 * 1024 * 1024))
                      .ToString("N0"), v.FileSystem))
                      .ToArray());

            auditRecord.FreeSpace = listVolume == null ? string.Empty :                                                         // WMI
                string.Join(" ", listVolume.Where(v => !string.IsNullOrWhiteSpace(v.DriveLetter))
                      .Select(v => string.Format("{0} {1} GB", v.DriveLetter, (v.FreeSpace / (double)(1024 * 1024 * 1024))
                      .ToString("N0")))
                      .ToArray());

            return auditRecord;
        }

        private static AuditRecord ReadAuditRecordFromFile(string auditFilePath)
        {
            try
            {
                if (File.Exists(auditFilePath))
                {
                    string[] lines = File.ReadAllLines(auditFilePath);
                    string[] fields = lines[1].Split('|'); // lines[0] would be the headers

                    // Loop through and populate all of the properties in AuditRecord
                    AuditRecord auditRecord = new AuditRecord();
                    List<PropertyInfo> auditRecordProperties = GetProperties<AuditRecord>();
                    foreach (PropertyInfo propertyInfo in auditRecordProperties)
                    {
                        if (propertyInfo != null && propertyInfo.CanWrite)
                        {
                            int index = auditRecordProperties.IndexOf(propertyInfo);
                            string value = fields[index];

                            // Check for any dates and convert as needed
                            DateTime @out = new DateTime();
                            if (DateTime.TryParse(value, out @out))
                                propertyInfo.SetValue(auditRecord, @out);
                            else
                                propertyInfo.SetValue(auditRecord, value);
                        }
                    }

                    return auditRecord;
                }
            }
            catch (Exception ex)
            {
                Global.ExitApp(10, String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }

            return null;
        }

        private static List<PropertyInfo> GetProperties<T>()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
        }

        private static void WriteAuditRecordToCsv(AuditRecord auditRecord, string path)
        {
            try
            {
                using (StreamWriter streamWriter = new StreamWriter(path))
                {
                    List<PropertyInfo> auditRecordProperties = GetProperties<AuditRecord>();

                    // Write column header fields
                    streamWriter.WriteLine(string.Join("|", auditRecordProperties.Select(p => p.Name)));
                    // Write all fields based on the property value
                    streamWriter.WriteLine(string.Join("|", auditRecordProperties.Select(p => p.GetValue(auditRecord))));
                }
            }
            catch (Exception ex)
            {
                Global.ExitApp(5, string.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }
        }

        private static bool PostAuditRecord(AuditRecord auditRecord)
        {
            try
            {
                ControllerSQL controllerSQL = new ControllerSQL(connectionString: Settings.Default.ConnectionStringSQL,
                                                                tableName: Global.Pluralize(nameof(AuditRecord)),
                                                                idField: nameof(AuditRecord.ComputerName));
                controllerSQL.Type = typeof(AuditRecord);
                controllerSQL.PostRecord(auditRecord);

                return true;
            }
            catch (Exception ex)
            {
                Global.ExitApp(7, String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }

            return false;
        }

        private static bool PostAuditFile(string auditFilePath)
        {
            try
            {
                if (File.Exists(auditFilePath))
                {
                    AuditRecord auditRecord = ReadAuditRecordFromFile(auditFilePath);
                    Global.LogEvent(string.Format("Generated audit record from file: {0}", auditFilePath), Global.LogEvents.Info, logToLog4Net: false);

                    if (PostAuditRecord(auditRecord)) return true;
                }
                else
                    Global.ExitApp(11, string.Format("ERROR: Audit file not found: {0}", auditFilePath));
            }
            catch (Exception ex)
            {
                Global.ExitApp(9, String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }

            return false;
        }

        private static bool CreateAuditFile(string auditFilePath)
        {
            try
            {
                string auditFileFolder = Path.GetDirectoryName(auditFilePath);
                if (CreateAuditFileFolder(auditFileFolder))
                {
                    string computerName = Environment.GetEnvironmentVariable(nameof(EnvironmentVariables.ComputerName));
                    AuditRecord auditRecord = GenerateAuditRecord(computerName);

                    WriteAuditRecordToCsv(auditRecord, auditFilePath);
                }
                else
                    Global.ExitApp(3, string.Format("ERROR: Failed to create audit file folder: {0}", auditFilePath));

                if (File.Exists(auditFilePath))
                    return true;
            }
            catch (Exception ex)
            {
                Global.ExitApp(6, String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }

            return false;
        }

        private static void DisplayConsoleHelp()
        {
            string tabs = new string(ControlChars.Tab, 2);

            // Usage
            string helpMessage = string.Format("Creates and posts a system audit file to SQL{0}", Environment.NewLine);
            helpMessage += Environment.NewLine;
            helpMessage += string.Format("Usage: {0} [OPTION] FILE{1}", Global.appName, Environment.NewLine);
            helpMessage += Environment.NewLine;
            helpMessage += string.Format("Option{0}Description{1}", tabs, Environment.NewLine);
            helpMessage += Environment.NewLine;
            helpMessage += string.Format(" -AUDIT{0}Create audit file at the specified location{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" -POST{0}Post specified audit file to SQL{1}", tabs, Environment.NewLine);

            // Exit codes
            helpMessage += Environment.NewLine;
            helpMessage += string.Format("Exit Code{0}Meaning{1}", ControlChars.Tab, Environment.NewLine);
            helpMessage += Environment.NewLine;
            helpMessage += string.Format(" 0{0}No errors{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" 01{0}Error logging{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" 02{0}User failed to provide audit file path{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" 03{0}Error creating audit file folder{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" 04{0}Error accessing audit file folder{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" 05{0}Error writing to audit file{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" 06{0}Audit file was not created{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" 07{0}Error posting audit file to SQL{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" 08{0}User failed to run post as SYSTEM{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" 09{0}Error accessing audit file during post{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" 10{0}Error parsing audit file during post{1}", tabs, Environment.NewLine);
            helpMessage += string.Format(" 11{0}Audit file not found during post", tabs);

            Console.WriteLine(helpMessage);

            Environment.Exit(0);
        }

    }
}