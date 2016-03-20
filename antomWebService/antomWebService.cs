using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Owin.Hosting;
using System.Reflection;


namespace antom.WebApi
{
    /// <summary>
    /// 繼承自 System.ServiceProcess.ServiceBase 的 Windows Service 類別，用以啟動 OWIN self-hosted web server。
    /// </summary>
    public partial class WebService : ServiceBase
    {
        /// <summary>
        /// antom.WebApi 之 WebService 的建構子。
        /// </summary>
        public WebService()
        {
            InitializeComponent();
            antomLog.LogMessage = "Antom Web Service starting: \n";
        }

        /// <summary>
        /// 自存放於此 WebService 安裝目錄的 webconfig.json 讀取 Web 設定值。
        /// 包括 URL，Port 及 WebDir (指存放網頁之實體目錄)。
        /// WebService 之啟動記錄會寫入系統事件簿 AntomERP 中。
        /// </summary>
        /// <param name="args">傳入字串陣列型態之參數</param>
        protected override void OnStart(string[] args)
        {
            InternalStart(args);
        }

        internal void InternalStart(string[] args)
        {
            // Check if a webconfig.json exists first and retrieve setting values
            try
            {
                using (StreamReader readConfig = new StreamReader(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "webconfig.json"), Encoding.UTF8))
                {
                    string cfgContent = readConfig.ReadToEnd();
                    WebSettings cfg = JsonConvert.DeserializeObject<WebSettings>(cfgContent);
                    WebConfig.Url = cfg.Url.ToString();
                    WebConfig.Port = cfg.Port;
                    WebConfig.WebDir = cfg.WebDir.ToString();
                }
                antomLog.LogMessage = "Reading web setting from webconfig.json file successfully.\n";
            }
            catch (Exception ex)
            {
                antomLog.LogMessage = "Reading web setting from webconfig.json file failed, using default values instead.\n";
                antomLog.LogMessage += "Error message:\n" + ex.ToString();
            }
            finally
            {
                // If WebConfig.RootDir = "", meaning default setting of webroot directory is used, 
                // then translate the empty string to actual full path directory.
                // Otherwise, user customed directory is used.
                if (WebConfig.WebDir.ToString().Trim() == "")
                {
                    WebConfig.WebDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "webroot");
                }
            }

            antomLog.LogMessage += "Url: " + WebConfig.Url + "\n";
            antomLog.LogMessage += "Port: " + WebConfig.Port.ToString() + "\n";
            antomLog.LogMessage += "WebDir: " + WebConfig.WebDir + "\n";

            try
            {
                // Trying to start OWIN services
                WebApp.Start<WebStartup>(WebConfig.GetWebUrl);
                antomLog.LogMessage += "Web Service started successfully!\n";
                antomLog.LogMessage += "The web server is listening at " + WebConfig.GetWebUrl;
            }
            catch (Exception ex)
            {
                // OWIN services started failed.
                antomLog.LogMessage += "Service started failed!\n\n";
                antomLog.LogMessage += "Error message: \n" + ex.ToString();
            }

            eventLog1.WriteEntry(antomLog.LogMessage);

        }

        /// <summary>
        /// 停用此 WebService，並將停用記錄寫入系統事件簿 AntomERP。
        /// </summary>
        protected override void OnStop()
        {
            InternalStop();
        }

        internal void InternalStop()
        {
            eventLog1.WriteEntry("Antom Web Service is stopped on " + DateTime.Now.ToString());
        }
    }
}
