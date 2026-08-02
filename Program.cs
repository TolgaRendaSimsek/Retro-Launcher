namespace RetroLauncher;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        ApplicationPaths.EnsureDirectoriesExist();
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
#endif
        Application.Run(new MainForm());
    }    
}