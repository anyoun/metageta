Public MustInherit Class NavigationTab
    Implements INotifyPropertyChanged

    'Private m_Icon As ImageSource
    'Private m_Caption As String
    'Private m_Parent As NavigationTabGroupBase

    Public Sub New()
    End Sub

    'Public Property Icon() As ImageSource
    '    Get
    '        Return m_Icon
    '    End Get
    '    Set(ByVal value As ImageSource)
    '        If m_Icon IsNot value Then
    '            m_Icon = value
    '            OnPropertyChanged("Icon")
    '        End If
    '    End Set
    'End Property

    'Public Property Caption() As String
    '    Get
    '        Return m_Caption
    '    End Get
    '    Set(ByVal value As String)
    '        If m_Caption IsNot value Then
    '            m_Caption = value
    '            OnPropertyChanged("Caption")
    '        End If
    '    End Set
    'End Property

    'Public Property Parent() As NavigationTabGroupBase
    '    Get
    '        Return m_Parent
    '    End Get
    '    Set(ByVal value As NavigationTabGroupBase)
    '        If m_Parent IsNot value Then
    '            m_Parent = value
    '            OnPropertyChanged("Parent")
    '        End If
    '    End Set
    'End Property
    Public MustOverride ReadOnly Property Icon() As ImageSource
    Public MustOverride ReadOnly Property Caption() As String

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Protected Sub OnPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
End Class
