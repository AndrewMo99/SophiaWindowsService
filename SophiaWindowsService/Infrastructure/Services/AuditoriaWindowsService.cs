using Autofac;
using SophiaWindowsService.Application.Abstractions;
using SophiaWindowsService.Application.Extensions;
using SophiaWindowsService.Infrastructure.Jobs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;

namespace SophiaWindowsService.Infrastructure.Services
{
    public partial class AuditoriaWindowsService : ServiceBase
    {
        private Timer _timer;
        private readonly ILifetimeScope _autofacScope;

        public AuditoriaWindowsService(ILifetimeScope autofacScope)
        {
            InitializeComponent();
            _autofacScope = autofacScope;
        }

        protected override void OnStart(string[] args)
        {
            int serviceInterval;

            using (var startupScope = _autofacScope.BeginLifetimeScope())
            {
                var job = startupScope.Resolve<ParametricaJob>();
                job.Execute();

                var appConfig = startupScope.Resolve<IAppConfig>();
                serviceInterval = appConfig.ParametricaResult.SophiaWindowsServiceInterval;
            }

            if (serviceInterval <= 0) return;

            _timer = new Timer();
            _timer.Interval = serviceInterval * 60 * 1000;
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();

            Task.Run(ExecuteScheduledTask);
        }

        protected override void OnStop()
        {
            if (_timer == null) return;
            _timer.Stop();
            _timer.Dispose();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            ExecuteScheduledTask();
        }

        private void ExecuteScheduledTask()
        {
            try
            {
                using (var scopePerRun = _autofacScope.BeginLifetimeScope())
                {
                    LogExtensions.WriteEventLog("Service step running successfully!", EventLogEntryType.Information);

                    var jobsToRun = scopePerRun.Resolve<IEnumerable<IJob>>();

                    foreach (var job in jobsToRun)
                    {
                        job.Execute();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.GetErrorMessage().WriteLog();
            }
        }
    }
}