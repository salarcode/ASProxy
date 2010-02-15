using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace SalarSoft.ASProxy.Update
{
	public class ProviderUpdateInfo
	{
		private string _providerName;
		private string _version;
		private DateTime _date;
		private String _asproxyMinVersion;
		private String _asproxyMaxVersion;
		private int _updateSize;
		private List<UpdateFileInfo> _providerFiles;
		private List<UpdateFileInfo> _assemblyFiles;
		private string _updatePackageUrl;

		public string ProviderName
		{
			get { return _providerName; }
			set { _providerName = value; }
		}
		public string Version
		{
			get { return _version; }
			set { _version = value; }
		}
		public DateTime Date
		{
			get { return _date; }
			set { _date = value; }
		}
		public String ASProxyMinVersion
		{
			get { return _asproxyMinVersion; }
			set { _asproxyMinVersion = value; }
		}
		public String ASProxyMaxVersion
		{
			get { return _asproxyMaxVersion; }
			set { _asproxyMaxVersion = value; }
		}
		public int UpdateSize
		{
			get { return _updateSize; }
			set { _updateSize = value; }
		}
		public List<UpdateFileInfo> ProviderFiles
		{
			get { return _providerFiles; }
			set { _providerFiles = value; }
		}
		public List<UpdateFileInfo> AssemblyFiles
		{
			get { return _assemblyFiles; }
			set { _assemblyFiles = value; }
		}
		public string UpdatePackageUrl
		{
			get { return _updatePackageUrl; }
			set { _updatePackageUrl = value; }
		}

		public ProviderUpdateInfo()
		{
			ProviderFiles = new List<UpdateFileInfo>();
			AssemblyFiles = new List<UpdateFileInfo>();
		}
	}
	public class ProvidersUpdater
	{
		/// <summary>
		/// Reminder timer for next execution
		/// </summary>
		static Timer _reminderTimer;
		static DateTime _lastRun = DateTime.MinValue;

		static void UpdaterCallback(object state)
		{
			try
			{
				// Just update loaded providers
				InstallLoadedProviders();
			}
			catch { }
		}

		/// <summary>
		/// Starts the timer to download the providers update if available
		/// </summary>
		public static void StartWaiter()
		{
			try
			{
				if (_lastRun == DateTime.MinValue)
				{
					_lastRun = Updaters.GetLastRun("ProvidersUpdater");
					if (_lastRun == DateTime.MinValue)
						_lastRun = DateTime.Now;
				}

				long nextRun = Updaters.GetNextUpdateTime(_lastRun, Updaters.UpdatersCheckPerriod);

				if (_reminderTimer != null)
				{
					_reminderTimer.Dispose();
					_reminderTimer = null;
				}
				_reminderTimer = new Timer(new TimerCallback(UpdaterCallback), null, nextRun, nextRun);
			}
			catch { }
		}

		/// <summary>
		/// Stops the timer and saves the last start time
		/// </summary>
		public static void StopWaiter()
		{
			try
			{
				if (_reminderTimer != null)
				{
					_reminderTimer.Dispose();
					_reminderTimer = null;
				}

				// save last run
				Updaters.SetLastRun("ProvidersUpdater", _lastRun);
			}
			catch { }
		}

		/// <summary>
		/// Downloads provider update info
		/// </summary>
		public static ProviderUpdateInfo DownloadProviderUpdateInfo(ProviderInfo provider)
		{
			if (provider != null)
			{
				ProviderUpdateInfo info = (ProviderUpdateInfo)Updaters.DownloadUpdateInfo(typeof(ProviderUpdateInfo), provider.UpdateInfoUrl);
				return info;
			}
			return null;
		}

		/// <summary>
		/// Download the provider update package and install it
		/// </summary>
		public static bool Install(ProviderUpdateInfo providerInfo)
		{
			string tempFile = Path.GetTempFileName();
			try
			{
				// download the package
				Updaters.GetDataFile(providerInfo.UpdatePackageUrl, tempFile);

				// apply the downloaded package
				ApplyPackage(providerInfo, tempFile);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				// delete downloaded package
				File.Delete(tempFile);
			}
		}

		/// <summary>
		/// Download and install the provider
		/// </summary>
		public static bool Install(string providerName)
		{
			// Download the provider
			ProviderInfo provider = Providers.FindProvider(Providers.InstalledProviders, providerName);
			ProviderUpdateInfo updateInfo = DownloadProviderUpdateInfo(provider);

			// Check if update is required
			if (Common.CompareASProxyVersions(updateInfo.Version, provider.Version) == 1)
				// Download the package and install it
				return Install(updateInfo);
			return false;
		}

		/// <summary>
		/// Will automatically install all loaded providers
		/// </summary>
		public static void InstallLoadedProviders()
		{
			// Only loaded providers
			foreach (ProviderInfo provider in Providers.LoadedProviders)
			{
				ProviderUpdateInfo updateInfo = DownloadProviderUpdateInfo(provider);

				// Check if update is required
				if (Common.CompareASProxyVersions(updateInfo.Version, provider.Version) == 1)
					// Download and install the provider
					Install(updateInfo);
			}
		}

		/// <summary>
		/// Saves the package file to ASProxy location
		/// </summary>
		static void ApplyPackage(ProviderUpdateInfo updateInfo, string zipFile)
		{
			ZipFile package = new ZipFile(zipFile);
			try
			{
				// First we install provider files
				Updaters.InstallExtenderPackage(updateInfo.ProviderFiles, package);

				// If installing provider files was successful, we install assembly files
				Updaters.InstallAssemblyPackage(updateInfo.AssemblyFiles, package);
			}
			finally
			{
				package.Close();
			}
		}
	}
}
