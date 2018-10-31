using ERPToPLMImplement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.PLL.Admin.DataModel;
using Thyt.TiPLM.PLL.Product2;
using Thyt.TiPLM.UIL.Common;

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
                if (matchData.PlmRelation == null) {
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
            if (itemIds ==null) {
                itemIds = new List<string>();
            }
            errText = "";
            if (string.IsNullOrEmpty(matchData.RelationName)) {
                throw new ArgumentNullException("matchData.RelationName");
            }
            var relations = GetLinks(bItem, matchData.RelationName);
            if (relations==null||relations.Count==0) {
                return true;
            }
            for (int i = 0; i < relations.BizItems.Count; i++) {
                var item = relations.BizItems[i] as DEBusinessItem;
                var rlt = relations.RelationList[i] as DERelation2;
                if (item==null||rlt==null) {
                    continue;
                }
                if (itemIds.Contains(item.Id)) {
                    continue;
                }
                var fdValue = BuildFieldValueArrayWithRlt(matchData, item, rlt, bItem);
                if (!ERPServiceHelper.Instance.SaveBaseWithConfig(matchData.BaseId, matchData.Addkeyvalue, fdValue, out errText)) {
                    return false;
                }
                itemIds.Add(item.Id);
                return ImportRelations(item, matchData, ref itemIds, out errText);
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
                var classType = string.IsNullOrEmpty(matchData.FieldValues[i].PlmType) ? "0" : matchData.FieldValues[i].PlmType;
                switch (classType) {
                    default:
                    case "0"://当前实体
                        if (matchData.FieldValues[i] != null && !string.IsNullOrEmpty(matchData.FieldValues[i].Plmfield) && matchData.FieldValues[i].Plmfield.ToUpper() == "ID") {
                            fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, item.Id);
                        } else {
                            fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, item.GetAttrValue(item.ClassName, matchData.FieldValues[i].Plmfield.ToUpper()));
                        }
                        break;
                    case "1"://关联属性
                        fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, rlt.GetAttrValue(matchData.FieldValues[i].Plmfield.ToUpper()));
                        break;
                    case "2"://父实体属性
                        if (matchData.FieldValues[i] != null && !string.IsNullOrEmpty(matchData.FieldValues[i].Plmfield) && matchData.FieldValues[i].Plmfield.ToUpper() == "ID") {
                            fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, parentItem.Id);
                        } else {
                            fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, parentItem.GetAttrValue(parentItem.ClassName, matchData.FieldValues[i].Plmfield.ToUpper()));
                        } break;
                    case "3"://最上层实体属性
                        if (matchData.FieldValues[i] != null && !string.IsNullOrEmpty(matchData.FieldValues[i].Plmfield) && matchData.FieldValues[i].Plmfield.ToUpper() == "ID") {
                            fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, _bItem.Id);
                        } else {
                            fdValue[i] = string.Format("@{0}={1}", matchData.FieldValues[i].Erpfield, _bItem.GetAttrValue(_bItem.ClassName, matchData.FieldValues[i].Plmfield.ToUpper()));
                        }
                        break;
                }
            }
            return fdValue;
        }



        public DERelationBizItemList GetLinks(DEBusinessItem item, string relation) {
            DERelationBizItemList relationBizItemList = item.Iteration.LinkRelationSet.GetRelationBizItemList(relation);
            if (relationBizItemList == null) {
                try {
                    relationBizItemList = PLItem.Agent.GetLinkRelationItems(item.Iteration.Oid, item.Master.ClassName, relation, ClientData.LogonUser.Oid, ClientData.UserGlobalOption);
                } catch {
                    return null;
                }
            }
            return relationBizItemList;
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
