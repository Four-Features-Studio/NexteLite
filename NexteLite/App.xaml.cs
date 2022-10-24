using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexteLite.Interfaces;
using NexteLite.Pages;
using NexteLite.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NexteLite
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IPagesRepository PagesRepository { get; set; }

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

            services.AddSingleton<IPagesRepository,PagesRepository>();

            services.AddSingleton<IMainWindow,MainWindow>();
            services.AddSingleton<ICoreLaucnher, CoreLauncher>();
            services.AddSingleton<ISettingsLauncher, SettingsLauncher>();

            services.AddSingleton<LoginPage>();
            services.AddSingleton<MainPage>();
            services.AddSingleton<ConsolePage>();
            services.AddSingleton<SettingsPage>();

            services.AddTransient<IWebService, WebService>();

            //var serilogLogger = new LoggerConfiguration()
            //    .MinimumLevel.Debug()
            //    .Enrich.FromLogContext()
            //    .WriteTo.Console()
            //    .WriteTo.File("C:\\Services\\logs\\log-.txt", rollingInterval: RollingInterval.Day)
            //    .CreateLogger();
            //serilogLogger.Information("Init main");

            //services.AddLogging(builder =>
            //{
            //    builder.AddSerilog(logger: serilogLogger, dispose: true);
            //});
        }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        }

        public void Configure()
        {
           
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            var core = ServiceProvider.GetRequiredService<ICoreLaucnher>();
        }
    }
}
