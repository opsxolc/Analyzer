// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Analyzer
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSOutlineView InterView { get; set; }

		[Outlet]
		AppKit.NSTextField StatLabel { get; set; }

		[Outlet]
		AppKit.NSTextField StatPath { get; set; }

		[Outlet]
		AppKit.NSScrollView StatText { get; set; }

		[Outlet]
		AppKit.NSTextView Text { get; set; }

		[Action ("CloseButton:")]
		partial void CloseButton (Foundation.NSObject sender);

		[Action ("ReadStat:")]
		partial void ReadStat (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (StatLabel != null) {
				StatLabel.Dispose ();
				StatLabel = null;
			}

			if (StatPath != null) {
				StatPath.Dispose ();
				StatPath = null;
			}

			if (StatText != null) {
				StatText.Dispose ();
				StatText = null;
			}

			if (Text != null) {
				Text.Dispose ();
				Text = null;
			}

			if (InterView != null) {
				InterView.Dispose ();
				InterView = null;
			}
		}
	}
}
