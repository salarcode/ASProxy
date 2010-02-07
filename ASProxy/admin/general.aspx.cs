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

    bool ValidateForm()
    {
		// Start with ~/
		// chkActivityLogLocation.Text

		// Start with ~/
		// chkErrorLogLocation.Text
        return true;
    }
    void LoadLanguagesCombo()
    {
        NameValueCollection lngList = Common.GetInstalledLanguagesList();

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
        chkUpdatePlugins.Checked = Configurations.AutoUpdate.Plugins;
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

    private void ApplyToConfig()
    {
        Configurations.Pages.UILanguage = cmbUiLanguage.SelectedValue;

        Configurations.ImageCompressor.Enabled = chkImageCompressor.Checked;
        Configurations.ImageCompressor.Quality = Convert.ToInt32(txtImgQuality.Text);

		// Disable ImageCompressor for the user too
		if (!Configurations.ImageCompressor.Enabled)
			Configurations.UserOptions.ImageCompressor.Enabled = false;


        Configurations.AutoUpdate.UpdateInfoUrl = txtUpdateInfoUrl.Text;
        Configurations.AutoUpdate.Engine = chkUpdateEngine.Checked;
        Configurations.AutoUpdate.Plugins = chkUpdatePlugins.Checked;
        Configurations.AutoUpdate.Providers = chkUpdateProviders.Checked;

        Configurations.LogSystem.MaxFileSize = Convert.ToInt64(txtLogMaxFileSize.Text);
        Configurations.LogSystem.FileFormat = txtLogFileFormat.Text;

        Configurations.LogSystem.ActivityLog_Enabled = chkActivityLogEnabled.Checked;
        Configurations.LogSystem.ActivityLog_Images = chkActivityLogImages.Checked;
        Configurations.LogSystem.ActivityLog_Pages = chkActivityLogPages.Checked;
        Configurations.LogSystem.ActivityLog_Location = chkActivityLogLocation.Text;

        Configurations.LogSystem.ErrorLog_Enabled = chkErrorLogEnabled.Checked;
        Configurations.LogSystem.ErrorLog_location = chkErrorLogLocation.Text;
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        Page.Validate();
        if (Page.IsValid && ValidateForm())
        {
            ApplyToConfig();
            Configurations.SaveSettings();
        }
    }
}
