Imports System.Data.SqlClient
Imports System.Reflection

Public Class ControllerSQL

#Region " Properties "
    ' Public Properties
    Public Property Type As Type
        Get
            If _Type IsNot Nothing Then Return _Type
            Throw New Exception(String.Format("{0}: Domain object return type was not specified", NameOf(ControllerSQL)))
        End Get
        Set(value As Type)
            Me._Type = value
            Me.PropertyInfos = GetPropertyInfos()
        End Set
    End Property
    Public Property ConnectionString As String
        Get
            If Not String.IsNullOrWhiteSpace(_ConnectionString) Then Return _ConnectionString
            Throw New Exception(String.Format("{0}: SQLClient connection string was not specified", NameOf(ControllerSQL)))
        End Get
        Set(Value As String)
            _ConnectionString = Value
        End Set
    End Property
    Public Property Schema As String
        Get
            If Not String.IsNullOrWhiteSpace(_Schema) Then Return _Schema
            Throw New Exception(String.Format("{0}: Database schmea was not specified", NameOf(ControllerSQL)))
        End Get
        Set(Value As String)
            _Schema = Value
        End Set
    End Property
    Public Property TableName As String
        Get
            If Not String.IsNullOrWhiteSpace(_TableName) Then Return _TableName
            Throw New Exception(String.Format("{0}: Database table name was not specified", NameOf(ControllerSQL)))
        End Get
        Set(Value As String)
            _TableName = Value
        End Set
    End Property
    Public Property IdField As String
        Get
            If Not String.IsNullOrWhiteSpace(_IdField) Then Return _IdField
            Throw New Exception(String.Format("{0}: Identify field was not specified", NameOf(ControllerSQL)))
        End Get
        Set(Value As String)
            _IdField = Value
        End Set
    End Property

    ' Private Properties
    Private Property PropertyInfos As PropertyInfo()
        Get
            Return Me._PropertyInfos
        End Get
        Set(Value As PropertyInfo())
            Me._PropertyInfos = Value
            ' Get the Fields string as soon as the PropertyInfos are available
            Me.Fields = GetFields(FieldTypes.Fields)
        End Set
    End Property
    Private Property Fields As String

    ' Backing Fields
    Private _Type As Type
    Private _ConnectionString As String
    Private _Schema As String
    Private _TableName As String
    Private _IdField As String
    Private _PropertyInfos As PropertyInfo()
