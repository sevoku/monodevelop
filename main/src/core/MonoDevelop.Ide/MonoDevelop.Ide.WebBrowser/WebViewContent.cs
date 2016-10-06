//
// WebViewContent.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2016 Vsevolod Kukol
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Threading.Tasks;
using MonoDevelop.Components;
using MonoDevelop.Ide.Gui;
using Xwt;

namespace MonoDevelop.Ide.WebBrowser
{
	public class WebViewContent : ViewContent
	{
		IWebBrowser browser;
		Control browserControl;
		string fileName;
		
		public WebViewContent ()
		{
			if (WebBrowserService.CanGetWebBrowser) {
				browser = WebBrowserService.GetWebBrowser ();
				browserControl = browser as Gtk.Widget;
			} else {
				var defaultBrowser = new WebBrowser ();
				browser = defaultBrowser;
				browserControl = defaultBrowser.Control;
			}
			LocalNavigation = true;
			browser.TitleChanged += (sender, e) => OnTitleChanged (browser.Title);
			browser.LocationChanging += (sender, args) => args.SuppressChange = OnLocationChanging (args.NextLocation);
			browser.LocationChanged += (sender, args) => OnLocationChanged ();
			browser.PageLoaded += (sender, args) => OnContentLoaded ();
		}

		public virtual bool LocalNavigation { get; set; }

		public override Control Control {
			get {
				return browserControl;
			}
		}

		protected IWebBrowser Browser {
			get {
				return browser;
			}
		}

		bool isLoading;

		public override Task Load (FileOpenInformation fileOpenInformation)
		{
			isLoading = true;
			fileName = fileOpenInformation.FileName;
			browser.LoadUrl (fileName);
			ContentName = fileName;
			return TaskUtil.Default<object> ();
		}

		public Task Load (string html, string initialTitle)
		{
			isLoading = true;
			fileName = null;
			ContentName = initialTitle;
			browser.LoadHtml (html);
			return TaskUtil.Default<object> ();
		}

		public override bool CanReuseView (string fileName)
		{
			return (this.fileName == fileName);
		}

		public override bool IsViewOnly {
			get {
				return true;
			}
		}

		public override bool IsReadOnly {
			get {
				return IsFile;
			}
		}

		public override bool IsFile {
			get {
				return !string.IsNullOrEmpty (fileName);
			}
		}

		protected virtual void OnTitleChanged (string newTitle)
		{
			if (string.IsNullOrEmpty(fileName))
				ContentName = browser.Title;
		}

		protected virtual bool OnLocationChanging (string newUri)
		{
			bool stopNavigation = false;
			if (!LocalNavigation && !isLoading) {
				DesktopService.ShowUrl (newUri);
				stopNavigation = true;
			}
			isLoading = false;
			return stopNavigation;
		}

		protected virtual void OnLocationChanged ()
		{
			Uri newUri;
			if (Uri.TryCreate (browser.Location, UriKind.RelativeOrAbsolute, out newUri)) {
				if (!string.IsNullOrEmpty(fileName) && newUri.AbsolutePath != fileName) {
					fileName = null;
					ContentName = browser.Title;
				} else if (System.IO.File.Exists (newUri.AbsolutePath)) {
					ContentName = fileName = newUri.AbsolutePath;
				}
			}
		}

		protected virtual void OnContentLoaded ()
		{
		}
	}
}

