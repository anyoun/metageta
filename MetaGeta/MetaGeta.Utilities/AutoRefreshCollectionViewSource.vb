Imports System.Windows.Data
Imports System.Collections.Specialized
Imports System.ComponentModel

Public Class AutoRefreshCollectionViewSource
    Inherits CollectionViewSource

    Protected Overrides Sub OnSourceChanged(ByVal oldSource As Object, ByVal newSource As Object)
        If oldSource IsNot Nothing Then
            RemoveEventHandlers(oldSource)
        End If
        If newSource IsNot Nothing Then
            AddEventHandlers(newSource)
        End If

        MyBase.OnSourceChanged(oldSource, newSource)
    End Sub

    Private Sub AddEventHandlers(ByVal source As Object)
        If TypeOf source Is INotifyCollectionChanged Then
            AddHandler CType(source, INotifyCollectionChanged).CollectionChanged, AddressOf CollectionChanged
        End If

        For Each item In CType(source, IEnumerable)
            AddEventHandler(item)
        Next
    End Sub

    Private Sub RemoveEventHandlers(ByVal source As Object)
        If TypeOf source Is INotifyCollectionChanged Then
            RemoveHandler CType(source, INotifyCollectionChanged).CollectionChanged, AddressOf CollectionChanged
        End If

        For Each item In CType(source, IEnumerable)
            RemoveEventHandler(item)
        Next
    End Sub

    Private Sub AddEventHandler(ByVal item As Object)
        If TypeOf item Is INotifyPropertyChanged Then
            AddHandler CType(item, INotifyPropertyChanged).PropertyChanged, AddressOf PropertyChanged
        End If
    End Sub

    Private Sub RemoveEventHandler(ByVal item As Object)
        If TypeOf item Is INotifyPropertyChanged Then
            RemoveHandler CType(item, INotifyPropertyChanged).PropertyChanged, AddressOf PropertyChanged
        End If
    End Sub

    Private Sub CollectionChanged(ByVal sender As Object, ByVal e As NotifyCollectionChangedEventArgs)
        Select Case e.Action
            Case NotifyCollectionChangedAction.Add
                For Each item In e.NewItems
                    AddEventHandler(item)
                Next

            Case NotifyCollectionChangedAction.Remove
                For Each item In e.OldItems
                    RemoveEventHandler(item)
                Next

            Case NotifyCollectionChangedAction.Replace
                For Each item In e.OldItems
                    RemoveEventHandler(item)
                Next
                For Each item In e.NewItems
                    AddEventHandler(item)
                Next

            Case NotifyCollectionChangedAction.Move
            Case NotifyCollectionChangedAction.Reset
                Me.View.Refresh()

            Case Else
                Throw New NotSupportedException()
        End Select
    End Sub

    Private Sub PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
        If (From s In SortDescriptions Where s.PropertyName = e.PropertyName Select s).Any() Then
            Me.View.Refresh()
        ElseIf (From g In GroupDescriptions Where TypeOf g Is PropertyGroupDescription Let pg = CType(g, PropertyGroupDescription) Where pg.PropertyName = e.PropertyName Select pg).Any() Then
            Me.View.Refresh()
        End If
    End Sub
End Class
