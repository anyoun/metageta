Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation
Imports System.ComponentModel

Partial Public Class SettingsEditor
    Public Shared SettingsProperty As DependencyProperty = DependencyProperty.Register("Settings", GetType(SettingInfoCollection), GetType(SettingsEditor))

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insert code required on object creation below this point.
    End Sub

    <Category("Content")> _
    Public Property Settings() As SettingInfoCollection
        Get
            Return CType(GetValue(SettingsProperty), SettingInfoCollection)
        End Get
        Set(ByVal value As SettingInfoCollection)
            SetValue(SettingsProperty, value)
        End Set
    End Property
End Class

Public Class EditorTemplateSelector
    Inherits DataTemplateSelector

    Private m_StringTemplate, m_ExtensionListTemplate, m_DirectoryListTemplate, m_FileListTemplate As DataTemplate

    Public Property StringTemplate() As DataTemplate
        Get
            Return m_StringTemplate
        End Get
        Set(ByVal value As DataTemplate)
            m_StringTemplate = value
        End Set
    End Property

    Public Property ExtensionListTemplate() As DataTemplate
        Get
            Return m_ExtensionListTemplate
        End Get
        Set(ByVal value As DataTemplate)
            m_ExtensionListTemplate = value
        End Set
    End Property

    Public Property DirectoryListTemplate() As DataTemplate
        Get
            Return m_DirectoryListTemplate
        End Get
        Set(ByVal value As DataTemplate)
            m_DirectoryListTemplate = value
        End Set
    End Property

    Public Property FileListTemplate() As DataTemplate
        Get
            Return m_FileListTemplate
        End Get
        Set(ByVal value As DataTemplate)
            m_FileListTemplate = value
        End Set
    End Property

    Public Overrides Function SelectTemplate(ByVal item As Object, ByVal container As System.Windows.DependencyObject) As System.Windows.DataTemplate
        Dim info = CType(item, SettingInfo)

        Select Case info.Metadata.Type
            Case SettingType.Directory, SettingType.ShortText, SettingType.File, SettingType.LongText
                Return StringTemplate

            Case SettingType.ExtensionList
                Return ExtensionListTemplate

            Case SettingType.DirectoryList
                Return DirectoryListTemplate

            Case SettingType.FileList
                Return FileListTemplate

            Case SettingType.Int
                Return StringTemplate

            Case SettingType.Float
                Return StringTemplate

            Case Else
                Throw New Exception(String.Format("Unknown setting type: {0}.", info.Metadata.Type))
        End Select
    End Function
End Class