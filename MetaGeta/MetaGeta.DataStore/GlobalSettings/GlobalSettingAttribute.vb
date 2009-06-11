'<AttributeUsage(AttributeTargets.Assembly, AllowMultiple:=True)> _
'Public Class GlobalSettingAttribute
'    Inherits Attribute

'    Private ReadOnly m_Name As String
'    Private ReadOnly m_DefaultValue As String
'    Private ReadOnly m_Type As GlobalSettingType

'    Public Sub New(ByVal name As String, ByVal defaultValue As String, ByVal type As GlobalSettingType)
'        m_Name = name
'        m_DefaultValue = defaultValue
'        m_Type = type
'    End Sub

'    Public ReadOnly Property Name() As String
'        Get
'            Return m_Name
'        End Get
'    End Property

'    Public ReadOnly Property DefaultValue() As String
'        Get
'            Return m_DefaultValue
'        End Get
'    End Property

'    Public ReadOnly Property Type() As GlobalSettingType
'        Get
'            Return m_Type
'        End Get
'    End Property
'End Class

Public Enum GlobalSettingType
    ShortText
    LongText
    Number
    Directory
    File
End Enum