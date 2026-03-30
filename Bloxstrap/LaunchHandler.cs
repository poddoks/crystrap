﻿using System.Windows;

using Windows.Win32;
using Windows.Win32.Foundation;

using Bloxstrap.AppData;
using Bloxstrap.UI.Elements.Dialogs;
using Bloxstrap.Integrations;

namespace Bloxstrap
{
    public static class LaunchHandler
    {
        private static int _menuUpdateCheckQueued;

        private static bool TryUpdateBeforeMenuLaunch()
        {
            const string LOG_IDENT = "LaunchHandler::TryUpdateBeforeMenuLaunch";

            if (App.LaunchSettings.BypassUpdateCheck || App.LaunchSettings.UpgradeFlag.Active || !App.Settings.Prop.CheckForUpdates)
                return false;

            if (Interlocked.Exchange(ref _menuUpdateCheckQueued, 1) == 1)
                return false;

            App.Logger.WriteLine(LOG_IDENT, "Queueing background update prompt check before opening the menu");

            _ = Task.Run(async () =>
            {
                try
                {
                    var releaseInfo = await App.GetLatestRelease();

                    if (releaseInfo is null)
                        return;

                    var versionComparison = Utilities.CompareVersions(App.Version, releaseInfo.TagName);

                    if (App.IsProductionBuild && versionComparison == VersionComparison.Equal || versionComparison == VersionComparison.GreaterThan)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "No updates found for menu launch");
                        return;
                    }

                    var asset = App.GetLatestReleaseAsset(releaseInfo);
                    string downloadUrl = asset?.BrowserDownloadUrl ?? $"{App.ProjectDownloadLink}/releases/latest/download/{App.ProjectReleaseAssetName}";

                    App.Logger.WriteLine(LOG_IDENT, $"Update {releaseInfo.TagName} is available for menu launch");

                    var result = Frontend.ShowMessageBox(
                        $"Crystrap {releaseInfo.TagName} is available. Would you like to download it now?",
                        MessageBoxImage.Information,
                        MessageBoxButton.YesNo
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Opening browser download for {releaseInfo.TagName}");
                        Utilities.ShellExecute(downloadUrl);
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            });

            return false;
        }

        private static void RefreshPostInstallState()
        {
            App.Settings.Load();
            App.State.Load();
            App.RobloxState.Load();
            App.FastFlags.Load();
            App.GlobalSettings.Load();
            App.GlobalSettings.SetReadOnly(false);
            App.GlobalSettings.previousReadOnlyState = false;
        }

        private static void StartInstalledCrystrap(string arguments, string logIdent)
        {
            App.Logger.WriteLine(logIdent, $"Starting installed Crystrap from {Paths.Application} with arguments '{arguments}'");

            Process.Start(new ProcessStartInfo
            {
                FileName = Paths.Application,
                Arguments = arguments,
                WorkingDirectory = Paths.Base,
                UseShellExecute = true
            });
        }

        private static bool InstallRobloxThenOpenSettingsIfNeeded()
        {
            const string LOG_IDENT = "LaunchHandler::InstallRobloxThenOpenSettingsIfNeeded";

            var playerData = new RobloxPlayerData();
            bool playerInstalled = File.Exists(playerData.ExecutablePath);

            App.Logger.WriteLine(LOG_IDENT, $"Roblox player present: {playerInstalled} ({playerData.ExecutablePath})");

            if (playerInstalled)
                return false;

            App.Logger.WriteLine(LOG_IDENT, "Roblox player is not installed yet, installing it before opening settings");
            App.LaunchSettings.NoLaunchFlag.Active = true;
            LaunchRoblox(LaunchMode.Player, true);
            return true;
        }

