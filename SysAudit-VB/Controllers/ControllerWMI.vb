Imports System.Management
Imports System.Reflection

Public Class ControllerWMI

#Region " Properties "
    ' Public Properties
    Public Property Type As Type
        Get
            If _Type IsNot Nothing Then Return _Type
            Throw New Exception("Domain object return type was not specified")
        End Get
        Set(Value As Type)
            Me._Type = Value
            ' Set the Class and get the PropertyInfos as soon as the type is available
            Me.Class = Value.Name
            Me.PropertyInfos = GetPropertyInfos()
        End Set
    End Property
    Public Property [Namespace] As String
        Get
            If Not String.IsNullOrWhiteSpace(_Namespace) Then Return _Namespace
            Throw New Exception("WMI Namespace was not specified")
        End Get
        Set(Value As String)
            _Namespace = Value
        End Set
    End Property
    Public Property [Class] As String
    Public Property IdField As String
        Get
            If Not String.IsNullOrWhiteSpace(_IdField) Then Return _IdField
            Throw New Exception("Identify field was not specified")
        End Get
        Set(Value As String)
            _IdField = Value
        End Set
    End Property
    Public Property ComputerName As String
        Get
            If Not String.IsNullOrWhiteSpace(_ComputerName) Then Return _ComputerName
            Throw New Exception("Computer name was not specified")
        End Get
        Set(Value As String)
            Me._ComputerName = Value
            ' Get the ProviderArchitecture as soon as the ComputerName is available
            Me.ProviderArchitecture = GetProviderArchitecture(Value)
        End Set
    End Property

    ' Private Properties
    Private Property ProviderArchitecture As ProviderArchitectures
    Private Property ConnectionOptions As ConnectionOptions
    Private Property PropertyInfos As PropertyInfo()
        Get
            Return Me._PropertyInfos
        End Get
        Set(Value As PropertyInfo())
            Me._PropertyInfos = Value
            ' Get the Fields string as soon as the PropertyInfos are available
            Me.Fields = GetFields()
        End Set
    End Property
    Private Property Fields As String

    ' Backing Fields
    Private _Type As Type
    Private _Namespace As String
    Private _IdField As String
    Private _ComputerName As String
    Private _PropertyInfos As PropertyInfo()
#End Region

    ''' <summary>
    ''' Initialized a new instance of the ControllerWMI class.
    ''' </summary>
    ''' <param name="ComputerName">The name of the computer to connect to.</param>
    ''' <param name="Namespace">The WMI Namespace containing the needed Class.</param>
    ''' <param name="Class">The WMI Class to control.</param>
    ''' <param name="IdField">The name of the identification field to be used.</param>
    Public Sub New(Optional ComputerName As String = Nothing, Optional [Namespace] As String = Nothing, Optional [Class] As String = Nothing, Optional IdField As String = Nothing)
        Me.ConnectionOptions = New ConnectionOptions With
            {
                .Impersonation = ImpersonationLevel.Impersonate,
                .Authentication = AuthenticationLevel.PacketPrivacy,
                .EnablePrivileges = True,
                .Timeout = New TimeSpan(0, 0, 0, 5, 0)
            }
        Me.[Namespace] = [Namespace]
        Me.[Class] = [Class]
        Me.IdField = IdField
        Me.ComputerName = ComputerName
    End Sub

#Region " Public Members "

    ''' <summary>
    ''' Gets a record of the specified type when given the Id of the desired record.
    ''' </summary>
    ''' <param name="Id">The Id of the record to get.</param>
    ''' <returns>The record associated with the Id.</returns>
    Public Function GetRecord(Of T As New)(Id As String) As T
        Dim ManagementScope As ManagementScope = GetManagementScope()

        Dim CommandText As String = String.Format("SELECT {0} FROM {1} WHERE {2} = {3}", Me.Fields, Me.[Class], Me.IdField, Id)
        Dim Results As ManagementObjectCollection = GetCommandResults(CommandText, ManagementScope)
        If Results IsNot Nothing Then
            Return GetPopulatedObject(Of T)(Results.Cast(Of ManagementBaseObject)()(0))
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Gets a list containing all records of the specified data object type.
    ''' </summary>
    ''' <returns>A list containing all records of the specified type.</returns>
    Public Function GetAllRecords(Of T As New)() As List(Of T)
        Try
            Dim ManagementScope As ManagementScope = GetManagementScope()

            Dim CommandText As String = String.Format("SELECT {0} FROM {1}", Me.Fields, Me.[Class])
            Dim Results As ManagementObjectCollection = GetCommandResults(CommandText, ManagementScope)

            Dim ReturnList As New List(Of T)()
            If Results IsNot Nothing Then
                For Each Result As ManagementBaseObject In Results
                    ReturnList.Add(GetPopulatedObject(Of T)(Result))
                Next
            End If

            Return ReturnList
        Catch ex As Exception
            Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
        End Try

        Return Nothing
    End Function

