using System.Drawing;
using AppKit;

namespace Analyzer
{
    public class DiagramView : NSView
    {

        public override void Display()
        {
            base.Display();
            Layer.BackgroundColor = NSColor.Blue.CGColor;
        }

        public DiagramView()
        {
        }
    }
}
