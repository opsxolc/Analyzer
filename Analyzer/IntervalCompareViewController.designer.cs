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
	[Register ("IntervalCompareViewController")]
	partial class IntervalCompareViewController
	{
		[Outlet]
		AppKit.NSTextField HelloLabel { get; set; }

		[Outlet]
		AppKit.NSOutlineView IntervalTree { get; set; }

		[Outlet]
		OxyPlot.Xamarin.Mac.PlotView Plot { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (Plot != null) {
				Plot.Dispose ();
				Plot = null;
			}

			if (IntervalTree != null) {
				IntervalTree.Dispose ();
				IntervalTree = null;
			}

			if (HelloLabel != null) {
				HelloLabel.Dispose ();
				HelloLabel = null;
			}
		}
	}
}
