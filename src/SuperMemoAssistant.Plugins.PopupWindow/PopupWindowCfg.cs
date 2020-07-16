using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.PopupWindow
{

  [Form(Mode = DefaultFields.None)]
  [Title("Dictionary Settings",
       IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
      "Cancel",
      IsCancel = true)]
  [DialogAction("save",
      "Save",
      IsDefault = true,
      Validates = true)]
  public class PopupWindowCfg : CfgBase<PopupWindowCfg>, INotifyPropertyChangedEx
  {
    [Title("Popup Window Plugin")]

    [Heading("By Jamesb | Experimental Learning")]

    [Heading("Features:")]
    [Text(@"- ")]

    [Heading("General Settings")]

    //[Field(Name = "Setting 1")]
    //public string Name { get; set; }

    [JsonIgnore]
    public bool IsChanged { get; set; }

    public override string ToString()
    {
      return "Popup Window Settings";
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
