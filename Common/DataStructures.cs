using System;
using System.Collections.Generic;
using System.Drawing;

namespace gvaduha.Common
{
    public struct BBox
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Bottom { get; set; }
        public int Right { get; set; }
        public BBox(int top, int left, int bottom, int right)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
        }
        public Rectangle ToRectangle()
        {
            return new Rectangle(Top, Left, Bottom - Top, Right - Left);
        }
    }

    public struct DetectionResult
    {
        public int Class { get; set; }
        public float Score { get; set; }
        public BBox Box { get; set; }
    }

    public struct ImageProcessorResult
    {
        public string Uri { get; set; }
        public DateTime TimeStamp { get; set; }
        public List<DetectionResult> DetectionResults { get; set; }
    }
}
