Public Class FileProgress
    Implements INotifyPropertyChanged, IProgressReportCallback

    Private ReadOnly m_Lock As New Object()

    Private m_AllFiles As ICollection
    Private m_CurrentIndex As Integer
    Private m_CurrentItemPercent As Double
    Private ReadOnly m_ProcedureName As String

    Public Sub New(ByVal procedureName As String)
        m_ProcedureName = procedureName

        m_AllFiles = Nothing
        m_CurrentIndex = -1

        OnCompletedItemsChanged()
        OnCurrentItemChanged()
        OnIsDoneChanged()
        OnPercentDoneChanged()
    End Sub

    Public ReadOnly Property AllItems() As ICollection
        Get
            SyncLock m_Lock
                Return m_AllFiles
            End SyncLock
        End Get
    End Property
    Public ReadOnly Property CompletedItems() As IEnumerable
        Get
            SyncLock m_Lock
                Return From file In m_AllFiles Take m_CurrentIndex
            End SyncLock
        End Get
    End Property
    Public ReadOnly Property TotalPercentDone() As Double
        Get
            SyncLock m_Lock
                If m_CurrentIndex = m_AllFiles.Count Then
                    Return 1
                Else
                    Return CType(m_CurrentIndex, Double) / m_AllFiles.Count
                End If
            End SyncLock
        End Get
    End Property
    Public ReadOnly Property IsDone() As Boolean
        Get
            SyncLock m_Lock
                Return m_CurrentIndex = m_AllFiles.Count
            End SyncLock
        End Get
    End Property
    Public ReadOnly Property CurrentItem() As Object
        Get
            SyncLock m_Lock
                If m_CurrentIndex = m_AllFiles.Count Then
                    Return Nothing
                Else
                    Return m_AllFiles(m_CurrentIndex)
                End If
            End SyncLock
        End Get
    End Property
    Public ReadOnly Property CurrentItemPercentDone() As Double
        Get
            SyncLock m_Lock
                If m_CurrentIndex = m_AllFiles.Count Then
                    Return 1
                Else
                    Return m_CurrentItemPercent
                End If
            End SyncLock
        End Get
    End Property
    Public ReadOnly Property ProcedureName() As String
        Get
            Return m_ProcedureName
        End Get
    End Property

#Region "IProgressReporter"
    Public Sub SetCurrentItem(ByVal index As Integer) Implements IProgressReportCallback.SetCurrentItem
        Dim done = False
        SyncLock m_Lock
            m_CurrentIndex = index
            If m_CurrentIndex = m_AllFiles.Count Then
                done = True
            End If
        End SyncLock
        OnCurrentItemChanged()
        OnCompletedItemsChanged()
        OnPercentDoneChanged()
        If done Then
            OnIsDoneChanged()
        End If
    End Sub

    Public Sub SetItemProgress(ByVal percent As Double) Implements IProgressReportCallback.SetItemProgress
        SyncLock m_Lock
            m_CurrentItemPercent = percent
        End SyncLock
        OnPercentDoneChanged()
    End Sub

    Public Sub SetItems(ByVal items As System.Collections.ICollection) Implements IProgressReportCallback.SetItems
        Dim done = False
        SyncLock m_Lock
            m_AllFiles = items
            If m_AllFiles.Count = 0 Then
                done = True
            End If
        End SyncLock
        OnCurrentItemChanged()
        OnCompletedItemsChanged()
        If done Then
            OnIsDoneChanged()
        End If
    End Sub
#End Region

#Region "Property changes"
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
#End Region

End Class

Public Class ProgressHelper
    Implements IEnumerable(Of MGFile)

    Private ReadOnly m_Files As List(Of MGFile)
    Private ReadOnly m_Reporter As IProgressReportCallback

    Public Sub New(ByVal reporter As IProgressReportCallback, ByVal files As IEnumerable(Of MGFile))
        m_Reporter = reporter
        m_Files = New List(Of MGFile)(files)
    End Sub

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of MGFile) Implements System.Collections.Generic.IEnumerable(Of MGFile).GetEnumerator
        Return New ProgressHelperEnumerator(m_Reporter, m_Files)
    End Function

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    Private Class ProgressHelperEnumerator
        Implements IEnumerator(Of MGFile)

        Private ReadOnly m_Files As List(Of MGFile)
        Private m_CurrentIndex As Integer
        Private ReadOnly m_Reporter As IProgressReportCallback

        Public Sub New(ByVal reporter As IProgressReportCallback, ByVal files As List(Of MGFile))
            m_Reporter = reporter
            m_Files = files
            m_CurrentIndex = -1

            reporter.SetItems(m_Files)
            reporter.SetCurrentItem(-1)
        End Sub

        Public ReadOnly Property Current() As MGFile Implements System.Collections.Generic.IEnumerator(Of MGFile).Current
            Get
                Return m_Files(m_CurrentIndex)
            End Get
        End Property

        Public ReadOnly Property Current1() As Object Implements System.Collections.IEnumerator.Current
            Get
                Return Current()
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements System.Collections.IEnumerator.MoveNext
            m_CurrentIndex += 1
            m_Reporter.SetCurrentItem(m_CurrentIndex)
            Return m_CurrentIndex < m_Files.Count
        End Function

        Public Sub Reset() Implements System.Collections.IEnumerator.Reset
            m_CurrentIndex = -1
        End Sub

#Region " IDisposable Support "

        Private disposedValue As Boolean = False        ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: free other state (managed objects).
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

End Class