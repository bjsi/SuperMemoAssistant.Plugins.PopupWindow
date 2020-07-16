using Anotar.Serilog;
using mshtml;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Plugins.PopupWindow.Interop;
using SuperMemoAssistant.Plugins.PopupWindow.Models;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Sys.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SuperMemoAssistant.Plugins.PopupWindow.UI
{
  /// <summary>
  /// Interaction logic for PopupBrowserWdw.xaml
  /// </summary>
  public partial class PopupBrowserWdw
  {

    private PopupWindowCfg Config => Svc<PopupWindowPlugin>.Plugin.Config;
    private Dictionary<string, ContentProviderInfo> ContentProviders => Svc<PopupWindowPlugin>.Plugin.ContentProviders;
    public bool IsClosed { get; set; } = false;
    public BrowserContent browserContent { get; set; }

    public PopupBrowserWdw(BrowserContent content)
    {

      if (content.IsNull() || content.Html.IsNullOrEmpty())
        return;

      var initialTab = new PopupTabItem(content, this);

      InitializeComponent();

      ConfigureWindow();
      Focus();

      AddTab(initialTab);

      Closed += (sender, args) => IsClosed = true;
    }

    public void AddTab(PopupTabItem newTab)
    {
      if (newTab != null)
      {
        tabControl.Items.Add(newTab);
      }
    }

    private void ConfigureWindow()
    {
      // Set window size and startup location
      Width = Config.WindowWidth;
      Height = Config.WindowHeight;

      // Adds a random offset to top and left variables to prevent stacking windows.
      Random rnd = new Random();
      int topRndOffset = rnd.Next(1, 40);
      int leftRndOffset = rnd.Next(1, 50);

      WindowStartupLocation = WindowStartupLocation.Manual;
      Left = Config.WindowLeft + leftRndOffset;
      Top = Config.WindowTop + topRndOffset;

    }

    /// <summary>
    /// Creates a new extract from the selected HTML.
    /// </summary>
    /// <param name="priority">
    /// Optionally set the priority of the extract.
    /// </param>
    private void CreateSMExtract(double priority = -1)
    {
      bool ret = false;
      bool hasText = false;
      bool hasImage = false;
      var contents = new List<ContentBase>();
      var parentEl = Svc.SM.UI.ElementWdw.CurrentElement;
      string ExtractTitle = null;
      var activeTab = (PopupTabItem)tabControl.SelectedItem;

      if (activeTab == null)
      {
        LogTo.Error($"Failed to create an SM extract - the activeTab was null.");
        return;
      }

      if (parentEl == null)
      {
        LogTo.Error("Failed to create SM extract - parentEl was null");
        return;
      }

      IHTMLTxtRange range = activeTab.GetPopupWikiWdwSelRange();

      if (string.IsNullOrEmpty(range.htmlText))
      {
        LogTo.Error($"Attempted to create an extract with null or empty selected range.");
        return;
      }

      // Add text content
      string selTextHtml = HtmlEx.RemoveImages(range.htmlText);
      if (!range.text.IsNullOrEmpty())
      {
        contents.Add(new TextContent(true, selTextHtml));
        hasText = true;
      }

      // Add image content
      List<string> selImageUrls = HtmlEx.GetImageUrls(range.htmlText);
      if (selImageUrls.Count > 0)
      {
        WebClient wc = new WebClient();

        foreach (string imageUrl in selImageUrls)
        {

          if (string.IsNullOrEmpty(imageUrl))
            continue;

          try
          {

            // Download image as bytes
            byte[] bytes = wc.DownloadData(imageUrl);
            if (bytes.IsNull())
              continue;

            // Convert to memory stream
            MemoryStream ms = new MemoryStream(bytes);
            if (ms == null)
              continue;

            // Convert memory stream to Image
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
            if (image == null)
              continue;

            // Create file name from the url
            var uri = new Uri(imageUrl);
            var filename = uri?.Segments?.Last() ?? "image";

            // Create Image component
            var ImageContent = CreateImageContent(image, filename);
            if (ImageContent != null)
            {
              contents.Add(ImageContent);
              hasImage = true;
            }
          }
          catch (WebException e) { }
          catch (Exception e)
          {
            LogTo.Warning($"Exception {e} caught while attempting to create image content.");
          }
        }
      }

      // Add an empty Html component to image extracts if desired.
      if (Config.ImageExtractAddHtml && !hasText && hasImage)
      {
        ExtractTitle = $"{activeTab.BrowserContent.References.Title} -- {contents.Count} image{(contents.Count == 1 ? "" : "s")}";
        contents.Add(new TextContent(true, string.Empty));
      }

      if (contents.Count > 0)
      {
        if (priority < 0 || priority > 100)
        {
          LogTo.Debug("Adding priority using the default SMExtractPriority.");
          priority = Config.SMExtractPriority;
        }

        // Create the element in SM
        ret = Svc.SM.Registry.Element.Add(
          out _,
          ElemCreationFlags.ForceCreate,
          new ElementBuilder(ElementType.Topic,
                             contents.ToArray())
            .WithParent(Config.ExtractType == ExtractMode.Child ? parentEl : null)
            .WithConcept(Config.ExtractType == ExtractMode.Hook ? parentEl.Concept : null)
            .WithLayout("Article")
            .WithTitle(ExtractTitle)
            .WithPriority(priority)
            .WithReference(
              r => r.WithTitle(activeTab.BrowserContent.References.Title)
                    .WithSource(activeTab.BrowserContent.References.Source)
                    .WithLink(activeTab.BrowserContent.References.Link)
                    .WithAuthor(activeTab.BrowserContent.References.Author)
             )
            .DoNotDisplay()
        );

        GetWindow(this)?.Activate();

        if (ret)
        {
          AddExtractHighlight(range);
        }
      }
    }

    protected ContentBase CreateImageContent(System.Drawing.Image image, string title)
    {
      if (image == null)
        return null;

      int imgRegistryId = Svc.SM.Registry.Image.Add(
        new ImageWrapper(image),
        title
      );

      if (imgRegistryId <= 0)
        return null;

      return new ImageContent(imgRegistryId, Config.ImageStretchType);
    }


    private void AddExtractHighlight(IHTMLTxtRange range)
      {
        if (range != null)
        {
          range.execCommand("BackColor", false, "#44C2FF");
        }
      }

      private void BtnSMExtract_Click(object sender, RoutedEventArgs e)
      {
        var activeTab = (PopupTabItem)tabControl.SelectedItem;
        if (activeTab != null)
        {
          // Do not allow extracts from search result pages
          if (activeTab.BrowserContent.AllowExtracts)
          {
            CreateSMExtract();
          }
        }
      }

    // TODO: Create a resusable extract w/ priority window
      private async void CreateSMExtractWithPriority()
      {

        var activeTab = (PopupTabItem)tabControl.SelectedItem;

        if (activeTab == null)
        {
          return;
        }

        // If there's no selection, just return.
        var selRange = activeTab.GetPopupWikiWdwSelRange();

        if (selRange == null || selRange.htmlText == null)
        {
          return;
        }

      }

      private void BtnSMPriorityExtract_Click(object sender, RoutedEventArgs e)
      {
        var activeTab = (PopupTabItem)tabControl.SelectedItem;
        if (activeTab != null)
        {
          // Do not allow extracts from search results pages.
          if (activeTab.BrowserContent.AllowExtracts)
          {
            CreateSMExtractWithPriority();
          }
        }
      }

      private string GetPopupWikiWdwSelText()
      {
        var activeTab = (PopupTabItem)tabControl.SelectedItem;
        string cleanSelText = null;

        if (activeTab != null)
        {
          var selRange = activeTab.GetPopupWikiWdwSelRange();
          if (!string.IsNullOrEmpty(selRange.text))
          {
            cleanSelText = Regex.Replace(selRange.text, @"[\.|\?|\,|\!]", "");
            cleanSelText = Regex.Replace(cleanSelText, @"[\n|\t|\r]", " ");
            cleanSelText = cleanSelText.Trim();
          }
        }

        return cleanSelText;
      }

      /**
       * TODO: Hotkeys not working consistently. 
       * Couldn't get Alt + hotkey to work.
     */
      public async void WebBrowser_Keypress(object sender, HtmlElementEventArgs e)
      {

        var activeTab = (PopupTabItem)tabControl.SelectedItem;
        // TODO: actveTab null check
        // TODO: KeyboardModEx

        // Extracts only allowed on non-search pages
        if (activeTab.BrowserContent.AllowExtracts)
        {
          // x
          if (e.KeyPressedCode == 120)
          {
            CreateSMExtract();
          }
          // ctrl + x
          else if (e.KeyPressedCode == 24)
          {
            CreateSMExtractWithPriority();
          }
          // ctrl + c
          else if (e.KeyPressedCode == 3 && e.CtrlKeyPressed)
          {
            if (activeTab != null)
            {
              var selRange = activeTab.GetPopupWikiWdwSelRange();
              if (!string.IsNullOrEmpty(selRange.text))
              {
                System.Windows.Clipboard.SetText(selRange.text);
              }
            }
          }
        }

        // ctrl + w
        if (e.KeyPressedCode == 23)
        {
          //await SearchWikipedia();
        }
        // ctrl d
        else if (e.KeyPressedCode == 4)
        {
          //await SearchWiktionary();
        }
        // Escape
        else if (e.KeyPressedCode == 27)
        {
          Close();
        }
      }

    private void BtnOpenAllIE_Click(object sender, RoutedEventArgs e)
    {
      var tabs = tabControl.Items;

      if (tabs.IsNull() || tabs.Count == 0)
        return;

      foreach (var tab in tabs)
      {

        var tabItem = tab as PopupTabItem;
        string link = tabItem?.BrowserContent.References.Link;
        if (link.IsNullOrEmpty())
          continue;

        System.Diagnostics.Process.Start(tabItem.BrowserContent.References.Link);
        Thread.Sleep(200);

      }
    }

    private void BtnOpenIE_Click(object sender, RoutedEventArgs e)
    {

      var activeTab = (PopupTabItem)tabControl.SelectedItem;
      if (activeTab.IsNull())
        return;

      string url = activeTab.BrowserContent.References.Link;
      if (url.IsNullOrEmpty() || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
        return;

      System.Diagnostics.Process.Start(url);

    }
  }
}
