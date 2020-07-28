using PluginManager.Interop.Sys;
using SuperMemoAssistant.Plugins.PopupWindow.Interop;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Sys.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.PopupWindow
{
  class PopupWindowSvc : PerpetualMarshalByRefObject, IPopupWindowSvc
  {

    public bool RegisterPopupWindowProvider(string name, string[] urlRegexes, IBrowserContentProvider provider)
    {

      if (name.IsNullOrEmpty())
        return false;

      if (urlRegexes.IsNull() || !urlRegexes.Any())
        return false;

      if (provider.IsNull())
        return false;

      return Svc<PopupWindowPlugin>.Plugin.RegisterPopupWindowProvider(name, urlRegexes, provider);

    }

    public RemoteTask<bool> Open(string url, ContentType type)
    {

      return Svc<PopupWindowPlugin>.Plugin.Open(url, type);

    }
  }
}