#End Region

    ''' <summary>
    ''' Initialized a new instance of the ControllerSQL class.
    ''' </summary>
    ''' <param name="ConnectionString">The connection string property formatted for the System.Data.SqlClient provider.</param>
    ''' <param name="Schema">The database schema holding the needed table(s).</param>
    ''' <param name="TableName">The name of the needed database table.</param>
    ''' <param name="IdField">The name of the identification field to be used.</param>
    Public Sub New(Optional ConnectionString As String = Nothing, Optional Schema As String = "dbo", Optional TableName As String = Nothing, Optional IdField As String = Nothing)
        Me.ConnectionString = ConnectionString
        Me.Schema = Schema
        Me.TableName = TableName
        Me.IdField = IdField
    End Sub

#Region " Public Members "

    ''' <summary>
    ''' Gets a record of the specified type when given the Id of the desired record.
    ''' </summary>
    ''' <typeparam name="T">The specified data object type to return.</typeparam>
    ''' <param name="Id">The Id of the record to get.</param>
    ''' <returns>The record associated with the Id.</returns>
    Public Function GetRecord(Of T As New)(Id As Object) As T
        Try
            Using SqlConnection As New SqlConnection(ConnectionString)
                SqlConnection.Open()

                Dim CommandText As String = String.Format("SELECT {0} FROM [{1}].[{2}] WHERE [{3}] = @Id", Me.Fields, Me.Schema, Me.TableName, Me.IdField)
                Using SqlCommand As New SqlCommand(CommandText, SqlConnection)
                    SqlCommand.Parameters.AddWithValue("@Id", Id)

                    Dim SqlDataReader As SqlDataReader = SqlCommand.ExecuteReader()
                    If SqlDataReader.Read() Then Return GetPopulatedObject(Of T)(SqlDataReader)
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
        End Try

        Return Nothing
    End Function

    ''' <summary>
    ''' Gets a list containing all records of the specified data object type.
    ''' </summary>
    ''' <typeparam name="T">The specified data object type to return.</typeparam>
    ''' <returns>A list containing all records of the specified type.</returns>
    Public Function GetAllRecords(Of T As New)() As List(Of T)
        Try
            Using SqlConnection As New SqlConnection(ConnectionString)
                SqlConnection.Open()

                Dim CommandText As String = String.Format("SELECT {0} FROM [{1}].[{2}]", Me.Fields, Me.Schema, Me.TableName)
                Using SqlCommand As New SqlCommand(CommandText, SqlConnection)

                    Dim SqlDataReader As SqlDataReader = SqlCommand.ExecuteReader()
                    Dim ReturnList As New List(Of T)()
                    While SqlDataReader.Read()
                        ReturnList.Add(GetPopulatedObject(Of T)(SqlDataReader))
                    End While

                    Return ReturnList
                End Using

            End Using
        Catch ex As Exception
            Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
        End Try

        Return Nothing
    End Function

    ''' <summary>
    ''' Checks if a record of the specified data object type exists when given the Id of the record to check.
    ''' </summary>
    ''' <param name="Id">The Id of the record to check.</param>
    ''' <returns>True if the record was successfully found. False if the record could not be found or an error occurred.</returns>
    Public Function RecordExists(Id As Object) As Boolean
        Try
            Using SqlConnection As New SqlConnection(ConnectionString)
                SqlConnection.Open()

                Dim CommandText As String = String.Format("SELECT COUNT(*) FROM [{0}].[{1}] WHERE [{2}] = @Id", Me.Schema, Me.TableName, Me.IdField)
                Using SqlCommand As New SqlCommand(CommandText, SqlConnection)
                    SqlCommand.Parameters.AddWithValue("@Id", Id)
                    If SqlCommand.ExecuteScalar() = 0 Then Return True
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
        End Try

        Return False
    End Function

    ''' <summary>
    ''' Executes a custom command when given the CommandText and an optional list of Parameters.
    ''' </summary>
    ''' <param name="CommandText">The custom command to execute.</param>
    ''' <returns>The number of rows affected.</returns>
    Public Function ExecuteCustomCommand(CommandText As String) As Integer
        Try
            Using SqlConnection As New SqlConnection(ConnectionString)
                SqlConnection.Open()

                Using SqlCommand As New SqlCommand(CommandText, SqlConnection)
                    For Each [Property] In PropertyInfos
                        SqlCommand.Parameters.AddWithValue(String.Format("@{0}", [Property].Name), [Property].GetValue(Nothing))
                    Next

                    Return SqlCommand.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
        End Try

        Return Nothing
    End Function

    ''' <summary>
    ''' Posts a record of the specified type. Performs an update if supplied with the Id.
    ''' </summary>
    ''' <param name="Record">The record of the specified type to post.</param>
    ''' <param name="Id">The optional Id provided when performing an update.</param>
    ''' <returns>The number of rows affected.</returns>
    Public Function PostRecord(Of T)(Record As T, Optional Id As Object = Nothing)
        Try
            Using SqlConnection As New SqlConnection(Me.ConnectionString)
                SqlConnection.Open()

                Dim CommandText As String = Nothing
                If Id IsNot Nothing Then
                    CommandText = String.Format("UPDATE [{0}] SET {1} WHERE [{2}] = @Id", Me.TableName, GetFields(FieldTypes.Updates), Me.IdField)
                Else
                    CommandText = String.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", Me.TableName, Fields, GetFields(FieldTypes.Values))
                End If

                Using SqlCommand As New SqlCommand(CommandText, SqlConnection)
                    If Id <> Nothing Then SqlCommand.Parameters.AddWithValue("@Id", Id)
                    For Each [Property] In PropertyInfos
                        SqlCommand.Parameters.AddWithValue(String.Format("@{0}", [Property].Name), [Property].GetValue(Record))
                    Next

                    Return SqlCommand.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
        End Try

        Return Nothing
    End Function

    ''' <summary>
    ''' Posts a list of records of the specified type. Performs an update if supplied with the Id.
    ''' </summary>
    ''' <param name="Id">The optional Id provided when performing an update.</param>
    ''' <returns>The number of rows affected.</returns>
    Public Function PostRecords(Of T)(ListRecords As List(Of T)) As Integer
        Try
            Using SqlConnection As New SqlConnection(Me.ConnectionString)
                SqlConnection.Open()

                Using SqlBulkCopy As New SqlBulkCopy(SqlConnection)
                    SqlBulkCopy.DestinationTableName = String.Format("[{0}].[{1}]", Me.Schema, Me.TableName)

                    Dim DataTable As New DataTable
                    ListRecords.ForEach(Function(Record) DataTable.Rows.Add(Record))
                    'SqlBulkCopy.WriteToServer()
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine(String.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message))
        End Try

        Return Nothing
    End Function

#End Region

#Region " Private Members "

    Private Function GetPopulatedObject(Of T As New)(SqlDataReader As SqlDataReader) As T
        Dim [Object] As New T
        For Each PropertyInfo As PropertyInfo In PropertyInfos
            If PropertyInfo IsNot Nothing AndAlso PropertyInfo.CanWrite Then
                Dim Value = SqlDataReader.Item(PropertyInfo.Name)
                PropertyInfo.SetValue([Object], Value)
            End If
        Next

        Return [Object]
    End Function

    Private Function GetPropertyInfos() As PropertyInfo()
        ' Get all non shared/static properties of the generic object
        Return Me.Type.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function([Property]) Not [Property].GetGetMethod().IsStatic).ToArray()
    End Function

    Private Function GetFields(FieldType As FieldTypes) As String
        Dim ReturnValue As String = Nothing

        Select Case FieldType
            Case FieldTypes.Fields
                ' Concatenated fields for SELECT command
                ReturnValue = String.Join(", ", PropertyInfos.Select(Function([Property]) String.Format("[{0}]", [Property].Name)).ToArray())

            Case FieldTypes.Values
                ' Concatenated fields for INSERT command
                ReturnValue = String.Join(", ", PropertyInfos.Select(Function([Property]) String.Format("@{0}", [Property].Name)).ToArray())

            Case FieldTypes.Updates
                ' Concatenated fields for UPDATE command
                ReturnValue = String.Join(", ", PropertyInfos.Where(Function([Property]) [Property].Name <> Me.IdField).Select(Function([Property]) String.Format("[{0}] = @{1}", [Property].Name, [Property].Name)).ToArray())

        End Select

        Return ReturnValue
    End Function

    Private Enum FieldTypes
        Fields
        Values
        Updates
    End Enum

#End Region

End Class