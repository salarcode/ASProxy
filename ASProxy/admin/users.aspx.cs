using System;
using System.Collections.Generic;
using SalarSoft.ASProxy;

public partial class Admin_Users : System.Web.UI.Page
{
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
        LoadFormData();
    }


    protected override void OnPreRender(EventArgs e)
    {
        DisplayErrors(_errorsList);
        FillTheListBoxes();

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
            AddError("Not implemented!");

            txtHighRangeIP.Text = "";
            txtLowRangeIP.Text = "";
            txtSingleIP.Text = "";
        }
    }
    protected void btnUACDelete_Click(object sender, EventArgs e)
    {
        if (chkUACDeleteSure.Checked == false)
        {
            AddError("Please check the \"Are you sure?\" checkbox.");
        }
        else
        {
            AddError("Not implemented!");
        }
    }
}
