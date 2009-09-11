using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SalarSoft.ASProxy;
using System.Reflection;

public partial class Admin_Providers : System.Web.UI.Page
{
    List<PluginInfo> _availablePlugins;

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
        chkCanBeOverwritten.Checked = Configurations.Providers.EngineCanBeOverwritten;
        chkPluginsEnabled.Checked = Configurations.Providers.PluginsEnabled;
    }

    private void LoadProvidersList()
    {
        // Still nothing
        ltNoProvider.Visible = true;
    }

    private void LoadPluginsList()
    {
        Type pluginsType = typeof(Plugins);

        PropertyInfo asproxyPluginsInfo = pluginsType.GetProperty("AllASProxyPlugins", BindingFlags.NonPublic | BindingFlags.Static);
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

                    Type pluginsType = typeof(Plugins);
                    pluginsType.GetMethod("SetPluginEnableStatus",
                        BindingFlags.Static | BindingFlags.NonPublic)
                        .Invoke(null, new object[] { plugin.Name, false });
                    
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

                    Type pluginsType = typeof(Plugins);
                    pluginsType.GetMethod("SetPluginEnableStatus",
                        BindingFlags.Static | BindingFlags.NonPublic)
                        .Invoke(null, new object[] { plugin.Name, true });

                    LoadPluginsList();
                    break;
                }
            }
        }

    }

    protected void btnSaveProviders_Click(object sender, EventArgs e)
    {
        Configurations.Providers.EngineCanBeOverwritten = chkCanBeOverwritten.Checked;
        Configurations.Providers.PluginsEnabled = chkPluginsEnabled.Checked;
        Configurations.SaveSettings();
    }
}
