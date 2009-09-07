using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SalarSoft.ASProxy;

public partial class admin_users : System.Web.UI.Page
{
    List<string> _errorsList = new List<string>();

    void AddError(string errorMessage)
    {
        _errorsList.Add(errorMessage);
    }

    void DisplayErrors(List<string> errorsList)
    {
        if (errorsList.Count == 0)
            return;
        //ltErrorsList.Visible = true;
        string display = "<ul style='color:Red;'>";
        display += "</ul>";
        foreach (string item in errorsList)
        {
            display = "<li>" + item + "</li>";
        }
        //ltErrorsList.Text = display;
    }


    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void btnLoginEnable_Click(object sender, EventArgs e)
    {
        if (Configurations.Authentication.Users.Count == 0)
        {
            AddError("There is no user. Please add a user first.");
            return;
        }

        
        Configurations.Authentication.Enabled = true;
        Configurations.SaveSettings();
    }
    protected void btnLoginDisable_Click(object sender, EventArgs e)
    {
        Configurations.Authentication.Enabled = false;
        Configurations.SaveSettings();
    }
    protected void btnNewUserAdd_Click(object sender, EventArgs e)
    {

    }
    protected void btnDeleteUser_Click(object sender, EventArgs e)
    {

    }
    protected void btnUacAddRule_Click(object sender, EventArgs e)
    {

    }
    protected void btnUACDelete_Click(object sender, EventArgs e)
    {

    }
}
