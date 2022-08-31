using System.IO;
using UnityEditor;

namespace TooltipGenerator {
    class Processor : AssetPostprocessor {

        private static TooltipGenerator tooltipGenerator = new TooltipGenerator();
        private static readonly string[] illegalKeyWords = new string[] { "Packages/", "Library/" };

        /// <summary>
        /// Called when any asset changed
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        /// <param name="didDomainReload"></param>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload) {
            bool changeDetected = false;
            bool classesUpdated = false;

            foreach (string str in importedAssets) {
                //UnityEngine.Debug.Log(str);
                if (isValidPath(str)) {

                    if (tooltipGenerator == null) {
                        tooltipGenerator = new TooltipGenerator();
                    }

                    if (!classesUpdated) {
                        tooltipGenerator.UpdateValidClasses();
                        classesUpdated = true;
                    }
                    if(tooltipGenerator.TryProcessFile(str)) {
                        changeDetected = true;
                    } 
                }
            }
            if (changeDetected) {
                AssetDatabase.Refresh();
                tooltipGenerator = null;
            }
        }

        private static bool isValidPath(string path) {
            if (Path.GetExtension(path) != ".cs") {
                return false;
            }

            for (int i = 0, count = illegalKeyWords.Length; i<count; i++) {
                if (path.IndexOf(illegalKeyWords[i]) >= 0) {
                    return false;
                }
            }

            return true;
        }
    }
}

