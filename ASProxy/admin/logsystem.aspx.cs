using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SalarSoft.ASProxy;
using System.IO;
using System.Threading;

public partial class Admin_Logsystem : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void Page_Init(object sender, EventArgs e)
    {
        LoadErrorLogList();
        LoadActivityLogList();
        LoadFormData();
    }

    void GetActivityLogFileDetails(out string logsPath, out string logFilePattern)
    {
        string logFiles;

        logFiles = string.Format(Configurations.LogSystem.ActivityLog_Location, "TEST");
        logFiles = HttpContext.Current.Server.MapPath(logFiles);

        logsPath = Path.GetDirectoryName(logFiles);
        logFilePattern = "*" + Path.GetExtension(logFiles);
    }

    void GetErrorLogFileDetails(out string logsPath, out string logFilePattern)
    {
        string logFiles;

        logFiles = string.Format(Configurations.LogSystem.ErrorLog_location, "TEST");
        logFiles = HttpContext.Current.Server.MapPath(logFiles);

        logsPath = Path.GetDirectoryName(logFiles);
        logFilePattern = "*" + Path.GetExtension(logFiles);
    }

    string GetActivityLogSelectedLog()
    {
        string logsPath;
        string logsPattern;
        GetActivityLogFileDetails(out logsPath, out logsPattern);

        return Path.Combine(logsPath, cmbActivityList.SelectedValue);
    }

    string GetErrorLogSelectedLog()
    {
        string logsPath;
        string logsPattern;
        GetErrorLogFileDetails(out logsPath, out logsPattern);

        return Path.Combine(logsPath, cmbErrorsList.SelectedValue);
    }

    private void LoadFormData()
    {

    }

    private void LoadActivityLogList()
    {
        string logsPath;
        string logsPattern;
        GetActivityLogFileDetails(out logsPath, out logsPattern);

        if (!Directory.Exists(logsPath))
            return;

        string[] files = Directory.GetFiles(logsPath, logsPattern, SearchOption.TopDirectoryOnly);
        Array.Reverse(files);

        cmbActivityList.Items.Clear();
        for (int i = 0; i < files.Length; i++)
        {
            string fileName = Path.GetFileName(files[i]);

            // Activity log files
            cmbActivityList.Items.Add(fileName);
        }
    }

    private void LoadErrorLogList()
    {
        string logsPath;
        string logsPattern;
        GetErrorLogFileDetails(out logsPath, out logsPattern);

        if (!Directory.Exists(logsPath))
            return;

        string[] files = Directory.GetFiles(logsPath, logsPattern, SearchOption.TopDirectoryOnly);
        Array.Reverse(files);

        cmbErrorsList.Items.Clear();
        for (int i = 0; i < files.Length; i++)
        {
            string fileName = Path.GetFileName(files[i]);

            // Error log files
            cmbErrorsList.Items.Add(fileName);
        }
    }



    protected void btnAView_Click(object sender, EventArgs e)
    {
        string logFile = GetActivityLogSelectedLog();

        if (File.Exists(logFile))
            txtViewLog.Text = File.ReadAllText(logFile);
        else
            txtViewLog.Text = "File not found!";
    }
    protected void btnADownload_Click(object sender, EventArgs e)
    {
        try
        {
            string logFile = GetActivityLogSelectedLog();
            string headerFileName = Path.GetFileName(logFile).Replace(' ', '_');

            Response.ClearContent();
            Response.ClearHeaders();

            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment; filename="+headerFileName);
            Response.TransmitFile(logFile);
            Response.End();
        }
        catch (ThreadAbortException)
        {
        }
        catch (Exception ex)
        {
            txtViewLog.Text = ex.Message;
        }
    }
    protected void btnADelete_Click(object sender, EventArgs e)
    {
        string logFile = GetActivityLogSelectedLog();
        try
        {
            File.Delete(logFile);

            LoadActivityLogList();
        }
        catch (Exception ex)
        {
            txtViewLog.Text = ex.Message;
        }
    }
    protected void btnAClear_Click(object sender, EventArgs e)
    {
        string logsPath;
        string logsPattern;
        GetActivityLogFileDetails(out logsPath, out logsPattern);

        if (!Directory.Exists(logsPath))
            return;

        string[] files = Directory.GetFiles(logsPath, logsPattern, SearchOption.TopDirectoryOnly);
        for (int i = 0; i < files.Length; i++)
        {
            string fileName = files[i];

            try
            {
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                txtViewLog.Text += "\nError in deleteing file " + Path.GetFileName(fileName) + " " + ex.Message;
            }
        }
    }


    protected void btnEView_Click(object sender, EventArgs e)
    {
        string logFile = GetErrorLogSelectedLog();

        if (File.Exists(logFile))
            txtViewLog.Text = File.ReadAllText(logFile);
        else
            txtViewLog.Text = "File not found!";
    }
    protected void btnEDownload_Click(object sender, EventArgs e)
    {
        try
        {
            string logFile = GetErrorLogSelectedLog();
            string headerFileName = Path.GetFileName(logFile).Replace(' ', '_');

            Response.ClearContent();
            Response.ClearHeaders();

            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + headerFileName);
            Response.TransmitFile(logFile);
            Response.End();
        }
        catch (ThreadAbortException)
        {
        }
        catch (Exception ex)
        {
            txtViewLog.Text = ex.Message;
        }
    }
    protected void btnEDelete_Click(object sender, EventArgs e)
    {
        string logFile = GetErrorLogSelectedLog();
        try
        {
            File.Delete(logFile);

            LoadErrorLogList();
        }
        catch (Exception ex)
        {
            txtViewLog.Text = ex.Message;
        }
    }
    protected void btnEClear_Click(object sender, EventArgs e)
    {
        string logsPath;
        string logsPattern;
        GetErrorLogFileDetails(out logsPath, out logsPattern);

        if (!Directory.Exists(logsPath))
            return;

        string[] files = Directory.GetFiles(logsPath, logsPattern, SearchOption.TopDirectoryOnly);
        for (int i = 0; i < files.Length; i++)
        {
            string fileName = files[i];

            try
            {
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                txtViewLog.Text += "\nError in deleteing file " + Path.GetFileName(fileName) + " " + ex.Message;
            }
        }
    }
}
