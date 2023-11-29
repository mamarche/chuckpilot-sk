using System.Reflection;

namespace ChuckPilot.Api
{
    public static class LocalFiles
    {
        /// <summary>
        /// Scan the local folders from the repo, looking for "Plugins" folder.
        /// </summary>
        /// <returns>The full path to Plugins</returns>
        public static string GetPath(string folder)
        {
            bool SearchPath(string pathToFind, out string result, int maxAttempts = 10)
            {
                var currDir = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
                bool found;
                do
                {
                    result = Path.Join(currDir, pathToFind);
                    found = Directory.Exists(result);
                    currDir = Path.GetFullPath(Path.Combine(currDir, ".."));
                } while (maxAttempts-- > 0 && !found);

                return found;
            }

            if (!SearchPath(Path.DirectorySeparatorChar + folder, out string path)
                && !SearchPath(folder, out path))
            {
                throw new Exception("Plugins directory not found. The app needs the plugins from the repo to work.");
            }

            return path;
        }
    }
}
