Imports MetaGeta.DataStore

Module Main

    Sub Main(ByVal args As String())
        DataStoreManager.IsInDesignMode = False
        Dim dsm As New DataStoreManager()
        dsm.Startup()

        For Each ds In dsm.DataStores
            Console.WriteLine("Refreshing {0}...", ds.Name)
            ds.RefreshFileSources()
        Next

        Console.WriteLine("Done.")
        dsm.Shutdown()
    End Sub

End Module
