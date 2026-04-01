using Bloxstrap.AppData;
using Bloxstrap.Integrations;

namespace Bloxstrap
{
    public class Watcher : IDisposable
    {
        private readonly InterProcessLock _lock = new("Watcher");

        private readonly WatcherData? _watcherData;
        
        private readonly NotifyIconWrapper? _notifyIcon;

        public readonly WindowManipulation? WindowManipulation;

        public Watcher()
        {
            const string LOG_IDENT = "Watcher";


            if (!_lock.IsAcquired)
            {
                App.Logger.WriteLine(LOG_IDENT, "Watcher instance already exists");
                return;
            }

            string? watcherDataArg = App.LaunchSettings.WatcherFlag.Data;

            if (String.IsNullOrEmpty(watcherDataArg))
            {
#if DEBUG
                string path = new RobloxPlayerData().ExecutablePath;
                if (!File.Exists(path))
                    throw new ApplicationException("Roblox player is not been installed");

                using var gameClientProcess = Process.Start(path);

                while (gameClientProcess.MainWindowHandle == IntPtr.Zero)
                    Thread.Sleep(100);

                _watcherData = new() { ProcessId = gameClientProcess.Id, Handle = gameClientProcess.MainWindowHandle.ToInt64() };
#else
                throw new Exception("Watcher data not specified");
#endif
            }
            else
            {
                _watcherData = JsonSerializer.Deserialize<WatcherData>(Encoding.UTF8.GetString(Convert.FromBase64String(watcherDataArg)));
            }

            if (_watcherData is null)
                throw new Exception("Watcher data is invalid");

            WindowManipulation = new(_watcherData.Handle, _watcherData.ProcessId);

            _notifyIcon = new(this);
        }

        public void KillRobloxProcess() => CloseProcess(_watcherData!.ProcessId, true);

        private void KillAllRobloxProcessesForCurrentInstall()
        {
            const string LOG_IDENT = "Watcher::KillAllRobloxProcessesForCurrentInstall";

            if (_watcherData is null)
                return;

            string? installDirectory = null;

            try
            {
                using var trackedProcess = Process.GetProcessById(_watcherData.ProcessId);
                installDirectory = Path.GetDirectoryName(trackedProcess.MainModule?.FileName ?? trackedProcess.ProcessName);
            }
            catch
            {
                // fall back to broad cleanup below
            }

            var processNames = new[] { "RobloxPlayerBeta", "RobloxCrashHandler" };

            foreach (string processName in processNames)
            {
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    try
                    {
                        string? processDirectory = null;

                        try
                        {
                            processDirectory = Path.GetDirectoryName(process.MainModule?.FileName ?? process.ProcessName);
                        }
                        catch
                        {
                            // if path inspection fails, still allow kill for the tracked player process
                        }

                        bool sameInstall = installDirectory is null
                            || processDirectory is null
                            || String.Equals(processDirectory, installDirectory, StringComparison.OrdinalIgnoreCase);

                        if (!sameInstall)
                            continue;

                        App.Logger.WriteLine(LOG_IDENT, $"Force killing lingering {process.ProcessName} process (pid={process.Id})");
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Failed to kill lingering {processName} process");
                        App.Logger.WriteException(LOG_IDENT, ex);
                    }
                }
            }
        }

        public void CloseProcess(int pid, bool force = false)
        {
            const string LOG_IDENT = "Watcher::CloseProcess";

            try
            {
                using var process = Process.GetProcessById(pid);

                App.Logger.WriteLine(LOG_IDENT, $"Killing process '{process.ProcessName}' (pid={pid}, force={force})");

                if (process.HasExited)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"PID {pid} has already exited");
                    return;
                }

                if (force)
                    process.Kill();
                else
                    process.CloseMainWindow();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"PID {pid} could not be closed");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public async Task Run()
        {
            if (!_lock.IsAcquired || _watcherData is null)
                return;

            DateTimeOffset? noWindowSince = null;

            WindowManipulation?.ApplyWindowModifications();

            while (Utilities.GetProcessesSafe().Any(x => x.Id == _watcherData.ProcessId))
            {
                if (_watcherData.IsPlayer)
                {
                    try
                    {
                        using var process = Process.GetProcessById(_watcherData.ProcessId);
                        process.Refresh();

                        if (process.MainWindowHandle == IntPtr.Zero)
                        {
                            noWindowSince ??= DateTimeOffset.Now;

                            if (DateTimeOffset.Now - noWindowSince >= TimeSpan.FromSeconds(5))
                            {
                                App.Logger.WriteLine("Watcher::Run", $"Process {_watcherData.ProcessId} has no main window after close grace period, force closing all Roblox processes for this install");
                                KillAllRobloxProcessesForCurrentInstall();
                                break;
                            }
                        }
                        else
                        {
                            noWindowSince = null;
                        }
                    }
                    catch
                    {
                        break;
                    }
                }

                await Task.Delay(1000);
            }

            if (_watcherData.IsPlayer)
            {
                try
                {
                    using var process = Process.GetProcessById(_watcherData.ProcessId);
                    process.Refresh();

                    if (!process.HasExited && process.MainWindowHandle == IntPtr.Zero)
                    {
                        App.Logger.WriteLine("Watcher::Run", $"Process {_watcherData.ProcessId} is still headless at watcher shutdown, force closing all Roblox processes for this install");
                        KillAllRobloxProcessesForCurrentInstall();
                    }
                }
                catch
                {
                    // process already exited, nothing left to do
                }
            }

            if (_watcherData.AutoclosePids is not null)
            {
                foreach (int pid in _watcherData.AutoclosePids)
                    CloseProcess(pid);
            }

            if (App.LaunchSettings.TestModeFlag.Active)
                Process.Start(Paths.Process, "-settings -testmode");
        }

        public void Dispose()
        {
            App.Logger.WriteLine("Watcher::Dispose", "Disposing Watcher");

            _notifyIcon?.Dispose();

            App.State.Prop.WatcherRunning = false;

            GC.SuppressFinalize(this);
        }
    }
}
