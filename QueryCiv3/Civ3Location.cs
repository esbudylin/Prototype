using System.Linq;
using System.Runtime.InteropServices;

namespace QueryCiv3 {
	public class Civ3Location {
		public static readonly string RegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Infogrames Interactive\Civilization III";

		private static string SteamCommonDir() {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				string programFilesX86 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86);
				return System.IO.Path.Join(programFilesX86, "Steam\\steamapps\\common");
			}
			string home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			return home == null ? null : System.IO.Path.Combine(home, "Library/Application Support/Steam/steamapps/common");
		}

		private static bool FolderIsCiv3(System.IO.DirectoryInfo di) {
			return di.EnumerateFiles().Any(f => f.Name == "civ3id.mb");
		}

		public static string GetCiv3Path() {
			// Use CIV3_HOME env var if present
			string path = System.Environment.GetEnvironmentVariable("CIV3_HOME");
			if (path != null) { return path; }

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				// Look up in Windows registry if present
				path = (string)Microsoft.Win32.Registry.GetValue(RegistryKey, "install_path", "");
				if (!string.IsNullOrEmpty(path)) {
					return path;
				}
			}
			// Check for a civ3 folder in steamapps/common
			string steam = SteamCommonDir();
			if (!string.IsNullOrEmpty(steam)) {
				System.IO.DirectoryInfo root = new(steam);
				foreach (System.IO.DirectoryInfo di in root.GetDirectories()) {
					if (FolderIsCiv3(di)) {
						return di.FullName;
					}
				}
			}
			return "/civ3/path/not/found";
		}
	}
}
