<AttributeUsage(AttributeTargets.Property, AllowMultiple:=False)> _
Public Class SettingsAttribute
    Inherits Attribute

    Private ReadOnly m_Name As String
    Private ReadOnly m_Category As String
    Private ReadOnly m_DefaultValue As String
    Private ReadOnly m_Type As SettingType

    Public Sub New(ByVal name As String, ByVal defaultValue As String, ByVal type As SettingType, ByVal category As String)
        m_Name = name
        m_DefaultValue = defaultValue
        m_Type = type
        m_Category = category
    End Sub

    Public ReadOnly Property Name() As String
        Get
            Return m_Name
        End Get
    End Property

    Public ReadOnly Property DefaultValue() As String
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
    File
End Enum
