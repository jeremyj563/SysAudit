Imports System.Data.SqlTypes

Public Class AuditRecord
    <Order> Public Property TimeStamp As SqlDateTime
    <Order> Public Property UserName As String
    <Order> Public Property ComputerName As String
    <Order> Public Property OsVersion As String
    <Order> Public Property ComputerDomain As String
    <Order> Public Property UserDomain As String
    <Order> Public Property LogonServer As String
    <Order> Public Property SystemType As String
    <Order> Public Property NetworkCard As String
    <Order> Public Property IpAddress As String
    <Order> Public Property SubnetMask As String
    <Order> Public Property DefaultGateway As String
    <Order> Public Property MacAddress As String
    <Order> Public Property NetworkSpeed As String
    <Order> Public Property NetworkType As String
    <Order> Public Property DhcpServer As String
    <Order> Public Property DnsServer As String
    <Order> Public Property Cpu As String
    <Order> Public Property Memory As String
    <Order> Public Property BootTime As SqlDateTime
    <Order> Public Property SnapshotTime As SqlDateTime
    <Order> Public Property Volumes As String
    <Order> Public Property FreeSpace As String
End Class