﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Threading.Tasks;

namespace gvaduha.Common
{
    public static class DrawingConfig
    {
        public static Bgr DefaultClassColor = new Bgr(Color.Gray);
        public static readonly Dictionary<int, Bgr> DetectedClassColorMap = new Dictionary<int, Bgr>
        {
            { 1, new Bgr(Color.Red) },
            { 2, new Bgr(Color.Blue) },
            { 3, new Bgr(Color.Green) },
            { 4, new Bgr(Color.Yellow) },
            { 5, new Bgr(Color.Purple) },
        };
        public static int BoxBorderWidth = 2;
        public static FontFace CaptionFont = FontFace.HersheySimplex;
        public static double CaptionFontScale = .4f;
        public static int CaptionFontThickness = 1;
        public static int CaptionFontBottomMargin = 5;
    }

    public class DetectedBoxPainter
    {
        //private string GetStaticImage()
        //{
        //    int width = 128;
        //    int height = 128;
        //    using (FileStream pngStream = new FileStream(@"x:\qr.png", FileMode.Open, FileAccess.Read))
        //    using (var image = new Bitmap(pngStream))
        //    {
        //        var resized = new Bitmap(width, height);
        //        using (var graphics = Graphics.FromImage(resized))
        //        {
        //            graphics.CompositingQuality = CompositingQuality.HighSpeed;
        //            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //            graphics.CompositingMode = CompositingMode.SourceCopy;
        //            graphics.DrawImage(image, 0, 0, width, height);
        //            //graphics.DrawString("SHIIIIII!!!!", new Font(FontFamily.GenericSansSerif, 15), new SolidBrush(Color.Red), new PointF(1,1));
        //            graphics.Save();
        //        }

        //        using var ms = new MemoryStream();
        //        image.Save(ms, ImageFormat.Jpeg);
        //        //var videoframe = $"<img alt='frame' src='data:image/jpeg;base64,{Convert.ToBase64String(ms.ToArray())}'/>";
        //        var videoframe = Convert.ToBase64String(ms.ToArray());
        //        return videoframe;
        //    }
        //}

        private IDetectionResultSource _drs;

        public DetectedBoxPainter(IDetectionResultSource drs)
        {
            _drs = drs;
        }

        private Bgr ComplemetaryColor(Bgr c)
        {
            return new Bgr(255 - c.Blue, 255 - c.Green, 255 - c.Red);
        }

        private void DrawBox(Image<Bgr, byte> image, DetectionResult dr)
        {
            if (!DrawingConfig.DetectedClassColorMap.TryGetValue(dr.Class, out Bgr color))
                color = DrawingConfig.DefaultClassColor;

            image.Draw(dr.Box.ToRectangle(), color, DrawingConfig.BoxBorderWidth);

            var text = $"C:{dr.Class} S:{dr.Score:#.##}";
            int baseline = 0;
            var textSize = CvInvoke.GetTextSize(text, DrawingConfig.CaptionFont, DrawingConfig.CaptionFontScale, DrawingConfig.CaptionFontThickness, ref baseline);
            var blPoint = new Point(dr.Box.Left, dr.Box.Bottom + textSize.Height + DrawingConfig.CaptionFontBottomMargin);
            image.Draw(text, blPoint, DrawingConfig.CaptionFont, DrawingConfig.CaptionFontScale, ComplemetaryColor(color), DrawingConfig.CaptionFontThickness * 2 + 1);
            image.Draw(text, blPoint, DrawingConfig.CaptionFont, DrawingConfig.CaptionFontScale, color, DrawingConfig.CaptionFontThickness);
        }

        public async Task<string> GetNextImageAsync()
        {
            var result = await _drs.GetNextResultsAsync();

            result.detectionResults.ToList().ForEach(x => DrawBox(result.image, x));

            var videoframe = Convert.ToBase64String(result.image.ToJpegData());
            return videoframe;
        }
    }
}
