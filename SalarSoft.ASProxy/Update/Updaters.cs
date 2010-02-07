using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Web;

namespace SalarSoft.ASProxy.Update
{
	public struct UpdateFileInfo
	{
		public string FilePath;
		public bool IgnoreExistence;
		public string IgnoreASProxyVersion;
	}

	internal static class Updaters
	{
		private static string[] _forbiddenFiles = new string[] { "Configurations.xml", "Global.asax", "SalarSoft.ASProxy.dll", "ICSharpCode.SharpZipLib.dll", "web.config" };

		/// <summary>
		/// 20 days
		/// </summary>
		public const int UpdatersCheckPerriod = 20;



		public static string UpdatersLocation
		{
			get
			{
				return CurrentContext.MapAppPath(Consts.FilesConsts.Dir_Updater);
			}
		}

		/// <summary>
		/// Copies assembly files from package to Bin folder
		/// </summary>
		public static void InstallAssemblyPackage(List<UpdateFileInfo> updateFiles, ZipFile zipPackage)
		{
			// Bin path!
			string extenderPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Bin");

			foreach (UpdateFileInfo fileInfo in updateFiles)
			{
				if (IsPackageFileForbidden(fileInfo.FilePath))
					continue;

				// the file path
				string installPath = Path.Combine(extenderPath, fileInfo.FilePath);

				// Ignore existence
				if (fileInfo.IgnoreExistence && File.Exists(installPath))
					continue;

				// find file in zip package
				int index = zipPackage.FindEntry(fileInfo.FilePath, true);

				if (index == -1)
					throw new InvalidDataException("Package files are invalid");

				// Ignore specifed version
				int compare = 1;
				if (!string.IsNullOrEmpty(fileInfo.IgnoreASProxyVersion))
					compare = Common.CompareASProxyVersions(Consts.General.ASProxyVersion, fileInfo.IgnoreASProxyVersion);

				// Only check if they are not equal
				if (compare != 0)
				{
					// Installable, do it!
					using (Stream zipStream = zipPackage.GetInputStream(index))
					using (FileStream file = new FileStream(installPath, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						ReadWriteStream(zipStream, file);
					}
				}
			}
		}

		/// <summary>
		/// Copies update files from package to ASProxy
		/// </summary>
		public static void InstallExtenderPackage(List<UpdateFileInfo> updateFiles, ZipFile zipPackage)
		{
			// Application path
			string extenderPath = HttpRuntime.AppDomainAppPath;
			foreach (UpdateFileInfo fileInfo in updateFiles)
			{
				if (IsPackageFileForbidden(fileInfo.FilePath))
					continue;

				// the file path
				string installPath = Path.Combine(extenderPath, fileInfo.FilePath);

				// Ignore existence
				if (fileInfo.IgnoreExistence && File.Exists(installPath))
					continue;

				// find file in zip package
				int index = zipPackage.FindEntry(fileInfo.FilePath, true);

				if (index == -1)
					throw new FileNotFoundException("Package files are invalid");

				// Ensure path existance
				Directory.CreateDirectory(Path.GetDirectoryName(installPath));

				// Ignore specifed version
				int compare = 1;
				if (!string.IsNullOrEmpty(fileInfo.IgnoreASProxyVersion))
					compare = Common.CompareASProxyVersions(Consts.General.ASProxyVersion, fileInfo.IgnoreASProxyVersion);

				// Only check if they are not equal
				if (compare != 0)
				{
					// Installable, do it!
					using (Stream zipStream = zipPackage.GetInputStream(index))
					using (FileStream file = new FileStream(installPath, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						ReadWriteStream(zipStream, file);
					}
				}
			}
		}

		/// <summary>
		/// Copies all package coontent to ASProxy
		/// </summary>
		public static void InstallUpdatePackage(string zipPackage)
		{
			string updatePath = HttpRuntime.AppDomainAppPath;

			FastZip zip = new FastZip();
			zip.CreateEmptyDirectories = true;

			// extracts and overwites
			zip.ExtractZip(zipPackage, updatePath, FastZip.Overwrite.Always, null, "", "");
		}

		private static void ReadWriteStream(Stream readStream, Stream writeStream)
		{
			int Length = 1024;
			Byte[] buffer = new Byte[Length];
			int bytesRead = readStream.Read(buffer, 0, Length);
			// write the required bytes
			while (bytesRead > 0)
			{
				writeStream.Write(buffer, 0, bytesRead);
				bytesRead = readStream.Read(buffer, 0, Length);
			}
			readStream.Close();
			writeStream.Close();
		}

		/// <summary>
		/// Checks if file is allowed to be installed
		/// </summary>
		private static bool IsPackageFileForbidden(string fileName)
		{
			// security check
			if (fileName.Contains("../") || fileName.Contains("..\\"))
				return true;

			// forbidden files
			foreach (string file in _forbiddenFiles)
			{
				if (fileName.EndsWith(file))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Last execution time of specified updater
		/// </summary>
		public static DateTime GetLastRun(string updaterName)
		{
			try
			{
				string filePath = Path.Combine(UpdatersLocation, updaterName);
				if (!File.Exists(filePath))
					return DateTime.MinValue;

				string lastRun = File.ReadAllText(filePath);
				return Convert.ToDateTime(lastRun);
			}
			catch (Exception)
			{
				return DateTime.MinValue;
			}
		}

		/// <summary>
		/// Set last execution time of specified updater
		/// </summary>
		public static void SetLastRun(string updaterName, DateTime lastRun)
		{
			try
			{
				Directory.CreateDirectory(UpdatersLocation);

				string filePath = Path.Combine(UpdatersLocation, updaterName);
				File.WriteAllText(filePath, lastRun.ToString());
			}
			catch { }
		}

		public static long GetNextUpdateTime(DateTime lastRun, int periodDays)
		{
			// convert to millisecond
			long periodMS = (periodDays * 24 * 60 * 60 * 1000);
			long nextRun = periodMS - (DateTime.Now - lastRun).Milliseconds;

			if (nextRun <= 0)
				return 1;
			else
				return nextRun;
		}

		public static object DownloadUpdateInfo(Type updateInfoClas, string xmlUpdateInfoUrl)
		{
			byte[] xmlBytes = GetDataBytes(xmlUpdateInfoUrl);
			return GetUpdateClassInfo(updateInfoClas, xmlBytes);
		}

		public static object GetUpdateClassInfo(Type updateInfoClas, byte[] classBytes)
		{
			XmlSerializer serializer = new XmlSerializer(updateInfoClas);
			using (MemoryStream mem = new MemoryStream(classBytes))
			{
				return serializer.Deserialize(mem);
			}
		}

		public static byte[] GenerateUpdateClassInfo(Type updateInfoClas, object classObject)
		{
			XmlSerializer serializer = new XmlSerializer(updateInfoClas);
			using (MemoryStream mem = new MemoryStream())
			{
				serializer.Serialize(mem, classObject);
				return mem.ToArray();
			}
		}

		public static void GetDataFile(string url, string destFilename)
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

		public static byte[] GetDataBytes(string url)
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
