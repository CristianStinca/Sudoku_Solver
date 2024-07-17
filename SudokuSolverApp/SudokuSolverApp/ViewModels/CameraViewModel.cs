using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Maui.Graphics.Platform;
using System.Reflection;
using IImage = Microsoft.Maui.Graphics.IImage;
using System.IO;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using Plugin.Maui.OCR;
using SkiaSharp;
using static SkiaSharp.SKImageFilter;
using Microsoft.Maui.ApplicationModel;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using System.Collections;
using Size = System.Drawing.Size;
using Point = System.Drawing.Point;
using PointF = System.Drawing.PointF;
using VectorOfPoint = Emgu.CV.Util.VectorOfPoint;
using VectorOfVectorOfPoint = Emgu.CV.Util.VectorOfVectorOfPoint;
using Emgu.Util;
using Emgu.CV.Text;
using Emgu.CV.Platform.Maui;
using TesseractOcrMaui;
using TesseractOcrMaui.Results;

namespace SudokuSolverApp.ViewModels
{
    public partial class CameraViewModel : ObservableObject
    {
        public CancellationToken Token => CancellationToken.None;
        public readonly string imagePath;
        public readonly string cropPath;
        public readonly string outPath;

        public SKBitmap originalImg;
        public SKBitmap bwImg;
        public SKBitmap contouredImg;
        public SKBitmap deskewedImg;
        public ITesseract tes;
        public SKBitmap[,] num_arr = new SKBitmap[9, 9];

        public CameraViewModel() 
        {
            string cacheDir = FileSystem.Current.CacheDirectory;
            imagePath = Path.Combine(cacheDir, "camera-view-image.jpg");
            cropPath = Path.Combine(cacheDir, "camera-view-11.jpg");
            outPath = Path.Combine(cacheDir, "camera-view-out.jpg");

            MauiInvoke.Init();
            OcrPlugin.Default.InitAsync();
            //tes = new Emgu.CV.OCR.Tesseract("tessdata", "eng", OcrEngineMode.TesseractOnly, "0123456789");
            //tes = new Emgu.CV.OCR.Tesseract();
        }

        public async Task<SKBitmap> SaveImage(Stream source_stream)
        {
            using var localFileStream = File.Create(imagePath);
            source_stream.CopyTo(localFileStream);
            localFileStream.Dispose();
            source_stream.Dispose();

            //SKRect cropRect = new SKRect(0, 0, 2000, 2000);

            try
            {
                //CropAndSaveImage(imagePath, cropPath);
                return await DetectImage(imagePath);
                //return File.OpenRead(imagePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"_______________{ex}");
                return null;
            }
        }

        public async Task<SKBitmap> GetFromMedia()
        {
            try
            {
                //CropAndSaveImage(imagePath, cropPath);
                return await DetectImage(imagePath);
                //return File.OpenRead(imagePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"_______________{ex}");
                return null;
            }

        }

        private async Task<SKBitmap> DetectImage(string inputPath)
        {
//#if ANDROID
            string cachePath = Path.Combine(FileSystem.Current.CacheDirectory, "camera-view-cache.jpg");
            //var img_src = ImageSource.FromFile("dotnet_bot.png");
            //using var inputStream = File.OpenRead(inputPath);
            //var originalBitmap = SKBitmap.Decode(inputStream);

            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Pick a photo"
            });

            if (result == null)
            {
                return null; // shall never happen
            }

            var stream = await result.OpenReadAsync();
            using var localFileStream = File.Create(cachePath);
            stream.CopyTo(localFileStream);

            stream.Seek(0, SeekOrigin.Begin);
            originalImg = SKBitmap.Decode(stream);

            localFileStream.Dispose();
            stream.Dispose();

            Image<Bgr, byte> img = new(cachePath);
            Image<Gray, byte> gray = new(cachePath);
            Image<Gray, byte> blurred = new(cachePath);
            CvInvoke.CvtColor(img, gray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(gray, blurred, new Size(5, 5), 3);

            Image<Gray, byte> thresh = blurred.ThresholdAdaptive(
                new Gray(255), Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC, Emgu.CV.CvEnum.ThresholdType.Binary, 9, new Gray(2));
            CvInvoke.BitwiseNot(thresh, thresh);

            //Image<Gray, byte> img_bw = img.Convert<Gray, byte>().ThresholdAdaptive(
            //new Gray(255), Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC, Emgu.CV.CvEnum.ThresholdType.Binary, 11, new Gray(2));
            //.ThresholdBinary(new Gray(100), new Gray(255));
            //CvInvoke.FastNlMeansDenoising(img_bw, img_bw);
            //var blur = img_bw.SmoothGaussian(7);

            //bwImg = ToSKBitmap(img_bw);
            bwImg = ToSKBitmap(thresh);

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
                //Debug.WriteLine($"___INDEX: {i}");
            }

            //sorted = (SortedList<double, VectorOfPoint>)sorted.Reverse();
            var sorted_arr = sorted.ToArray();

            VectorOfPoint puzzle_contour = null;

            //foreach (var contour_pair in sorted)
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

            contouredImg = ToSKBitmap(img_cont);
            SKBitmap skBitmap = ToSKBitmap(puzzle);

            deskewedImg = skBitmap;

            sbyte[,] out_arr = new sbyte[9, 9];
            int cell_size = warped.Width / 9;
            //running the array

