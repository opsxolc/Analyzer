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
	[Register ("CommPopoverController")]
	partial class CommPopoverController
	{
		[Outlet]
		AppKit.NSTextField ColOpLabel { get; set; }

		[Outlet]
		AppKit.NSLevelIndicator ColOpLevel { get; set; }

		[Outlet]
		AppKit.NSTextField ColOpNameLabel { get; set; }

		[Outlet]
		AppKit.NSTextField CommLabel { get; set; }

		[Outlet]
		AppKit.NSLevelIndicator CommLevel { get; set; }

		[Outlet]
		AppKit.NSTextField LoadImbLabel { get; set; }

		[Outlet]
		AppKit.NSLevelIndicator LoadImbLevel { get; set; }

		[Outlet]
		AppKit.NSTextField ReasonLabel { get; set; }

		[Outlet]
		AppKit.NSTextField SynchLabel { get; set; }

		[Outlet]
		AppKit.NSLevelIndicator SynchLevel { get; set; }

		[Outlet]
		AppKit.NSTextField TimeVarLabel { get; set; }

		[Outlet]
		AppKit.NSLevelIndicator TimeVarLevel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CommLabel != null) {
				CommLabel.Dispose ();
				CommLabel = null;
			}

			if (CommLevel != null) {
				CommLevel.Dispose ();
				CommLevel = null;
			}

			if (TimeVarLabel != null) {
				TimeVarLabel.Dispose ();
				TimeVarLabel = null;
			}

			if (TimeVarLevel != null) {
				TimeVarLevel.Dispose ();
				TimeVarLevel = null;
			}

			if (SynchLevel != null) {
				SynchLevel.Dispose ();
				SynchLevel = null;
			}

			if (SynchLabel != null) {
				SynchLabel.Dispose ();
				SynchLabel = null;
			}

			if (LoadImbLabel != null) {
				LoadImbLabel.Dispose ();
				LoadImbLabel = null;
			}

			if (LoadImbLevel != null) {
				LoadImbLevel.Dispose ();
				LoadImbLevel = null;
			}

			if (ColOpLevel != null) {
				ColOpLevel.Dispose ();
				ColOpLevel = null;
			}

			if (ColOpLabel != null) {
				ColOpLabel.Dispose ();
				ColOpLabel = null;
			}

			if (ReasonLabel != null) {
				ReasonLabel.Dispose ();
				ReasonLabel = null;
			}

			if (ColOpNameLabel != null) {
				ColOpNameLabel.Dispose ();
				ColOpNameLabel = null;
			}
		}
	}
}
