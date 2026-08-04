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