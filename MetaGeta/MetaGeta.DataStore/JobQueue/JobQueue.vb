Public Class JobQueue
    Implements IDisposable, INotifyPropertyChanged

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_ActionWaitingQueue As New Queue(Of Job)
    Private ReadOnly m_AllActionItems As New List(Of Job)

    Private ReadOnly m_ProcessActionThread As Thread
    Private ReadOnly m_StopProcessingActions As New ManualResetEvent(False)
    Private ReadOnly m_QueueThreadIsSleeping As New ManualResetEvent(False)

    Public Sub New()
        m_ProcessActionThread = New Thread(AddressOf ProcessActionQueueThread)
        m_ProcessActionThread.Start()
    End Sub

    Public ReadOnly Property ActionItems() As IEnumerable(Of Job)
        Get
            SyncLock m_ActionWaitingQueue
                Return m_AllActionItems.ToArray()
            End SyncLock
        End Get
    End Property

    Public Sub EnqueueAction(ByVal action As String, ByVal dataStore As MGDataStore, ByVal file As MGFile)
        SyncLock m_ActionWaitingQueue
            Dim item As New Job(action, dataStore, file)
            m_ActionWaitingQueue.Enqueue(item)
            Monitor.PulseAll(m_ActionWaitingQueue)
            m_AllActionItems.Add(item)
        End SyncLock
        OnActionItemsChanged()
    End Sub

    Private Sub CloseActionQueue()
        log.Info("Stopping action queue...")
        SyncLock m_ActionWaitingQueue
            m_ActionWaitingQueue.Enqueue(Nothing)
            Monitor.Pulse(m_ActionWaitingQueue)
        End SyncLock
        m_StopProcessingActions.Set()
        log.Info("Joining...")
        m_ProcessActionThread.Join()
        log.Info("Joined")
    End Sub

    Private Sub ProcessActionQueueThread()
        Thread.CurrentThread.Name = "ProcessActionQueue"
        While True
            If m_StopProcessingActions.WaitOne(0, False) Then Return

            Dim nextItem As Job
            SyncLock m_ActionWaitingQueue
                While m_ActionWaitingQueue.Count = 0
                    m_QueueThreadIsSleeping.Set()
                    Monitor.Wait(m_ActionWaitingQueue)
                    m_QueueThreadIsSleeping.Reset()
                End While
                nextItem = m_ActionWaitingQueue.Dequeue()
            End SyncLock
            If nextItem Is Nothing Then Return

            Dim action = nextItem.DataStore.LookupAction(nextItem.Action)
            nextItem.Status.Start()
            Try
                log.DebugFormat("Starting action ""{0}"" for file ""{1}""...", nextItem.Action, If(nextItem.File IsNot Nothing, nextItem.File.FileName, ""))
                action.DoAction(nextItem.Action, nextItem.File, nextItem.Status)
                nextItem.Status.Done(True)
                nextItem.Status.StatusMessage = String.Empty
                log.DebugFormat("Action ""{0}"" for file ""{1}"" done.", nextItem.Action, If(nextItem.File IsNot Nothing, nextItem.File.FileName, ""))
            Catch ex As Exception
                nextItem.Status.Done(False)
                nextItem.Status.StatusMessage = ex.ToString()
                log.ErrorFormat("Action ""{0}"" for file ""{1}"" failed with exception:\n{2}", nextItem.Action, If(nextItem.File IsNot Nothing, nextItem.File.FileName, ""), ex)
            End Try
        End While
    End Sub

    Public Sub WaitForQueueToEmpty()
        While True
            SyncLock m_ActionWaitingQueue
                'Prevents race condition were worker hasn't started yet
                If m_ActionWaitingQueue.Count = 0 Then Exit While
            End SyncLock
            Thread.Sleep(100)
        End While
        m_QueueThreadIsSleeping.WaitOne()
    End Sub

    Private Sub OnActionItemsChanged()
        OnPropertyChanged("ActionItems")
    End Sub

    Private Sub OnPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged


#Region " IDisposable Support "
    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                CloseActionQueue()
            End If

            ' TODO: free your own state (unmanaged objects).
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
