Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Reflection

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

    <Extension()> Public Function SingleToEnumerable(Of T)(ByVal item As T) As IEnumerable(Of T)
        Return New T() {item}
    End Function

    <Extension()> Public Function IsDefined(Of T As Attribute)(ByVal info As MemberInfo, Optional ByVal inherit As Boolean = False) As Boolean
        Return info.IsDefined(GetType(T), inherit)
    End Function

    <Extension()> Public Function GetCustomAttribute(Of T As Attribute)(ByVal info As MemberInfo, Optional ByVal inherit As Boolean = False) As T
        Return info.GetCustomAttributes(GetType(T), inherit).Cast(Of T)().Single()
    End Function

    <Extension()> Public Function IndexInnerJoin(Of T, U)(ByVal left As IEnumerable(Of T), ByVal right As IEnumerable(Of U)) As IEnumerable(Of Tuple(Of T, U, Integer))
        Dim l = left.GetEnumerator(), r = right.GetEnumerator()
        Dim results As New List(Of Tuple(Of T, U, Integer))
        Dim i = 0
        While l.MoveNext() And r.MoveNext()
            results.Add(New Tuple(Of T, U, Integer)(l.Current, r.Current, i))
            i += 1
        End While
        Return results
    End Function

    <Extension()> Public Function Agree(Of T, TResult)(ByVal collection As IEnumerable(Of T), ByVal f As Func(Of T, TResult)) As TResult
        Dim results = (From item In collection Select f(item)).ToArray()
        If results.Count = 0 Then Return Nothing
        Dim firstItem = results.First()
        For Each r In results
            If Not firstItem.Equals(r) Then Return Nothing
        Next
        Return firstItem
    End Function

    <Extension()> Public Function Coalesce(Of T)(ByVal collection As IEnumerable(Of T)) As T
        For Each item In collection
            If item IsNot Nothing Then Return item
        Next
        Return Nothing
    End Function
    <Extension()> Public Function Coalesce(Of T)(ByVal item As T, ByVal ParamArray rest As T()) As T
        Return If(item IsNot Nothing, item, rest.Coalesce())
    End Function
End Module