Public Class Job
    Private ReadOnly m_Action As String
    Private ReadOnly m_DataStore As MGDataStore
    Private ReadOnly m_File As MGFile
    Private ReadOnly m_Status As New ProgressStatus()

    Public Sub New(ByVal action As String, ByVal dataStore As MGDataStore, ByVal file As MGFile)
        m_Action = action
        m_DataStore = dataStore
        m_File = file
    End Sub

    Public ReadOnly Property Action() As String
        Get
            Return m_Action
        End Get
    End Property

    Public ReadOnly Property DataStore() As MGDataStore
        Get
            Return m_DataStore
        End Get
    End Property

    Public ReadOnly Property File() As MGFile
        Get
            Return m_File
        End Get
    End Property
    Public ReadOnly Property Status() As ProgressStatus
        Get
            Return m_Status
        End Get
    End Property
End Class
