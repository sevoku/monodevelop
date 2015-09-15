//
// GtkWPFWidget.cs
//
// Author:
//       Marius Ungureanu <marius.ungureanu@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc (http://www.xamarin.com)
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
#if WIN32
using System;
using System.Windows;

namespace Windows
{
	public class GtkWPFWidget : Gtk.Widget
	{
		readonly IntPtr wpfWindowPtr;
		readonly Window wpfWindow;

		public GtkWPFWidget (Window WpfWindow)
		{
			wpfWindowPtr = new WindowInteropHelper (WpfWindow).Handle;
			wpfWindow = WpfWindow;
		}

		protected override void OnRealized ()
		{
			base.OnRealized ();

			IntPtr gtkWindowPtr = GtkWin32Interop.HWndGet (GdkWindow);
			GtkWin32Interop.SetWindowLongPtr (wpfWindowPtr, (int)GtkWin32Interop.GWLParameter.GWL_HWNDPARENT, gtkWindowPtr);
		}

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);

			wpfWindow.Left = allocation.Left;
			wpfWindow.Top = allocation.Top;
			wpfWindow.Width = allocation.Width;
			wpfWindow.Height = allocation.Height;
		}

		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();

			wpfWindow.Close ();
		}

		protected override void OnShown ()
		{
			base.OnShown ();

			wpfWindow.Show ();
		}

		protected override void OnHidden ()
		{
			base.OnHidden ();

			wpfWindow.Hide ();
		}
	}
}
#endif
