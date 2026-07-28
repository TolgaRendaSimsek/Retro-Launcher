namespace RetroLauncher;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
#if DEBUG
        ReleaseAssetSelectorTests.RunTests();
        ArchiveExtractorTests.RunTestsAsync().GetAwaiter().GetResult();
        HttpPackageDownloaderTests.RunTestsAsync().GetAwaiter().GetResult();
        EmulatorUpdateServiceTests.RunTests();
        GitHubReleaseProviderTests.RunTests();
        EmulatorInstallationServiceTests.RunTests();
#endif
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }    
}