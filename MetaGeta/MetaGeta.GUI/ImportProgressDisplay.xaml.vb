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

Partial Public Class ImportProgressDisplay
    Public Sub New(ByVal progress As FileProgress)
        MyBase.New()
        Me.InitializeComponent()

        Me.DataContext = progress
        AddHandler progress.PropertyChanged, AddressOf Progress_PropertyChanged
    End Sub

    Private Sub Progress_PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
        Dim fp = DirectCast(sender, FileProgress)
        If fp.IsDone Then
            Me.Dispatcher.BeginInvoke(Windows.Threading.DispatcherPriority.Normal, New MethodDelegate(AddressOf OnIsDone))
        End If
    End Sub

    Private Sub OnIsDone()
        Me.Close()
    End Sub
    Private Delegate Sub MethodDelegate()

    Private Sub btnCancel_Click() Handles btnCancel.Click
        Me.Close()
    End Sub
End Class
