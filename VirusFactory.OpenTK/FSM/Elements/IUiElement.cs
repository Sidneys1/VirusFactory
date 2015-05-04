using System.Drawing;
using OpenTK;
using OpenTK.Graphics;

namespace VirusFactory.OpenTK.FSM.Elements {
    public interface IUiElement {
        Vector2 MousePosition { get; }
        SizeF Size { get; }
        RectangleF Bounds { get; }
        bool IsMouseOver { get; }
        bool IsMouseDown { get; }
        Vector2 Position { get; set; }
        Color4 Color { get; }
        Color4? MouseOverColor { get; set; }
        Color4 NormalColor { get; set; }
        Vector2 PositionAdd { get; set; }
    }
}