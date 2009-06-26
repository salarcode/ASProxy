using System;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;
using System.Web;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;

namespace SalarSoft.ASProxy
{
	/// <summary>
	/// ASProxy update engine
	/// </summary>
	public class Updater
	{
		/// <summary>
		/// Reminder timer for next execution
		/// </summary>
		static Timer reminderTimer = new Timer(21 * 24 * 60 * 60 * 1000); // (3 weeks) 21 days later
		//static Timer reminderTimer = new Timer(1 * 60 * 1000); // TEST 1 min later

		/// <summary>
		/// Update package information
		/// </summary>
		public struct UpdateInformationType
		{
			public string DownloadUrl;
			public string UpdateVersion;
		}

		/// <summary>
		/// Adds next reminder
		/// </summary>
		public static void AddUpdateReminder()
		{
			//ASProxyExceptions.LogException(new Exception(), "AddUpdateReminder called");
			reminderTimer.Elapsed += new ElapsedEventHandler(UpdateReminder_Callback);
			reminderTimer.Enabled = true;
		}

		private static void UpdateReminder_Callback(object sender, ElapsedEventArgs e)
		{
			//ASProxyExceptions.LogException(new Exception(), "UpdateReminder_Callback");
			try
			{
				UpdateInformationType updateInfo;
				Updater up = new Updater();

				if (up.CanUpdate(out updateInfo))
				{
					//ASProxyExceptions.LogException(new Exception(), "UpdateReminder_Callback CanUpdate=true");
					up.DownloadUpdateAndInstall(updateInfo);
				}
				//else
				//	ASProxyExceptions.LogException(new Exception(), "UpdateReminder_Callback CanUpdate=false");
			}
			catch (Exception ex)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, null, "Error in Updater.UpdateReminder_Callback");
			}
		}

		private string _AutoUpdateInfoUrl;
		private string _CurrentVersion;

		public Updater()
		{
			_AutoUpdateInfoUrl = Configurations.AutoUpdate.UpdateInfoUrl;
			_CurrentVersion = Consts.General.ASProxyVersion;
		}

		/// <summary>
		/// Downloads update package and installs it
		/// </summary>
		/// <param name="updateInfo"></param>
		public void DownloadUpdateAndInstall(UpdateInformationType updateInfo)
		{
			string updatePhisycalFile = GetUpdateLocalFileName(updateInfo.DownloadUrl);

			if (File.Exists(updatePhisycalFile))
				File.Delete(updatePhisycalFile);

			GetDataFile(updateInfo.DownloadUrl, updatePhisycalFile);

			Install(updatePhisycalFile);
		}

		/// <summary>
		/// Check update package information for existing update
		/// </summary>
		/// <returns>Returns if package contains update data</returns>
		public bool CanUpdate(out UpdateInformationType updateInfo)
		{
			byte[] bytes = GetDataBytes(_AutoUpdateInfoUrl);
			updateInfo = GetUpdateInfoFromXmlBytes(bytes);
			try
			{
				double current = Convert.ToDouble(_CurrentVersion);
				double update = Convert.ToDouble(updateInfo.UpdateVersion);
				return update > current;
			}
			catch
			{
				return (updateInfo.UpdateVersion.CompareTo(_CurrentVersion) > 0);
			}
		}

		/// <summary>
		/// Extracts all package files into application destination with replace option
		/// </summary>
		/// <param name="zipFile"></param>
		private void Install(string zipFile)
		{
			string baseDir = GetUpdateBaseDirectory();
			FastZip fzip = new FastZip();
			fzip.CreateEmptyDirectories = true;
			fzip.ExtractZip(zipFile, baseDir, FastZip.Overwrite.Always, null, "", "");
		}

		private string GetUpdateLocalFileName(string url)
		{
			string file = Path.ChangeExtension(Path.GetFileName(url), ".zip");
			return Path.Combine(GetUpdateHoldingPlace(), file);
		}

		private string GetUpdateHoldingPlace()
		{
			const string sep = "\\";
			string appPath = HttpRuntime.AppDomainAppPath;
			appPath = Path.Combine(appPath, Consts.FilesConsts.Dir_AppData + sep + Consts.FilesConsts.Dir_UpdateSources + sep);

			Directory.CreateDirectory(appPath);
			return appPath;
		}

		/// <summary>
		/// Gets where should update files apply.
		/// </summary>
		private string GetUpdateBaseDirectory()
		{
			return HttpRuntime.AppDomainAppPath;
		}

		private UpdateInformationType GetUpdateInfoFromXmlBytes(byte[] xmlBytes)
		{
			UpdateInformationType result = new UpdateInformationType();
			string docContent = Encoding.UTF8.GetString(xmlBytes);


			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(docContent);
				XmlNode node;
				XmlNode rootNode = doc.SelectSingleNode("root");

				node = rootNode.SelectSingleNode("UpdateVersion");
				result.UpdateVersion = node.InnerText;

				node = rootNode.SelectSingleNode("DownloadUrl");
				result.DownloadUrl = node.InnerText;
			}
			catch { }
			return result;
		}


		private void GetDataFile(string url, string destFilename)
		{
			using (WebClient client = new WebClient())
			{
				// Use a proxy server between ASProxy and web
				if (Configurations.NetProxy.WebProxyEnabled)
				{
					client.Proxy = Configurations.NetProxy.GenerateWebProxy();
				}

				client.DownloadFile(url, destFilename);
			}
		}
		private byte[] GetDataBytes(string url)
		{
			using (WebClient client = new WebClient())
			{
				// Use a proxy server between ASProxy and web
				if (Configurations.NetProxy.WebProxyEnabled)
				{
					client.Proxy = Configurations.NetProxy.GenerateWebProxy();
				}

				byte[] data = client.DownloadData(url);
				return data;
			}
		}
	}
}