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
using System.Windows.Interop;
using Gtk;
using Gdk;

namespace MonoDevelop.Components.Windows
{
	public class GtkWPFWidget : Gtk.Widget
	{
		readonly IntPtr wpfWindowPtr;
		readonly System.Windows.Window wpfWindow;

		public GtkWPFWidget (System.Windows.Window wpfWindow)
		{
			wpfWindowPtr = new WindowInteropHelper (wpfWindow).Handle;
			this.wpfWindow = wpfWindow;

			SetWindowDecorations ();
		}

		void SetWindowDecorations()
		{
			wpfWindow.ShowInTaskbar = false;

			int exStyle = (int)GtkWin32Interop.GetWindowLongPtr (wpfWindowPtr, (int)GtkWin32Interop.GWLParameter.GWL_EXSTYLE);
			exStyle |= (int)GtkWin32Interop.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
			GtkWin32Interop.SetWindowLongPtr (wpfWindowPtr, (int)GtkWin32Interop.GWLParameter.GWL_EXSTYLE, (IntPtr)exStyle);
		}

		protected override void OnRealized ()
		{
			WidgetFlags |= WidgetFlags.Realized;
			WindowAttr attributes = new WindowAttr {
				WindowType = Gdk.WindowType.Child,
				X = Allocation.X,
				Y = Allocation.Y,
				Width = Allocation.Width,
				Height = Allocation.Height,
				Wclass = WindowClass.InputOutput,
				Visual = Visual,
				Colormap = Colormap,
				EventMask = (int)(Events | Gdk.EventMask.ExposureMask),
				Mask = Events | Gdk.EventMask.ExposureMask
			};

			WindowAttributesType mask = WindowAttributesType.X | WindowAttributesType.Y | WindowAttributesType.Colormap | WindowAttributesType.Visual;
			GdkWindow = new Gdk.Window (ParentWindow, attributes, mask);
			GdkWindow.UserData = Raw;
			GdkWindow.Background = Style.Background (StateType.Normal);
			Style = Style.Attach(GdkWindow);

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
