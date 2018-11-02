Imports System.Data.Entity.Design.PluralizationServices
Imports System.Globalization

Module [Global]

    Public Function Pluralize([String] As String) As String
        Dim CultureInfo As CultureInfo = New CultureInfo("en-us")
        Dim PluralizationService As PluralizationService = PluralizationService.CreateService(CultureInfo)
        If PluralizationService.IsSingular([String]) Then
            Return PluralizationService.Pluralize([String])
        End If

        Return Nothing
    End Function

End Module