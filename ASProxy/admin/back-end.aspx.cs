using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SalarSoft.ASProxy;

public partial class Admin_BackEnd : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{

	}
	protected void Page_Init(object sender, EventArgs e)
	{
		FillComboboxes();
		LoadFormData();
	}

	public override void Validate()
	{
		base.Validate();
	}
	bool ValidateForm()
	{
		List<String> errorsList = new List<string>();

		try
		{
			if (Encoding.GetEncoding(txtPreferredLocalEncoding.Text) == null)
			{
				errorsList.Add("Preferred local encoding has invalid encoding!");
			}
		}
		catch
		{
			errorsList.Add("Preferred local encoding has invalid encoding!");
		}

		DisplayErrors(errorsList);
		return true;
	}

	void DisplayErrors(List<string> errorsList)
	{
		if (errorsList.Count == 0)
			return;
		ltErrorsList.Visible = true;
		string display = "<ul style='color:Red;'>";
		foreach (string item in errorsList)
		{
			display += "<li>" + item + "</li>";
		}
		display += "</ul>";
		ltErrorsList.Text = display;
	}

	void FillComboboxes()
	{
		cmbNetProxyMode.Items.Clear();
		cmbNetProxyMode.Items.Add(new ListItem("Direct", "0"));
		cmbNetProxyMode.Items.Add(new ListItem("SystemDefault", "1"));
		cmbNetProxyMode.Items.Add(new ListItem("Custom", "2"));

		cmbUserAgentMode.Items.Clear();
		cmbUserAgentMode.Items.Add(new ListItem("Default", "0"));
		cmbUserAgentMode.Items.Add(new ListItem("ASProxy", "1"));
		cmbUserAgentMode.Items.Add(new ListItem("Custom", "2"));
	}

	void ApplyToConfig()
	{
		// NetProxy Authentication
		Configurations.NetProxy.Authentication = chkAuthentication.Checked;
		Configurations.NetProxy.Authentication_Username = txtAuthUsername.Text;
		Configurations.NetProxy.Authentication_Password = txtAuthPassword.Text;

		// NetProxy Authentication
		Configurations.NetProxy.Mode = (SalarSoft.ASProxy.Configurations.NetProxyConfig.NetProxyMode)Convert.ToInt16(cmbNetProxyMode.SelectedValue);
		Configurations.NetProxy.Address = txtNetCustomProxyAddress.Text;
		Configurations.NetProxy.Port = Convert.ToInt32(txtNetCustomProxyPort.Text);

		// WebData
		Configurations.WebData.Downloader_MaxContentLength = Convert.ToInt64(txtDownMaxContentLength.Text);
		Configurations.WebData.Downloader_ReadWriteTimeOut = Convert.ToInt32(txtDownRequestReadWriteTimeOut.Text);
		Configurations.WebData.Downloader_Timeout = Convert.ToInt32(txtDownRequestTimeout.Text);
		Configurations.WebData.Downloader_ResumeSupport = chkResumeSupport.Checked;

		Configurations.WebData.MaxContentLength = Convert.ToInt64(txtMaxContentLength.Text);
		Configurations.WebData.RequestTimeout = Convert.ToInt32(txtRequestTimeout.Text);

		Configurations.WebData.RequestReadWriteTimeOut = Convert.ToInt32(txtRequestReadWriteTimeOut.Text);
		Configurations.WebData.PreferredLocalEncoding = txtPreferredLocalEncoding.Text;
		Configurations.WebData.SendSignature = chkSendSignature.Checked;

		Configurations.WebData.UserAgentCustom = txtCustomUserAgent.Text;
		Configurations.WebData.UserAgent = (SalarSoft.ASProxy.Configurations.WebDataConfig.UserAgentMode)Convert.ToInt16(cmbUserAgentMode.SelectedValue);

	}

	private void LoadFormData()
	{
		ListItem listItem;


		// NetProxy Authentication
		chkAuthentication.Checked = Configurations.NetProxy.Authentication;
		txtAuthUsername.Text = Configurations.NetProxy.Authentication_Username;
		txtAuthPassword.Text = Configurations.NetProxy.Authentication_Password;

		// NetProxy Authentication
		//cmbNetProxyMode.SelectedValue = Configurations.NetProxy.Mode
		txtNetCustomProxyAddress.Text = Configurations.NetProxy.Address;
		txtNetCustomProxyPort.Text = Configurations.NetProxy.Port.ToString();
		listItem = cmbNetProxyMode.Items.FindByValue(((int)Configurations.NetProxy.Mode).ToString());
		if (listItem != null)
			listItem.Selected = true;

		// WebData
		txtDownMaxContentLength.Text = Configurations.WebData.Downloader_MaxContentLength.ToString();
		txtDownRequestReadWriteTimeOut.Text = Configurations.WebData.Downloader_ReadWriteTimeOut.ToString();
		txtDownRequestTimeout.Text = Configurations.WebData.Downloader_Timeout.ToString();
		chkResumeSupport.Checked = Configurations.WebData.Downloader_ResumeSupport;

		txtMaxContentLength.Text = Configurations.WebData.MaxContentLength.ToString();
		txtRequestTimeout.Text = Configurations.WebData.RequestTimeout.ToString();

		txtRequestReadWriteTimeOut.Text = Configurations.WebData.RequestReadWriteTimeOut.ToString();
		txtPreferredLocalEncoding.Text = Configurations.WebData.PreferredLocalEncoding;
		chkSendSignature.Checked = Configurations.WebData.SendSignature;

		txtCustomUserAgent.Text = Configurations.WebData.UserAgentCustom;
		listItem = cmbUserAgentMode.Items.FindByValue(((int)Configurations.WebData.UserAgent).ToString());
		if (listItem != null)
			listItem.Selected = true;

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
