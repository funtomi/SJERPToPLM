using ERPToPLMImplement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thyt.TiPLM.Common;
using Thyt.TiPLM.DEL.Admin.DataModel;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.PLL.Admin.DataModel;
using Thyt.TiPLM.PLL.Product2;
using Thyt.TiPLM.UIL.ArchivManage;
using Thyt.TiPLM.UIL.Common;
using Thyt.TiPLM.UIL.Controls;
using Thyt.TiPLM.UIL.Product.Common;
using Thyt.TiPLM.UIL.Product.Common.UserControls;

namespace PLMConnection {
    class NewItemManager {
        public NewItemManager(DEBusinessItem parentItem, string relationName,ExtendRelationData extendData) {
            #region 参数检查
            if (parentItem == null) {
                throw new ArgumentNullException("parentItem");
            }
            if (string.IsNullOrEmpty(relationName)) {
                throw new ArgumentNullException("relationName");
            } 
            if (extendData==null) {
                throw new ArgumentException("extendData");
            }
            #endregion
            _parentItem = parentItem;
            _currentRelationName = relationName;
            _extendData = extendData;
        }

        private DEBusinessItem _parentItem;
        private string _currentRelationName;
        private ExtendRelationData _extendData;
        //private int _bizNumber;

        internal void CreateNewItemWithParent() {
            //获取当前对象的连接 
                DERelationBizItemList list = PLItem.Agent.GetLinkRelationItems(_parentItem.IterOid, _parentItem.ClassName, _currentRelationName, ClientData.LogonUser.Oid, new ObjectNavigateContext().Option);
         
                List<object> items = FindChildrenWithSeq(list, _extendData.OrderSeq);//连接中的标准产品
            if (items==null||items.Count==0) {
                return;
            }
            RemoveLink(items); 
            PLMOperationArgs args = new PLMOperationArgs(FrmLogon.PLMProduct.ToString(), PLMLocation.None.ToString(), items, ClientData.UserGlobalOption.CloneAsLocal());
            var newItems =BizOperationHelper.Instance.Clone(null, args);
            if (newItems==null||newItems.Count==0) {
                return;
            }
            //20190103 modified by kexp:隐藏这些文件夹，当合同明细添加到一定数量的时候，个人文件夹下的目录就会非常多，不方便使用。
            //var folder = CreateTempFolder();
            //if (folder == null) {
            //    return;
            //}
            AddNewLink(newItems);  
            CreateRelationPart(null,newItems);
            //SetShortCut(newItems, folder); 
        }

        private void CreateRelationPart(DEFolder2 folder,List<IBizItem> items) {
            if (items==null||items.Count==0) {
                return;
            }
            foreach (var iItem in items) {
                var item = iItem as DEBusinessItem;
                if (item==null) {
                    continue;
                }
                //创建相关项目
                DEMetaClassEx classEx = ModelContext.MetaModel.GetClassEx(_extendData.PartClassName);
                DEMetaRelation relation = ModelContext.MetaModel.GetRelation(_extendData.PartRelationName);
                PropertyPageContent input = new PropertyPageContent(classEx, null, ClientData.UserGlobalOption, folder, null, relation, PropertyPageMode.COMPOSITE, item);
                FrmItemFactory3 factory = new FrmItemFactory3();
                factory.SetInput(input);
                factory.ShowDialog();
            }
           
        }

        private void AddNewLink(List<IBizItem> newItems) {
            if (newItems==null||newItems.Count==0) {
                return;
            }
            DERelationBizItemList list2 = PLItem.Agent.GetLinkRelationItems(_parentItem.IterOid, _parentItem.ClassName, _currentRelationName, ClientData.LogonUser.Oid, ClientData.UserGlobalOption);
            _parentItem.Iteration.LinkRelationSet.RelationBizItemLists[_currentRelationName] = list2;
            foreach (var cItem in newItems) {
                var item = cItem as DEBusinessItem;
                if (item==null) {
                    continue;
                }
                int index = list2.Count;
                item.Iteration.SetAttrValue(_extendData.OrderSeq, ++index);
                SetInheritAttr(_parentItem, item,_extendData.FromParentlists);
                PLItem.Agent.UpdateItemIteration(item.Iteration, ClientData.LogonUser.Oid, ClientData.UserGlobalOption);
                var rltItem = ArchiveManageCommon.AddNewRelItem(item, _currentRelationName, _parentItem);
                list2.AddRelationItem(rltItem); 
            }
            PLItem.UpdateLinkRelations(_parentItem, _currentRelationName, ClientData.LogonUser.Oid, ClientData.UserGlobalOption); 
        }

        private void SetInheritAttr(DEBusinessItem parentItem, DEBusinessItem childItem, List<InheritAttrData> inheritDatas) {
            if (parentItem == null || childItem == null || inheritDatas == null || inheritDatas.Count == 0) {
                return;
            }
            foreach (InheritAttrData data in inheritDatas) {
                if (data == null) {
                    continue;
                }

                var value = GetValue(parentItem, data);
                if (value == null) {
                    continue;
                }
                childItem.Iteration.SetAttrValue(data.To, value);
            }
            PLItem.Agent.UpdateItemIteration(childItem.Iteration, ClientData.LogonUser.Oid, ClientData.UserGlobalOption);
        }

        private object GetValue(DEBusinessItem parentItem, InheritAttrData data) {
            if (parentItem==null||data==null) {
                return null;
            }
            if (string.IsNullOrEmpty(data.From)) {
                return null;
            }
            var attr = data.From.Split('.');
            if (attr==null||attr.Length<2) {
                var value = parentItem.GetAttrValue(parentItem.ClassName, attr[0]);
                return value;
            }
            var value1 = parentItem.GetAttrValue(attr[0], attr[1]);
            return value1;
        }


