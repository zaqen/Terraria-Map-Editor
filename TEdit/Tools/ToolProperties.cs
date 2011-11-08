using System;
using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.Common.Structures;
using System.Collections.Generic;

namespace TEdit.Tools
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Serializable]
    public class ToolProperties : ObservableObject
    {
        private int _Height;
        private WriteableBitmap _Image;
        private bool _IsOutline;
        private bool _IsSquare;
        private int _MaxHeight;
        private int _MaxOutlineThickness;
        private int _MaxWidth;
        private int _MinHeight;
        private int _MinWidth;
        private ToolAnchorMode _Mode;
        private ToolAnchorMode _PreviewMode;
        private PointInt32 _Offset = new PointInt32();
        private PointInt32 _PreviewOffset = new PointInt32();
        private int _OutlineThickness;
        private int _Width;
        private ToolBrushShape _brushShape;

        public ToolProperties()
        {
            _MaxOutlineThickness = 10;
            _OutlineThickness = 1;
            _Height = 10;
            _Width = 10;
            _MinHeight = 1;
            _MinWidth = 1;
            _MaxHeight = 100;
            _MaxWidth = 100;

            _IsSquare = true;
            _brushShape = ToolBrushShape.Round;
            _Mode = ToolAnchorMode.Center;
            _PreviewMode = ToolAnchorMode.TopLeft;
            CalcOffset();
            AnchorModes = Enum.GetValues(typeof (ToolAnchorMode));
            BrushShapes = Enum.GetValues(typeof (ToolBrushShape));
        }

        public Array AnchorModes { get; set; }
        public Array BrushShapes { get; set; }

        public ToolAnchorMode Mode
        {
            get { return _Mode; }
            set { SetProperty(ref _Mode, ref value, "Mode"); CalcOffset(); }
        }

        public ToolBrushShape BrushShape
        {
            get { return _brushShape; }
            set { SetProperty(ref _brushShape, ref value, "BrushShape"); CalcOffset(); }
        }

        public int Width
        {
            get { return _Width; }
            set
            {
                int validWidth = value;
                if (validWidth < MinWidth)
                    validWidth = MinWidth;

                if (validWidth > MaxWidth)
                    validWidth = MaxWidth;

                if (_Width != validWidth)
                {
                    _Width = validWidth;

                    if (IsSquare)
                        Height = validWidth;

                    RaisePropertyChanged("Width");
                    CalcOffset();
                }
            }
        }

        public int Height
        {
            get { return _Height; }
            set
            {
                int validHeight = value;
                if (validHeight < MinHeight)
                    validHeight = MinHeight;

                if (validHeight > MaxHeight)
                    validHeight = MaxHeight;

                if (_Height != validHeight)
                {
                    _Height = validHeight;

                    if (IsSquare)
                        Width = validHeight;


                    RaisePropertyChanged("Height");
                    CalcOffset();
                }
            }
        }

        public int MinWidth
        {
            get { return _MinWidth; }
            set
            {
                if (value > MaxWidth)
                    MaxWidth = value;

                if (_MinWidth != value)
                {
                    _MinWidth = value;
                    RaisePropertyChanged("MinWidth");
                    Width = _Width; // Validate Width 
                }
            }
        }

        public int MinHeight
        {
            get { return _MinHeight; }
            set
            {
                if (value > MaxHeight)
                    MaxHeight = value;

                if (_MinHeight != value)
                {
                    _MinHeight = value;
                    RaisePropertyChanged("MinHeight");
                    Height = _Height; // Validate Height
                }
            }
        }

        public int MaxWidth
        {
            get { return _MaxWidth; }
            set
            {
                if (value < MinWidth)
                    MinWidth = value;

                if (_MaxWidth != value)
                {
                    _MaxWidth = value;
                    RaisePropertyChanged("MaxWidth");
                    Width = _Width; // Validate Width 
                }
            }
        }

        public int MaxHeight
        {
            get { return _MaxHeight; }
            set
            {
                if (value < MinHeight)
                    MinHeight = value;

                if (_MaxHeight != value)
                {
                    _MaxHeight = value;
                    RaisePropertyChanged("MaxHeight");
                    Height = _Height; // Validate Height
                }
            }
        }

        public bool IsSquare
        {
            get { return _IsSquare; }
            set { SetProperty(ref _IsSquare, ref value, "IsSquare"); }
        }

        public bool IsOutline
        {
            get { return _IsOutline; }
            set { SetProperty(ref _IsOutline, ref value, "IsOutline"); }
        }

        public int OutlineThickness
        {
            get { return _OutlineThickness; }
            set { SetProperty(ref _OutlineThickness, ref value, "OutlineThickness"); }
        }

        public int MaxOutlineThickness
        {
            get { return _MaxOutlineThickness; }
            set
            {
                int validValue = value;
                if (validValue < 1)
                    validValue = 1;

                if (_MaxOutlineThickness != validValue)
                {
                    _MaxOutlineThickness = validValue;
                    RaisePropertyChanged("MaxOutlineThickness");

                    if (_OutlineThickness > _MaxOutlineThickness)
                    {
                        OutlineThickness = _MaxOutlineThickness;
                    }
                }
            }
        }


        //public int RadiusX
        //{
        //    get { return (int) Math.Floor(this.Width/2.0D); }
        //}
        //public int RadiusY
        //{
        //    get { return (int)Math.Floor(this.Height / 2.0D); }
        //}

        public PointInt32 Offset
        {
            get { return _Offset; }
            set { SetProperty(ref _Offset, ref value, "Offset"); }
        }

        public WriteableBitmap Image
        {
            get { return _Image; }
            set { SetProperty(ref _Image, ref value, "Image"); }
        }

        public event EventHandler ToolPreviewRequest;

        protected virtual void OnToolPreviewRequest(object sender, EventArgs e)
        {
            if (ToolPreviewRequest != null)
                ToolPreviewRequest(sender, e);
        }

        public ToolAnchorMode PreviewMode {
            get { return _PreviewMode; }
            set { SetProperty(ref _PreviewMode, ref value, "PreviewMode"); CalcOffset(false); }
        }
        public PointInt32 PreviewOffset {
            get { return _PreviewOffset; }
            set { SetProperty(ref _PreviewOffset, ref value, "PreviewOffset"); }
        }

        private void CalcOffset(ref PointInt32 off, ToolAnchorMode mode, int W, int H) {
            off = new PointInt32();

            if (mode.Has(ToolAnchorModeParts.Left))   off.X = 0;
            if (mode.Has(ToolAnchorModeParts.Center)) off.X = W/2;
            if (mode.Has(ToolAnchorModeParts.Right))  off.X = W;
            
            if (mode.Has(ToolAnchorModeParts.Top))    off.Y = 0;
            if (mode.Has(ToolAnchorModeParts.Middle)) off.Y = H/2;
            if (mode.Has(ToolAnchorModeParts.Bottom)) off.Y = H;
        }
        private void CalcOffset(bool triggerPreview = true) {
            CalcOffset(ref _Offset, Mode, _Width, _Height);
            if (_Image != null) CalcOffset(ref _PreviewOffset, PreviewMode, (int)_Image.Width - 1, (int)_Image.Height - 1);
            RaisePropertyChanged("Offset");
            RaisePropertyChanged("PreviewOffset");

            MaxOutlineThickness = (int) Math.Max(1, Math.Min(Math.Floor(Height/2.0), Math.Floor(Width/2.0)));
            if (triggerPreview) OnToolPreviewRequest(this, new EventArgs());
        }
    }
}