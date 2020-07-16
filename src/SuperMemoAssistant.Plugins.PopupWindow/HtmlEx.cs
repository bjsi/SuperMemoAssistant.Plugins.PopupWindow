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

    // TODO: Need to convert relative to absolute?
    public static List<string> GetImageUrls(string html)
    {

      var imageUrls = new List<string>();

      if (html.IsNullOrEmpty())
        return imageUrls;

      var doc = new HtmlDocument();
      doc.LoadHtml(html);

      var imageNodes = doc.DocumentNode.SelectNodes("//img[@src]");
      if (imageNodes.IsNull() || imageNodes.Count == 0)
        return imageUrls;

      foreach (var imageNode in imageNodes)
      {

        string url = imageNode.GetAttributeValue("src", null);
        if (url.IsNullOrEmpty() || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
          continue;

        // Convert relative links to absolute
        // url = WikiUrlUtils.ConvRelToAbsLink(baseUrl, url);

        imageUrls.Add(url);
      }

      return imageUrls;

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
