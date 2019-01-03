using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ERPToPLMImplement {
    public class InheritAttrData {
        public InheritAttrData(XmlNode node) {
            if (node==null||node.ChildNodes==null||node.ChildNodes.Count==0) {
                throw new ArgumentNullException("node");
            }
            InitData(node);
        }
        private static string PATH_FROM = "From";
        private static string PATH_TO = "To";

        public string From {
            get { return _from; }
        }
        private string _from;

        public string To {
            get { return _to; }
        }
        private string _to;

        private void InitData(XmlNode node) {
            var fromNode = node.SelectSingleNode(PATH_FROM);
            _from = fromNode == null || string.IsNullOrEmpty(fromNode.InnerText) ? null : fromNode.InnerText;
            var toNode = node.SelectSingleNode(PATH_TO);
            _to = toNode == null || string.IsNullOrEmpty(toNode.InnerText) ? null : toNode.InnerText;
        }
    }
}
