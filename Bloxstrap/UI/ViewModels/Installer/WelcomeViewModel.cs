namespace Bloxstrap.UI.ViewModels.Installer
{
    public class WelcomeViewModel : NotifyPropertyChangedViewModel
    {
        // formatting is done here instead of in xaml, it's just a bit easier
        public string MainText => String.Format(
            Strings.Installer_Welcome_MainText,
            "Thank you for downloading Crystrap. This installation process will be quick and simple, and you will be able to configure Crystrap after installation."
        );

        public bool CanContinue { get; set; } = false;
    }
}
