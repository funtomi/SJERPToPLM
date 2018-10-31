using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ERPToPLMImplement {
    public class ImportPLMToERP {
        public ImportPLMToERP(XmlDocument doc) {
            if (doc==null) {
                throw new ArgumentNullException("doc");
            }
            InitData(doc);
        }

        private List<MatchData> _matchDatas;

        private void InitData(XmlDocument doc) {
            if (doc==null) {
                throw new ArgumentNullException("doc");
            }
            var matchsNode = doc.SelectSingleNode("config/matches");
            if (matchsNode==null||matchsNode.ChildNodes==null||matchsNode.ChildNodes.Count==0) {
                return;
            }
            var matchNodes = matchsNode.SelectNodes("match");
            if (matchNodes==null||matchNodes.Count==0) {
                return;
            }
            _matchDatas = new List<MatchData>();
            foreach (XmlElement item in matchNodes) {
                var matchData = new MatchData(item);
                _matchDatas.Add(matchData);
            }
        }

        //todo:测试数据版
        //public string ImportToERP() {
        //    var matchData = _matchDatas[0];
        //    var fdValue = BuildFieldValueArray(matchData);
        //    return ERPServiceHelper.Instance.SaveBaseWithConfig(matchData.BaseId, "0",fdValue);
        //}

        private string[] BuildFieldValueArray(MatchData matchData) {
            if (matchData==null) {
                throw new ArgumentNullException("matchData");
            }
            if (matchData.FieldValues==null||matchData.FieldValues.Count==0) {
                throw new ArgumentNullException("matchData.FieldValues");
            }
            var fdValue = new string[matchData.FieldValues.Count];
            for (int i = 0; i < fdValue.Length; i++) {
                fdValue[i] = string.Format("@{0}={1}",matchData.FieldValues[i].Erpfield,matchData.FieldValues[i].PlmValue);
            }
            return fdValue;
        }
    }
}
