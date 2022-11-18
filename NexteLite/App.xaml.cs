using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexteLite.Interfaces;
using NexteLite.Pages;
using NexteLite.Services;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.AxHost;

namespace NexteLite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        public string Args { get; set; }


        /// <summary>The event mutex name.</summary>
        private const string UniqueEventName = "NexteLiteAgentProcess";

        /// <summary>The event wait handle.</summary>
        private EventWaitHandle eventWaitHandle;

        public IServiceProvider ServiceProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private void ConfigureServices(IServiceCollection services)
        {
            using (var resourceStream = this.GetType().Assembly.GetManifestResourceStream("NexteLite.appsettings.json"))
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonStream(resourceStream);
                Configuration = builder.Build();
            }

            /*services.AddDbContext<EmployeeDbContext>(options =>
            {
                options.UseSqlite("Data Source = Employee.db");
            });*/

            services.Configure<AppSettings>(Configuration);

            services.AddHttpClient<IWebService, WebService>();

            services.AddSingleton<IPagesRepository,PagesRepository>();

            services.AddSingleton<IMainWindow,MainWindow>();
            services.AddSingleton<ICoreLaucnher, CoreLauncher>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<ISettingsLauncher, SettingsLauncher>();
            services.AddSingleton<IMineStat, MineStat>();

            services.AddSingleton<IPathRepository, PathRepository>();
            services.AddSingleton<IAccountService, AccountService>();

            services.AddSingleton<LoginPage>();
            services.AddSingleton<MainPage>();
            services.AddSingleton<ConsolePage>();
            services.AddSingleton<SettingsPage>();
            services.AddSingleton<UpdatePage>();

            services.AddSingleton<IMinecraftService, MinecraftService>();

            services.AddSingleton<IMessageService, MessageService>();

            services.AddTransient<DownloadingPage>();
            services.AddTransient<RunningPage>();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(CreateLogger());
            });
        }

        Logger CreateLogger()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appdata, "FourFeatures", "NexteLite", "logs", "log-.txt");

            var serilogLogger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(path, rollingInterval: RollingInterval.Hour)
                    .CreateLogger();

            return serilogLogger;
        }


        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            FreeConsole();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
            Exception ex = (Exception)e.ExceptionObject;
            logger.LogCritical($"Необработанное исключение - {ex.ToString()}");  
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var args = e.Args;

            if (args.Any(x => x == "-debug" || x == "-d"))
                AllocConsole();

            Args = string.Join(" ", args);

            var services = new ServiceCollection();

            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var logger = ServiceProvider.GetRequiredService<ILogger<App>>();

            var ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            logger.LogInformation($"Nexte Lite Launcher v{ver} - Author FourFeatures Studio, design - Wasdiv");

            var core = ServiceProvider.GetRequiredService<ICoreLaucnher>();
        }
    }
}
