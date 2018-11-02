Public Class Win32_NetworkAdapterConfiguration
    Public Property Caption As String
    Public Property DefaultIpGateway As String()
    Public Property Description As String
    Public Property DHCPEnabled As Boolean
    Public Property DHCPServer As String
    Public Property DNSDomain As String
    Public Property DNSDomainSuffixSearchOrder As String()
    Public Property DNSEnabledForWINSResolution As Boolean
    Public Property DNSHostName As String
    Public Property DNSServerSearchOrder As String()
    Public Property DomainDNSRegistrationEnabled As Boolean
    Public Property FullDNSRegistrationEnabled As Boolean
    Public Property GatewayCostMetric As UShort()
    Public Property Index As UInteger
    Public Property InterfaceIndex As UInteger
    Public Property IPAddress As String()
    Public Property IPConnectionMetric As UInteger
    Public Property IPEnabled As Boolean
    Public Property IPFilterSecurityEnabled As Boolean
    Public Property IPSubnet As String()
    Public Property MACAddress As String
    Public Property ServiceName As String
    Public Property SettingID As String
    Public Property WINSEnableLMHostsLookup As Boolean
End Class