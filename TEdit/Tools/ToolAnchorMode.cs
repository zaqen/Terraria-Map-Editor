using System;
namespace TEdit.Tools
{
    [Flags]
    public enum ToolAnchorMode {
        TopLeft      = ToolAnchorModeParts.Top    + ToolAnchorModeParts.Left,
        TopCenter    = ToolAnchorModeParts.Top    + ToolAnchorModeParts.Center,
        TopRight     = ToolAnchorModeParts.Top    + ToolAnchorModeParts.Right,
        MiddleLeft   = ToolAnchorModeParts.Middle + ToolAnchorModeParts.Left,
        Center       = ToolAnchorModeParts.Middle + ToolAnchorModeParts.Center,
        MiddleRight  = ToolAnchorModeParts.Middle + ToolAnchorModeParts.Right,
        BottomLeft   = ToolAnchorModeParts.Bottom + ToolAnchorModeParts.Left,
        BottomCenter = ToolAnchorModeParts.Bottom + ToolAnchorModeParts.Center,
        BottomRight  = ToolAnchorModeParts.Bottom + ToolAnchorModeParts.Right,
    }

    [Flags]
    public enum ToolAnchorModeParts {
        None   = 0,
        Left   = 1,
        Center = 2,
        Right  = 4,
        Top    = 8,
        Middle = 16,
        Bottom = 32,
    }

}