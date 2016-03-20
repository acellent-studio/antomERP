using Microsoft.Owin;
using Owin;
using System;
using System.IO;
using System.Reflection;
using System.Web.Http;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using System.Web.Http.Dispatcher;
using System.Threading.Tasks;
using Microsoft.Owin.Extensions;
using System.Web;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

[assembly: OwinStartup(typeof(antom.WebApi.WebStartup))]

namespace antom.WebApi
{

    using AppFunc = Func<
        IDictionary<string, object>, // Environment
        Task>; // Done

    /// <summary>
    /// OWIN 的啟動類別 (startup class)。
    /// </summary>
    public class WebStartup
    {
        /// <summary>
        /// OWIN self-hosted web startup 設定方法。此方法會使用 OWIN 的 UseFileServer 及 UseStaticFiles，
        /// 並從 webconfig.json 檔案中讀取網站根目錄的設定值。\n 
        /// 預設根目錄為 webroot，其下的 public 目錄為允許瀏覽，而 files 目錄則為靜態目錄。\n
        /// 註：苦置於 webroot 下的 front-end 網頁使用 AngularJS，此 web 可搭配 html5Mode=true。
        /// </summary>
        /// <param name="app">傳入 Owin.IAppBuilder 型態的界面 (interface)</param>
        public void Configuration(IAppBuilder app)
        {
            string staticFilesDir = "";

            if (WebConfig.WebDir.ToString().Trim() != "")
            {
                // Use user customized directory as web root directory
                staticFilesDir = WebConfig.WebDir;
            }
            else
            {
                // Use service install path as default web root directory
                staticFilesDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "webroot");
            }

            // This line is used for URL rewrite
            //app.Use(typeof(UrlRewritter));

            // 使用自定義 UseAngularServer extention
            app.UseAngularServer("/", "/index.html");

            // Setting OWIN based web root directory
            // 此 UseFileServer 的設定能 regular 啟動 web，但無法正確解析 AngularJS html5Mode
            app.UseFileServer(new FileServerOptions()
            {
                RequestPath = PathString.Empty,
                FileSystem = new PhysicalFileSystem(@staticFilesDir),
            });

            // If needs a staticfile folder, unmark the following code
            // Only serve files requested by name.
            app.UseStaticFiles("/files");

            // Turns on static files, directory browsing, and default files.
            app.UseFileServer(new FileServerOptions()
            {
                RequestPath = new PathString("/public"),
                FileSystem = new PhysicalFileSystem(Path.Combine(@staticFilesDir, "public")),
                EnableDirectoryBrowsing = true,
            });

            // Setting up a default .Net WebApi route
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(IHttpControllerSelector), new antomController(config)); //使用定義於外部dll的controller取代內部controller
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            app.UseWebApi(config);

            // Making URL rewrite working
            //app.UseStageMarker(PipelineStage.MapHandler);
        }
    }

    #region AngularJS html5Mode Middleware
    /// <summary>
    /// 此為 OWIN self-hosted web 的擴充類別 (Extention class)。
    /// 讓 OWIN 可使用 AngularJS html5Mode 為 enabled(=true) 的前端網頁程式。
    /// </summary>
    public static class AngularServerExtension
    {
        /// <summary>
        /// OWIN self-hosted web 的擴充方法。
        /// </summary>
        /// <param name="builder">傳入 Owin.IAppBuilder 型態的界面 (interface)</param>
        /// <param name="rootPath">網站的根目錄，預設值為 "/"。</param>
        /// <param name="entryPath">網站的啟始網頁，預設值為 "/index.html"。</param>
        /// <returns>傳回 public static 型態的 OWIN.IAppBuilder。</returns>
        public static IAppBuilder UseAngularServer(this IAppBuilder builder, string rootPath = "/", string entryPath = "/index.html")
        {
            var options = new AngularServerOptions()
            {
                FileServerOptions = new FileServerOptions()
                {
                    EnableDirectoryBrowsing = false,
                    FileSystem = new PhysicalFileSystem(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rootPath))
                },
                EntryPath = new PathString(entryPath)
            };

            builder.UseDefaultFiles(options.FileServerOptions.DefaultFilesOptions);

            return builder.Use(new Func<AppFunc, AppFunc>(next => new AngularServerMiddleware(next, options).Invoke));
        }
    }

    /// <summary>
    /// OWIN self-hosted web 的 Server Options 的擴充類別 (Extention class)。
    /// </summary>
    public class AngularServerOptions
    {
        /// <summary>
        /// 欄位：設定(set)或讀取(get) Microsoft.Owin.StaticFiles.FileServerOptions。
        /// </summary>
        public FileServerOptions FileServerOptions { get; set; }

        /// <summary>
        /// 欄位：設定(set)或讀取(get) Microsoft.Owin.PathString。
        /// </summary>
        public PathString EntryPath { get; set; }

        /// <summary>
        /// 欄位：讀取(get) AngularServerOptions.EntryPath 的值。
        /// </summary>
        public bool Html5Mode
        {
            get
            {
                return EntryPath.HasValue;
            }
        }

        /// <summary>
        /// AngularServerOptions 建構子。
        /// </summary>
        public AngularServerOptions()
        {
            FileServerOptions = new FileServerOptions();
            EntryPath = PathString.Empty;
        }
    }

    /// <summary>
    /// OWIN Middleware 的擴充類別，用以搭配 AngularJS 使用。
    /// </summary>
    public class AngularServerMiddleware
    {
        private readonly AngularServerOptions _options;
        private readonly AppFunc _next;
        private readonly StaticFileMiddleware _innerMiddleware;

        /// <summary>
        /// AngularServerMiddleware 建構子。有兩個固定參數 next 及 options。
        /// </summary>
        /// <param name="next">傳入 AppFunc 型態之委派參數 (delegate)</param>
        /// <param name="options">傳入 AngularServerOptions 型態之類別</param>
        public AngularServerMiddleware(AppFunc next, AngularServerOptions options)
        {
            _next = next;
            _options = options;

            _innerMiddleware = new StaticFileMiddleware(next, options.FileServerOptions.StaticFileOptions);
        }

        /// <summary>
        /// 以非同步方式趨動 AngularServerMiddleware 的 threading task.
        /// </summary>
        /// <param name="arg">傳入 System.Collections.Generic.IDictionary 型態的參數</param>
        /// <returns>回傳非同步執行結果。若找不到網頁，server 產生 404 錯誤時，將 web routing 指回 EntryPath。</returns>
        public async Task Invoke(IDictionary<string, object> arg)
        {
            await _innerMiddleware.Invoke(arg);
            // route to root path if the status code is 404
            // and need support angular html5mode
            if ((int)arg["owin.ResponseStatusCode"] == 404 && _options.Html5Mode)
            {
                arg["owin.RequestPath"] = _options.EntryPath.Value;
                await _innerMiddleware.Invoke(arg);
            }
        }
    }
    #endregion

}
