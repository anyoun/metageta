Imports System.Collections.ObjectModel
Imports System.ComponentModel

Namespace Editors
    Partial Public MustInherit Class ItemListEditor
        Private Shared ReadOnly ItemsProperty As DependencyProperty = DependencyProperty.Register("Items", GetType(ReadOnlyCollection(Of String)), GetType(ItemListEditor))

        Public Sub New()
            ' This call is required by the Windows Form Designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
        End Sub

        Protected MustOverride Function CreateItem() As String

        Public Property Items() As ReadOnlyCollection(Of String)
            Get
                Return CType(GetValue(ItemsProperty), ReadOnlyCollection(Of String))
            End Get
            Set(ByVal value As ReadOnlyCollection(Of String))
                SetValue(ItemsProperty, value)
            End Set
        End Property

        Public Property ItemTemplate() As DataTemplate
            Get
                Return CType(GetValue(ItemsControl.ItemTemplateProperty), DataTemplate)
            End Get
            Set(ByVal value As DataTemplate)
                SetValue(ItemsControl.ItemTemplateProperty, value)
            End Set
        End Property

        Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
            Dim item = CreateItem()
            If item IsNot Nothing Then
                Items = New ReadOnlyCollection(Of String)(Items.Union(item.SingleToEnumerable()).ToArray())
            End If
        End Sub

        Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
            Items = New ReadOnlyCollection(Of String)(Items.Where(Function(s) s IsNot Me.ListBox1.SelectedItem).ToArray())
        End Sub

    End Class
End Namespace