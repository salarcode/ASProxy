//**************************************************************************
// Product Name: ASProxy
// Description:  SalarSoft ASProxy is a free and open-source web proxy
// which allows the user to surf the net anonymously.
//                     
// MPL 1.1 License agreement.
// The contents of this file are subject to the Mozilla Public License
// Version 1.1 (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
// 
// Software distributed under the License is distributed on an "AS IS"
// basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
// License for the specific language governing rights and limitations
// under the License.
// 
// The Original Code is ASProxy (an ASP.NET Proxy).
// 
// The Initial Developer of the Original Code is Salar.Kh (SalarSoft).
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.Kh < salarsoftwares [@] gmail[.]com > (original author)
//
//**************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;

namespace SalarSoft.ASProxy.Update
{
	public class PluginUpdateInfo
	{
		private string _pluginName;
		private string _version;
		private DateTime _date;
		private int _updateSize;
		private List<UpdateFileInfo> _pluginFiles;
		private List<UpdateFileInfo> _assemblyFiles;
		private string _updatePackageUrl;

		public string PluginName
		{
			get { return _pluginName; }
			set { _pluginName = value; }
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
		public int UpdateSize
		{
			get { return _updateSize; }
			set { _updateSize = value; }
		}
		public List<UpdateFileInfo> PluginFiles
		{
			get { return _pluginFiles; }
			set { _pluginFiles = value; }
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

		public PluginUpdateInfo()
		{
			PluginFiles = new List<UpdateFileInfo>();
			AssemblyFiles = new List<UpdateFileInfo>();
		}
	}

	public class PluginsUpdater
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
				// Just update loaded plugins
				InstallLoadedPlugins();
			}
			catch { }
		}

		/// <summary>
		/// Starts the timer to download the plugins update if available
		/// </summary>
		public static void StartWaiter()
		{
			try
			{
				if (_lastRun == DateTime.MinValue)
				{
					// Saved last run
					_lastRun = Updaters.GetLastRun("PluginsUpdater");
					if (_lastRun == DateTime.MinValue)
						_lastRun = DateTime.Now;
				}
				// milliseconds!
				long nextRun = Updaters.GetNextUpdateTime(_lastRun, Updaters.UpdatersCheckPerriod);

				if (_reminderTimer != null)
				{
					_reminderTimer.Dispose();
					_reminderTimer = null;
				}

				// Thread timer
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
				Updaters.SetLastRun("PluginsUpdater", _lastRun);
			}
			catch { }
		}

		/// <summary>
		/// Downloads plugin update info
		/// </summary>
		public static PluginUpdateInfo DownloadPluginUpdateInfo(PluginInfo plugin)
		{
			if (plugin != null)
			{
				PluginUpdateInfo info = (PluginUpdateInfo)Updaters.DownloadUpdateInfo(typeof(PluginUpdateInfo), plugin.UpdateInfoUrl);
				return info;
			}
			return null;
		}

		/// <summary>
		/// Download the plugin update package and install it
		/// </summary>
		public static bool Install(PluginUpdateInfo pluginInfo)
		{
			string tempFile = Path.GetTempFileName();
			try
			{
				// download the package
				Updaters.GetDataFile(pluginInfo.UpdatePackageUrl, tempFile);

				// apply the downloaded package
				ApplyPackage(pluginInfo, tempFile);

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
		/// Download and install the plugin
		/// </summary>
		public static bool Install(string pluginName)
		{
			// Download the plugin
			PluginInfo plugin = Plugins.FindPlugin(Plugins.InstalledPlugins, pluginName);
			PluginUpdateInfo updateInfo = DownloadPluginUpdateInfo(plugin);

			// Check if update is required
			if (Common.CompareASProxyVersions(updateInfo.Version, plugin.Version) == 1)
				// Download the package and install it
				return Install(updateInfo);
			return false;
		}

		/// <summary>
		/// Will automatically install all loaded plugins
		/// </summary>
		public static void InstallLoadedPlugins()
		{
			// Only loaded plugins
			foreach (PluginInfo plugin in Plugins.LoadedPlugins)
			{
				PluginUpdateInfo updateInfo = DownloadPluginUpdateInfo(plugin);

				// Check if update is required
				if (Common.CompareASProxyVersions(updateInfo.Version, plugin.Version) == 1)
					// Download and install the plugin
					Install(updateInfo);
			}
		}

		/// <summary>
		/// Saves the package file to ASProxy location
		/// </summary>
		static void ApplyPackage(PluginUpdateInfo updateInfo, string zipFile)
		{
			ZipFile package = new ZipFile(zipFile);
			try
			{
				// First we install plugin files
				Updaters.InstallExtenderPackage(updateInfo.PluginFiles, package);

				// If installing plugin files was successful, we install assembly files
				Updaters.InstallAssemblyPackage(updateInfo.AssemblyFiles, package);
			}
			finally
			{
				package.Close();
			}
		}
	}
}
