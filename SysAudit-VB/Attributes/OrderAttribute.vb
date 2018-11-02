<AttributeUsage(AttributeTargets.Property, Inherited:=False, AllowMultiple:=False)>
Public NotInheritable Class OrderAttribute : Inherits Attribute

    Public ReadOnly Property Order As Integer

    Public Sub New(Optional Order As Integer = 0)
        Me.Order = Order
    End Sub

End Class