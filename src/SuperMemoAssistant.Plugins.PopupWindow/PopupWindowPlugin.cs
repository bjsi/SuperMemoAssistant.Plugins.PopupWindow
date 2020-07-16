#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   7/7/2020 3:24:42 AM
// Modified By:  james

#endregion




namespace SuperMemoAssistant.Plugins.PopupWindow
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.Linq;
  using System.Text.RegularExpressions;
  using System.Threading.Tasks;
  using System.Windows;
  using SuperMemoAssistant.Extensions;
  using SuperMemoAssistant.Plugins.PopupWindow.Interop;
  using SuperMemoAssistant.Plugins.PopupWindow.Models;
  using SuperMemoAssistant.Plugins.PopupWindow.UI;
  using SuperMemoAssistant.Services;
  using SuperMemoAssistant.Services.IO.HotKeys;
  using SuperMemoAssistant.Services.Sentry;
  using SuperMemoAssistant.Services.UI.Configuration;
  using SuperMemoAssistant.Sys.Remoting;

  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class PopupWindowPlugin : SentrySMAPluginBase<PopupWindowPlugin>
  {
    #region Constructors

    /// <inheritdoc />
    public PopupWindowPlugin() : base("Enter your Sentry.io api key (strongly recommended)") { }

    #endregion

    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "PopupWindow";

    /// <inheritdoc />
    public override bool HasSettings => true;
    public Dictionary<string, ContentProviderInfo> ContentProviders { get; set; } = new Dictionary<string, ContentProviderInfo>();
    public PopupWindowCfg Config;

    private PopupWindowSvc _popupWindowSvc = new PopupWindowSvc();

    private PopupBrowserWdw CurrentWindow { get; set; }

    #endregion

    #region Methods Impl

    /// <inheritdoc />
    public override void ShowSettings()
    {
      ConfigurationWindow.ShowAndActivate(HotKeyManager.Instance, Config);
    }

    private void LoadConfig()
    {
      Config = Svc.Configuration.Load<PopupWindowCfg>() ?? new PopupWindowCfg();
    }

    /// <inheritdoc />
    protected override void PluginInit()
    {

      LoadConfig();

      PublishService<IPopupWindowSvc, PopupWindowSvc>(_popupWindowSvc);
    }

    public bool RegisterPopupWindowProvider(string name, List<string> urlRegexes, IContentProvider provider)
    {

      if (string.IsNullOrEmpty(name))
        return false;

      if (provider == null)
        return false;

      if (urlRegexes.IsNull() || !urlRegexes.Any())
        return false;

      if (ContentProviders.ContainsKey(name))
        return false;

      ContentProviders[name] = new ContentProviderInfo(urlRegexes, provider);
      return true;

    }

    public async Task<bool> Open(string url)
    {

      // Find content provider
      if (url.IsNullOrEmpty())
        return false;

      if (ContentProviders.IsNull() || !ContentProviders.Any())
        return false;

      foreach (KeyValuePair<string, ContentProviderInfo> kvpair in ContentProviders)
      {
        
        var info = kvpair.Value;
        var regexes = info.urlRegexes;
        var provider = info.provider;

        if (!regexes.Any(r => new Regex(r).Match(url).Success))
          continue;

        // fetch html
        BrowserContent content = await provider.FetchHtml(url);

        // Open tab / window
        if (!CurrentWindow.IsNull() && !CurrentWindow.IsClosed)
        {
          CurrentWindow.Open(content);
        }
        else
        {
          OpenNewPopupBrowser(content);
        }

        return true;
      }

      return false;

    }

    private void OpenNewPopupBrowser(BrowserContent content)
    {
      
      if (content.IsNull() || content.Html.IsNullOrEmpty())
        return;

      Application.Current.Dispatcher.Invoke(() =>
      {
        var wdw = new PopupBrowserWdw(content);
        wdw.ShowAndActivate();
      });

    }

    #endregion

    #region Methods

    #endregion
  }
}
