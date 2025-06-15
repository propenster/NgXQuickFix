using Microsoft.Extensions.Configuration;
using NgXQuickFix;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

internal class Program
{
    public static IConfiguration Configuration { get; private set; }
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();


        var fixSettingsPath = Configuration["FixClientConfig:CfgPath"] ?? string.Empty;
        var username = Configuration["FixClientConfig:Username"];
        var password = Configuration["FixClientConfig:Password"];


        string file = args.Length > 0 && args[0] != null ? args[0] : fixSettingsPath;
        SessionSettings settings = new SessionSettings(file);
        IMyQuickFixApp myQuickFixApp = new MyQuickFixApp(username, password);
        IMessageStoreFactory factory = new FileStoreFactory(settings);
        ILogFactory logger = new FileLogFactory(settings);
        SocketInitiator initiator = new SocketInitiator(myQuickFixApp, factory, settings, logger);
        initiator.Start();
        myQuickFixApp.Run();
        initiator.Stop();
    }
}