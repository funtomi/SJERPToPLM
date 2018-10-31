﻿using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.UIL.Common;

namespace PLMConnection {
  public class ExportHandlerHelper {
      public void OnExportToERP(object sender, PLMOperationArgs args) {
          if (((args != null) && (args.BizItems != null)) && (args.BizItems.Length != 0)) {
              IBizItem item = args.BizItems[0];
              var bItem = BusinessHelper.Instance.GetDEBusinessItem(item);
              BusinessHelper.Instance.ExportToERP(bItem,false);
              //DEBusinessItem theItem = PSConvert.ToBizItem(args.BizItems[0], args.Option.CurView, ClientData.LogonUser);
              
          }
      }

  }
}
