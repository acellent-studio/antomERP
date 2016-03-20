using Newtonsoft.Json;

namespace antom.WebApi
{
    /// <summary>
    /// 讀取(get)或設定(set) web 設定之資料結構。所有欄位均為 JSON 型態。
    /// </summary>
    public class WebSettings
    {
        /// <summary>
        /// 欄位：讀取(get)或設定(set) web 的 URL，例如：http://+ 或 http://localhost...等。
        /// </summary>
        [JsonProperty]
        public string Url { get; set; }
        /// <summary>
        /// 欄位：設定(set)或讀取(get) web 的連接埠(port)，例如：80。
        /// </summary>
        [JsonProperty]
        public uint Port { get; set; }
        /// <summary>
        /// 欄位：設定(set)或讀取(get) web 的實體目錄。 
        /// </summary>
        [JsonProperty]
        public string WebDir { get; set; }
    }

    /// <summary>
    /// 設定(set)或讀取(get) web 設定之資料結構。
    /// 此類別主要用於接收來自於 WebSettings 類別的資料，並為 URL，Port及 WebDir 等欄位提供預設值。
    /// 同時 GetWebUrl 方法則傳回合併 URL + Port 之完整 web URL。例如：http://localhost:80。
    /// </summary>
    public class WebConfig
    {
        // Set the system default values
        private static string _url = "http://+";
        private static uint _port = 80;
        private static string _webDir = "";

        /// <summary>
        /// 欄位：設定(set)或讀取(get) web 的 URL，例如：http://+ 或 http://localhost...等。
        /// </summary>
        public static string Url
        {
            get { return _url; }
            set { _url= value; }
        }
        /// <summary>
        /// 欄位：設定(set)或讀取(get) web 的連接埠(port)，例如：80。
        /// </summary>
        public static uint Port
        {
            get { return _port; }
            set { _port = value; }
        }
        /// <summary>
        /// 欄位：設定(set)或讀取(get) web 的實體目錄。
        /// </summary>
        public static string WebDir
        {
            get { return _webDir; }
            set { _webDir = value; }
        }
        /// <summary>
        /// 方法：傳回 URL + Port 後的完整 web URL。
        /// </summary>
        public static string GetWebUrl
        {
            get { return Url + ":" + Port.ToString(); }
        }
    }

    /// <summary>
    /// 客製化 Log 類別。
    /// </summary>
    public class antomLog
    {
        /// <summary>
        /// 欄位：設定(set)或讀取(get) Log 內容。
        /// </summary>
        public static string LogMessage { get; set; }
    }
}
