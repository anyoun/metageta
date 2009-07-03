Imports MetaGeta.DataStore

Module Main

    Sub Main(ByVal args As String())
        log4net.Config.XmlConfigurator.Configure()

        DataStoreManager.IsInDesignMode = False
        Dim dsm As New DataStoreManager()

        For Each ds In dsm.DataStores
            Console.WriteLine("Refreshing {0}...", ds.Name)
            ds.EnqueueRefreshFileSources()
        Next

        dsm.WaitForQueueToEmpty()

        Console.WriteLine("Done.")
        dsm.Shutdown()
    End Sub

End Module
