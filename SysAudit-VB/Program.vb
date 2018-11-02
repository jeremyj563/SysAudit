Imports System.IO
Imports System.Reflection
Imports System.Management
Imports Microsoft.VisualBasic.FileIO

Public Module Program

    Public AppName As String = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)
    Public SystemDrive As String = Directory.GetDirectoryRoot(Environment.SystemDirectory)

    Public Sub Main()
        Dim AuditFilePath As String = Path.Combine(SystemDrive, "IT", "SysAudit", "audit.csv")

        ' ********************************************************************************************************************
        '' ********************************************************************************************************************
        'If File.Exists(AuditFilePath) Then
        '    Console.WriteLine("Found audit file: {0}", AuditFilePath)
        '    Dim AuditRecord As AuditRecord = GenerateAuditRecordFromFile(AuditFilePath)
        '    PostAuditRecord(AuditRecord)
        '    Console.WriteLine("Posted audit record")
        'End If
        ' ********************************************************************************************************************
        ' ********************************************************************************************************************






        Dim CommandLineArgs = Environment.GetCommandLineArgs()
        If CommandLineArgs.Length > 1 AndAlso CommandLineArgs(1).ToLower = "-runassystem" Then
            Console.WriteLine(Environment.UserName)
            If File.Exists(AuditFilePath) Then
                Console.WriteLine("Found audit file: {0}", AuditFilePath)
                Dim AuditRecord As AuditRecord = GenerateAuditRecordFromFile(AuditFilePath)
                PostAuditRecord(AuditRecord)
                Console.WriteLine("Posted audit record")
            End If

            Console.ReadKey()
            End
        End If

        Dim _AuditRecord As AuditRecord = GenerateAuditRecordFromWMI(ComputerNames.LocalHost)
        GenerateAuditFile(_AuditRecord, AuditFilePath)

        If File.Exists(AuditFilePath) Then
            Console.WriteLine("Generated audit file at: {0}", AuditFilePath)
        End If

        'Console.ReadKey()


        RelaunchAppAsSystem()

    End Sub

    Private Sub GenerateAuditFile(AuditRecord As AuditRecord, Path As String)
        Dim ListAuditRecords As List(Of AuditRecord) = New List(Of AuditRecord) From {AuditRecord}
        WriteCSV(ListAuditRecords, Path)
    End Sub

    Private Function GenerateAuditRecordFromWMI(ComputerName As String) As AuditRecord
        Dim ControllerWMI As New ControllerWMI(ComputerName:=ComputerName, Namespace:=Namespaces.cimv2)

        ControllerWMI.Type = GetType(Win32_ComputerSystem)
        Dim ComputerSystem = ControllerWMI.GetAllRecords(Of Win32_ComputerSystem)().First()

        ControllerWMI.Type = GetType(Win32_Processor)
        Dim Processor = ControllerWMI.GetAllRecords(Of Win32_Processor)().First()

        ControllerWMI.Type = GetType(Win32_OperatingSystem)
        Dim OperatingSystem = ControllerWMI.GetAllRecords(Of Win32_OperatingSystem)().First()

        ControllerWMI.Type = GetType(Win32_NetworkAdapterConfiguration)
        Dim ListNetworkAdapterConfiguration = ControllerWMI.GetAllRecords(Of Win32_NetworkAdapterConfiguration)()

        ControllerWMI.Type = GetType(Win32_NetworkAdapter)
        Dim ListNetworkAdapter = ControllerWMI.GetAllRecords(Of Win32_NetworkAdapter)()

        ControllerWMI.Type = GetType(Win32_PhysicalMemory)
        Dim ListPhysicalMemory = ControllerWMI.GetAllRecords(Of Win32_PhysicalMemory)()

        ControllerWMI.Type = GetType(Win32_Volume)
        Dim ListVolume = ControllerWMI.GetAllRecords(Of Win32_Volume)()

        Dim AuditRecord As New AuditRecord With
            {
                .TimeStamp = Now,
                .UserName = ComputerSystem.UserName.Split("\")(1),
                .ComputerName = ComputerSystem.Name,
                .OsVersion = OperatingSystem.Caption,
                .ComputerDomain = ComputerSystem.Domain,
                .UserDomain = ComputerSystem.UserName.Split("\")(0),
                .LogonServer = Environment.GetEnvironmentVariable(NameOf(AuditRecord.LogonServer)).Replace("\\", String.Empty),
                .SystemType = ComputerSystem.SystemType,
                .NetworkCard = String.Join(" ", ListNetworkAdapterConfiguration.Where(Function(n) Not String.IsNullOrWhiteSpace(n.Description)) _
                                                                             .Select(Function(n) n.Description).Cast(Of String)() _
                                                                             .ToArray()), _
 _
                .IpAddress = String.Join(" ", ListNetworkAdapterConfiguration.Where(Function(n) n.IPAddress IsNot Nothing) _
                                                                             .Select(Function(n) String.Join(" ", n.IPAddress)) _
                                                                             .ToArray()), _
 _
                .SubnetMask = String.Join(" ", ListNetworkAdapterConfiguration.Where(Function(n) n.IPSubnet IsNot Nothing) _
                                                                             .Select(Function(n) String.Join(" ", n.IPSubnet)) _
                                                                             .ToArray()), _
 _
                .DefaultGateway = ListNetworkAdapterConfiguration.Where(Function(n) n.DefaultIpGateway IsNot Nothing) _
                                                                             .Select(Function(n) n.DefaultIpGateway).First() _
                                                                             .First(), _
 _
                .MacAddress = String.Join(" ", ListNetworkAdapterConfiguration.Where(Function(n) Not String.IsNullOrWhiteSpace(n.MACAddress)) _
                                                                             .Select(Function(n) n.MACAddress) _
                                                                             .ToArray()), _
 _
                .NetworkSpeed = String.Join(" ", ListNetworkAdapter.Where(Function(n) Not n.Speed.Equals(Nothing)) _
                                                                             .Select(Function(n) n.Speed.ToString()) _
                                                                             .ToArray()), _
 _
                .NetworkType = String.Join(" ", ListNetworkAdapter.Where(Function(n) Not String.IsNullOrWhiteSpace(n.AdapterType)) _
                                                                             .Select(Function(n) n.AdapterType) _
                                                                             .ToArray()), _
 _
                .DhcpServer = String.Join(" ", ListNetworkAdapterConfiguration.Where(Function(n) Not String.IsNullOrWhiteSpace(n.DHCPServer)) _
                                                                             .Select(Function(n) n.DHCPServer) _
                                                                             .ToArray()), _
 _
                .DnsServer = String.Join(" ", ListNetworkAdapterConfiguration.Where(Function(n) n.DNSServerSearchOrder IsNot Nothing) _
                                                                             .Select(Function(n) String.Join(" ", n.DNSServerSearchOrder)) _
                                                                             .ToArray()), _
 _
                .Cpu = Processor.Caption,
                .Memory = String.Format("{0} MB", (ListPhysicalMemory.Select(Function(n) n.Capacity) _
                                                                             .Aggregate(Function(result, item) result + item) / (1024 * 1024)) _
                                                                             .ToString()), _
 _
                .BootTime = ManagementDateTimeConverter.ToDateTime(OperatingSystem.LastBootUpTime),
                .SnapshotTime = Now,
                .Volumes = String.Join(" ", ListVolume.Where(Function(v) Not String.IsNullOrWhiteSpace(v.DriveLetter)) _
                                                                             .Select(Function(v) String.Format("{0} {1} GB {2}", v.DriveLetter, (v.Capacity / (1024 * 1024 * 1024)).ToString("N0"), v.FileSystem)) _
                                                                             .ToArray()), _
 _
                .FreeSpace = String.Join(" ", ListVolume.Where(Function(v) Not String.IsNullOrWhiteSpace(v.DriveLetter)) _
                                                                             .Select(Function(v) String.Format("{0} {1} GB", v.DriveLetter, (v.FreeSpace / (1024 * 1024 * 1024)).ToString("N0"))) _
                                                                             .ToArray())
            }

        Return AuditRecord
    End Function

    Private Function GenerateAuditRecordFromFile(AuditFilePath As String) As AuditRecord
        If File.Exists(AuditFilePath) Then
            Try
                Using TextFieldParser As New TextFieldParser(AuditFilePath)
                    ' Configure parser as CSV reader
                    TextFieldParser.SetDelimiters({","})
                    TextFieldParser.ReadLine() ' Skip column header row

                    Dim Fields As String() = TextFieldParser.ReadFields()

                    Dim AuditRecord As New AuditRecord With
                        {
                            .TimeStamp = Fields(AuditFile.TimeStamp),
                            .UserName = Fields(AuditFile.UserName),
                            .ComputerName = Fields(AuditFile.ComputerName),
                            .OsVersion = Fields(AuditFile.OsVersion),
                            .ComputerDomain = Fields(AuditFile.ComputerDomain),
                            .UserDomain = Fields(AuditFile.UserDomain),
                            .LogonServer = Fields(AuditFile.LogonServer),
                            .SystemType = Fields(AuditFile.SystemType),
                            .NetworkCard = Fields(AuditFile.NetworkCard),
                            .IpAddress = Fields(AuditFile.IpAddress),
                            .SubnetMask = Fields(AuditFile.SubnetMask),
                            .DefaultGateway = Fields(AuditFile.DefaultGateway),
                            .MacAddress = Fields(AuditFile.MacAddress),
                            .NetworkSpeed = Fields(AuditFile.NetworkSpeed),
                            .NetworkType = Fields(AuditFile.NetworkType),
                            .DhcpServer = Fields(AuditFile.DhcpServer),
                            .DnsServer = Fields(AuditFile.DnsServer),
                            .Cpu = Fields(AuditFile.Cpu),
                            .Memory = Fields(AuditFile.Memory),
                            .BootTime = Fields(AuditFile.BootTime),
                            .SnapshotTime = Fields(AuditFile.SnapshotTime),
                            .Volumes = Fields(AuditFile.Volumes),
                            .FreeSpace = Fields(AuditFile.FreeSpace)
                        }

                    Return AuditRecord
                End Using
            Catch ex As Exception
                Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
            End Try
        End If

        Return Nothing
    End Function

    Private Sub WriteCSV(Of T)(Items As IEnumerable(Of T), Path As String)
        ' This function only accepts a type that has been decorated with OrderAttribute
        Try
            Dim Properties = GetType(T).GetProperties(BindingFlags.[Public] Or BindingFlags.Instance)

            ' Ensures that the order of the CSV fields will be the same as defined within the object being written out
            Dim OrderedProperties = Properties.Where(Function([Property]) Attribute.IsDefined([Property], GetType(OrderAttribute))) _
                                              .OrderBy(Function([Property]) CType([Property].GetCustomAttributes(GetType(OrderAttribute), False) _
                                                                                            .Single(), OrderAttribute).Order)
            If OrderedProperties IsNot Nothing Then
                Using StreamWriter As New StreamWriter(Path)
                    ' Write column header fields
                    StreamWriter.WriteLine(String.Join(", ", OrderedProperties.Select(Function([Property]) [Property].Name)))

                    ' Write all records in the collection
                    For Each Item As T In Items
                        ' Base each field on the property value
                        StreamWriter.WriteLine(String.Join(", ", OrderedProperties.Select(Function([Property]) [Property].GetValue(Item))))
                    Next
                End Using
            End If
        Catch ex As Exception
            Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
        End Try
    End Sub

    Private Sub RelaunchAppAsSystem()
        Dim AppPathWithArgs As String = Path.Combine(Directory.GetCurrentDirectory(), String.Format("{0}.exe -runassystem", AppName))
        Dim ToolsDir As String = Path.Combine(Directory.GetCurrentDirectory(), "Tools")
        Dim NirCmdPath As String = Path.Combine(ToolsDir, "nircmd.exe")
        Dim NirCmdArgs As String = String.Format("elevatecmd runassystem {0}", AppPathWithArgs)

        Process.Start(NirCmdPath, NirCmdArgs)
    End Sub

    Private Sub PostAuditRecord(AuditRecord As AuditRecord)
        Dim ControllerSQL As New ControllerSQL(ConnectionString:=My.Settings.ConnectionString,
                                               TableName:=Pluralize(NameOf(SysAudit_VB.AuditRecord)),
                                               IdField:=NameOf(SysAudit_VB.AuditRecord.ComputerName))

        ControllerSQL.Type = GetType(AuditRecord)
        ControllerSQL.PostRecord(AuditRecord)
    End Sub

End Module