#End Region

#Region " Private Members "

    Private Function GetManagementScope(Optional ComputerName As String = Nothing, Optional [Namespace] As String = Nothing, Optional SpecifyArchitecture As Boolean = True) As ManagementScope
        Try
            If String.IsNullOrWhiteSpace(ComputerName) Then ComputerName = Me.ComputerName
            If [Namespace] Is Nothing Then [Namespace] = Me.[Namespace]

            Dim ManagmentScope As New ManagementScope()
            With ManagmentScope
                .Path.Server = ComputerName
                .Path.NamespacePath = [Namespace]
                .Options = Me.ConnectionOptions

                If SpecifyArchitecture Then
                    .Options.Context.Add("__ProviderArchitecture", Me.ProviderArchitecture)
                End If

                .Connect()
            End With

            Return ManagmentScope

        Catch ex As Exception
            Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
        End Try

        Return Nothing
    End Function

    Private Function GetCommandResults(CommandText As String, ManagementScope As ManagementScope) As ManagementObjectCollection
        Try
            Dim ManagementObjectSearcher As New ManagementObjectSearcher(ManagementScope.Path.ToString(), CommandText)
            Return ManagementObjectSearcher.Get()
        Catch ex As Exception
            Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
        End Try

        Return Nothing
    End Function

    Private Function GetPropertyInfos() As PropertyInfo()
        ' Get all non shared/static properties of the generic object
        Return Me.Type.GetProperties(BindingFlags.Public Or BindingFlags.Instance) _
                      .Where(Function([Property]) Not [Property].GetGetMethod().IsStatic) _
                      .ToArray()
    End Function

    Private Function GetFields() As String
        ' Get all non shared/static properties of the generic object
        Return String.Join(", ", PropertyInfos.Select(Function([Property]) String.Format("{0}", [Property].Name)).ToArray())
    End Function

    Private Function GetPopulatedObject(Of T As New)(ManagementBaseObject As ManagementBaseObject) As T
        Dim [Object] As New T
        For Each PropertyInfo As PropertyInfo In PropertyInfos
            If PropertyInfo IsNot Nothing AndAlso PropertyInfo.CanWrite Then
                Dim Value = ManagementBaseObject.GetPropertyValue(PropertyInfo.Name)
                PropertyInfo.SetValue([Object], Value)
            End If
        Next

        Return [Object]
    End Function

    Private Function GetProviderArchitecture(ComputerName As String) As ProviderArchitectures
        Try
            Dim ManagementScope As ManagementScope = GetManagementScope(ComputerName:=ComputerName, Namespace:=Namespaces.cimv2, SpecifyArchitecture:=False)
            Dim CommandText As String = String.Format("SELECT {0} FROM {1}", NameOf(Win32_Processor.AddressWidth), NameOf(Win32_Processor))
            Dim Results As ManagementObjectCollection = GetCommandResults(CommandText, ManagementScope)

            Dim AddressWidth As String = Results.Cast(Of ManagementBaseObject)()(0).GetPropertyValue(NameOf(Win32_Processor.AddressWidth)).ToString()
            If Not String.IsNullOrWhiteSpace(AddressWidth) Then
                If AddressWidth = ProviderArchitectures.x64.ToString("D") Then Return ProviderArchitectures.x64
                If AddressWidth = ProviderArchitectures.x86.ToString("D") Then Return ProviderArchitectures.x86
            End If
        Catch ex As Exception
            Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
        End Try

        Return Nothing
    End Function

    Private Enum ProviderArchitectures
        x86 = 32
        x64 = 64
    End Enum

#End Region

End Class