Imports System.ComponentModel

Partial Public Class NewDataStoreWindow
    Implements INotifyPropertyChanged

    Private ReadOnly m_Arguments As DataStoreCreationArguments

    Public Sub New()
        Me.New(New DataStoreCreationArguments())
    End Sub

    Public Sub New(ByVal arguments As DataStoreCreationArguments)
        m_Arguments = arguments
        'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("DataStoreCreationArguments"))

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub

    Public ReadOnly Property DataStoreCreationArguments() As DataStoreCreationArguments
        Get
            Return m_Arguments
        End Get
    End Property

    Public ReadOnly Property TemplateFinder() As TemplateFinder
        Get
            Return New TemplateFinder()
        End Get
    End Property

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnOK.Click
        Me.DialogResult = True
        Me.Close()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnCancel.Click
        Me.DialogResult = False
        Me.Close()
    End Sub

    Private Sub btnDirectory_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnDirectory.Click
        Dim fbd As New System.Windows.Forms.FolderBrowserDialog()
        fbd.ShowNewFolderButton = True
        fbd.Description = "Select a folder to be monitored for files"
        If fbd.ShowDialog() = Forms.DialogResult.OK Then
            If tbDirectories.Text.Length <> 0 Then
                tbDirectories.Text += ";"
            End If
            tbDirectories.Text += fbd.SelectedPath
        End If
    End Sub

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class
