Imports System.Security.Policy
Imports System.Security
Imports System.Security.Permissions
Imports Microsoft.Scripting.Hosting


Module Main

    Sub Main(ByVal args As String())
        Dim engine = IronPython.Hosting.Python.CreateEngine()
        Dim scope = engine.CreateScope()

        Dim dsm As New DataStoreManager
        Dim host As New ScriptInterface

        scope.SetVariable("dsm", dsm)
        scope.SetVariable("host", host)
        Dim scriptFile = args(0)

        Try
            engine.ExecuteFile(scriptFile, scope)
        Catch ex As Exception
            Dim es = engine.GetService(Of ExceptionOperations)()
            Console.Error.WriteLine(es.FormatException(ex))
        End Try

        Console.WriteLine("Done. Press any key to continue...")
        Console.ReadKey()
    End Sub

End Module
