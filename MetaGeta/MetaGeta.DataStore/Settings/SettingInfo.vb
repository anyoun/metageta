Imports System.Reflection
Imports System.Runtime.CompilerServices

Public Class SettingInfo
    Private ReadOnly m_PropertyInfo As PropertyInfo
    Private ReadOnly m_Metadata As SettingsAttribute

    Public Sub New(ByVal propertyInfo As PropertyInfo, ByVal metadata As SettingsAttribute)
        m_PropertyInfo = propertyInfo
        m_Metadata = metadata
    End Sub

    Public ReadOnly Property PropertyInfo() As PropertyInfo
        Get
            Return m_PropertyInfo
        End Get
    End Property

    Public ReadOnly Property Metadata() As SettingsAttribute
        Get
            Return m_Metadata
        End Get
    End Property

    Public Shared Function GetSettings(ByVal plugin As IMGPluginBase) As IEnumerable(Of SettingInfo)
        Return From p In plugin.GetType().GetProperties(BindingFlags.Instance Or BindingFlags.Public) _
               Where p.IsDefined(Of SettingsAttribute)() _
               Select New SettingInfo(p, p.GetCustomAttribute(Of SettingsAttribute)())
    End Function

End Class
