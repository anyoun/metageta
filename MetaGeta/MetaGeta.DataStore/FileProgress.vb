Public Class FileProgress
    Implements IProgressReporter

    Private m_AllFiles As List(Of MGFile)
    Private m_CurrentIndex As Integer
    Private m_CurrentItemPercent As Double

    Public Sub New(ByVal files As IEnumerable(Of MGFile))
        m_AllFiles = New List(Of MGFile)(files)
        m_CurrentIndex = -1

        OnCompletedItemsChanged()
        OnCurrentItemChanged()
        OnIsDoneChanged()
        OnPercentDoneChanged()
    End Sub

    Public ReadOnly Property AllItems() As IEnumerable Implements IProgressReporter.AllItems
        Get
            SyncLock Me
                Return m_AllFiles
            End SyncLock
        End Get
    End Property
    Public ReadOnly Property CompletedItems() As IEnumerable Implements IProgressReporter.CompletedItems
        Get
            SyncLock Me
                Return From file In m_AllFiles Take m_CurrentIndex
            End SyncLock
        End Get
    End Property
    Public ReadOnly Property TotalPercentDone() As Double Implements IProgressReporter.TotalPercentDone
        Get
            SyncLock Me
                If m_CurrentIndex = m_AllFiles.Count Then
                    Return 1
                Else
                    Return m_CurrentIndex / m_AllFiles.Count
                End If
            End SyncLock
        End Get
    End Property
    Public ReadOnly Property IsDone() As Boolean Implements IProgressReporter.IsDone
        Get
            SyncLock Me
                Return m_CurrentIndex = m_AllFiles.Count
            End SyncLock
        End Get
    End Property
    Public ReadOnly Property CurrentItem() As Object Implements IProgressReporter.CurrentItem
        Get
            SyncLock Me
                If m_CurrentIndex = m_AllFiles.Count Then
                    Return Nothing
                Else
                    Return m_AllFiles(m_CurrentIndex)
                End If
            End SyncLock
        End Get
    End Property
    Public ReadOnly Property CurrentItemPercentDone() As Double Implements IProgressReporter.CurrentItemPercentDone
        Get
            SyncLock Me
                If m_CurrentIndex = m_AllFiles.Count Then
                    Return 1
                Else
                    Return m_CurrentItemPercent
                End If
            End SyncLock
        End Get
    End Property


    Public Function GetNextItem() As MGFile
        SyncLock Me
            m_CurrentIndex += 1

            OnCompletedItemsChanged()
            OnCurrentItemChanged()
            OnPercentDoneChanged()

            Return m_AllFiles(m_CurrentIndex)
        End SyncLock
    End Function

    Public Sub SetCurrentItemProgress(ByVal percent As Double)
        m_CurrentItemPercent = percent
    End Sub

    Private Sub OnCompletedItemsChanged()
        OnPropertyChanged("CompletedItems")
    End Sub
    Private Sub OnIsDoneChanged()
        OnPropertyChanged("IsDone")
    End Sub
    Private Sub OnCurrentItemChanged()
        OnPropertyChanged("CurrentItem")
    End Sub
    Private Sub OnPercentDoneChanged()
        OnPropertyChanged("PercentDone")
    End Sub
    Private Sub OnPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
End Class