Imports System.Runtime.CompilerServices

Public Module DbHelpers
    <Extension()> Public Sub AddParam(ByVal cmd As System.Data.Common.DbCommand, ByVal value As Object)
        Dim param = cmd.CreateParameter()
        param.Value = value
        cmd.Parameters.Add(param)
    End Sub
End Module
