Public Interface IProgressReporter
    Inherits INotifyPropertyChanged

    ReadOnly Property AllItems() As IEnumerable
    ReadOnly Property CompletedItems() As IEnumerable

    ReadOnly Property TotalPercentDone() As Double
    ReadOnly Property IsDone() As Boolean

    ReadOnly Property CurrentItem() As Object
    ReadOnly Property CurrentItemPercentDone() As Double
End Interface
