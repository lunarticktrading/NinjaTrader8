#region Using declarations
using System;
using System.Linq;
using System.IO;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public partial class Indicator
	{
		public const string DefaultAlertFile = "Alert2.wav";

		public string DefaultAlertFilePath()
		{
			return System.IO.Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds");
        }

        public string ResolveAlertFilePath(string filename, string alertFilePath)
		{
			if (string.IsNullOrWhiteSpace(filename))
                return string.Empty;

			if (filename.Contains(";"))
			{
				// Multiple alert files specified, pick one at random.
				var alerts = filename.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				var random = new Random();
				var idx = random.Next(0, alerts.Length);
				return ResolveAlertFilePath(alerts[idx], alertFilePath);
			}

			if (filename.Contains(Path.DirectorySeparatorChar))
			{
				// Absolute path specified, return specified path if it exists, otherwise default alert sound.
				if (File.Exists(filename))
				{
					return filename;
				}
			}
			else
			{
				// No path specified, try audioFilePath, then DefaultAlertFilePath, otherwise default alert sound.

				if (!string.IsNullOrWhiteSpace(alertFilePath))
				{
					var fullPath = Path.Combine(alertFilePath, filename);
					if (File.Exists(fullPath))
					{
						return fullPath;
					}

					fullPath = Path.Combine(DefaultAlertFilePath(), filename);
					if (File.Exists(fullPath))
					{
						return fullPath;
					}
				}
			}

            // Default alert sound if specified file could not be located.
            return Path.Combine(DefaultAlertFilePath(), DefaultAlertFile);
        }
    }
}
