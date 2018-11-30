using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ERPToPLMImplement {
    public class MatchData {
        public MatchData(XmlElement doc) {
            if (doc==null) {
                throw new ArgumentNullException("doc");
            }
            InitData(doc);
        }
        private static string PATH_BASEID = "baseid";
        private static string PATH_ADDKEYVALUE = "addkeyvalue";
        private static string PATH_FIELDVALUES = "fieldvalues";
        private static string PATH_FIELDVALUE = "fieldvalue";
        private static string PATH_PLMCLASS = "PlmClass";
        private static string PATH_PLMRELATION = "PlmRelation";
        private static string PATH_RELATIONNAME = "RalationName";
        private static string PATH_RIGHTRELATION = "RightRelation";

        #region match节点的参数

        public int RelationNum {
            get { return _relationNum; }
            set { _relationNum = value; }
        }
        private int _relationNum = 0;
        public int BaseId {
            get { return _baseId; }
        }
        private int _baseId;

        public string Addkeyvalue {
            get { return _addkeyvalue; }
        }
        private string _addkeyvalue;

        public string PlmClass {
            get { return _plmClass; }
        }
        private string _plmClass;

        public string PlmRelation {
            get { return _plmRelation; }
        }
        private string _plmRelation;

        public string RelationName {
            get { return _relationName; }
        }
        private string _relationName;

        public string RightRelation {
            get { return _rightRelation; }
        }
        private string _rightRelation;
        public List<FieldValueData> FieldValues {
            get { return _fieldValues; }
        }
        private List<FieldValueData> _fieldValues;

        #endregion
        private void InitData(XmlElement doc) {
            if (doc==null) {
                throw new ArgumentNullException("doc");
            }
            var baseIdNode = doc.SelectSingleNode(PATH_BASEID);
            _baseId = baseIdNode == null || string.IsNullOrEmpty(baseIdNode.InnerText) ? 0 : Convert.ToInt32(baseIdNode.InnerText);
            var addkvNode = doc.SelectSingleNode(PATH_ADDKEYVALUE);
            _addkeyvalue = addkvNode == null || string.IsNullOrEmpty(addkvNode.InnerText) ? "" : addkvNode.InnerText;
            var plmClassNode = doc.SelectSingleNode(PATH_PLMCLASS);
            _plmClass = plmClassNode == null || string.IsNullOrEmpty(plmClassNode.InnerText) ? "" : plmClassNode.InnerText;

            var relationNode = doc.SelectSingleNode(PATH_PLMRELATION);
            if (relationNode!=null&&relationNode.ChildNodes!=null&&relationNode.ChildNodes.Count>0) {
                var relationNameNode = relationNode.SelectSingleNode(PATH_RELATIONNAME);
                _relationName = relationNameNode == null || string.IsNullOrEmpty(relationNameNode.InnerText) ? "" : relationNameNode.InnerText;
                var rightRelationNode = relationNode.SelectSingleNode(PATH_RIGHTRELATION);
                _rightRelation = rightRelationNode == null || string.IsNullOrEmpty(rightRelationNode.InnerText) ? "" : rightRelationNode.InnerText;
            }

            var fvaluesNode = doc.SelectSingleNode(PATH_FIELDVALUES);
            if (fvaluesNode==null||fvaluesNode.ChildNodes==null||fvaluesNode.ChildNodes.Count==0) {
                return;
            }
            var fvalueNodes = fvaluesNode.SelectNodes(PATH_FIELDVALUE);
            if (fvalueNodes==null||fvalueNodes.Count==0) {
                return;
            }
            _fieldValues = new List<FieldValueData>();
            foreach (XmlElement item in fvalueNodes) {
                FieldValueData fieldValue = new FieldValueData(item);
                _fieldValues.Add(fieldValue);
            }
        }
    }
}
