#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class IOSPostProcessor
{
    public static class PostProcessor
    {
        private static readonly Dictionary<string, string> NewValues = new Dictionary<string, string>
        {
            { "NSCameraUsageDescription", "Puzzle Paradise needs camera access to take photos and turn them into custom puzzles." },
            { "NSPhotoLibraryUsageDescription", "Puzzle Paradise needs access to your photo library to convert photos into custom puzzles." }
        };

        private const string LocalizationKey = "CFBundleLocalizations";

        private static readonly string[] Localizations =
        {
            "en", "Arabic", "zh_CN", "zh_TW", "Danish", "Dutch", "Filipino", "fr", "de", "Indonesian", "it", "ja", "Malay", "Persian", "Polish", "Portuguese", "Romanian",
            "Russian", "Spanish", "Swedish", "Thai", "Turkish", "Vietnamese"
        };

        [PostProcessBuild]
        public static void ModifyPList(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            var plistPath = pathToBuiltProject + "/Info.plist";
            var plist = new PlistDocument();

            plist.ReadFromFile(plistPath);

            var root = plist.root;

            // root.AddIOSInfoPlistItems(NewValues);

            var localizationsElements = new PlistElementArray();
            foreach (var language in Localizations)
            {
                localizationsElements.AddString(language);
            }
            root[LocalizationKey] = localizationsElements;
    
            plist.WriteToFile(plistPath);
        }
    }
}
#endif