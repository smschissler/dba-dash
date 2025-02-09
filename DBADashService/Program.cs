﻿using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using Topshelf;

namespace DBADashService
{
    internal class Program
    {
        public static readonly NamedLocker Locker = new();

        private static void Main(string[] args)
        {
            SetupLogging();
            Console.WriteLine(Properties.Resources.LogoText);
            Log.Information("Running as service {RunningAsService}", !Environment.UserInteractive);
            var cfg = SchedulerServiceConfig.Config;

            if (DBADash.Upgrade.IsUpgradeIncomplete)
            {
                const string message = $"Incomplete upgrade of DBA Dash detected.  File '{DBADash.Upgrade.UpgradeFile}' found in directory. Upgrade might have failed due to locked files. More info: https://dbadash.com/upgrades/";
                Log.Logger.Error(message);
                throw new Exception(message);
            }

            var rc = HostFactory.Run(x =>
            {
                x.Service<ScheduleService>(s =>
                {
                    s.ConstructUsing(name => new ScheduleService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.StartAutomaticallyDelayed();
                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(1);
                });

                x.SetDescription("DBADash Service - SQL Server monitoring tool");
                Log.Logger.Information("Service Name {ServiceName}", cfg.ServiceName);
                x.SetDisplayName(cfg.ServiceName);
                x.SetServiceName(cfg.ServiceName);
            });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }

        private static void SetupLogging()
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory); //  for Logs folder
            // https://swimburger.net/blog/dotnet/changing-serilog-minimum-level-without-application-restart-on-dotnet-framework-and-core
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                // reloadOnChange will allow you to auto reload the minimum level and level switches
                .AddJsonFile(path: "serilog.json", optional: false, reloadOnChange: true)
                .Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("ApplicationName", "DBADash")
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .CreateLogger();
        }
    }
}