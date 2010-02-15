using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SalarSoft.ASProxy;
using System.Reflection;
using SalarSoft.ASProxy.Update;
using System.Drawing;

public partial class Admin_Providers : System.Web.UI.Page
{
	List<PluginInfo> _availablePlugins;
	List<ProviderInfo> _installedProviders;

	protected void Page_Load(object sender, EventArgs e)
	{

	}

	protected void Page_Init(object sender, EventArgs e)
	{
		LoadProvidersList();
		LoadPluginsList();
		LoadFormData();
	}

	private void LoadFormData()
	{
		chkCanBeOverwritten.Checked = Configurations.Providers.ProviderExtensionsEnabled;
		chkPluginsEnabled.Checked = Configurations.Providers.PluginsEnabled;
	}

	private void LoadProvidersList()
	{
		Type providerType = typeof(Providers);

		PropertyInfo asproxyProvidersInfo = providerType.GetProperty("InstalledProviders", BindingFlags.NonPublic | BindingFlags.Static);
		_installedProviders = (List<ProviderInfo>)asproxyProvidersInfo.GetValue(null, null);
		if (_installedProviders != null)
		{
			rptProviders.DataSource = _installedProviders;
			rptProviders.DataBind();
		}

		if (!(_installedProviders != null && _installedProviders.Count > 0))
		{
			ltNoProvider.Visible = true;
		}
	}

	private void LoadPluginsList()
	{
		Type pluginsType = typeof(Plugins);

		PropertyInfo asproxyPluginsInfo = pluginsType.GetProperty("InstalledPlugins", BindingFlags.NonPublic | BindingFlags.Static);
		_availablePlugins = (List<PluginInfo>)asproxyPluginsInfo.GetValue(null, null);
		if (_availablePlugins != null)
		{
			rptPlugins.DataSource = _availablePlugins;
			rptPlugins.DataBind();
		}

		if (!(_availablePlugins != null && _availablePlugins.Count > 0))
		{
			ltNoPlugin.Visible = true;
		}
	}

	protected void btnProviderDisableClick(object sender, EventArgs e)
	{
		Button btn = (Button)sender;
		if (btn.CommandName == "Disable")
		{
			string providerKey = btn.CommandArgument;
			foreach (ProviderInfo provider in _installedProviders)
			{
				if (provider.Name + provider.Author == providerKey)
				{
					Configurations.Providers.DisabledProviders.Add(provider.Name);
					Configurations.SaveSettings();

					ChangeLoadedProviderStatus(provider.Name, false);

					LoadProvidersList();
					break;
				}
			}
		}
	}

	protected void btnProviderEnableClick(object sender, EventArgs e)
	{
		Button btn = (Button)sender;
		if (btn.CommandName == "Enable")
		{
			string providerKey = btn.CommandArgument;
			foreach (ProviderInfo provider in _installedProviders)
			{
				if (provider.Name + provider.Author == providerKey)
				{
					Configurations.Providers.DisabledProviders.Remove(provider.Name);
					Configurations.SaveSettings();

					ChangeLoadedProviderStatus(provider.Name, true);

					LoadProvidersList();
					break;
				}
			}
		}
	}

	void ChangeLoadedProviderStatus(string providerName, bool enabled)
	{
		Type providerType = typeof(Providers);
		providerType.GetMethod("SetProviderEnableStatus",
			BindingFlags.Static | BindingFlags.NonPublic)
			.Invoke(null, new object[] { providerName, enabled });
	}

	protected void btnPluginDisableClick(object sender, EventArgs e)
	{
		Button btn = (Button)sender;
		if (btn.CommandName == "Disable")
		{
			string pluginKey = btn.CommandArgument;
			foreach (PluginInfo plugin in _availablePlugins)
			{
				if (plugin.Name + plugin.Author == pluginKey)
				{
					Configurations.Providers.DisabledPlugins.Add(plugin.Name);
					Configurations.SaveSettings();

					ChangeLoadedPluginStatus(plugin.Name, false);

					LoadPluginsList();
					break;
				}
			}
		}
	}
	protected void btnPluginEnableClick(object sender, EventArgs e)
	{
		Button btn = (Button)sender;

		if (btn.CommandName == "Enable")
		{
			string pluginKey = btn.CommandArgument;
			foreach (PluginInfo plugin in _availablePlugins)
			{
				if (plugin.Name + plugin.Author == pluginKey)
				{
					Configurations.Providers.DisabledPlugins.Remove(plugin.Name);
					Configurations.SaveSettings();

					ChangeLoadedPluginStatus(plugin.Name, true);

					LoadPluginsList();
					break;
				}
			}
		}
	}

	void ChangeLoadedPluginStatus(string pluginName, bool enabled)
	{
		Type pluginsType = typeof(Plugins);
		pluginsType.GetMethod("SetPluginEnableStatus",
			BindingFlags.Static | BindingFlags.NonPublic)
			.Invoke(null, new object[] { pluginName, enabled });
	}

	protected void btnSaveProviders_Click(object sender, EventArgs e)
	{
		Configurations.Providers.ProviderExtensionsEnabled = chkCanBeOverwritten.Checked;
		Configurations.Providers.PluginsEnabled = chkPluginsEnabled.Checked;
		Configurations.SaveSettings();
	}
	protected void rptProviders_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{

	}
	protected void rptPlugins_ItemDataBound(object sender, RepeaterItemEventArgs e)
	{

	}

