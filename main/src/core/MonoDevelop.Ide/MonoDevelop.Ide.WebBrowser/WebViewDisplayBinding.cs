//
// WebViewDisplayBinding.cs
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
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

namespace MonoDevelop.Ide.WebBrowser
{
	public class WebViewDisplayBinding : IViewDisplayBinding
	{
		public WebViewDisplayBinding ()
		{
		}

		public bool CanUseAsDefault {
			get {
				return true;
			}
		}

		public string Name {
			get {
				return "Web Viewer";
			}
		}

		public bool CanHandle (FilePath fileName, string mimeType, Project ownerProject)
		{
			if (string.IsNullOrEmpty (fileName) || string.IsNullOrEmpty (mimeType))
				return false;
			
			if (DesktopService.GetFileIsText (fileName, mimeType))
				return DesktopService.GetMimeTypeIsSubtype (mimeType, "text/html");
			
			return false;
		}

		public ViewContent CreateContent (FilePath fileName, string mimeType, Project ownerProject)
		{
			var view = new WebViewContent ();
			return view;
		}

		public bool CanHandleFile (string fileName)
		{
			var mimeType = DesktopService.GetMimeTypeForUri (fileName);
			if (DesktopService.GetMimeTypeIsSubtype (mimeType, "text/html"))
				return DesktopService.GetFileIsText (fileName, mimeType);
			return false;
		}
	}
}

