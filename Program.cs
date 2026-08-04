namespace RetroLauncher;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        if (RetroLauncher.Services.Updates.ApplicationUpdateInstaller.RunUpdaterCLIIfRequested(args))
        {
            return;
        }

        ApplicationConfiguration.Initialize();
        ApplicationPaths.EnsureDirectoriesExist();

        var verProvider = RetroLauncher.Core.Utilities.ApplicationVersionProvider.Instance;
        string startupMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [STARTUP INFO]{Environment.NewLine}" +
                            $"  ProcessPath: '{verProvider.ProcessPath}'{Environment.NewLine}" +
                            $"  BaseDirectory: '{verProvider.BaseDirectory}'{Environment.NewLine}" +
                            $"  AssemblyVersion: '{verProvider.AssemblyVersionString}'{Environment.NewLine}" +
                            $"  FileVersion: '{verProvider.FileVersionString}'{Environment.NewLine}" +
                            $"  BuildConfiguration: '{verProvider.BuildConfiguration}'{Environment.NewLine}" +
                            $"  ExecutableTimestamp: '{verProvider.ExecutableTimestampString}'{Environment.NewLine}";

        try
        {
            File.AppendAllText(Path.Combine(ApplicationPaths.LogsDir, "startup.log"), startupMsg);
        }
        catch { }

        RetroLauncher.Services.Logging.RetroLogger.Log(startupMsg, "STARTUP");
#if DEBUG
        ReleaseAssetSelectorTests.RunTests();
        ArchiveExtractorTests.RunTestsAsync().GetAwaiter().GetResult();
        HttpPackageDownloaderTests.RunTestsAsync().GetAwaiter().GetResult();
        EmulatorUpdateServiceTests.RunTests();
        GitHubReleaseProviderTests.RunTests();
        EmulatorInstallationServiceTests.RunTests();
        BiosSynchronizationServiceTests.RunTests();
        ControllerSyncServiceTests.RunTests();
        EmulatorInstallationActionTests.RunTests();
        EmulatorManagerLayoutTests.RunTests();
        MainWindowLayoutTests.RunTests();
        RetroLauncher.Tests.Unit.ApplicationUpdateServiceTests.RunTestsAsync().GetAwaiter().GetResult();
        RetroLauncher.Tests.Unit.GameLaunchServiceTests.RunTestsAsync().GetAwaiter().GetResult();
#endif
        Application.Run(new MainForm());
    }    
}