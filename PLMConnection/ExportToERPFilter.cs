using ERPToPLMImplement;
using Thyt.TiPLM.DEL.Operation;
using Thyt.TiPLM.PLL.Admin.DataModel;
using Thyt.TiPLM.UIL.Common;
using Thyt.TiPLM.UIL.Common.Operation;

namespace PLMConnection {
    public class ExportToERPFilter : IOperationFilter {
        //菜单过滤器
        public bool Filter(PLMOperationArgs args, DEOperationItem item) {
            if (args.BizItems == null || args.BizItems.Length == 0) {
                return false;
            }
            var iItem = args.BizItems[0];
            var bItem = BusinessHelper.Instance.GetDEBusinessItem(iItem);
            if (bItem == null) {
                return false;
            }

            var classes = ERPServiceHelper.Instance.ConfigData.Classes;
            foreach (var className in classes) {
                if (ModelContext.MetaModel.IsChild(className,bItem.ClassName)) {
                    return true;
                }
            }
            return false;
        }
    }
}
