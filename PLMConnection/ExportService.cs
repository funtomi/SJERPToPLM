using ERPToPLMImplement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.PLL.Admin.DataModel;
using Thyt.TiPLM.PLL.Product2;
using Thyt.TiPLM.UIL.Common;
using Thyt.TiPLM.UIL.Controls;

namespace PLMConnection {
   public class ExportService {
        public ExportService(DEBusinessItem bItem) {
            if (bItem==null) {
                throw new ArgumentNullException("bItem");
            }
            _bItem = bItem;
            InitData(ERPServiceHelper.Instance.ConfigDoc);
        }

        private DEBusinessItem _bItem;
        private List<MatchData> _matchDatas;
        private static string SYS_NUMBER = "sys_ralation_number";

        private void InitData(XmlDocument doc) {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }
            var matchsNode = doc.SelectSingleNode("config/matches");
            if (matchsNode == null || matchsNode.ChildNodes == null || matchsNode.ChildNodes.Count == 0) {
                return;
            }
            var matchNodes = matchsNode.SelectNodes("match");
            if (matchNodes == null || matchNodes.Count == 0) {
                return;
            }
            _matchDatas = new List<MatchData>();
            foreach (XmlElement item in matchNodes) {
                var matchData = new MatchData(item);
                _matchDatas.Add(matchData);
            }
        }

        
        public bool ImportToERP(out string errText) {
            errText="";
            if (_matchDatas==null||_matchDatas.Count==0) {
                errText = "没有配置match节点！";
                return false;
            }
            //var s =_matchDatas.FindAll(p=>p.PlmClass=="dd");
            List<MatchData> datas = _matchDatas.FindAll(p => ModelContext.MetaModel.IsChild(p.PlmClass, _bItem.ClassName));
            foreach (var matchData in datas) {
                if (string.IsNullOrEmpty(matchData.RelationName)) {
                    var fdValue = BuildFieldValueArray(matchData);
                    if (!ERPServiceHelper.Instance.SaveBaseWithConfig(matchData.BaseId, matchData.Addkeyvalue, fdValue, out errText)) {
                        return false;
                    }
                } else {
                    if(!ImportRelations(_bItem, matchData,out errText))
                    {
                        return false;
                    }
                }
 
                
            }
            return true;
            
        }

        private bool ImportRelations(DEBusinessItem bItem, MatchData matchData, out string errText) {
            if (bItem==null) {
                throw new ArgumentNullException("bItem");
            }
            if (matchData==null) {
                throw new ArgumentNullException("matchData");
            }
            errText = "";
            List<string> itemIds = new List<string>();
            return ImportRelations(bItem,matchData,ref itemIds,out errText);
        }

        private bool ImportRelations(DEBusinessItem bItem, MatchData matchData, ref List<string> itemIds, out string errText) {
            if (bItem == null) {
                throw new ArgumentNullException("bItem");
            }
            if (matchData == null) {
                throw new ArgumentNullException("matchData");
            }
            if (itemIds == null) {
                itemIds = new List<string>();
            }
            errText = "";
            if (string.IsNullOrEmpty(matchData.RelationName)) {
                throw new ArgumentNullException("matchData.RelationName");
            }
            var relations = GetLinks(bItem, matchData.RelationName);
            if (relations == null || relations.Count == 0) {
                return true;
            }
            int number = 0;
            foreach (DERelationBizItemList lsit in relations.Values) {
                if (lsit==null||lsit.Count==0) {
                    continue;
                }
                number += lsit.Count;
            }
            matchData.RelationNum = number;
            foreach (KeyValuePair<DEBusinessItem,DERelationBizItemList> rlts in relations) {
                if (rlts.Value.Count == 0) {
                    continue;
                }
                for (int i = 0; i < rlts.Value.BizItems.Count; i++) {
                    var item = rlts.Value.BizItems[i] as DEBusinessItem;
                    var rlt = rlts.Value.RelationList[i] as DERelation2; 
                    if (item == null || rlt == null) {
                        continue;
                    }
                    if (itemIds.Contains(item.Id)) {
                        continue;
                    }
                    var fdValue = BuildFieldValueArrayWithRlt(matchData, item, rlt, rlts.Key);
                    if (!ERPServiceHelper.Instance.SaveBaseWithConfig(matchData.BaseId, matchData.Addkeyvalue, fdValue, out errText)) {
                        return false;
                    }
                    //itemIds.Add(item.Id);
                    //return ImportRelations(item, matchData, ref itemIds, out errText);
                }
            }

            return true;
        }

