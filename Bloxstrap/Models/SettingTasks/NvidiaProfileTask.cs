using System.Windows;
using Bloxstrap.Integrations;

namespace Bloxstrap.Models.SettingTasks
{
    public class NvidiaProfileTask : BoolBaseTask
    {
        public NvidiaProfileTask() : base("NvidiaProfile")
        {
            OriginalState = NvidiaTweaks.IsApplied();
        }

        public override void Execute()
        {
            try
            {
                if (NewState)
                {
                    if (!NvidiaTweaks.IsNvidiaPresent())
                    {
                        Frontend.ShowMessageBox("No NVIDIA GPU installation was detected, so the NVIDIA Profile Inspector tweak was not applied.", MessageBoxImage.Warning);
                        NewState = OriginalState;
                        return;
                    }

                    NvidiaTweaks.Apply();
                }
                else
                {
                    NvidiaTweaks.Reset();
                }

                OriginalState = NewState;
            }
            catch (NpiNotFoundException)
            {
                string manualPath = NvidiaTweaks.WriteNipOnly(!NewState);
                Frontend.ShowMessageBox($"nvidiaProfileInspector.exe was not found, so Crystrap exported the profile for manual import instead.\n\nFile: {manualPath}", MessageBoxImage.Warning);
                NewState = OriginalState;
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Failed to apply the NVIDIA Profile Inspector tweak.\n\n{ex.Message}", MessageBoxImage.Warning);
                NewState = OriginalState;
            }
        }
    }
}
