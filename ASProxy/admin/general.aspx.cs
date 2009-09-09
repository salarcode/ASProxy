using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SalarSoft.ASProxy;
using System.Collections.Specialized;
using System.Collections;

public partial class Admin_General : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void Page_Init(object sender, EventArgs e)
    {
        LoadLanguagesCombo();
        LoadFormData();
    }

    void LoadLanguagesCombo()
    {
        NameValueCollection lngList= Common.GetInstalledLanguagesList();

        for (int i = 0; i < lngList.Count; i++)
        {
            cmbUiLanguage.Items.Add(new ListItem(lngList.Get(i), lngList.GetKey(i)));
        }
    }
    private void LoadFormData()
    {
        ListItem listItem = cmbUiLanguage.Items.FindByValue(Configurations.Pages.UILanguage.ToLower());
        if (listItem != null)
            listItem.Selected = true;

        chkImageCompressor.Checked = Configurations.ImageCompressor.Enabled;
        txtImgQuality.Text = Configurations.ImageCompressor.Quality.ToString();

        txtUpdateInfoUrl.Text = Configurations.AutoUpdate.UpdateInfoUrl;
        chkUpdateEngine.Checked = Configurations.AutoUpdate.Engine;
        chkUpdatePlugins.Checked=Configurations.AutoUpdate.Plugins;
        chkUpdateProviders.Checked = Configurations.AutoUpdate.Providers;

        txtLogMaxFileSize.Text = Configurations.LogSystem.MaxFileSize.ToString();
        txtLogFileFormat.Text = Configurations.LogSystem.FileFormat;
        
        chkActivityLogEnabled.Checked = Configurations.LogSystem.ActivityLog_Enabled;
        chkActivityLogImages.Checked = Configurations.LogSystem.ActivityLog_Images;
        chkActivityLogPages.Checked = Configurations.LogSystem.ActivityLog_Pages;
        chkActivityLogLocation.Text = Configurations.LogSystem.ActivityLog_Location;

        chkErrorLogEnabled.Checked = Configurations.LogSystem.ErrorLog_Enabled;
        chkErrorLogLocation.Text = Configurations.LogSystem.ErrorLog_location;
    }
}
