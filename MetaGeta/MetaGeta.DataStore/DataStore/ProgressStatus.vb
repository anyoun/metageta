Public Class ProgressStatus
    Implements INotifyPropertyChanged

    Private m_ProgressPct As Double = 0
    Private m_StatusMessage As String = Nothing
    Private m_StartTime As DateTime
    Private m_State As ProgressStatusState = ProgressStatusState.NotStarted

    Public Sub New()
    End Sub

    Public Property ProgressPct() As Double
        Get
            Return m_ProgressPct
        End Get
        Set(ByVal value As Double)
            If value <> m_ProgressPct Then
                m_ProgressPct = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ProgressPct"))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("TimeLeft"))
            End If
        End Set
    End Property
    Public ReadOnly Property TimeLeft() As TimeSpan
        Get
            Return If(Rate = 0, TimeSpan.Zero, New TimeSpan(0, 0, CInt((1 - ProgressPct) / Rate)))
        End Get
    End Property
    Public ReadOnly Property Rate() As Double
        Get
            Return ProgressPct / (DateTime.Now - m_StartTime).TotalSeconds
        End Get
    End Property
    Public Property StatusMessage() As String
        Get
            Return m_StatusMessage
        End Get
        Set(ByVal value As String)
            If value <> m_StatusMessage Then
                m_StatusMessage = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("StatusMessage"))
            End If
        End Set
    End Property
    Public ReadOnly Property State() As String
        Get
            Return m_State.ToString()
        End Get
    End Property

    Public ReadOnly Property IsRunning() As Boolean
        Get
            Return m_State = ProgressStatusState.Running
        End Get
    End Property
    Public ReadOnly Property IsDone() As Boolean
        Get
            Return m_State = ProgressStatusState.Done OrElse m_State = ProgressStatusState.Done
        End Get
    End Property

    Public Sub Start()
        m_StartTime = DateTime.Now
        m_State = ProgressStatusState.Running
        OnStateChanged()
    End Sub

    Public Sub Done(ByVal succeeded As Boolean)
        m_State = If(succeeded, ProgressStatusState.Done, ProgressStatusState.Failed)
        ProgressPct = 1
        OnStateChanged()
    End Sub

    Private Sub OnStateChanged()
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("State"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsRunning"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsDone"))
    End Sub

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class

Public Enum ProgressStatusState
    NotStarted
    Running
    Failed
    Done
End Enum

