using System;
using System.Collections.Generic;
using SalarSoft.ASProxy;
using System.Net;
using System.Web.UI.WebControls;

public partial class Admin_Users : System.Web.UI.Page
{
	private const string _Str_AllowedList = "AllowedList";
	private const string _Str_AllowedRange = "AllowedRange";
	private const string _Str_BlockedList = "BlockedList";
	private const string _Str_BlockedRange = "BlockedRange";
	private const string _Str_RangeSeperate = " - ";
	private const string _Str_BlockedName = "Blocked: ";
	private const string _Str_AllowedName = "Allowed: ";


	List<string> _errorsList = new List<string>();

	void AddError(string errorMessage)
	{
		_errorsList.Add(errorMessage);
	}

	private void LoadFormData()
	{
		btnLoginEnable.Enabled = !Configurations.Authentication.Enabled;
		btnLoginDisable.Enabled = Configurations.Authentication.Enabled;
		chkLoginEnabled.Checked = Configurations.Authentication.Enabled;

		btnUACEnable.Enabled = !Configurations.UserAccessControl.Enabled;
		btnUACDisable.Enabled = Configurations.UserAccessControl.Enabled;
		chkUACEnabled.Checked = Configurations.UserAccessControl.Enabled;
	}

	void FillTheListBoxes()
	{
		lstUsersList.Items.Clear();
		if (Configurations.Authentication.Users != null)
		{
			foreach (Configurations.AuthenticationConfig.User user in Configurations.Authentication.Users)
			{
				lstUsersList.Items.Add(user.UserName);
			}
		}

		lstUACList.Items.Clear();
		// Nothing still
	}

