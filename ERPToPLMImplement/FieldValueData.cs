using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ERPToPLMImplement {
   public class FieldValueData {
        public FieldValueData(XmlElement doc) {
            if (doc==null) {
                throw new ArgumentNullException("doc");
            }
            InitData(doc);
        }
        private static string PATH_ERPFIELD = "erpfield";
        private static string PATH_PLMFIELD = "plmfield";
        private static string PATH_PLMTYPE = "plmtype";
        #region fieldValue参数

        public string Erpfield {
            get { return _erpfield; }
        }
        private string _erpfield;

        public string Plmfield {
            get { return _plmfield; }
        }
        private string _plmfield;

        public string PlmValue {
            get { return _plmValue; }
        }
        private string _plmValue;

        /// <summary>
        /// 0-currentClass,1-relation,2-parentClass,3-firstParentClass
        /// </summary>
        public string PlmType {
            get { return _plmType; }
        }
        private string _plmType;
        #endregion

        private void InitData(XmlElement doc) {
            if (doc==null) {
                throw new ArgumentNullException("doc");
            }
            var erpFieldNode = doc.SelectSingleNode(PATH_ERPFIELD);
            _erpfield = erpFieldNode == null || string.IsNullOrEmpty(erpFieldNode.InnerText) ? "" : erpFieldNode.InnerText;
            var plmFieldNode = doc.SelectSingleNode(PATH_PLMFIELD);
            _plmfield = plmFieldNode == null || string.IsNullOrEmpty(plmFieldNode.InnerText) ? "" : plmFieldNode.InnerText; 
            var plmTypeNode = doc.SelectSingleNode(PATH_PLMTYPE);
            _plmType = plmTypeNode == null || string.IsNullOrEmpty(plmTypeNode.InnerText) ? "" : plmTypeNode.InnerText;
        }
    }

    
}
