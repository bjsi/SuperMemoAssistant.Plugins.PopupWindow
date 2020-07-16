using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Plugins.PopupWindow.Models;
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
    [Text(@"- A popup window for extraction from the web directly into SuperMemo.")]

    // 
    // EXTRACTION SETTINGS
    [Heading("Extraction Settings")]

    // PRIORITY
    [Field(Name = "Default SM Extract Priority (%)")]
    [Value(Must.BeGreaterThanOrEqualTo,
           0,
           StrictValidation = true)]
    [Value(Must.BeLessThanOrEqualTo,
           100,
           StrictValidation = true)]
    public double SMExtractPriority { get; set; } = 15;

    // IMAGE STRETCH MODE
    [Field(Name = "Default Image Stretch Type")]
    [SelectFrom(typeof(ImageStretchMode),
            SelectionType = SelectionType.RadioButtonsInline)]
    public ImageStretchMode ImageStretchType { get; set; } = ImageStretchMode.Proportional;

    // ADD HTML TO IMAGE-ONLY EXTRACTS
    [Field(Name = "Add HTML component to extracts containing only images?")]
    public bool ImageExtractAddHtml { get; set; } = false;

    // EXTRACT MODE
    [Field(Name = "Extract as child of current element or into concept hook?")]
    [SelectFrom(typeof(ExtractMode),
            SelectionType = SelectionType.RadioButtonsInline)]
    public ExtractMode ExtractType { get; set; } = ExtractMode.Child;

    //
    // WINDOW SETTINGS
    [Heading("Window Settings")]

    // WINDOW WIDTH
    [Field(Name = "Window Width")]
    public int WindowWidth { get; set; }

    // WINDOW HEIGHT
    [Field(Name = "Window Height")]
    public int WindowHeight { get; set; }

    // WINDOW LEFT
    [Field(Name = "Window Startup Location Left")]
    public int WindowLeft { get; set; }

    // WINDOW TOP
    [Field(Name = "Window Startup Location Top")]
    public int WindowTop { get; set; }



    [JsonIgnore]
    public bool IsChanged { get; set; }

    public override string ToString()
    {
      return "Popup Window Settings";
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