	void FillUACListBox()
	{
		if (Configurations.UserAccessControl.AllowedList != null)
			foreach (string ip in Configurations.UserAccessControl.AllowedList)
			{
				lstUACList.Items.Add(new ListItem(_Str_AllowedName + ip, _Str_AllowedList));
			}
		if (Configurations.UserAccessControl.AllowedRange != null)
			foreach (Configurations.UserAccessControlConfig.IPRange range in Configurations.UserAccessControl.AllowedRange)
			{
				lstUACList.Items.Add(new ListItem(_Str_AllowedName + range.Low + _Str_RangeSeperate + range.High, _Str_AllowedRange));
			}

		if (Configurations.UserAccessControl.BlockedList != null)
			foreach (string ip in Configurations.UserAccessControl.BlockedList)
			{
				lstUACList.Items.Add(new ListItem(_Str_BlockedName + ip, _Str_BlockedList));
			}

		if (Configurations.UserAccessControl.BlockedRange != null)
			foreach (Configurations.UserAccessControlConfig.IPRange range in Configurations.UserAccessControl.BlockedRange)
			{
				lstUACList.Items.Add(new ListItem(_Str_BlockedName + range.Low + _Str_RangeSeperate + range.High, _Str_BlockedRange));
			}
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

	private bool IsValid_AddNewUser()
	{
		bool result = true;

		txtNewUsername.Text = txtNewUsername.Text.Trim();
		txtNewPassword.Text = txtNewPassword.Text.Trim();

		if (txtNewUsername.Text.Length == 0)
		{
			AddError("Username is required for new users.");
			result = false;
		}
		if (txtNewPassword.Text.Length == 0)
		{
			AddError("Password is required for new users.");
			result = false;
		}

		if (chkNewPages.Checked == false && chkNewImages.Checked == false && chkNewDownloads.Checked)
		{
			AddError("You have to apply at least one access to the new user.");
			result = false;
		}

		return result;
	}

	bool IsValid_UacAddRule()
	{
		bool result = true;

		if (rbtnUacAllow.Checked == false && rbtnUacBlock.Checked == false)
		{
			AddError("Please select 'Rule mode' to add a new rule.");
			result = false;
		}
		if (rbtnUacAddIP.Checked == false && rbtnUacAddRange.Checked == false)
		{
			AddError("Please select 'IP range mode' to add a new rule.");
			result = false;
		}

		if (rbtnUacAddIP.Checked)
		{
			txtSingleIP.Text = txtSingleIP.Text.Trim();
			if (txtSingleIP.Text.Length == 0)
			{
				AddError("IP Address is required in order to add a new rule.");
				result = false;
			}
		}
		if (rbtnUacAddRange.Checked)
		{
			txtHighRangeIP.Text = txtHighRangeIP.Text.Trim();
			txtLowRangeIP.Text = txtLowRangeIP.Text.Trim();
			if (txtHighRangeIP.Text.Length == 0)
			{
				AddError("High range IP address is required in order to add a new rule.");
				result = false;
			}
			if (txtLowRangeIP.Text.Length == 0)
			{
				AddError("Low range IP address is required in order to add a new rule.");
				result = false;
			}
		}

		return result;
	}


	protected void Page_Init(object sender, EventArgs e)
	{
		FillTheListBoxes();
		FillUACListBox();
		LoadFormData();
	}


	protected override void OnPreRender(EventArgs e)
	{
		DisplayErrors(_errorsList);
		FillTheListBoxes();
		FillUACListBox();
		LoadFormData();

		base.OnPreRender(e);
	}

	protected void btnLoginEnable_Click(object sender, EventArgs e)
	{
		if (Configurations.Authentication.Users.Count == 0)
		{
			AddError("There is no user. Please add a user first.");
			return;
		}

		chkLoginEnabled.Checked = true;
		Configurations.Authentication.Enabled = true;
		Configurations.SaveSettings();
	}
	protected void btnLoginDisable_Click(object sender, EventArgs e)
	{
		chkLoginEnabled.Checked = false;
		Configurations.Authentication.Enabled = false;
		Configurations.SaveSettings();
	}
	protected void btnNewUserAdd_Click(object sender, EventArgs e)
	{
		if (IsValid_AddNewUser())
		{
			Configurations.AuthenticationConfig.User newUser = new Configurations.AuthenticationConfig.User();
			newUser.UserName = txtNewUsername.Text;
			newUser.Password = txtNewPassword.Text;
			newUser.Pages = chkNewPages.Checked;
			newUser.Images = chkNewImages.Checked;
			newUser.Downloads = chkNewDownloads.Checked;

			Configurations.Authentication.Users.Add(newUser);
			Configurations.SaveSettings();

			txtNewUsername.Text = "";
			txtNewPassword.Text = "";
		}
	}
	protected void btnDeleteUser_Click(object sender, EventArgs e)
	{
		if (chkDeleteUserSure.Checked == false)
		{
			AddError("Please check the \"Are you sure?\" checkbox.");
		}
		else
		{
			if (lstUsersList.SelectedIndex < 0)
			{
				AddError("Please select a user to delete.");
				return;
			}

			// at least one user should be there
			if (Configurations.Authentication.Users.Count <= 1)
			{
				AddError("There should be at least one user. This user can not be deleted.");
				return;
			}

			// the selected user
			string selectedUser = lstUsersList.SelectedValue;

			// search for the user in the users list
			Configurations.AuthenticationConfig.User user = Configurations.Authentication.GetByUsername(selectedUser);
			if (!string.IsNullOrEmpty(user.UserName) && user.UserName == selectedUser)
			{
				// remove it
				Configurations.Authentication.Users.Remove(user);
				Configurations.SaveSettings();
				return;
			}

			// there is no such user
			AddError("Could not find selected user.");
		}
	}
	protected void btnUacAddRule_Click(object sender, EventArgs e)
	{
		if (IsValid_UacAddRule())
		{
			if (rbtnUacAllow.Checked)
			{
				if (rbtnUacAddIP.Checked)
				{
					if (Configurations.UserAccessControl.AllowedList == null)
						Configurations.UserAccessControl.AllowedList = new List<string>();
					Configurations.UserAccessControl.AllowedList.Add(txtSingleIP.Text);

				}
				else if (rbtnUacAddRange.Checked)
				{
					if (Configurations.UserAccessControl.AllowedRange == null)
						Configurations.UserAccessControl.AllowedRange = new List<Configurations.UserAccessControlConfig.IPRange>();
					Configurations.UserAccessControlConfig.IPRange range;
					range.High = txtHighRangeIP.Text;
					range.Low = txtLowRangeIP.Text;
					Configurations.UserAccessControl.AllowedRange.Add(range);
				}
			}
			else if (rbtnUacBlock.Checked)
			{
				if (rbtnUacAddIP.Checked)
				{
					if (Configurations.UserAccessControl.BlockedList == null)
						Configurations.UserAccessControl.BlockedList = new List<string>();
					Configurations.UserAccessControl.BlockedList.Add(txtSingleIP.Text);
				}
				else if (rbtnUacAddRange.Checked)
				{
					if (Configurations.UserAccessControl.BlockedRange == null)
						Configurations.UserAccessControl.BlockedRange = new List<Configurations.UserAccessControlConfig.IPRange>();

					Configurations.UserAccessControlConfig.IPRange range;
					range.High = txtHighRangeIP.Text;
					range.Low = txtLowRangeIP.Text;
					Configurations.UserAccessControl.BlockedRange.Add(range);
				}
			}

			// save to files
			Configurations.SaveSettings();
			//AddError("Not implemented!");

			txtHighRangeIP.Text = "";
			txtLowRangeIP.Text = "";
			txtSingleIP.Text = "";
		}
	}
	protected void btnUACDelete_Click(object sender, EventArgs e)
	{
		if (chkUACDeleteSure.Checked)
		{
			if (lstUACList.SelectedIndex < 0)
			{
				AddError("Please select an IP from list to delete.");
				return;
			}

			string selectedType = lstUACList.SelectedValue;
			string selectedItem = lstUACList.SelectedItem.Text;

			if (selectedItem.StartsWith(_Str_AllowedName))
				selectedItem = selectedItem.Remove(0, _Str_AllowedName.Length);

			if (selectedItem.StartsWith(_Str_BlockedName))
				selectedItem = selectedItem.Remove(0, _Str_BlockedName.Length);


			if (selectedType == _Str_AllowedList)
			{
				Configurations.UserAccessControl.AllowedList.Remove(selectedItem);
			}
			else if (selectedType == _Str_AllowedRange)
			{
				string[] rangeParts = selectedItem.Split(new string[] { _Str_RangeSeperate }, StringSplitOptions.RemoveEmptyEntries);
				if (rangeParts.Length == 2)
				{
					RemoveIPRangeFromList(Configurations.UserAccessControl.AllowedRange,
						rangeParts[0], rangeParts[1]);
				}
				else
				{
					AddError("Invalid IP is selected!");
				}
			}
			if (selectedType == _Str_BlockedList)
			{
				Configurations.UserAccessControl.BlockedList.Remove(selectedItem);
			}
			else if (selectedType == _Str_BlockedRange)
			{
				string[] rangeParts = selectedItem.Split(new string[] { _Str_RangeSeperate }, StringSplitOptions.RemoveEmptyEntries);
				if (rangeParts.Length == 2)
				{
					RemoveIPRangeFromList(Configurations.UserAccessControl.BlockedRange,
						rangeParts[0], rangeParts[1]);
				}
				else
				{
					AddError("Invalid IP is selected!");
				}
			}

			// Save the configs
			Configurations.SaveSettings();
		}
		else
		{
			AddError("Please check the \"Are you sure?\" checkbox.");
		}
	}

	void RemoveIPRangeFromList(List<Configurations.UserAccessControlConfig.IPRange> ipRange, string low, string high)
	{
		int found = -1;
		for (int i = 0; i < ipRange.Count; i++)
		{
			Configurations.UserAccessControlConfig.IPRange range = ipRange[0];
			if (range.Low == low && range.High == high)
			{
				found = i;
				break;
			}
		}
		if (found != -1)
			ipRange.RemoveAt(found);
	}

	protected void btnUACEnable_Click(object sender, EventArgs e)
	{
		chkUACEnabled.Checked = true;
		Configurations.UserAccessControl.Enabled = true;
		Configurations.SaveSettings();
	}
	protected void btnUACDisable_Click(object sender, EventArgs e)
	{
		chkUACEnabled.Checked = false;
		Configurations.UserAccessControl.Enabled = false;
		Configurations.SaveSettings();
	}
}
