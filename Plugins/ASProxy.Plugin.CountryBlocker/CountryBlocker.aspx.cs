using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SalarSoft.ASProxy;
using System.Collections.Specialized;
using System.Collections;
using ASProxy.Plugin.CountryBlocker;

public partial class Admin_CountryBlocker : System.Web.UI.Page
{
	List<string> _errorsList = new List<string>();

	protected void Page_Load(object sender, EventArgs e)
	{

	}
	protected void Page_Init(object sender, EventArgs e)
	{
		ReadCountryCombo();
		ReadListBoxes();
	}
	protected override void OnPreRender(EventArgs e)
	{
		DisplayErrors(_errorsList);
		base.OnPreRender(e);
	}

	void AddError(string errorMessage)
	{
		_errorsList.Add(errorMessage);
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


	void ReadCountryCombo()
	{
		cmbAddCountry.Items.Clear();
		for (int i = 0; i < BlockedCountries.CountryNames.Length; i++)
		{
			cmbAddCountry.Items.Add(new ListItem(BlockedCountries.CountryNames[i], BlockedCountries.CountryCodes[i]));
		}
	}

	void ReadListBoxes()
	{
		lstAllowedCountries.Items.Clear();
		lstBlockedCountries.Items.Clear();

		for (int i = 0; i < BlockedCountries.AllowedList.Count; i++)
		{
			string code = BlockedCountries.AllowedList[i];
			int index = Array.IndexOf(BlockedCountries.CountryCodes, code);

			lstAllowedCountries.Items.Add(new ListItem(BlockedCountries.CountryNames[index], code));
		}
		
		for (int i = 0; i < BlockedCountries.BlockedList.Count; i++)
		{
			string code = BlockedCountries.BlockedList[i];
			int index = Array.IndexOf(BlockedCountries.CountryCodes, code);

			lstBlockedCountries.Items.Add(new ListItem(BlockedCountries.CountryNames[index], code));
		}
	}

	protected void btnAddRule_Click(object sender, EventArgs e)
	{
		if (!rbtnAllow.Checked && !rbtnBlock.Checked)
		{
			AddError("Please specify rule mode!");
			return;
		}
		if (cmbAddCountry.SelectedIndex < 0)
		{
			AddError("Please select a country.");
			return;
		}
		string countryCode = cmbAddCountry.SelectedValue;

		if (rbtnAllow.Checked)
		{
			if (BlockedCountries.AllowedList.IndexOf(countryCode) == -1)
				BlockedCountries.AllowedList.Add(countryCode);
		}
		else if (rbtnBlock.Checked)
		{
			if (BlockedCountries.BlockedList.IndexOf(countryCode) == -1)
				BlockedCountries.BlockedList.Add(countryCode);
		}

		BlockedCountries.SaveCountries();
		ReadListBoxes();
	}
	protected void btnDeleteBlocked_Click(object sender, EventArgs e)
	{
		if (chkDeleteBlockedSure.Checked == false)
		{
			AddError("Please check the \"Are you sure?\" checkbox.");
			return;
		}
		if (lstBlockedCountries.SelectedIndex < 0)
		{
			AddError("Please select a country to delete.");
			return;
		}
		string countryCode = lstBlockedCountries.SelectedValue;

		BlockedCountries.BlockedList.Remove(countryCode);
		BlockedCountries.SaveCountries();
		ReadListBoxes();
	}

	protected void btnDeleteAllowed_Click(object sender, EventArgs e)
	{
		if (chkDeleteAllowedSure.Checked == false)
		{
			AddError("Please check the \"Are you sure?\" checkbox.");
			return;
		}
		if (lstAllowedCountries.SelectedIndex < 0)
		{
			AddError("Please select a country to delete.");
			return;
		}

		string countryCode = lstAllowedCountries.SelectedValue;
		BlockedCountries.AllowedList.Remove(countryCode);
		BlockedCountries.SaveCountries();
		ReadListBoxes();
	}
}
