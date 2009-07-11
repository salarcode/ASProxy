<%@ WebHandler Language="C#" Class="ajax_test" %>

using System;
using System.Web;

public class ajax_test : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";
        context.Response.Write("<span style='color:gray; font-weight: bold'>Hello World! from ashx!</span>");
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}