            int y = 0;
            for (int i = 0; i < 9; i++)
            {
                int x = 0;
                string row = "";
                for (int j = 0; j < 9; j++)
                {
                    warped.ROI = new Rectangle(x, y, cell_size, cell_size);
                    var digit = await ExtractDigit(warped);

                    out_arr[i, j] = digit;
                    row += $"{digit}, ";

                    x += cell_size;
                }
                Debug.WriteLine($"___ {row}");
                
                y += cell_size;
            }

            return skBitmap;
//#else
//            return null;
//#endif
        }

        private void CropAndSaveImage(string inputPath, string outputPath)
        {
            // Load the image
            using var inputStream = File.OpenRead(inputPath);
            using var originalBitmap = SKBitmap.Decode(inputStream);

            //float min_side = Math.Min(originalBitmap.Width, originalBitmap.Height);

            float au = originalBitmap.Height / 12f;
            float width_mid = originalBitmap.Width / 2f;
            float clip_rect_side = au * 10f;

            SKRect cropRect = new SKRect(
                width_mid - (au * 5f),
                au,
                width_mid + (au * 5f),
                au + clip_rect_side
            );

            // Create the cropped bitmap
            var croppedBitmap = new SKBitmap((int)cropRect.Width, (int)cropRect.Height);

            // Draw the cropped area onto the new bitmap
            using (var canvas = new SKCanvas(croppedBitmap))
            {
                var destRect = new SKRect(0, 0, cropRect.Width, cropRect.Height);
                //canvas.Translate(cropRect.Width / 2, cropRect.Height / 2);
                //canvas.RotateDegrees(90);
                //canvas.Translate(-cropRect.Width / 2, -cropRect.Height / 2);
                canvas.Translate(croppedBitmap.Width, 0);
                canvas.RotateDegrees(90);
                canvas.DrawBitmap(originalBitmap, cropRect, destRect);
            }

            // Save the cropped bitmap to a file
            using var image = SKImage.FromBitmap(croppedBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
            using var stream = File.OpenWrite(outputPath);
            data.SaveTo(stream);
        }

        public SKBitmap ToSKBitmap<T1, T2>(Image<T1, T2> img) where T1 : struct, Emgu.CV.IColor where T2 : new()
        {
            string cacheDir = FileSystem.Current.CacheDirectory;
            var imagePath = Path.Combine(cacheDir, "cache1.jpg");
            img.Save(imagePath);
            SKBitmap bitmap = SKBitmap.Decode(imagePath);

            return bitmap;
        }

        public async Task<string> ToByteArr<T1, T2>(Image<T1, T2> img) where T1 : struct, Emgu.CV.IColor where T2 : new()
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

        private async Task<sbyte> ExtractDigit<T1, T2>(Image<T1, T2> cell) where T1 : struct, Emgu.CV.IColor where T2 : new()
        {
            var thresh = cell.Clone();
            CvInvoke.Threshold(cell, thresh, 0, 255, Emgu.CV.CvEnum.ThresholdType.Otsu);

            VectorOfVectorOfPoint contour = new();
            CvInvoke.FindContours(cell, contour, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            if (contour.Size == 0)
            {
                return -1;
            }

            Point[][] cnt_arr = contour.ToArrayOfArray();
            Point[] max_area = cnt_arr.MaxBy((cnt) => CvInvoke.ContourArea(new VectorOfPoint(cnt)));

            var mask = cell.Clone();
            mask.SetZero();
            CvInvoke.DrawContours(mask, new VectorOfVectorOfPoint(new VectorOfPoint(max_area)), -1, new MCvScalar(255), -1);
            var percentFilled = CvInvoke.CountNonZero(mask) / (double)(thresh.Width * thresh.Height);

            if (percentFilled < 0.03) // probably just noise
            {
                return -1;
            }

            Image<T1, T2> digit = cell.Clone();
            //using var tes = new Tesseract();
            CvInvoke.BitwiseAnd(thresh, thresh, digit, mask);

            //var bitarr = ToSKBitmap(digit).Bytes;
            var bitarr = await ToByteArr(digit);
            var datares = await tes.LoadTraineddataAsync();
            tes.EngineConfiguration = (engine) =>
            {
                engine.DefaultSegmentationMode = TesseractOcrMaui.Enums.PageSegmentationMode.SingleChar;
                engine.SetCharacterWhitelist("0123456789");
            };

            //OcrResult res = new();
            RecognizionResult res = RecognizionResult.InProgress;
            try
            {
                //res = await OcrPlugin.Default.RecognizeTextAsync(bitarr, true);
                res = tes.RecognizeText(bitarr);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            //return digit;

            //var tes = new Tesseract();

            //string number_s = "-1";
            //tes.Init("tessdata", "eng", OcrEngineMode.TesseractOnly);
            //tes.PageSegMode = Emgu.CV.OCR.PageSegMode.SingleChar;
            //tes.SetImage(digit);
            //tes.Recognize();
            //number_s = tes.GetOsdText();

            //if (res.Success && int.TryParse(res.AllText, out int num))
            //    return (sbyte)num;
            //else
            //    return -1;

            if (res.FinishedWithSuccess() && int.TryParse(res.RecognisedText, out int num))
                return (sbyte)num;
            else
                return -1;
        }
    }

    public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        #region IComparer<TKey> Members

        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1; // Handle equality as being greater. Note: this will break Remove(key) or
            else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
                return result;
        }

        #endregion
    }
}
