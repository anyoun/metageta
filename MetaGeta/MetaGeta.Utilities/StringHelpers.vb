Imports System.Text

Public Class StringHelpers
    Public Shared Function MultiplyString(ByVal s As String, ByVal count As Integer) As String
        Dim sb As New StringBuilder()

        For index As Integer = 0 To count
            sb.Append(s)
        Next

        Return sb.ToString()
    End Function

    Public Shared Sub DrawProgressBar(ByVal progress As Integer, ByVal total As Integer)
        Dim width = 60

        Console.CursorLeft = 0
        Console.Write("[")
        Console.CursorLeft = width + 2
        Console.Write("]")
        Console.CursorLeft = 1
        Dim onechunk = width / total

        Dim position = 1

        For i As Integer = 0 To CInt(onechunk * progress)
            Console.BackgroundColor = ConsoleColor.Green
            Console.CursorLeft = position
            position += 1
            Console.Write(" ")
        Next

        For i As Integer = position To width + 1
            Console.BackgroundColor = ConsoleColor.Black
            Console.CursorLeft = position
            position += 1
            Console.Write(" ")
        Next


        Console.CursorLeft = width + 5
        Console.BackgroundColor = ConsoleColor.Black
        Console.Write(progress.ToString() + " of " + total.ToString() + "    ")
    End Sub
    Public Shared Sub DrawProgressBar(ByVal progress As Double, Optional ByVal extraText As String = "")
        Dim width = 60

        Console.CursorTop -= 1
        Console.CursorLeft = 0
        Console.Write(extraText)

        Console.CursorTop += 1

        Console.CursorLeft = 0
        Console.Write("[")
        Console.CursorLeft = width + 2
        Console.Write("]")
        Console.CursorLeft = 1

        Dim position = 1

        For i As Integer = 0 To CInt(width * progress)
            Console.BackgroundColor = ConsoleColor.Green
            Console.CursorLeft = position
            position += 1
            Console.Write(" ")
        Next

        For i As Integer = position To width + 1
            Console.BackgroundColor = ConsoleColor.Gray
            Console.CursorLeft = position
            position += 1
            Console.Write(" ")
        Next


        Console.CursorLeft = width + 5
        Console.BackgroundColor = ConsoleColor.Black
        Console.Write(progress.ToString("#0%"))
    End Sub

End Class
