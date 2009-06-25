Imports System.Windows.Forms
Imports System.Windows.Interop

Namespace Editors
    Public Class DirectoryListEditor
        Inherits ItemListEditor

        Protected Overrides Function CreateItem() As String
            Dim fbd As New FolderBrowserDialog()
            fbd.ShowNewFolderButton = True
            Dim owner As New Win32Window(CType(PresentationSource.FromVisual(Me), HwndSource).Handle)
            If fbd.ShowDialog(owner) <> DialogResult.OK Then Return Nothing
            Return fbd.SelectedPath
        End Function

        Private Class Win32Window
            Implements System.Windows.Forms.IWin32Window

            Private ReadOnly m_Handle As IntPtr

            Public Sub New(ByVal handle As IntPtr)
                m_Handle = handle
            End Sub

            Public ReadOnly Property Handle() As System.IntPtr Implements System.Windows.Forms.IWin32Window.Handle
                Get
                    Return m_Handle
                End Get
            End Property
        End Class

    End Class
End Namespace