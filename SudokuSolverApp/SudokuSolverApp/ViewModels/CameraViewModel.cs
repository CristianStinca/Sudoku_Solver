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
using IronOcr;

namespace SudokuSolverApp.ViewModels
{
    public partial class CameraViewModel : ObservableObject
    {
        public CancellationToken Token => CancellationToken.None;
        public readonly string imagePath;

        public CameraViewModel() 
        {
            string cacheDir = FileSystem.Current.CacheDirectory;
            imagePath = Path.Combine(cacheDir, "camera-view-image.jpg");

            IronOcr.License.LicenseKey = "IRONOCR.MYLICENSE.KEY.1EF01";
        }

        public void SaveImage(Stream source_stream)
        {
            using var localFileStream = File.Create(imagePath);
            source_stream.CopyTo(localFileStream);

            ScanOCR();

            //IImage image = PlatformImage.FromStream(source_stream);

            //var asd = (float)DeviceDisplay.Current.MainDisplayInfo.Width;
            //float au = asd / 12f;
            //float height_mid = asd / 2f;
            //float clip_rect_side = au * 10f;

            //var imageAsBytes = new byte[source_stream.Length];
            //await source_stream.ReadAsync(imageAsBytes);

            //RectangleF crop_rect_f = new RectangleF(
            //    au,
            //    height_mid - (au * 5f),
            //    clip_rect_side,
            //    clip_rect_side
            //);

            //Rectangle crop_rect = Rectangle.Truncate(crop_rect_f);

            //var ocr = new IronTesseract();
            //using (var ocrInput = new OcrInput())
            //{
            //    ocrInput.LoadImage(imageAsBytes, crop_rect);

            //    // Optionally Apply Filters if needed:
            //    // ocrInput.Deskew();  // use only if image not straight
            //    // ocrInput.DeNoise(); // use only if image contains digital noise

            //    var ocrResult = ocr.Read(ocrInput);
            //    Debug.WriteLine($"____________________________Res: {ocrResult.Text}");
            //}

            ////if (image != null)
            ////{
            ////    IImage newImage = image.Resize(image.Width, image.Width, ResizeMode.Bleed, false);
            ////    RectangleF crop_rect = new RectangleF(
            ////        au,
            ////        height_mid - (au * 5f),
            ////        clip_rect_side,
            ////        clip_rect_side
            ////    );

            ////    using (MemoryStream memStream = new MemoryStream())
            ////    {
            ////        newImage.Save(memStream);
            ////        // Reset destination stream position to 0 if saving to a file
            ////        memStream.Seek(0, SeekOrigin.Begin);
            ////        using var localFileStream = File.Create(imagePath);
            ////        memStream.CopyTo(localFileStream);
            ////    }
            ////}

            ////Debug.WriteLine($"___________Image saved to {imagePath}");
        }

        private async void ScanOCR()
        {
            var images = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Pick image",
                FileTypes = FilePickerFileType.Images
            });

            var path = images.FullPath.ToString();
            var ocr = new IronTesseract();
            using (var input = new OcrInput())
            {
                input.LoadImage(path);
                OcrResult result = ocr.Read(input);
                string text = result.Text;
                Debug.WriteLine($"_____________________RESULT: {text}");
            }
        }
    }
}
