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

Partial Public Class GlobalSettingsEditor
    Implements INotifyPropertyChanged

    Private ReadOnly m_Settings As IList(Of GlobalSetting)
    Private m_MaxNameColumnWidth As Double = 0

    Public Sub New(ByVal setting As IList(Of GlobalSetting))
        MyBase.New()
        Me.InitializeComponent()

        m_Settings = setting
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("SettingsList"))
    End Sub

    Public ReadOnly Property SettingsList() As IList(Of GlobalSetting)
        Get
            Return m_Settings
        End Get
    End Property

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnOK.Click
        Me.DialogResult = True
        Me.Close()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnCancel.Click
        Me.DialogResult = True
        Me.Close()
    End Sub
End Class
