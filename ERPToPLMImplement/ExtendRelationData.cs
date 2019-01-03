using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ERPToPLMImplement {
    public class ExtendRelationData {
        public ExtendRelationData(XmlNode relationNode) {
            if (relationNode == null) {
                throw new ArgumentNullException("relationNode");
            }
            InitData(relationNode);
        }
        private static string PATH_RELATION_NAME = "RelatioName";
        private static string PATH_ORDER_SEQ = "orderSeq";
        private static string PATH_PART_RELATION_NAME = "PartRelation";
        private static string PATH_PART_CLASS_NAME = "PartClassName";
        private static string PATH_INHERIT_FROMPARENT = "InheritAttrsFromParent";
        private static string PATH_INHERT_TOCHILD = "InheritAttrsToChild";
        private static string PATH_INHERIT = "InheritAttr";


        public List<InheritAttrData> ToChildLists {
            get { return _toChildLists; }
        }
        private List<InheritAttrData> _toChildLists;

        public List<InheritAttrData> FromParentlists {
            get { return _fromParentlists; }
        }
        private List<InheritAttrData> _fromParentlists;
        /// <summary>
        /// 订单序号
        /// </summary>
        public string OrderSeq {
            get { return _orderSeq; }
        }
        private string _orderSeq;

        /// <summary>
        /// 关系名称
        /// </summary>
        public string RelationName {
            get { return _relationName; }
        }
        private string _relationName;

        public string PartRelationName {
            get { return _partRelationName; }
        }
        private string _partRelationName;

        public string PartClassName {
            get { return _partClassName; }
        }
        private string _partClassName;

        private void InitData(XmlNode node) {
            if (node == null) {
                throw new ArgumentNullException("node");
            }
            var relationNameNode = node.SelectSingleNode(PATH_RELATION_NAME);
            _relationName = relationNameNode == null || string.IsNullOrEmpty(relationNameNode.InnerText) ? null : relationNameNode.InnerText;
            var orderSeqNode = node.SelectSingleNode(PATH_ORDER_SEQ);
            _orderSeq = orderSeqNode == null || string.IsNullOrEmpty(orderSeqNode.InnerText) ? null : orderSeqNode.InnerText;
            var partRelationName = node.SelectSingleNode(PATH_PART_RELATION_NAME);
            _partRelationName = partRelationName == null || string.IsNullOrEmpty(partRelationName.InnerText) ? null : partRelationName.InnerText;
            var partClassNameNode = node.SelectSingleNode(PATH_PART_CLASS_NAME);
            _partClassName = partClassNameNode == null || string.IsNullOrEmpty(partClassNameNode.InnerText) ? null : partClassNameNode.InnerText;
            _fromParentlists = GetFromParentAttrs(node);
            _toChildLists = GetToChildAttrs(node);
        }

        private List<InheritAttrData> GetToChildAttrs(XmlNode node) {
            if (node==null||node.ChildNodes==null||node.ChildNodes.Count==0) {
                throw new ArgumentNullException("node");
            }
            List<InheritAttrData> list = new List<InheritAttrData>();

            var parentNode = node.SelectSingleNode(PATH_INHERT_TOCHILD);
            if (parentNode==null||parentNode.ChildNodes==null||parentNode.ChildNodes.Count==0) {
                return list;
            }
            var childNodes = parentNode.SelectNodes(PATH_INHERIT);
            if (childNodes==null||childNodes.Count==0) {
                return list;
            }
            foreach (XmlNode node1 in childNodes) {
                InheritAttrData data = new InheritAttrData(node1);
                list.Add(data);
            }
            return list;
        }

        private List<InheritAttrData> GetFromParentAttrs(XmlNode node) {
            if (node == null || node.ChildNodes == null || node.ChildNodes.Count == 0) {
                throw new ArgumentNullException("node");
            }
            List<InheritAttrData> list = new List<InheritAttrData>();

            var parentNode = node.SelectSingleNode(PATH_INHERIT_FROMPARENT);
            if (parentNode == null || parentNode.ChildNodes == null || parentNode.ChildNodes.Count == 0) {
                return list;
            }
            var childNodes = parentNode.SelectNodes(PATH_INHERIT);
            if (childNodes == null || childNodes.Count == 0) {
                return list;
            }
            foreach (XmlNode node1 in childNodes) {
                InheritAttrData data = new InheritAttrData(node1);
                list.Add(data);
            }
            return list;
        }
    }
}
