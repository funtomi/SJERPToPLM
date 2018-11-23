using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Json;
using System.Reflection;
using Thyt.TiPLM.UIL.Controls;

namespace ERPToPLMImplement {
    public class ERPServiceHelper {
        private ERPServiceHelper() {
            _configDoc = GetConfigDoc();
            _configData = new ConfigData(_configDoc);
            //InitMatchesData();
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
                    //InitMatchesData();
                }
                return _configDoc; }
        }
        private XmlDocument _configDoc;

        //public List<MatchData> MatchDatas {
        //    get { return _matchDatas; }
        //}
        //private List<MatchData> _matchDatas;

        public ConfigData ConfigData {
            get { return _configData; }
            set { _configData = value; }
        }
        private ConfigData _configData;
        private LoginData _loginData;
        /// <summary>
        /// 获取配置文档
        /// </summary>
        /// <returns></returns>
        private XmlDocument GetConfigDoc() {
            //if (!File.Exists(CONFIG_PATH)) {
            //    throw new ArgumentNullException(CONFIG_PATH);
            //}
            //XmlDocument doc = new XmlDocument();
            //doc.Load(CONFIG_PATH);
            //return doc;
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] cfgs = Directory.GetFiles(dir, "*config.xml");
            if (cfgs == null || cfgs.Length == 0) {
                MessageBoxPLM.Show("ERP导入配置没有配置在客户端！");
                return null;
            }
            return CheckEAIConfig(cfgs[0]);
        }
        /// <summary>
        /// 检查配置文件合法性
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private XmlDocument CheckEAIConfig(string fileName) {
            if (fileName == null) {
                MessageBoxPLM.Show("ERP导入配置文件没有内容！");
                return null;
            }
            if (!fileName.EndsWith(".xml")) {
                MessageBoxPLM.Show("ERP导入配置文件格式不正确，只支持xml格式的配置！");
                return null;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            var eaiAddNode = doc.SelectSingleNode("config//URL");
            if (eaiAddNode == null) {
                MessageBoxPLM.Show("ERP导入配置文件没有ERP地址，请补充！");
                return null;
            }
            var add = eaiAddNode.InnerText;
            if (string.IsNullOrEmpty(add)) {
                MessageBoxPLM.Show("ERP导入配置文件没有ERP地址，请补充！");
                return null;
            }
            return doc;
        }
        //private void InitMatchesData() {
        //    if (_configDoc == null) {
        //        throw new ArgumentNullException("_configDoc");
        //    }
        //    var matchsNode = _configDoc.SelectSingleNode("config/matches");
        //    if (matchsNode == null || matchsNode.ChildNodes == null || matchsNode.ChildNodes.Count == 0) {
        //        return;
        //    }
        //    var matchNodes = matchsNode.SelectNodes("match");
        //    if (matchNodes == null || matchNodes.Count == 0) {
        //        return;
        //    }
        //    _matchDatas = new List<MatchData>();
        //    foreach (XmlElement item in matchNodes) {
        //        var matchData = new MatchData(item);
        //        _matchDatas.Add(matchData);
        //    }
        //}

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
                ReadLoginResultData(result.ToString());
                return result.ToString(); 
            } catch (Exception) {
                throw;
            }
            
        }

        private void ReadLoginResultData(string result) {
            if (string.IsNullOrEmpty(result) || !result.StartsWith("{") || !result.EndsWith("}")) {
                return;
            }
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(result))) {
                DataContractJsonSerializer serializer1 = new DataContractJsonSerializer(typeof(LoginData));
                _loginData = (LoginData)serializer1.ReadObject(ms);
            }
        }

        /// <summary>
        /// 使用配置信息登录
        /// </summary>
        /// <returns></returns>
        public string LoginWithConfig() {
            return Login(_configData.LoginName,_configData.LoginPassword,_configData.ClientId);
        }

        public string Logout(int uid) {
            try {
                object[] args = new object[1];
                args[0] = uid;
                object result = WebServiceHelper.InvokeWebService(_configData.Url, "logout", args);
                return result.ToString();
            } catch (Exception) {
                
                throw;
            }
        }

        public bool SaveBaseWithConfig(int baseid, string keyvalue, string[] fieldvalues,out string errText) {
            var result = Login(_configData.LoginName, _configData.LoginPassword, _configData.ClientId);
            ReadLoginResultData(result);
            var saveResult = SaveBase(_configData.LoginName, _configData.LoginPassword, _configData.ClientId, _loginData.uid, baseid, keyvalue, fieldvalues,out errText);
            Logout(_loginData.uid);
            return saveResult;
        }

        public bool SaveBase(string username, string password, int clientid, int uid, int baseid, string keyvalue, string[] fieldvalues,out string errText) {
            try { 
                object[] args = new object[7];
                args[0] = username;
                args[1] = password;
                args[2] = clientid;
                args[3] = uid;
                args[4] = baseid;
                args[5] = keyvalue;
                args[6] = fieldvalues;
                object result = WebServiceHelper.InvokeWebService(_configData.Url, "saveBase", args);
                string err = "参数：";
                for (int i = 0; i < args.Length; i++) {
                    err += args[i].ToString() + ",";
                }
                MessageBoxPLM.Show(err+"/结果：" + result.ToString());
                return GetErrText(result,out errText);
            } catch (Exception) {
                throw;
            }
        }

        private bool GetErrText(object result,out string errText) {
            errText = "";
            if (!result.ToString().StartsWith("{")) {
                return true;
            }
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(result.ToString()))) {
                DataContractJsonSerializer serializer1 = new DataContractJsonSerializer(typeof(ErrData));
                errText = ((ErrData)serializer1.ReadObject(ms)).s4;
                return false;
            }  
        }

        public void OpenReport(string username, string password, int clientid, int uid,int repid, string requeststr) { }


    }
}
