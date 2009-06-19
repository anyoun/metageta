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
