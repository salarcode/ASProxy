using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SalarSoft.ASProxy;

public partial class Admin_UserConfig : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void Page_Init(object sender, EventArgs e)
    {
        LoadFormData();
    }


    private void ApplyToConfig()
    {
        Configurations.UserOptionsConfig userConfig = Configurations.UserOptions;

        userConfig.HttpCompression.Enabled = chkCompressionActive.Checked;
        userConfig.HttpCompression.Changeable = chkCompressionChangeable.Checked;

        userConfig.ImageCompressor.Enabled = chkImgCompressorActive.Checked;
		userConfig.ImageCompressor.Changeable = chkImgCompressorChangeable.Checked;


        userConfig.Cookies.Enabled = chkCookiesActive.Checked;
        userConfig.Cookies.Changeable = chkCookiesChangeable.Checked;

        userConfig.DocType.Enabled = chkDocTypeActive.Checked;
        userConfig.DocType.Changeable = chkDocTypeChangeable.Checked;

        userConfig.EncodeUrl.Enabled = chkEncodeUrlActive.Checked;
        userConfig.EncodeUrl.Changeable = chkEncodeUrlChangeable.Checked;

        userConfig.ForceEncoding.Enabled = chkForceEncodingActive.Checked;
        userConfig.ForceEncoding.Changeable = chkForceEncodingChangeable.Checked;

        userConfig.Frames.Enabled = chkFramesActive.Checked;
        userConfig.Frames.Changeable = chkFramesChangeable.Checked;

        userConfig.Images.Enabled = chkImagesActive.Checked;
        userConfig.Images.Changeable = chkImagesChangeable.Checked;

        userConfig.Links.Enabled = chkLinksActive.Checked;
        userConfig.Links.Changeable = chkLinksChangeable.Checked;

        userConfig.OrginalUrl.Enabled = chkOrginalUrlActive.Checked;
        userConfig.OrginalUrl.Changeable = chkOrginalUrlChangeable.Checked;

        userConfig.PageTitle.Enabled = chkPageTitleActive.Checked;
        userConfig.PageTitle.Changeable = chkPageTitleChangeable.Checked;

        userConfig.RemoveImages.Enabled = chkRemoveImagesActive.Checked;
        userConfig.RemoveImages.Changeable = chkRemoveImagesChangeable.Checked;

        userConfig.RemoveObjects.Enabled = chkRemoveObjectsActive.Checked;
        userConfig.RemoveObjects.Changeable = chkRemoveObjectsChangeable.Checked;

        userConfig.RemoveScripts.Enabled = chkRemoveScriptsActive.Checked;
        userConfig.RemoveScripts.Changeable = chkRemoveScriptsChangeable.Checked;

        userConfig.SubmitForms.Enabled = chkSubmitFormsActive.Checked;
        userConfig.SubmitForms.Changeable = chkSubmitFormsChangeable.Checked;

        userConfig.TempCookies.Enabled = chkTempCookiesActive.Checked;
        userConfig.TempCookies.Changeable = chkTempCookiesChangeable.Checked;
    }

    private void LoadFormData()
    {
        Configurations.UserOptionsConfig userConfig = Configurations.UserOptions;
		chkCompressionActive.Checked = userConfig.HttpCompression.Enabled;
		chkCompressionChangeable.Checked = userConfig.HttpCompression.Changeable;

		chkImgCompressorActive.Checked = userConfig.ImageCompressor.Enabled;
		chkImgCompressorChangeable.Checked = userConfig.ImageCompressor.Changeable;
		
		// If Image compressor is completly diabled
		if (!Configurations.ImageCompressor.Enabled)
		{
			chkImgCompressorActive.Enabled = false;
			chkImgCompressorChangeable.Enabled = false;
		}

		chkCookiesActive.Checked = userConfig.Cookies.Enabled;
        chkCookiesChangeable.Checked = userConfig.Cookies.Changeable;

        chkDocTypeActive.Checked = userConfig.DocType.Enabled;
        chkDocTypeChangeable.Checked = userConfig.DocType.Changeable;

        chkEncodeUrlActive.Checked = userConfig.EncodeUrl.Enabled;
        chkEncodeUrlChangeable.Checked = userConfig.EncodeUrl.Changeable;

        chkForceEncodingActive.Checked = userConfig.ForceEncoding.Enabled;
        chkForceEncodingChangeable.Checked = userConfig.ForceEncoding.Changeable;

        chkFramesActive.Checked = userConfig.Frames.Enabled;
        chkFramesChangeable.Checked = userConfig.Frames.Changeable;

        chkImagesActive.Checked = userConfig.Images.Enabled;
        chkImagesChangeable.Checked = userConfig.Images.Changeable;

        chkLinksActive.Checked = userConfig.Links.Enabled;
        chkLinksChangeable.Checked = userConfig.Links.Changeable;

        chkOrginalUrlActive.Checked = userConfig.OrginalUrl.Enabled;
        chkOrginalUrlChangeable.Checked = userConfig.OrginalUrl.Changeable;

        chkPageTitleActive.Checked = userConfig.PageTitle.Enabled;
        chkPageTitleChangeable.Checked = userConfig.PageTitle.Changeable;

        chkRemoveImagesActive.Checked = userConfig.RemoveImages.Enabled;
        chkRemoveImagesChangeable.Checked = userConfig.RemoveImages.Changeable;

        chkRemoveObjectsActive.Checked = userConfig.RemoveObjects.Enabled;
        chkRemoveObjectsChangeable.Checked = userConfig.RemoveObjects.Changeable;

        chkRemoveScriptsActive.Checked = userConfig.RemoveScripts.Enabled;
        chkRemoveScriptsChangeable.Checked = userConfig.RemoveScripts.Changeable;

        chkSubmitFormsActive.Checked = userConfig.SubmitForms.Enabled;
        chkSubmitFormsChangeable.Checked = userConfig.SubmitForms.Changeable;

        chkTempCookiesActive.Checked = userConfig.TempCookies.Enabled;
        chkTempCookiesChangeable.Checked = userConfig.TempCookies.Changeable;
    }
    protected void btnSave_Click(object sender, EventArgs e)
    {
        Page.Validate();
        if (Page.IsValid)
        {
            ApplyToConfig();
            Configurations.SaveSettings();
        }
    }
}
