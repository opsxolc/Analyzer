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
	[Register ("IdlePopoverController")]
	partial class IdlePopoverController
	{
		[Outlet]
		AppKit.NSTextField IdleLabel { get; set; }

		[Outlet]
		AppKit.NSLevelIndicator IdleLevel { get; set; }

		[Outlet]
		AppKit.NSTextField LoadLabel { get; set; }

		[Outlet]
		AppKit.NSLevelIndicator LoadLevel { get; set; }

		[Outlet]
		AppKit.NSTextField ReasonLabel { get; set; }

		[Outlet]
		AppKit.NSTextField VarLabel { get; set; }

		[Outlet]
		AppKit.NSLevelIndicator VarLevel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (IdleLevel != null) {
				IdleLevel.Dispose ();
				IdleLevel = null;
			}

			if (IdleLabel != null) {
				IdleLabel.Dispose ();
				IdleLabel = null;
			}

			if (LoadLevel != null) {
				LoadLevel.Dispose ();
				LoadLevel = null;
			}

			if (LoadLabel != null) {
				LoadLabel.Dispose ();
				LoadLabel = null;
			}

			if (VarLevel != null) {
				VarLevel.Dispose ();
				VarLevel = null;
			}

			if (VarLabel != null) {
				VarLabel.Dispose ();
				VarLabel = null;
			}

			if (ReasonLabel != null) {
				ReasonLabel.Dispose ();
				ReasonLabel = null;
			}
		}
	}
}
