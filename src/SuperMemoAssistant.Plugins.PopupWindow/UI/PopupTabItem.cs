using mshtml;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.PopupWindow.Interop;
using SuperMemoAssistant.Sys.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace SuperMemoAssistant.Plugins.PopupWindow.UI
{
  public class PopupTabItem : TabItem
  {

    public BrowserContent BrowserContent { get; set; }
    public IContentProvider provider { get; set; }
    public string url { get; set; }
    private PopupBrowserWdw parentWdw;


    public PopupTabItem(BrowserContent content, PopupBrowserWdw parent)
    {

      if (content.IsNull() || content.html.IsNullOrEmpty() || parent.IsNull())
        return;

      this.BrowserContent = content;

      this.parentWdw = parent;

      this.Content = new Browser();
      ((Browser)this.Content).webBrowser.DocumentText = content.html;
      ((Browser)this.Content).webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;

      var header = new TabItemHeader();
      if (!content.icon.IsNullOrEmpty())
        header.TabHeaderImage.Source = new BitmapImage(new Uri(content.icon));
      header.TabTitle.Content = content.references.Title;
      this.Header = header;
      // header.CloseBtn.Clickl

    }

    private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
    {
      AddLinkClickEvents();

      ((Browser)this.Content).webBrowser.Document.Body.KeyPress += parentWdw.WebBrowser_Keypress;
    }

    private void AddLinkClickEvents()
    {

      var links = ((Browser)this.Content).webBrowser.Document.Links;
      if (links.IsNull() || links.Count == 0)
        return;

      links
        .Cast<HtmlElement>()
        .ForEach(link => link.Click += new HtmlElementEventHandler(LinkClicked));
    }

    private async void LinkClicked(object sender, EventArgs e)
    {

      HtmlElement element = ((HtmlElement)sender);
      string href = element.GetAttribute("href");

      // Match against providers...

    }

    /// <summary>
    /// Get the selected range of the current PopupWiki window.
    /// </summary>
    /// <returns>
    /// IHTMLTxtRange representing the currently selected html on the PopupWiki window.
    /// </returns>
    public IHTMLTxtRange GetPopupWikiWdwSelRange()
    {
      // Gets called by the parent popupwikiwindow to get the selected range of the active tab.
      var htmlDoc = (((Browser)this.Content).webBrowser.Document.DomDocument as IHTMLDocument2);
      IHTMLSelectionObject selection = htmlDoc.selection;
      IHTMLTxtRange range = (IHTMLTxtRange)selection.createRange();
      return range;
    }

    private void TabCloseBtnClick()
    {
      if (parentWdw.tabControl.Items.Count > 1)
      {
        parentWdw.tabControl.Items.Remove(this);
        return;
      }
      parentWdw.Close();

    }

  }
}
