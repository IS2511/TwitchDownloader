﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using TwitchDownloaderCore;
using TwitchDownloaderCore.Options;

namespace TwitchDownloader.TwitchTasks
{
    class VodDownloadTask : ITwitchTask
    {
        public TaskData Info { get; set; } = new TaskData();
        public int Progress { get; set; }
        public TwitchTaskStatus Status { get; set; } = TwitchTaskStatus.Ready;
        public VideoDownloadOptions DownloadOptions { get; set; }
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();
        public ITwitchTask DependantTask { get; set; }
        public string TaskType { get; set; } = "VOD Download";
        public event PropertyChangedEventHandler PropertyChanged;

        public async Task RunAsync()
        {
            VideoDownloader downloader = new VideoDownloader(DownloadOptions);
            Progress<ProgressReport> progress = new Progress<ProgressReport>();
            progress.ProgressChanged += Progress_ProgressChanged;
            Status = TwitchTaskStatus.Running;
            OnPropertyChanged("Status");
            try
            {
                await downloader.DownloadAsync(progress, TokenSource.Token);
                if (TokenSource.IsCancellationRequested)
                {
                    Status = TwitchTaskStatus.Cancelled;
                    OnPropertyChanged("Status");
                }
                else
                {
                    Status = TwitchTaskStatus.Finished;
                    Progress = 100;
                    OnPropertyChanged("Progress");
                    OnPropertyChanged("Status");
                }
            }
            catch
            {
                Status = TwitchTaskStatus.Failed;
                OnPropertyChanged("Status");
            }
        }

        public void Cancel()
        {
            Status = TwitchTaskStatus.Stopping;
            OnPropertyChanged("Status");
            TokenSource.Cancel();
        }

        private void Progress_ProgressChanged(object sender, ProgressReport e)
        {
            if (e.reportType == ReportType.Percent)
            {
                int percent = (int)e.data;
                if (percent > Progress)
                {
                    Progress = percent;
                    OnPropertyChanged("Progress");
                }
            }
        }

        public bool CanRun()
        {
            return Status == TwitchTaskStatus.Ready;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
