using SuperMemoAssistant.Plugins.PopupWindow.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.PopupWindow.Models
{
  public class ContentProviderInfo
  {

    public string[] urlRegexes { get; set; }
    public IBrowserContentProvider provider { get; set; }

    public ContentProviderInfo(string[] urlRegexes, IBrowserContentProvider provider)
    {

      this.provider = provider;
      this.urlRegexes = urlRegexes;
      
    }
  }
}