        private static void CompleteQuietInstall()
        {
            const string LOG_IDENT = "LaunchHandler::CompleteQuietInstall";

            App.Logger.WriteLine(LOG_IDENT, "Installation finished, refreshing state before post-install handoff");
            RefreshPostInstallState();

            if (InstallRobloxThenOpenSettingsIfNeeded())
            {
                App.Logger.WriteLine(LOG_IDENT, "Roblox install bootstrapper was started, current installer process will exit after bootstrapper handoff");
                return;
            }

            App.Logger.WriteLine(LOG_IDENT, "Roblox is already installed, opening installed Crystrap settings");
            StartInstalledCrystrap("-settings", LOG_IDENT);
            App.Logger.WriteLine(LOG_IDENT, "Installed Crystrap launched successfully, terminating installer process");
            App.Terminate();
        }

        private static bool HandoffInteractiveInstallToInstalledCrystrap(NextAction closeAction)
        {
            const string LOG_IDENT = "LaunchHandler::HandoffInteractiveInstallToInstalledCrystrap";

            if (String.Equals(Paths.Process, Paths.Application, StringComparison.OrdinalIgnoreCase))
                return false;

            if (closeAction != NextAction.LaunchSettings)
                return false;

            App.Logger.WriteLine(LOG_IDENT, "Installer was launched from a temporary executable, handing off to the installed Crystrap settings window");
            StartInstalledCrystrap("-settings", LOG_IDENT);
            App.Logger.WriteLine(LOG_IDENT, "Installed Crystrap launched successfully, terminating installer process");
            App.Terminate();
            return true;
        }

        public static void ProcessNextAction(NextAction action, bool isUnfinishedInstall = false)
        {
            const string LOG_IDENT = "LaunchHandler::ProcessNextAction";

            switch (action)
            {
                case NextAction.LaunchSettings:
                    App.Logger.WriteLine(LOG_IDENT, "Opening settings");
                    LaunchSettings();
                    break;

                case NextAction.LaunchRoblox:
                    App.Logger.WriteLine(LOG_IDENT, "Opening Roblox");
                    LaunchRoblox(LaunchMode.Player);
                    break;

                case NextAction.LaunchRobloxThenSettings:
                    App.Logger.WriteLine(LOG_IDENT, "Opening Roblox, then settings");
                    LaunchRoblox(LaunchMode.Player, true);
                    break;

                case NextAction.LaunchRobloxStudio:
                    App.Logger.WriteLine(LOG_IDENT, "Opening Roblox Studio");
                    LaunchRoblox(LaunchMode.Studio);
                    break;

                case NextAction.LaunchPureRoblox:
                    App.Logger.WriteLine(LOG_IDENT, "Opening Pure Roblox");
                    LaunchPureRoblox();
                    break;

                default:
                    App.Logger.WriteLine(LOG_IDENT, "Closing");
                    App.Terminate(isUnfinishedInstall ? ErrorCode.ERROR_INSTALL_USEREXIT : ErrorCode.ERROR_SUCCESS);
                    break;
            }
        }