        /// <summary>
        /// 移除标准产品
        /// </summary>
        /// <param name="list"></param>
        /// <param name="items"></param>
        private void RemoveLink(List<object> items) {
            if (items == null || items.Count == 0) {
                return;
            }
            //foreach (var currentItem in items) {
            //    var item = currentItem as DEBusinessItem;
            //    if (item == null) {
            //        continue;
            //    }
            //    for (int i = 0; i < list.RelationList.Count; i++) {
            //        var paItem = list.RelationList[i] as DERelation2;
            //        if (paItem.RightObj.Equals(item.MasterOid)) {
            //            paItem.State = RelationState.Added;
            //            list.DeleteLinkRelation(paItem.RightObj);
            //            i--;
            //        }
            //    }
            //    PLItem.UpdateLinkRelationsDirectly(_parentItem, _currentRelationName, ClientData.LogonUser.Oid, ClientData.UserGlobalOption);
            //    //list.RemoveRelationItemByRightObj(item.Master.Oid);
            //    //((Thyt.TiPLM.DEL.Product.DERelation2)((new System.Collections.ArrayList.ArrayListDebugView(list.RelationList)).Items[0])).State = RelationState.Added;
            //    //list.DeleteLinkRelation(item.Master.Oid);
            //}


            foreach (var currentItem in items) {
                var item = currentItem as DEBusinessItem;
                if (item == null) {
                    continue;
                }
                DERelationBizItemList list2 = PLItem.Agent.GetLinkRelationItems(_parentItem.IterOid, _parentItem.ClassName, _currentRelationName, ClientData.LogonUser.Oid, ClientData.UserGlobalOption);
                _parentItem.Iteration.LinkRelationSet.RelationBizItemLists[_currentRelationName] = list2;
                list2.DeleteLinkRelation(item.Master.Oid);
                PLItem.UpdateLinkRelationsDirectly(_parentItem, _currentRelationName, ClientData.LogonUser.Oid, ClientData.UserGlobalOption);
            }
        }
         
         

        /// <summary>
        /// 获取连接中的标准产品
        /// </summary>
        /// <param name="rlts"></param>
        /// <param name="SEQ_ATTR_NAME"></param>
        /// <returns></returns>
        private List<object> FindChildrenWithSeq(DERelationBizItemList rlts, string SEQ_ATTR_NAME) {
            if (rlts == null) {
                return null;
            }
            if (string.IsNullOrEmpty(SEQ_ATTR_NAME)) {
                return null;
            }
            
            List<object> list = new List<object>();
            var bizItems = rlts.BizItems;
            if (bizItems==null||bizItems.Count==0) {
                return null;
            }
            foreach (var bizItem in bizItems) {
                var item = bizItem as DEBusinessItem;
                if (item==null) {
                    continue;
                }
                var attr = item.GetAttrValue(item.ClassName, SEQ_ATTR_NAME);
                if (attr==null||string.IsNullOrEmpty(attr.ToString())) {
                    list.Add(item);
                    continue;
                }
                if (Convert.ToInt64(attr)==0) {
                    list.Add(item);
                    continue;
                }
            }
            //_bizNumber = bizItems.Count - list.Count;
            return list;
        }

        /// <summary>
        /// 设置快捷方式
        /// </summary>
        /// <param name="items"></param>
        /// <param name="folder"></param>
        private void SetShortCut(List<IBizItem> items, DEFolder2 folder) {
            if (items==null||items.Count==0) {
                return;
            }
            foreach (var currentItem in items) {
                var item = currentItem as DEBusinessItem;
                if (item==null) {
                    continue;
                }
                bool replace = false;
                Hashtable result = new Hashtable();
                ArrayList shortCuts = new ArrayList();
                DEShortCut cut = new DEShortCut(item.MasterOid, item.RevOid, item.IterOid) {
                    MasterID = item.Id
                };
                shortCuts.Add(cut);
                if (folder.FolderEffway == RevisionEffectivityWay.PreciseIter) {
                    PLFolder.RemotingAgent.IsExsitShortCuts(folder.Oid, ClientData.LogonUser.Oid, shortCuts, out result);
                    if ((result != null) && (result.Count > 0)) {
                        return;
                    }
                }
                folder = PLFolder.RemotingAgent.CreateShortCuts(ClientData.LogonUser.Oid, folder.Oid, shortCuts, replace);
            }
            
        }

        /// <summary>
        /// 在个人文件夹下创建新文件夹
        /// </summary>
        private DEFolder2 CreateTempFolder() {

            DEFolder2 folder = new DEFolder2();
            folder.Name = DateTime.Now.ToString("yyyyMMddHHmmss");
            folder.Parent = ClientData.LogonUser.Oid;
            folder.Creator = ClientData.LogonUser.LogId;
            folder.FolderType = 'P';
            folder.FolderEffway = RevisionEffectivityWay.LastRev;
            try {
                PLFolder.CreateFolder(folder, ClientData.LogonUser.Oid);
                MessageBoxPLM.Show("在个人文件夹下创建文件夹\"" + folder.Name + "\"成功");
            } catch (PLMException exception) {
                PrintException.Print(exception, "文件夹管理");
            } catch (Exception exception2) {
                PrintException.Print(exception2, "文件夹管理");
            }
            return folder;
        }
    }
}
