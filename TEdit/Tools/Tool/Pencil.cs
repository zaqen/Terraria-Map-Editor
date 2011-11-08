using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.Common.Structures;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;
using TEdit.Tools.History;

namespace TEdit.Tools.Tool
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 3)]
    public class Pencil : ToolBase
    {
        private bool _isLeftDown;

        private bool _isSnapDirectionSet;
        [Import]
        private ToolProperties _properties;
        [Import]
        private WorldRenderer _renderer;
        [Import]
        private SelectionArea _selection;
        private Orientation _snapDirection = Orientation.Horizontal;
        private PointInt32? _startPoint;
        private PointInt32? _endPoint;
        private Color previewColor = new Color(0, 90, 255, 127);
        [Import]
        private TilePicker _tilePicker;
        [Import("World", typeof(World))]
        private World _world;

        [Import]
        private HistoryManager HistMan;
        

        public Pencil()
        {
            _image = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/pencil.png"));
            _name = "Pencil";
            _type = ToolType.Pencil;
            _isActive = false;
        }

        #region Properties

        private readonly BitmapImage _image;
        private readonly string _name;

        private readonly ToolType _type;
        private bool _isActive;
        private bool _isRightDown;

        public override string Name
        {
            get { return _name; }
        }

        public override ToolType Type
        {
            get { return _type; }
        }

        public override BitmapImage Image
        {
            get { return _image; }
        }

        public override bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    RaisePropertyChanged("IsActive");

                    if (_isActive)
                    {
                        _properties.MinHeight = 1;
                        _properties.MinWidth = 1;
                        _properties.MaxHeight = 1;
                        _properties.MaxWidth = 1;
                    }
                }
            }
        }

        #endregion

        public override bool PressTool(TileMouseEventArgs e)
        {
            if (!_isRightDown && !_isLeftDown)
                _startPoint = e.Tile;

            CheckDirectionandDraw(e);
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
            return true;
        }

        public override bool MoveTool(TileMouseEventArgs e)
        {
            WriteableBitmap bmp; 
            
            if (_startPoint != null) _endPoint = e.Tile;
            CheckDirectionandDraw(e);
            ToolAnchorMode mode = ToolAnchorMode.Center;

            // Line draw preview
            if (_isRightDown && _startPoint != null && _endPoint != null) {
                var sp = (PointInt32)_startPoint;
                var ep = (PointInt32)_endPoint;
                var delta = ep - sp;
                var rect = new RectI(new PointInt32(), new SizeInt32(Math.Abs(delta.X) + 1, Math.Abs(delta.Y) + 1));

                // figure out exactly which PreviewMode
                mode = 0;
                if      (delta.X <  0) mode += (int)ToolAnchorModeParts.Left;
                else if (delta.X == 0) mode += (int)ToolAnchorModeParts.Center;
                else if (delta.X >  0) mode += (int)ToolAnchorModeParts.Right;

                if      (delta.Y <  0) mode += (int)ToolAnchorModeParts.Top;
                else if (delta.Y == 0) mode += (int)ToolAnchorModeParts.Middle;
                else if (delta.Y >  0) mode += (int)ToolAnchorModeParts.Bottom;

                // which direction to draw the line
                var linePnts = new PointInt32[2];
                switch (mode) {
                    case ToolAnchorMode.TopLeft:     linePnts = new[] { rect.BottomRight, rect.TopLeft }; break;
                    case ToolAnchorMode.TopRight:    linePnts = new[] { rect.BottomLeft, rect.TopRight }; break;
                    case ToolAnchorMode.BottomLeft:  linePnts = new[] { rect.TopRight, rect.BottomLeft }; break;
                    case ToolAnchorMode.BottomRight: linePnts = new[] { rect.TopLeft, rect.BottomRight }; break;
                    default:  // has middle or center, order doesn't matter
                        linePnts = new[] { rect.TopLeft, rect.BottomRight };
                        break;
                }

                bmp = new WriteableBitmap(
                    rect.W,
                    rect.H,
                    96,
                    96,
                    System.Windows.Media.PixelFormats.Bgra32,
                    null);

                bmp.Clear();
                foreach (PointInt32 p in WorldRenderer.DrawLine(linePnts[0], linePnts[1])) {
                    if (_selection.IsValid(p)) bmp.SetPixel(p.X, p.Y, previewColor);
                }
            }
            // Single dot
            else {
                bmp = new WriteableBitmap(
                    1,
                    1,
                    96,
                    96,
                    System.Windows.Media.PixelFormats.Bgra32,
                    null);

                bmp.Clear();
                bmp.SetPixel(0, 0, previewColor);
            }

            _properties.Image = bmp;
            _properties.PreviewMode = mode;

            return false;
        }

        public override bool ReleaseTool(TileMouseEventArgs e)
        {
            if (_startPoint != null) DrawLine(e.Tile);
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);

            HistMan.AddBufferToHistory();
            _startPoint = null;
            _endPoint   = null;
            return true;
        }

        private void CheckDirectionandDraw(TileMouseEventArgs e)
        {
            PointInt32 p = e.Tile;
            // Free range draw only
            if (_isLeftDown && !_isRightDown) {
                DrawLine(p);
                _startPoint = p;
            }
        }

        public override WriteableBitmap PreviewTool() {            
            var bmp = new WriteableBitmap(
                1,
                1,
                96,
                96,
                System.Windows.Media.PixelFormats.Bgra32,
                null);

            bmp.Clear();
            bmp.SetPixel(0, 0, previewColor);

            return bmp;
        }

        private void DrawLine(PointInt32 endPoint)
        {
            var startPoint = (PointInt32)_startPoint;
            foreach (PointInt32 p in WorldRenderer.DrawLine(startPoint, endPoint))
            {
                if (_selection.IsValid(p))
                {
                    int x = p.X;
                    int y = p.Y;
                    if (HistMan.SaveHistory)
                        HistMan.AddTileToBuffer(x, y, ref _world.Tiles[x, y]);
                    _world.SetTileXY(ref x, ref y, ref _tilePicker, ref _selection);
                    _renderer.UpdateWorldImage(p);
                }
            }

            // Make sure the actual corners are defined
            /* if (_selection.IsValid(startPoint) bmp.SetPixel(startPoint.X, startPoint.Y, previewColor);
            if (_selection.IsValid(endPoint)   bmp.SetPixel(endPoint.X, endPoint.Y, previewColor); */
        }
    }
}