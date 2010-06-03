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
using System.IO;
using System.Threading;

namespace SalarSoft.ASProxy.Update
{
	public class EngineUpdateInfo
	{
		private string _updateVersion;
		private DateTime _date;
		private string _updatePackageUrl;

		public string UpdateVersion
		{
			get { return _updateVersion; }
			set { _updateVersion = value; }
		}
		public DateTime Date
		{
			get { return _date; }
			set { _date = value; }
		}
		public string UpdatePackageUrl
		{
			get { return _updatePackageUrl; }
			set { _updatePackageUrl = value; }
		}
	}
	public class EngineUpdater
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
				// Check the update, then update if necessary
				Install();
			}
			catch { }
		}


		/// <summary>
		/// Starts the timer to download the engine update if available
		/// </summary>
		public static void StartWaiter()
		{
			try
			{
				if (_lastRun == DateTime.MinValue)
				{
					_lastRun = Updaters.GetLastRun("EngineUpdater");
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
				Updaters.SetLastRun("EngineUpdater", _lastRun);
			}
			catch { }
		}


		/// <summary>
		/// Downloads engine update info
		/// </summary>
		public static EngineUpdateInfo DownloadEngineUpdateInfo()
		{
			string url = Configurations.AutoUpdate.UpdateInfoUrl;
			EngineUpdateInfo info = (EngineUpdateInfo)Updaters.DownloadUpdateInfo(typeof(EngineUpdateInfo), url);
			return info;
		}

		//static void Save()
		//{
		//    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(EngineUpdateInfo));
		//    using (var ff = File.OpenWrite("f:\\temp.xml"))
		//    {
		//        var en = new EngineUpdateInfo()
		//        {
		//            UpdateVersion = "a213.234.12",
		//            UpdatePackageUrl = "asdsad",
		//            Date = DateTime.Now
		//        };
		//        serializer.Serialize(ff, en);
		//    }
		//}


		/// <summary>
		/// Download the engine update package and install it
		/// </summary>
		public static bool Install(EngineUpdateInfo updateInfo)
		{
			string tempFile = Path.GetTempFileName();
			try
			{
				// download the package
				Updaters.GetDataFile(updateInfo.UpdatePackageUrl, tempFile);

				// apply the downloaded package
				ApplyPackage(updateInfo, tempFile);

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
		/// Download and update the package
		/// </summary>
		public static bool Install()
		{
			// Download the engine update info
			EngineUpdateInfo updateInfo = DownloadEngineUpdateInfo();

			// Check if update is required
			if (Common.CompareASProxyVersions(updateInfo.UpdateVersion, Consts.General.ASProxyVersion) == 1)
				// Download the package and install it
				return Install(updateInfo);

			return false;
		}


		/// <summary>
		/// Saves the package file to ASProxy location
		/// </summary>
		static void ApplyPackage(EngineUpdateInfo updateInfo, string zipFile)
		{
			// installs engine files
			Updaters.InstallUpdatePackage(zipFile);
		}
	}
}
