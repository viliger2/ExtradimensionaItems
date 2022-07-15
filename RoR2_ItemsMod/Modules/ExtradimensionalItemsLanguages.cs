using R2API;
using System.IO;

namespace ExtradimensionalItems.Modules
{
    public class ExtradimensionalItemsLanguages
    {
        public const string LanguageFileName = "ExtradimensionalItems.language";
        public const string LanguageFileFolder = "Languages";

        public void Init(BepInEx.PluginInfo info)
        {
            LanguageAPI.AddPath(Path.Combine(Path.GetDirectoryName(info.Location), LanguageFileFolder, LanguageFileName));
        }

    }
}
