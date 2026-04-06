using Bloxstrap.Models.SettingTasks.Base;

namespace Bloxstrap.Models.SettingTasks
{
    public class SkyboxModPresetTask : StringBaseTask
    {
        public static readonly string[] RequiredFileNames =
        {
            "sky512_bk.tex",
            "sky512_dn.tex",
            "sky512_ft.tex",
            "sky512_lf.tex",
            "sky512_rt.tex",
            "sky512_up.tex"
        };

        public SkyboxModPresetTask() : base("ModPreset", "Skybox")
        {
            if (Directory.Exists(Paths.CustomSkybox) && HasRequiredFiles(Paths.CustomSkybox))
                OriginalState = Paths.CustomSkybox;
        }

        public static bool HasRequiredFiles(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return false;

            var fileNames = Directory.EnumerateFiles(folderPath)
                .Select(Path.GetFileName)
                .Where(static name => !string.IsNullOrEmpty(name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return RequiredFileNames.All(fileNames.Contains);
        }

        public static IEnumerable<string> GetMissingFiles(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return RequiredFileNames;

            var fileNames = Directory.EnumerateFiles(folderPath)
                .Select(Path.GetFileName)
                .Where(static name => !string.IsNullOrEmpty(name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return RequiredFileNames.Where(required => !fileNames.Contains(required));
        }

        private static void DeleteCurrentSkybox()
        {
            if (!Directory.Exists(Paths.CustomSkybox))
                return;

            Filesystem.AssertReadOnlyDirectory(Paths.CustomSkybox);
            Directory.Delete(Paths.CustomSkybox, true);
        }

        public override void Execute()
        {
            if (!string.IsNullOrEmpty(NewState))
            {
                Directory.CreateDirectory(Paths.CustomSkybox);

                foreach (string existingFile in Directory.EnumerateFiles(Paths.CustomSkybox))
                {
                    Filesystem.AssertReadOnly(existingFile);
                    File.Delete(existingFile);
                }

                foreach (string fileName in RequiredFileNames)
                {
                    string sourceFile = Directory.EnumerateFiles(NewState, fileName).First();
                    string destinationFile = Path.Combine(Paths.CustomSkybox, fileName);

                    Filesystem.AssertReadOnly(destinationFile);
                    File.Copy(sourceFile, destinationFile, true);
                }
            }
            else
            {
                DeleteCurrentSkybox();
            }

            OriginalState = NewState;
        }
    }
}
