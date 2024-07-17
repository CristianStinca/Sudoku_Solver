using Emgu.CV;
using Emgu.CV.Platform.Maui;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesseractOcrMaui;
using TesseractOcrMaui.Results;
using Size = System.Drawing.Size;
using Point = System.Drawing.Point;
using PointF = System.Drawing.PointF;
using VectorOfPoint = Emgu.CV.Util.VectorOfPoint;
using VectorOfVectorOfPoint = Emgu.CV.Util.VectorOfVectorOfPoint;
using System.Drawing;

namespace SudokuSolverApp.Models
{
    internal class ImageToSudokuService
    {
#if DEBUG
        public static SKBitmap originalImg;
        public static SKBitmap bwImg;
        public static SKBitmap conturedImg;
        public static SKBitmap skewedImg;
        public static SKBitmap[,] separatedCells = new SKBitmap[9, 9];
#endif
        public ITesseract tes;

        public ImageToSudokuService(ITesseract tesseract)
        {
            MauiInvoke.Init();
            this.tes = tesseract;
            var datares = tes.LoadTraineddataAsync().Result;
            tes.EngineConfiguration = (engine) =>
            {
                engine.DefaultSegmentationMode = TesseractOcrMaui.Enums.PageSegmentationMode.SingleChar;
                engine.SetCharacterWhitelist("123456789");
            };
        }

