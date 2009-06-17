Imports System.Reflection
Imports System.ComponentModel

Public Class NavigationTab
    Inherits DependancyObjectNotifyPropertyChanged

    Public Shared ReadOnly GroupProperty As DependencyProperty = DependencyProperty.Register("Group", GetType(String), MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_View As UserControl
    Private ReadOnly m_Icon As ImageSource
    Private ReadOnly m_Caption As String
    Private m_Group As String

    Public Sub New(ByVal view As UserControl, ByVal icon As ImageSource, ByVal caption As String)
        m_View = view
        m_Icon = icon
        m_Caption = caption
    End Sub

    Public ReadOnly Property Icon() As ImageSource
        Get
            Return m_Icon
        End Get
    End Property
    Public ReadOnly Property Caption() As String
        Get
            Return m_Caption
        End Get
    End Property
    Public Property Group() As String
        Get
            Return CStr(GetValue(GroupProperty))
        End Get
        Set(ByVal value As String)
            SetValue(GroupProperty, value)
        End Set
    End Property

    Public ReadOnly Property View() As UserControl
        Get
            Return m_View
        End Get
    End Property

End Class

Public Class SeparatorNavigationTab
    Inherits NavigationTab

    Public Sub New()
        MyBase.New(New EmptyView(), Nothing, Nothing)
    End Sub
End Class