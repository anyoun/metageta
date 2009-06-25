<AttributeUsage(AttributeTargets.Property, AllowMultiple:=False)> _
Public Class SettingsAttribute
    Inherits Attribute

    Private ReadOnly m_FriendlyName As String
    Private ReadOnly m_Category As String
    Private ReadOnly m_DefaultValue As Object
    Private ReadOnly m_Type As SettingType

    Public Sub New(ByVal friendlyName As String, ByVal defaultValue As Object, ByVal type As SettingType, ByVal category As String)
        m_FriendlyName = friendlyName
        m_DefaultValue = defaultValue
        m_Type = type
        m_Category = category
    End Sub

    Public ReadOnly Property FriendlyName() As String
        Get
            Return m_FriendlyName
        End Get
    End Property

    Public ReadOnly Property DefaultValue() As Object
        Get
            Return m_DefaultValue
        End Get
    End Property

    Public ReadOnly Property Type() As SettingType
        Get
            Return m_Type
        End Get
    End Property

    Public ReadOnly Property Category() As String
        Get
            Return m_Category
        End Get
    End Property
End Class

Public Enum SettingType
    ShortText
    LongText
    Int
    Float
    Directory
    DirectoryList
    File
    FileList
    ExtensionList
End Enum