	protected void btnProviderUpdateCheckClick(object sender, EventArgs e)
	{
		Button btn = (Button)sender;

		if (btn.CommandName == "Update")
		{
			string pvName = btn.CommandArgument;
			foreach (ProviderInfo info in _installedProviders)
			{
				if (info.Name == pvName)
				{
					try
					{
						ProviderUpdateInfo updateInfo = ProvidersUpdater.DownloadProviderUpdateInfo(info);
						DisplayProviderUpdate(updateInfo, info, false, null);
					}
					catch
					{
						DisplayProviderUpdate(null, info, false, null);
					}
					return;
				}
			}
		}
	}
	void DisplayProviderUpdate(ProviderUpdateInfo updateInfo, ProviderInfo installedInfo, bool installed, string message)
	{
		pgUpdateProvider.Visible = true;
		lblUpProviderName.Text = installedInfo.Name;
		lblUpProviderVersion.Text = installedInfo.Version;

		if (installed)
		{
			lblUpProviderStatus.Text = "Update was successful.";
			lblUpProviderStatus.ForeColor = Color.Green;
			btnUpProviderUpdate.Enabled = false;
		}
		else if (updateInfo == null)
		{
			lblUpProviderStatus.Text = "Unable to check for update.";
			lblUpProviderStatus.ForeColor = Color.Red;
			btnUpProviderUpdate.Enabled = false;
		}
		else
		{
			lblUpProviderName.Text = updateInfo.ProviderName;
			lblUpProviderVersion.Text = updateInfo.Version;
			if (Common.CompareASProxyVersions(updateInfo.Version, installedInfo.Version) == 1)
			{
				lblUpProviderStatus.Text = "Update is available.";
				lblUpProviderStatus.ForeColor = Color.Green;
				btnUpProviderUpdate.Enabled = true;
			}
			else
			{
				lblUpProviderStatus.Text = "Update is not available.";
				lblUpProviderStatus.ForeColor = Color.Red;
				btnUpProviderUpdate.Enabled = false;
			}
		}

		if (!string.IsNullOrEmpty(message))
			lblUpProviderStatus.Text = message;

	}

	protected void btnPluginUpdateCheckClick(object sender, EventArgs e)
	{
		Button btn = (Button)sender;

		if (btn.CommandName == "Update")
		{
			string pvName = btn.CommandArgument;
			foreach (PluginInfo info in _availablePlugins)
			{
				if (info.Name == pvName)
				{
					try
					{
						PluginUpdateInfo updateInfo = PluginsUpdater.DownloadPluginUpdateInfo(info);
						DisplayPluginUpdate(updateInfo, info, false, null);
					}
					catch
					{
						DisplayPluginUpdate(null, info, false, null);
					}
					return;
				}
			}
		}
	}

	void DisplayPluginUpdate(PluginUpdateInfo updateInfo, PluginInfo installedInfo, bool installed, string message)
	{
		pgUpdatePlugin.Visible = true;
		lblUpPluginName.Text = installedInfo.Name;
		lblUpPluginVersion.Text = installedInfo.Version;

		if (installed)
		{
			lblUpPluginStatus.Text = "Update was successful";
			lblUpPluginStatus.ForeColor = Color.Green;
			btnUpPluginUpdate.Enabled = false;
		}
		else if (updateInfo == null)
		{
			lblUpPluginStatus.Text = "Unable to check for update.";
			lblUpPluginStatus.ForeColor = Color.Red;
			btnUpPluginUpdate.Enabled = false;
		}
		else
		{
			lblUpPluginName.Text = updateInfo.PluginName;
			lblUpPluginVersion.Text = updateInfo.Version;
			if (Common.CompareASProxyVersions(updateInfo.Version, installedInfo.Version) == 1)
			{
				lblUpPluginStatus.Text = "Update is available.";
				lblUpPluginStatus.ForeColor = Color.Green;
				btnUpPluginUpdate.Enabled = true;
			}
			else
			{
				lblUpPluginStatus.Text = "Update is not available.";
				lblUpPluginStatus.ForeColor = Color.Red;
				btnUpPluginUpdate.Enabled = false;
			}
		}
		if (!string.IsNullOrEmpty(message))
			lblUpPluginStatus.Text = message;

	}

	protected void btnUpProviderUpdate_Click(object sender, EventArgs e)
	{
		string pvName = lblUpProviderName.Text;
		foreach (ProviderInfo info in _installedProviders)
		{
			if (info.Name == pvName)
			{
				try
				{
					// download the update info
					ProviderUpdateInfo updateInfo = ProvidersUpdater.DownloadProviderUpdateInfo(info);

					// download package and install it
					if (ProvidersUpdater.Install(updateInfo))
					{
						DisplayProviderUpdate(updateInfo, info, true, null);
					}
					else
						DisplayProviderUpdate(null, info, false, "Update installation failed!");

				}
				catch
				{
					DisplayProviderUpdate(null, info, false, "Update installation failed!");
				}
				return;
			}
		}

	}
	protected void btnUpProviderDismiss_Click(object sender, EventArgs e)
	{
		pgUpdateProvider.Visible = false;
	}
	protected void btnUpPluginUpdate_Click(object sender, EventArgs e)
	{
		string pvName = lblUpPluginName.Text;
		foreach (PluginInfo info in _availablePlugins)
		{
			if (info.Name == pvName)
			{
				try
				{
					PluginUpdateInfo updateInfo = PluginsUpdater.DownloadPluginUpdateInfo(info);
					if (PluginsUpdater.Install(updateInfo))
						DisplayPluginUpdate(updateInfo, info, true, null);
					else
						DisplayPluginUpdate(null, info, false, "Update installation failed!");
				}
				catch
				{
					DisplayPluginUpdate(null, info, false, "Update installation failed!");
				}
				return;
			}
		}
	}
	protected void btnUpPluginDismiss_Click(object sender, EventArgs e)
	{
		pgUpdatePlugin.Visible = false;
	}
}
