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
		AppKit.NSTextView InterText { get; set; }

		[Outlet]
		AppKit.NSOutlineView InterView { get; set; }

		[Outlet]
		AppKit.NSTextField StatPath { get; set; }

		[Outlet]
		AppKit.NSTableView StatTableView { get; set; }

		[Action ("CloseButton:")]
		partial void CloseButton (Foundation.NSObject sender);

		[Action ("CompareStat:")]
		partial void CompareStat (Foundation.NSObject sender);

		[Action ("DeleteStat:")]
		partial void DeleteStat (Foundation.NSObject sender);

		[Action ("LoadStat:")]
		partial void LoadStat (Foundation.NSObject sender);

		[Action ("ReadStat:")]
		partial void ReadStat (Foundation.NSObject sender);

		[Action ("SelectionChanged:")]
		partial void SelectionChanged (AppKit.NSOutlineView sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (InterText != null) {
				InterText.Dispose ();
				InterText = null;
			}

			if (InterView != null) {
				InterView.Dispose ();
				InterView = null;
			}

			if (StatPath != null) {
				StatPath.Dispose ();
				StatPath = null;
			}

			if (StatTableView != null) {
				StatTableView.Dispose ();
				StatTableView = null;
			}
		}
	}
}
