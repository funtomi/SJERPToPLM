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
        private static string PATH_DEBUG = "Debug";
        private static string PATH_EXTEND_RELATIONS = "ExtendRelations";
        private static string PATH_EXTEND_RELATION = "ExtendRelation";
        #region 配置字段

        /// <summary>
        /// 是否调试状态
        /// </summary>
        public bool IsDebug {
            get { return _isDebug; }
        }
        private bool _isDebug;
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

        public List<ExtendRelationData> ExtendRelations {
            get { return _extendRelations; }
        }
        private List<ExtendRelationData> _extendRelations;
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
            var debugNode = configNode.SelectSingleNode(PATH_DEBUG);
            _isDebug = debugNode == null || string.IsNullOrEmpty(debugNode.InnerText) ? false : Convert.ToBoolean(debugNode.InnerText);
            _extendRelations = GetExtendRelations(configNode);
        }


        private List<ExtendRelationData> GetExtendRelations(XmlNode configNode) {
            if (configNode==null||configNode.ChildNodes==null||configNode.ChildNodes.Count==0) {
                throw new ArgumentNullException("configNode");
            }
            var relationsNode = configNode.SelectSingleNode(PATH_EXTEND_RELATIONS);
            if (relationsNode==null||relationsNode.ChildNodes==null||relationsNode.ChildNodes.Count==0) {
                return null;
            }
            var relationNodes = relationsNode.SelectNodes(PATH_EXTEND_RELATION);
            if (relationNodes==null||relationNodes.Count==0) {
                return null;
            }
            List<ExtendRelationData> list = new List<ExtendRelationData>();
            foreach (XmlNode item in relationNodes) {
                ExtendRelationData data = new ExtendRelationData(item);
                list.Add(data);
            }
            return list;
        }
    }
}
