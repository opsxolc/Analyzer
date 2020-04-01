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
		AppKit.NSTextField ChooseLabel { get; set; }

		[Outlet]
		AppKit.NSPopUpButton CompareSort { get; set; }

		[Outlet]
		AppKit.NSTextView InterText { get; set; }

		[Outlet]
		AppKit.NSButton IntervalCompareButton { get; set; }

		[Outlet]
		AppKit.NSOutlineView InterView { get; set; }

		[Outlet]
		OxyPlot.Xamarin.Mac.PlotView plotView { get; set; }

		[Outlet]
		AppKit.NSTextField StatPath { get; set; }

		[Outlet]
		AppKit.NSTableView StatTableView { get; set; }

		[Outlet]
		AppKit.NSTabView TabView { get; set; }

		[Action ("CloseButton:")]
		partial void CloseButton (Foundation.NSObject sender);

		[Action ("CompareBack:")]
		partial void CompareBack (Foundation.NSObject sender);

		[Action ("CompareReset:")]
		partial void CompareReset (Foundation.NSObject sender);

		[Action ("CompareStat:")]
		partial void CompareStat (Foundation.NSObject sender);

		[Action ("DeleteStat:")]
		partial void DeleteStat (Foundation.NSObject sender);

		[Action ("IntervalCompare:")]
		partial void IntervalCompare (Foundation.NSObject sender);

		[Action ("LoadStat:")]
		partial void LoadStat (Foundation.NSObject sender);

		[Action ("ReadStat:")]
		partial void ReadStat (Foundation.NSObject sender);

		[Action ("SelectionChanged:")]
		partial void SelectionChanged (AppKit.NSOutlineView sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (ChooseLabel != null) {
				ChooseLabel.Dispose ();
				ChooseLabel = null;
			}

			if (CompareSort != null) {
				CompareSort.Dispose ();
				CompareSort = null;
			}

			if (InterText != null) {
				InterText.Dispose ();
				InterText = null;
			}

			if (InterView != null) {
				InterView.Dispose ();
				InterView = null;
			}

			if (plotView != null) {
				plotView.Dispose ();
				plotView = null;
			}

			if (StatPath != null) {
				StatPath.Dispose ();
				StatPath = null;
			}

			if (StatTableView != null) {
				StatTableView.Dispose ();
				StatTableView = null;
			}

			if (TabView != null) {
				TabView.Dispose ();
				TabView = null;
			}

			if (IntervalCompareButton != null) {
				IntervalCompareButton.Dispose ();
				IntervalCompareButton = null;
			}
		}
	}
}
