Partial Public Class NewDataStoreWindow
    Public ReadOnly Property DataStoreCreationArguments() As DataStoreCreationArguments
        Get
            Return CType(CType(Me.Resources("CreationArgs"), ObjectDataProvider).Data, DataStoreCreationArguments)
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

End Class
