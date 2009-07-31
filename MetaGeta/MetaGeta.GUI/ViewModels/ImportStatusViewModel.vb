﻿Public Class ImportStatusViewModel
    Inherits NavigationTab

    Private ReadOnly m_DataStore As MGDataStore

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore

        m_ImportCommand = New RelayCommand(AddressOf Import)
    End Sub

    Private Sub Import()
        m_DataStore.BeginRefresh()
    End Sub

    Private m_ImportCommand As RelayCommand
    Public ReadOnly Property ImportCommand() As ICommand
        Get
            Return m_ImportCommand
        End Get
    End Property

    Public ReadOnly Property ImportStatus() As ImportStatus
        Get
            Return m_DataStore.ImportStatus
        End Get
    End Property

    Public Overrides ReadOnly Property Caption() As String
        Get
            Return "Import"
        End Get
    End Property

    Public Overrides ReadOnly Property Icon() As System.Windows.Media.ImageSource
        Get
            Return Nothing
        End Get
    End Property

End Class
