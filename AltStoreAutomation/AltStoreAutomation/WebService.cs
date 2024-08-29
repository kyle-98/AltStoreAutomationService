using System;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;

namespace AltStoreAutomation
{
     public class SettingsObj
     {
          [JsonProperty("PORT")]
          public string Port { get; set; }

          [JsonProperty("AUTH_KEY")]
          public string AuthToken { get; set; }
     }

     public class WebService : ServiceBase
     {
          private HttpListener _listener;
          private Thread _listenerThread;
          private SettingsObj _appSettings;
          private EventLog _eventLog;

          public WebService()
          {

               this.ServiceName = "AltStoreAutomation";
               _eventLog = new EventLog();
               if (!EventLog.SourceExists("AltStoreAutomation")) EventLog.CreateEventSource("AltStoreAutomation", "Application");
               _eventLog.Source = "AltStoreAutomation";
               _eventLog.Log = "Application";
          }


          private void ReadConfig()
          { 
               string jsonData = File.ReadAllText($"C:\\AltStoreAutomation\\config.json");
               _appSettings = JsonConvert.DeserializeObject<SettingsObj>(jsonData);
          }

          private void LogError(string errorMsg)
          {
               _eventLog.WriteEntry(errorMsg, EventLogEntryType.Error);
          }


          protected override void OnStart(string[] args)
          {
               try
               {
                    ReadConfig();

                    _listener = new HttpListener();
                    _listener.Prefixes.Add($"http://+:{_appSettings.Port}/");
                    _listener.Start();

                    _listenerThread = new Thread(new ThreadStart(HandleRequests));
                    _listenerThread.Start();
                    _eventLog.WriteEntry("Sucessfully started the http listener", EventLogEntryType.Information);
               }
               catch (Exception ex) 
               { 
                    LogError($"Error on service start: {ex}");
                    this.Stop();
               }
          }

          protected override void OnStop()
          {
               try
               {
                    _listener.Stop();
                    _listenerThread.Abort();
               }
               catch(Exception ex) { LogError($"Error on service stop: {ex.Message}"); }
          }

          private bool IsValidToken(HttpListenerRequest request)
          {
               var authenticationHeader = request.Headers["Authorization"];
               return !string.IsNullOrEmpty(authenticationHeader) && authenticationHeader == $"Important {_appSettings.AuthToken}";
          }

          private void RestartAppleMobileDeviceService()
          {
               try
               {
                    ServiceController appleService = new ServiceController("Apple Mobile Device Service");
                    if(appleService.Status == ServiceControllerStatus.Running)
                    {
                         _eventLog.WriteEntry("Stopping Apple TrashCan Service", EventLogEntryType.Information);
                         appleService.Stop();
                         appleService.WaitForStatus(ServiceControllerStatus.Stopped);
                    }

                    appleService.Start();
                    appleService.WaitForStatus(ServiceControllerStatus.Running);
                    _eventLog.WriteEntry("Started Apple TrashCan Service", EventLogEntryType.Information);
               }
               catch(Exception ex) { LogError($"Failed to restart service: {ex.Message}"); }
          }

          private void HandleRequests()
          {
               while(_listener.IsListening)
               {
                    try
                    {
                         HttpListenerContext context = _listener.GetContext();
                         HttpListenerRequest request = context.Request;
                         HttpListenerResponse response = context.Response;

                         //make sure proper request type
                         if (request.HttpMethod != "GET" || request.Url.AbsolutePath != $"/{_appSettings.AuthToken}")
                         {
                              response.StatusCode = (int)HttpStatusCode.NotFound;
                              byte[] buffer = Encoding.UTF8.GetBytes("Invalid Request");
                              response.ContentLength64 = buffer.Length;
                              response.OutputStream.Write(buffer, 0, buffer.Length);
                              response.OutputStream.Close();
                              
                              //log unknown traffic to event viewer
                              LogError($"Unknown or failed reqeust sent to host machine. => {request.Url.AbsolutePath} => {request.HttpMethod}");
                              continue;
                         }

                         //make sure authentication token exists and is correct
                         if (!IsValidToken(request))
                         {
                              response.StatusCode = (int)HttpStatusCode.Forbidden;
                              byte[] buffer = Encoding.UTF8.GetBytes("Forbidden");
                              response.ContentLength64 = buffer.Length;
                              response.OutputStream.Write(buffer, 0, buffer.Length);
                              response.OutputStream.Close();

                              //log unknown traffic to event viewer
                              LogError($"Invalid token sent to host machine");
                              continue;
                         }

                         RestartAppleMobileDeviceService();
                         response.StatusCode = (int)HttpStatusCode.OK;
                         byte[] responseBuffer = Encoding.UTF8.GetBytes("Success");
                         response.ContentLength64 = responseBuffer.Length;
                         response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
                         response.OutputStream.Close();
                    }
                    catch(HttpListenerException ex) { LogError($"Error in HttpListeningEvents: {ex.Message}"); }
                    catch(Exception ex) { LogError($"Something went wrong: {ex.Message}"); }
               }
          }

     }
}