        private string[] BuildFieldValueArrayWithRlt(MatchData matchData, DEBusinessItem item, DERelation2 rlt, DEBusinessItem parentItem) {
            if (matchData == null) {
                throw new ArgumentNullException("matchData");
            }
            if (matchData.FieldValues == null || matchData.FieldValues.Count == 0) {
                throw new ArgumentNullException("matchData.FieldValues");
            }
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            if (rlt == null) {
                throw new ArgumentNullException("rlt");
            }
            var fdValue = new string[matchData.FieldValues.Count];
            for (int i = 0; i < fdValue.Length; i++) {
                if (matchData.FieldValues[i] != null && !string.IsNullOrEmpty(matchData.FieldValues[i].Plmfield)) {
                    if (matchData.FieldValues[i].Plmfield.ToUpper() == SYS_NUMBER.ToUpper()) {
                        //fdValue[i] = matchData.RelationNum.ToString();
                        fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, matchData.RelationNum.ToString());
                        // MessageBoxPLM.Show(matchData.RelationNum.ToString());
                        continue;
                    }
                }
                var classType = string.IsNullOrEmpty(matchData.FieldValues[i].PlmType) ? "0" : matchData.FieldValues[i].PlmType;
                if (matchData.FieldValues[i] == null || string.IsNullOrEmpty(matchData.FieldValues[i].Plmfield)) {
                    continue;
                }
                string plmValue = "";
                switch (classType) {
                    default:
                    case "0"://当前实体 
                        plmValue = GetAttrWithName(item, matchData.FieldValues[i].Plmfield);
                        fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, plmValue);
                        break;
                    case "1"://关联属性
                        fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, rlt.GetAttrValue(matchData.FieldValues[i].Plmfield.ToUpper()));
                        break;
                    case "2"://父实体属性
                        plmValue = GetAttrWithName(parentItem, matchData.FieldValues[i].Plmfield);
                        fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, plmValue);
                        break;
                    case "3"://最上层实体属性
                        plmValue = GetAttrWithName(_bItem, matchData.FieldValues[i].Plmfield);
                        fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, plmValue);
                        break;
                }
            }
                return fdValue;
        }

       /// <summary>
       /// 获取PLM属性值
       /// </summary>
       /// <param name="item"></param>
       /// <param name="attrName"></param>
       /// <returns></returns>
        private string GetAttrWithName(DEBusinessItem item,string attrName) {
            if (string.IsNullOrEmpty(attrName)) {
                throw new ArgumentNullException("Plmfield");
            }
            if (item==null) {
                throw new ArgumentNullException("item");
            }
            if (attrName.ToUpper()=="ID") {//代号
                return item.Id;
            }
            if (attrName.ToUpper()=="REVISION") {//版本号
                return item.LastRevision.ToString();
            }
            var val = item.GetAttrValue(item.ClassName, attrName.ToUpper());
            if (val==null) {
                return "";
            }
            return val.ToString();
        }

        private Dictionary<DEBusinessItem,DERelationBizItemList> GetLinks(DEBusinessItem item, string relation) {
            if (item==null) {
                throw new ArgumentNullException("item");
            }
            if (string.IsNullOrEmpty(relation)) {
                throw new ArgumentNullException("relation");
            }

            Dictionary<DEBusinessItem, DERelationBizItemList> lists = new Dictionary<DEBusinessItem, DERelationBizItemList>();
            GetLink(item, relation, ref lists);
            if (lists==null) {
                return lists;
            }
            return lists;
        }

        public void GetLink(DEBusinessItem item, string relation, ref Dictionary<DEBusinessItem,DERelationBizItemList> lists) {
            if (item == null) {
                return;
            }
            if (lists.Keys.Contains(item)) {
                return;
            }
            if (lists == null || lists.Count == 0) {
                lists = new Dictionary<DEBusinessItem, DERelationBizItemList>();
            }
            DERelationBizItemList relationBizItemList = item.Iteration.LinkRelationSet.GetRelationBizItemList(relation);
            if (relationBizItemList == null) {
                try {
                    relationBizItemList = PLItem.Agent.GetLinkRelationItems(item.Iteration.Oid, item.Master.ClassName, relation, ClientData.LogonUser.Oid, ClientData.UserGlobalOption);
                } catch {
                    return;
                }
            }
            if (relationBizItemList == null || relationBizItemList.Count == 0) {
                return;
            }
            lists.Add(item,relationBizItemList);
            foreach (var rltItem in relationBizItemList.BizItems) {
                var item2 = rltItem as DEBusinessItem;
                if (item2 == null) {
                    continue;
                }
                GetLink(item2, relation, ref lists);
            }
        }

        private string[] BuildFieldValueArray(MatchData matchData) {
            if (matchData == null) {
                throw new ArgumentNullException("matchData");
            }
            if (matchData.FieldValues == null || matchData.FieldValues.Count == 0) {
                throw new ArgumentNullException("matchData.FieldValues");
            }
            var fdValue = new string[matchData.FieldValues.Count];
            for (int i = 0; i < fdValue.Length; i++) {
                if (matchData.FieldValues[i] != null && !string.IsNullOrEmpty(matchData.FieldValues[i].Plmfield) && matchData.FieldValues[i].Plmfield.ToUpper() == "ID") {
                    fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, _bItem.Id);
                } else {
                    fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, _bItem.GetAttrValue(_bItem.ClassName, matchData.FieldValues[i].Plmfield.ToUpper()));
                }
            }
            return fdValue;
        }

    }
}