        public async Task<sbyte[,]> GetFromPathAsync(string imagePath)
        {
            Image<Bgr, byte> img = new(imagePath);
            Image<Gray, byte> gray = new(imagePath);
            Image<Gray, byte> blurred = new(imagePath);
            CvInvoke.CvtColor(img, gray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(gray, blurred, new Size(5, 5), 3);

            Image<Gray, byte> thresh = blurred.ThresholdAdaptive(
                new Gray(255), Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC, Emgu.CV.CvEnum.ThresholdType.Binary, 9, new Gray(2));
            CvInvoke.BitwiseNot(thresh, thresh);

            var contours = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(
                thresh,
                contours,
                null,
                Emgu.CV.CvEnum.RetrType.External,
                Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple
            );

            SortedList<double, VectorOfPoint> sorted = new(new DuplicateKeyComparer<double>());
            for (int i = 0; i < contours.Size; i++)
            {
                double area = CvInvoke.ContourArea(contours[i]);
                sorted.Add(area, contours[i]);
            }

            var sorted_arr = sorted.ToArray();

            VectorOfPoint puzzle_contour = null;

            for (int i = sorted_arr.Length - 1; i >= 0; i--)
            {
                var contour_pair = sorted_arr[i].Value;
                VectorOfPoint cont = contour_pair;
                var peri = CvInvoke.ArcLength(cont, true);
                VectorOfPoint approx = contour_pair;
                CvInvoke.ApproxPolyDP(cont, approx, 0.02 * peri, true);

                if (approx.Size == 4)
                {
                    puzzle_contour = approx;
                    break;
                }
            }

            if (puzzle_contour == null)
            {
                throw new ArgumentException("Board could not be found");
            }

            Image<Bgr, byte> img_cont = img.Clone();
            CvInvoke.DrawContours(img_cont, new VectorOfVectorOfPoint(puzzle_contour), 0, new MCvScalar(0, 0, 255), 5);

            var puzzle = FourPointTranform(img, puzzle_contour);
            var warped = FourPointTranform(gray, puzzle_contour);

            sbyte[,] out_arr = new sbyte[9, 9];
            int cell_size = warped.Width / 9;

            int y = 0;
            for (int i = 0; i < 9; i++)
            {
                int x = 0;
                string row = "";
                for (int j = 0; j < 9; j++)
                {
                    warped.ROI = new Rectangle(x, y, cell_size, cell_size);
                    var digit = ExtractDigit(warped, out var bw_cell);
#if DEBUG
                    //separatedCells[j, i] = ToSKBitmap(warped);
                    separatedCells[j, i] = ToSKBitmap(bw_cell);
#endif

                    out_arr[j, i] = digit;
                    row += $"{digit}, ";

                    x += cell_size;
                }

                y += cell_size;
            }

            warped.ROI = System.Drawing.Rectangle.Empty;

#if DEBUG
            originalImg = ToSKBitmap(img);
            bwImg = ToSKBitmap(thresh);
            conturedImg = ToSKBitmap(img_cont);
            skewedImg = ToSKBitmap(puzzle);
#endif

            return out_arr;
        }

        private SKBitmap ToSKBitmap<T1, T2>(Image<T1, T2> img) where T1 : struct, Emgu.CV.IColor where T2 : new()
        {
            string cacheDir = FileSystem.Current.CacheDirectory;
            var imagePath = Path.Combine(cacheDir, "cache1.jpg");
            img.Save(imagePath);
            SKBitmap bitmap = SKBitmap.Decode(imagePath);

            return bitmap;
        }

        private string ToByteArr<T1, T2>(Image<T1, T2> img) where T1 : struct, Emgu.CV.IColor where T2 : new()
        {
            string cacheDir = FileSystem.Current.CacheDirectory;
            var imagePath = Path.Combine(cacheDir, "cache2.jpg");
            img.Save(imagePath);
            return imagePath;
        }

        private Image<T1, T2> FourPointTranform<T1, T2>(Image<T1, T2> img, VectorOfPoint pts) where T1 : struct, Emgu.CV.IColor where T2 : new()
        {
            PointF[] rect = OrderPoints(pts);

            double widthA = Math.Sqrt(Math.Pow(rect[2].X - rect[3].X, 2)) + Math.Pow(rect[2].Y - rect[3].Y, 2);
            double widthB = Math.Sqrt(Math.Pow(rect[1].X - rect[0].X, 2)) + Math.Pow(rect[1].Y - rect[0].Y, 2);

            int maxWidth = (int)Math.Max(widthA, widthB);

            double heightA = Math.Sqrt(Math.Pow(rect[1].X - rect[2].X, 2) + Math.Pow(rect[1].Y - rect[2].Y, 2));
            double heightB = Math.Sqrt(Math.Pow(rect[0].X - rect[3].X, 2) + Math.Pow(rect[0].Y - rect[3].Y, 2));

            int maxHeight = (int)Math.Max(heightA, heightB);

            int side = Math.Max(maxHeight, maxWidth);

            PointF[] dest = new PointF[4]
            {
                new(0, 0),
                new(side - 1, 0),
                new(side - 1, side - 1),
                new(0, side - 1)
            };

            Mat M = CvInvoke.GetPerspectiveTransform(rect, dest);
            Image<T1, T2> out_img = new Image<T1, T2>(new Size(side, side));
            CvInvoke.WarpPerspective(img, out_img, M, new Size(side, side));
            return out_img;
        }

        private PointF[] OrderPoints(VectorOfPoint pts)
        {
            Point[] pts_arr = pts.ToArray();
            PointF[] rect = new PointF[4];

            var md = pts_arr.OrderBy((p) => p.X + p.Y);
            rect[0] = md.First();
            rect[2] = md.Last();

            md = pts_arr.OrderBy((p) => p.X - p.Y);
            rect[1] = md.Last();
            rect[3] = md.First();

            return rect;
        }

        private sbyte ExtractDigit<T1, T2>(Image<T1, T2> cell, out Image<T1, T2> bw_cell) where T1 : struct, Emgu.CV.IColor where T2 : new()
        {
            bw_cell = cell.Clone();
            var thresh = cell.Clone();
            CvInvoke.Threshold(cell, thresh, 0, 255, Emgu.CV.CvEnum.ThresholdType.Otsu);

            VectorOfVectorOfPoint contour = new();
            CvInvoke.FindContours(cell, contour, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            if (contour.Size == 0)
            {
                return 0;
            }

            Point[][] cnt_arr = contour.ToArrayOfArray();
            Point[] max_area = cnt_arr.MaxBy((cnt) => CvInvoke.ContourArea(new VectorOfPoint(cnt)));

            var mask = cell.Clone();
            mask.SetZero();
            CvInvoke.DrawContours(mask, new VectorOfVectorOfPoint(new VectorOfPoint(max_area)), -1, new MCvScalar(255), -1);
            var percentFilled = CvInvoke.CountNonZero(mask) / (double)(thresh.Width * thresh.Height);

            if (percentFilled < 0.03) // probably just noise
            {
                return 0;
            }

            Image<T1, T2> digit = cell.Clone();
            CvInvoke.BitwiseAnd(thresh, thresh, digit, mask);

            bw_cell = digit.Clone();
            var bitarr = ToByteArr(digit);

            RecognizionResult res = RecognizionResult.InProgress;
            try
            {
                res = tes.RecognizeText(bitarr);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            if (res.FinishedWithSuccess() && int.TryParse(res.RecognisedText, out int num) && num < 10 && num > 0 && res.Confidence > 0.60f)
                return (sbyte)num;
            else
                return 0;
        }
    }

    internal class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        #region IComparer<TKey> Members

        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1;
            else
                return result;
        }

        #endregion
    }
}
