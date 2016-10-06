//
// WebBrowser.cs
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
using MonoDevelop.Components;
using Xwt;

namespace MonoDevelop.Ide.WebBrowser
{
	public class WebBrowserLoader : IWebBrowserLoader
	{
		public bool CanCreateBrowser {
			get {
				return true;
			}
		}

		public IWebBrowser GetBrowser ()
		{
			return new WebBrowser ();
		}
	}

	public class WebBrowser : Control, IWebBrowser
	{
		WebView browser;
		
		public WebBrowser ()
		{
			browser = new WebView ();
			browser.TitleChanged += HandleTitleChanged;
			browser.Loading += HandleLoading;
			browser.NavigateToUrl += HandleNavigateToUrl;
			browser.Loaded += HandleLoaded;
		}

		protected override object CreateNativeWidget<T> ()
		{
			return browser.Surface.NativeWidget;
		}

		public Control Control {
			get {
				return this;
			}
		}

		public bool CanGoBack {
			get {
				return browser.CanGoBack;
			}
		}

		public bool CanGoForward {
			get {
				return browser.CanGoForward;
			}
		}

		public string JSStatus {
			get {
				return null;
			}
		}

		public string LinkStatus {
			get {
				return null;
			}
		}

		public string Location {
			get {
				return browser.Url;
			}
		}

		public string Title {
			get {
				return browser.Title;
			}
		}

		void HandleTitleChanged (object sender, EventArgs e)
		{
			TitleChanged?.Invoke (this, new TitleChangedEventArgs (browser.Title));
		}

		void HandleLoading (object sender, EventArgs e)
		{
			LocationChanged?.Invoke (this, new LocationChangedEventArgs (Location));
			NetStart?.Invoke (this, EventArgs.Empty);
		}

		void HandleNavigateToUrl (object sender, NavigateToUrlEventArgs e)
		{
			if (LocationChanging != null) {
				var args = new LocationChangingEventArgs (e.Uri.AbsoluteUri, false);
				LocationChanging (this, args);
				if (args.SuppressChange)
					e.SetHandled ();
			}
		}

		void HandleLoaded (object sender, EventArgs e)
		{
			NetStop?.Invoke (this, EventArgs.Empty);
			PageLoaded?.Invoke (this, EventArgs.Empty);
		}

		public event StatusMessageChangedHandler JSStatusChanged;
		public event LocationChangingHandler LinkClicked;
		public event StatusMessageChangedHandler LinkStatusChanged;
		public event LoadingProgressChangedHandler LoadingProgressChanged;
		public event LocationChangedHandler LocationChanged;
		public event LocationChangingHandler LocationChanging;
		public event EventHandler NetStart;
		public event EventHandler NetStop;
		public event PageLoadedHandler PageLoaded;
		public event TitleChangedHandler TitleChanged;

		public void GoBack ()
		{
			browser.GoBack ();
		}

		public void GoForward ()
		{
			browser.GoForward ();
		}

		public void LoadHtml (string html)
		{
			browser.LoadHtml (html, string.Empty);
		}

		public void LoadUrl (string url)
		{
			browser.Url = url;
		}

		public void Reload ()
		{
			browser.Reload ();
		}

		public void StopLoad ()
		{
			browser.StopLoading ();
		}
	}
}

