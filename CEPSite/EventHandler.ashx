<%@ WebHandler Language="C#" Class="EventHandler" %>

using System;
using System.Web;

public class EventHandler : IHttpHandler {
    
    public void ProcessRequest (HttpContext context) {

        string[] eventIDs = context.Request["Ids"].TrimEnd(",".ToCharArray()).Split(',');

        string returnHTML = "";
        
        foreach (var Id in eventIDs)
        {
            returnHTML += Parser.GetEventHTML(Id);
        }
        
        context.Response.ContentType = "text/html";
        context.Response.Write(returnHTML);
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

}