        public static void ProcessLaunchArgs()
        {
            const string LOG_IDENT = "LaunchHandler::ProcessLaunchArgs";

            // this order is specific

            if (App.LaunchSettings.UninstallFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening uninstaller");
                LaunchUninstaller();
            }
            else if (App.LaunchSettings.MenuFlag.Active)
            {
                TryUpdateBeforeMenuLaunch();
                App.Logger.WriteLine(LOG_IDENT, "Opening settings");
                LaunchSettings();
            }
            else if (App.LaunchSettings.WatcherFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening watcher");
                LaunchWatcher();
            }
            else if (App.LaunchSettings.MultiInstanceWatcherFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening multi-instance watcher");
                LaunchMultiInstanceWatcher();
            }
            else if (App.LaunchSettings.BackgroundUpdaterFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening background updater");
                LaunchBackgroundUpdater();
            }
            else if (App.LaunchSettings.RobloxLaunchMode != LaunchMode.None)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Opening bootstrapper ({App.LaunchSettings.RobloxLaunchMode})");
                LaunchRoblox(App.LaunchSettings.RobloxLaunchMode);
            }
            else if (App.LaunchSettings.BloxshadeFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Opening Bloxshade");
                LaunchBloxshadeConfig();
            }
            else if (!App.LaunchSettings.QuietFlag.Active)
            {
                TryUpdateBeforeMenuLaunch();
                App.Logger.WriteLine(LOG_IDENT, "Opening menu");
                LaunchMenu();
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, "Closing - quiet flag active");
                App.Terminate();
            }
        }

        public static void LaunchInstaller()
        {
            using var interlock = new InterProcessLock("Installer");

            if (!interlock.IsAcquired)
            {
                Frontend.ShowMessageBox(Strings.Dialog_AlreadyRunning_Installer, MessageBoxImage.Stop);
                App.Terminate();
                return;
            }

            if (App.LaunchSettings.UninstallFlag.Active)
            {
                Frontend.ShowMessageBox(Strings.Bootstrapper_FirstRunUninstall, MessageBoxImage.Error);
                App.Terminate(ErrorCode.ERROR_INVALID_FUNCTION);
                return;
            }

            if (App.LaunchSettings.QuietFlag.Active)
            {
                var installer = new Installer();

                if (!installer.CheckInstallLocation())
                    App.Terminate(ErrorCode.ERROR_INSTALL_FAILURE);

                installer.DoInstall();

                interlock.Dispose();
                CompleteQuietInstall();
            }
            else
            {
#if QA_BUILD
                Frontend.ShowMessageBox("You are about to install a QA build of Bloxstrap. The red window border indicates that this is a QA build.\n\nQA builds are handled completely separately of your standard installation, like a virtual environment.", MessageBoxImage.Information);
#endif

                new LanguageSelectorDialog().ShowDialog();

                var installer = new UI.Elements.Installer.MainWindow();
                installer.ShowDialog();

                interlock.Dispose();

                if (installer.Finished)
                {
                    RefreshPostInstallState();
                    
                    if (installer.CloseAction == NextAction.LaunchSettings && InstallRobloxThenOpenSettingsIfNeeded())
                        return;

                    if (HandoffInteractiveInstallToInstalledCrystrap(installer.CloseAction))
                        return;
                }

                ProcessNextAction(installer.CloseAction, !installer.Finished);
            }

        }

        public static void LaunchUninstaller()
        {
            using var interlock = new InterProcessLock("Uninstaller");

            if (!interlock.IsAcquired)
            {
                Frontend.ShowMessageBox(Strings.Dialog_AlreadyRunning_Uninstaller, MessageBoxImage.Stop);
                App.Terminate();
                return;
            }

            bool confirmed = false;
            bool keepData = true;

            if (App.LaunchSettings.QuietFlag.Active)
            {
                confirmed = true;
            }
            else
            {
                var dialog = new UninstallerDialog();
                dialog.ShowDialog();

                confirmed = dialog.Confirmed;
                keepData = dialog.KeepData;
            }

            if (!confirmed)
            {
                App.Terminate();
                return;
            }

            Installer.DoUninstall(keepData);

            Frontend.ShowMessageBox(Strings.Bootstrapper_SuccessfullyUninstalled, MessageBoxImage.Information);

            App.Terminate();
        }

        public static void LaunchSettings()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchSettings";

            using var interlock = new InterProcessLock("Settings");

            if (interlock.IsAcquired)
            {
                bool showAlreadyRunningWarning = Process.GetProcessesByName(App.ProjectName).Length > 1;

                var window = new UI.Elements.Settings.MainWindow(showAlreadyRunningWarning);

                // typically we'd use Show(), but we need to block to ensure IPL stays in scope
                window.ShowDialog();
            }
            else
            {
                App.Logger.WriteLine(LOG_IDENT, "Found an already existing menu window");

                var process = Utilities.GetProcessesSafe().Where(x => x.MainWindowTitle == Strings.Menu_Title).FirstOrDefault();

                if (process is not null)
                    PInvoke.SetForegroundWindow((HWND)process.MainWindowHandle);

                App.Terminate();
            }
        }

        public static void LaunchMenu()
        {
            var dialog = new LaunchMenuDialog();
            dialog.ShowDialog();

            ProcessNextAction(dialog.CloseAction);
        }

        public static void LaunchRoblox(LaunchMode launchMode, bool openSettingsOnFinish = false)
        {
            const string LOG_IDENT = "LaunchHandler::LaunchRoblox";

            if (launchMode == LaunchMode.None)
                throw new InvalidOperationException("No Roblox launch mode set");

            if (!File.Exists(Path.Combine(Paths.System, "mfplat.dll")))
            {
                Frontend.ShowMessageBox(Strings.Bootstrapper_WMFNotFound, MessageBoxImage.Error);

                if (!App.LaunchSettings.QuietFlag.Active)
                    Utilities.ShellExecute("https://support.microsoft.com/en-us/topic/media-feature-pack-list-for-windows-n-editions-c1c6fffa-d052-8338-7a79-a4bb980a700a");

                App.Terminate(ErrorCode.ERROR_FILE_NOT_FOUND);
            }

            if (App.Settings.Prop.ConfirmLaunches && Utilities.IsRobloxRunning() && !App.Settings.Prop.MultiInstanceLaunching)
            {
                // this currently doesn't work very well since it relies on checking the existence of the singleton mutex
                // which often hangs around for a few seconds after the window closes
                // it would be better to have this rely on the activity tracker when we implement IPC in the planned refactoring

                var result = Frontend.ShowMessageBox(Strings.Bootstrapper_ConfirmLaunch, MessageBoxImage.Warning, MessageBoxButton.YesNo);

                if (result != MessageBoxResult.Yes)
                {
                    App.Terminate();
                    return;
                }
            }

            // start bootstrapper and show the bootstrapper modal if we're not running silently
            App.Logger.WriteLine(LOG_IDENT, "Initializing bootstrapper");
            App.Bootstrapper = new Bootstrapper(launchMode);
            IBootstrapperDialog? dialog = null;

            if (!App.LaunchSettings.QuietFlag.Active)
            {
                App.Logger.WriteLine(LOG_IDENT, "Initializing bootstrapper dialog");
                dialog = App.Settings.Prop.BootstrapperStyle.GetNew();
                App.Bootstrapper.Dialog = dialog;
                dialog.Bootstrapper = App.Bootstrapper;
            }

            Task.Run(App.Bootstrapper.Run).ContinueWith(t =>
            {
                App.Logger.WriteLine(LOG_IDENT, "Bootstrapper task has finished");

                if (t.IsFaulted)
                {
                    App.Logger.WriteLine(LOG_IDENT, "An exception occurred when running the bootstrapper");

                    if (t.Exception is not null)
                        App.FinalizeExceptionHandling(t.Exception);
                }
                else if (openSettingsOnFinish)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Opening settings after Roblox bootstrapper finished");
                    StartInstalledCrystrap("-settings", LOG_IDENT);
                }

                App.Terminate();
            });

            dialog?.ShowBootstrapper();

            App.Logger.WriteLine(LOG_IDENT, "Exiting");
        }

        public static void LaunchPureRoblox()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchPureRoblox";

            try
            {
                var bootstrapper = new Bootstrapper(LaunchMode.Player);
                bool prepared = Task.Run(bootstrapper.PreparePureLaunchAsync).GetAwaiter().GetResult();

                if (!prepared)
                {
                    Frontend.ShowMessageBox("Crystrap couldn't prepare a clean Roblox launch because Roblox isn't installed yet.", MessageBoxImage.Warning);
                    App.Terminate(ErrorCode.ERROR_FILE_NOT_FOUND);
                    return;
                }

                var playerData = new RobloxPlayerData();

                if (!File.Exists(playerData.ExecutablePath))
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Roblox executable was not found at {playerData.ExecutablePath}");
                    Frontend.ShowMessageBox("Crystrap couldn't find the Roblox player executable for a clean launch.", MessageBoxImage.Error);
                    App.Terminate(ErrorCode.ERROR_FILE_NOT_FOUND);
                    return;
                }

                App.Logger.WriteLine(LOG_IDENT, $"Launching stock Roblox from {playerData.ExecutablePath}");

                var startInfo = new ProcessStartInfo()
                {
                    FileName = playerData.ExecutablePath,
                    WorkingDirectory = playerData.Directory
                };

                Process.Start(startInfo);
                App.Terminate();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to launch Pure Roblox");
                App.Logger.WriteException(LOG_IDENT, ex);
                App.FinalizeExceptionHandling(ex);
            }
        }

        public static void LaunchWatcher()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchWatcher";

            // this whole topology is a bit confusing, bear with me:
            // main thread: strictly UI only, handles showing of the notification area icon, context menu, server details dialog
            // - server information task: queries server location, invoked if either the explorer notification is shown or the server details dialog is opened
            // - discord rpc thread: handles rpc connection with discord
            //    - discord rich presence tasks: handles querying and displaying of game information, invoked on activity watcher events
            // - watcher task: runs activity watcher + waiting for roblox to close, terminates when it has

            var watcher = new Watcher();

            Task.Run(watcher.Run).ContinueWith(t => 
            {
                App.Logger.WriteLine(LOG_IDENT, "Watcher task has finished");

                watcher.Dispose();

                if (t.IsFaulted)
                {
                    App.Logger.WriteLine(LOG_IDENT, "An exception occurred when running the watcher");

                    if (t.Exception is not null)
                        App.FinalizeExceptionHandling(t.Exception);
                }

                // shouldnt this be done after client closes?
                if (App.Settings.Prop.CleanerOptions != CleanerOptions.Never)
                    Cleaner.DoCleaning();

                App.Terminate();
            });
        }

        public static void LaunchMultiInstanceWatcher()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchMultiInstanceWatcher";

            App.Logger.WriteLine(LOG_IDENT, "Starting multi-instance watcher");

            Task.Run(MultiInstanceWatcher.Run).ContinueWith(t =>
            {
                App.Logger.WriteLine(LOG_IDENT, "Multi instance watcher task has finished");

                if (t.IsFaulted)
                {
                    App.Logger.WriteLine(LOG_IDENT, "An exception occurred when running the multi-instance watcher");

                    if (t.Exception is not null)
                        App.FinalizeExceptionHandling(t.Exception);
                }

                App.Terminate();
            });
        }
        public static void LaunchBloxshadeConfig()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchBloxshade";

            App.Logger.WriteLine(LOG_IDENT, "Showing unsupported warning");

            new BloxshadeDialog().ShowDialog();
            App.SoftTerminate();
        }

        public static void LaunchBackgroundUpdater()
        {
            const string LOG_IDENT = "LaunchHandler::LaunchBackgroundUpdater";

            // Activate some LaunchFlags we need
            App.LaunchSettings.QuietFlag.Active = true;
            App.LaunchSettings.NoLaunchFlag.Active = true;

            App.Logger.WriteLine(LOG_IDENT, "Initializing bootstrapper");
            App.Bootstrapper = new Bootstrapper(LaunchMode.Player)
            {
                MutexName = "Bloxstrap-BackgroundUpdater",
                QuitIfMutexExists = true
            };

            CancellationTokenSource cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                App.Logger.WriteLine(LOG_IDENT, "Started event waiter");
                using (EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.AutoReset, "Bloxstrap-BackgroundUpdaterKillEvent"))
                    handle.WaitOne();

                App.Logger.WriteLine(LOG_IDENT, "Received close event, killing it all!");
                App.Bootstrapper.Cancel();
            }, cts.Token);

            Task.Run(App.Bootstrapper.Run).ContinueWith(t =>
            {
                App.Logger.WriteLine(LOG_IDENT, "Bootstrapper task has finished");
                cts.Cancel(); // stop event waiter

                if (t.IsFaulted)
                {
                    App.Logger.WriteLine(LOG_IDENT, "An exception occurred when running the bootstrapper");

                    if (t.Exception is not null)
                        App.FinalizeExceptionHandling(t.Exception);
                }

                App.Terminate();
            });

            App.Logger.WriteLine(LOG_IDENT, "Exiting");
        }
    }
}
