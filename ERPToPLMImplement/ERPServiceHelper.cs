using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace ERPToPLMImplement {
    public class ERPServiceHelper {
        private ERPServiceHelper() {
            _configDoc = GetConfigDoc();
            _configData = new ConfigData(_configDoc);
        }
        static ERPServiceHelper() {
            Instance = new ERPServiceHelper();
        }

        public static ERPServiceHelper Instance;
        private static string CONFIG_PATH = "config.xml"; 

        public XmlDocument ConfigDoc {
            get {
                if (_configDoc==null) {
                    _configDoc = GetConfigDoc();
                    _configData = new ConfigData(_configDoc);
                }
                return _configDoc; }
        }
        private XmlDocument _configDoc;

        private ConfigData _configData;
        /// <summary>
        /// 获取配置文档
        /// </summary>
        /// <returns></returns>
        private XmlDocument GetConfigDoc() {
            if (!File.Exists(CONFIG_PATH)) {
                throw new ArgumentNullException(CONFIG_PATH);
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(CONFIG_PATH);
            return doc;
        }
         
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public string Login(string username, string password, int clientId) {
            //return _service.login("ad", "123456", 1003);

            //string url = "http://192.168.91.131:8080/acc/services/ERPService";
            try {
                object[] args = new object[3];
                args[0] = username;
                args[1] = password;
                args[2] = clientId;
                object result = WebServiceHelper.InvokeWebService(_configData.Url, "login", args);
                return result.ToString(); 
            } catch (Exception) {
                throw;
            }
            
        }

        /// <summary>
        /// 使用配置信息登录
        /// </summary>
        /// <returns></returns>
        public string LoginWithConfig() {
            return Login(_configData.LoginName,_configData.LoginPassword,_configData.ClientId);
        }

        public void Logout(int uid) { 
        
        }

        public void SaveBase(string username, string password, int clientid, int uid,int baseid, string keyvalue, string[] fieldvalues) { }

        public void OpenReport(string username, string password, int clientid, int uid,int repid, string requeststr) { }


    }
}
