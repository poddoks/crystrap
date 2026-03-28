using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Bloxstrap.Extensions
{
    public static class IconEx
    {
        public static Icon GetSized(this Icon icon, int width, int height) => new(icon, new Size(width, height));

        public static ImageSource GetImageSource(this Icon icon, bool handleException = true)
        {
            if (icon is null)
            {
                if (handleException)
                {
                    Frontend.ShowMessageBox(Strings.Dialog_IconLoadFailed);
                    return Properties.Resources.IconBloxstrap.GetImageSource(false);
                }

                throw new ArgumentNullException(nameof(icon));
            }

            using MemoryStream stream = new();
            icon.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            if (handleException)
            {
                try
                {
                    return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("IconEx::GetImageSource", ex);
                    Frontend.ShowMessageBox(string.Format(Strings.Dialog_IconLoadFailed, ex.Message));
                    Icon fallbackIcon = BootstrapperIcon.IconCrystrap.GetIcon();

                    if (fallbackIcon is null || ReferenceEquals(fallbackIcon, icon))
                        fallbackIcon = Properties.Resources.IconBloxstrap;

                    return fallbackIcon.GetImageSource(false);
                }
            }
            else
            {
                return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }
    }
}
