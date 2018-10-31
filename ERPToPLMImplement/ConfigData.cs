using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ERPToPLMImplement {
    public class ConfigData {
        public ConfigData(XmlDocument doc) {
            if (doc==null) {
                throw new ArgumentNullException("doc");
            }
            InitData(doc);
        }
        private static string PATH_CONFIG = "config";
        private static string PATH_URL = "URL";
        private static string PATH_NAME = "name";
        private static string PATH_PWD = "password";
        private static string PATH_CLTID = "clientId";
        private static string PATH_CLASSES = "classes";
        #region 配置字段
        
        /// <summary>
        /// url
        /// </summary>
        public string Url {
            get { return _url; }
        }
        private string _url;

        /// <summary>
        /// 登录账号
        /// </summary>
        public string LoginName {
            get { return _loginName; }
        }
        private string _loginName;
        
        /// <summary>
        /// 登录密码
        /// </summary>
        public string LoginPassword {
            get { return _loginPassword; }
        }
        private string _loginPassword;

        /// <summary>
        /// clientId
        /// </summary>
        public int ClientId {
            get { return _clientId; }
        }
        private int _clientId;

        public int Uid {
            get { return _uid; }
            set { _uid = value; }
        }
        private int _uid;

        public List<string> Classes {
            get { return _classes; }
        }
        private List<string> _classes;
        #endregion

        /// <summary>
        /// 读取xml配置的数据
        /// </summary>
        /// <param name="doc"></param>
        private void InitData(XmlDocument doc) {
            if (doc==null) {
                return;
            }
            var configNode = doc.SelectSingleNode(PATH_CONFIG);
            if (configNode==null||configNode.ChildNodes==null||configNode.ChildNodes.Count==0) {
                return;
            }
            var urlNode = configNode.SelectSingleNode(PATH_URL);
            _url = urlNode == null || string.IsNullOrEmpty(urlNode.InnerText) ? "" : urlNode.InnerText;
            var nameNode = configNode.SelectSingleNode(PATH_NAME);
            _loginName = nameNode == null || string.IsNullOrEmpty(nameNode.InnerText) ? "" : nameNode.InnerText;
            var pwdNode = configNode.SelectSingleNode(PATH_PWD);
            _loginPassword = pwdNode == null || string.IsNullOrEmpty(pwdNode.InnerText) ? "" : pwdNode.InnerText;
            var cltIdName = configNode.SelectSingleNode(PATH_CLTID);
            _clientId = cltIdName == null || string.IsNullOrEmpty(cltIdName.InnerText) ? 0 : Convert.ToInt32(cltIdName.InnerText);
            var classesNode = configNode.SelectSingleNode(PATH_CLASSES);
            _classes = classesNode == null || string.IsNullOrEmpty(classesNode.InnerText) ? null : classesNode.InnerText.Split(',').ToList<string>();
        }
    }
}
