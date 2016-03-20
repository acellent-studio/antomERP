using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace antom.WebApi
{
    /// <summary>
    /// 使用外部 DLL 檔所定義的 controller，取代定義於 Antom Web Service 內部的 controller
    /// </summary>
    public class antomController : DefaultHttpControllerSelector
    {
        private readonly HttpConfiguration _configuration;

        /// <summary>
        /// Antom WebApi controller 建構子
        /// </summary>
        /// <param name="configuration">System.Web.Http.HttpConfiguration 型態的設定內容</param>
        public antomController(HttpConfiguration configuration)
            : base(configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 覆寫 System.Web.Http.HttpControllerDescriptor 的 SelectController 方法
        /// </summary>
        /// <param name="request">傳入 System.Net.Http.HttpRequestMessage 型態的 web request.</param>
        /// <returns></returns>
        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            var assembly = Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "antom.WebApi.dll"));
            var types = assembly.GetTypes(); //GetExportedTypes doesn't work with dynamic assemblies
            var matchedTypes = types.Where(i => typeof(IHttpController).IsAssignableFrom(i)).ToList();

            var controllerName = base.GetControllerName(request);
            var matchedController =
                matchedTypes.FirstOrDefault(i => i.Name.ToLower() == controllerName.ToLower() + "controller");
            
            return new HttpControllerDescriptor(_configuration, controllerName, matchedController);
        }
    }
}
