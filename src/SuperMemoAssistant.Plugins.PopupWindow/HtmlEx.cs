using HtmlAgilityPack;
using SuperMemoAssistant.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.PopupWindow
{
  public static class HtmlEx
  {
    public static string RemoveImages(string html)
    {
      if (html.IsNullOrEmpty())
        return null;

      var doc = new HtmlDocument();
      doc.LoadHtml(html);
      string[] xpaths = new string[] { "//img" };
      doc.RemoveHtmlNodes(xpaths);

      return doc.DocumentNode.OuterHtml;

    }

    public static string[] GetImageUrls()
    {

    }

    // Pass a list of xpaths to remove nodes that match any of them
    // Doesn't save the children
    public static void RemoveHtmlNodes(this HtmlDocument doc, string[] xpaths)
    {

      if (doc.IsNull() || xpaths.Length == 0)
        return;

      string selectExpression = string.Join(" | ", xpaths);

      var removeNodes = doc.DocumentNode.SelectNodes(selectExpression);
      if (removeNodes.IsNull())
        return;

      removeNodes.ForEach(n => n.ParentNode.RemoveChild(n));

    }
  }
}
