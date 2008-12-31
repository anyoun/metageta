Imports System.Runtime.CompilerServices
Imports System.Text

Public Module LinqExtensions

    <Extension()> Public Function JoinToCsv(ByVal list As IEnumerable(Of String)) As String
        Dim bdr As New StringBuilder()
        Dim firstItem As Boolean = True

        For Each i As String In list
            If firstItem Then
                firstItem = False
            Else
                bdr.Append(",")
            End If
            Dim quoted = i.Replace("""", """""")
            If quoted.Contains(",") Then
                bdr.Append("""")
                bdr.Append(quoted)
                bdr.Append("""")
            Else
                bdr.Append(quoted)
            End If
        Next

        Return bdr.ToString()
    End Function

    <Extension()> Public Function JoinToString(ByVal list As IEnumerable(Of String), ByVal seperator As String) As String
        Dim bdr As New StringBuilder()
        Dim firstItem As Boolean = True

        For Each i As String In list
            If firstItem Then
                firstItem = False
            Else
                bdr.Append(seperator)
            End If
            bdr.Append(i)
        Next

        Return bdr.ToString()
    End Function

    <Extension()> Public Function JoinToString(Of T)(ByVal list As IEnumerable(Of T), _
                                              ByVal selector As Func(Of T, String), _
                                              ByVal separator As String) As String
        Return (From element In list Select selector(element)).JoinToString(separator)
    End Function

    <Extension()> Public Function JoinToCsv(Of T)(ByVal list As IEnumerable(Of T), _
                                              ByVal selector As Func(Of T, String)) As String
        Return (From element In list Select selector(element)).JoinToCsv()
    End Function

    <Extension()> Public Sub ForEach(Of T)(ByVal list As IEnumerable(Of T), ByVal action As Action(Of T))
        For Each item As T In list
            action(item)
        Next
    End Sub

    <Extension()> Public Sub AddRange(Of T)(ByVal collection As ICollection(Of T), ByVal items As IEnumerable(Of T))
        For Each item As T In items
            collection.Add(item)
        Next
    End Sub

End Module