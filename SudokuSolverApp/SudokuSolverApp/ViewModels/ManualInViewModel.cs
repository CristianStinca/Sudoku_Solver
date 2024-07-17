using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SudokuLogicLibr.SudokuLogic;
using SudokuSolverApp.Models;
using SudokuSolverApp.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TesseractOcrMaui;

namespace SudokuSolverApp.ViewModels
{
    public partial class ManualInViewModel : ObservableObject
    {
        public event MatrixCalculatedHandler MatrixCalculated;
        public delegate void MatrixCalculatedHandler();

        public event MatrixFailedReadImgHandler MatrixFailedReadImg;
        public delegate void MatrixFailedReadImgHandler();
        
        public event MatrixFailedCalculatedHandler MatrixFailedCalculated;
        public delegate void MatrixFailedCalculatedHandler();
        
        public event MatrixFinishedHandler MatrixFinished;
        public delegate void MatrixFinishedHandler();

        public event MatrixChangedHandler MatrixChanged;
        public delegate void MatrixChangedHandler(object obj, EventArgs e, int i, int j);

        [ObservableProperty]
        int[,] matrix = new int[9, 9];

        int[,] temp_matrix = new int[9, 9];

        [ObservableProperty]
        string text;

        public byte? number;

        ITesseract tesseract = null;
        ImageToSudokuService imageToSudokuService = null;

        public ManualInViewModel()
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = 0;
                }
            }

            MatrixCalculated += OnMatrixCalculated;
        }

        public void OnMatrixCalculated()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Matrix = temp_matrix;

                this.OnPropertyChanged("matrix");
            });
        }

        public void BoardClicked(int i, int j)
        {
            if (number is null) return;
            if (number < 1 || number > 9) return;

            Matrix[i, j] = (byte)number;
            //this.OnPropertyChanged(new PropertyChangedEventArgs("matrix"));
            this.MatrixChanged?.Invoke(this, new PropertyChangedEventArgs("matrix"), i, j);
        }

        [RelayCommand]
        async Task Solve()
        {
            SudokuBoard sdkMat = new SudokuBoard(Matrix);

            try
            {
                int[,] arr = sdkMat.Solve();
                Text = sdkMat.ToString();

                await Shell.Current.GoToAsync(nameof(ResultPage),
                    new Dictionary<string, object>
                        {
                            {"Solved", arr},
                            {"Raw", Matrix}
                        });
            }
            catch (SudokuCannotBeSolvedException)
            {
                Text = "Sudoku cannot be solved";
            }
        }

        [RelayCommand]
        void TextTask()
        {
            Console.WriteLine(Text);
        }

        public void PullFromMemory()
        {
            var t = Task.Run(async () =>
            {
                string cachePath = Path.Combine(FileSystem.Current.CacheDirectory, "camera-view-cache.jpg");
                FileResult result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Pick a sudoku photo"
                });

                if (result == null)
                {
                    MatrixFailedReadImg?.Invoke();
                    //WeakReferenceMessenger.Default.Send("DOWNLOAD FAILED");
                }

                var stream = await result.OpenReadAsync();
                using var localFileStream = File.Create(cachePath);
                stream.CopyTo(localFileStream);

                localFileStream.Dispose();
                stream.Dispose();

                sbyte[,] arr = null;
                try
                {
                    arr = await imageToSudokuService.GetFromPathAsync(cachePath);
                    //this.temp_matrix = arr;
                }
                catch (ArgumentException ex)
                {
                    MatrixFailedCalculated?.Invoke();
                    //WeakReferenceMessenger.Default.Send("READING FAILED");
                    //Debug.WriteLine(ex.Message);
                    //return false;
                }

                //WeakReferenceMessenger.Default.Send("TASK FINISHED");

                if (arr != null)
                {
                    for (int i = 0; i < arr.GetLength(0); i++)
                    {
                        for (int j = 0; j < arr.GetLength(1); j++)
                        {
                            temp_matrix[i, j] = (int)arr[i, j];
                            //Debug.WriteLine($"current ind: i = {i}, j = {j}");
                        }
                    }
                }

                MatrixCalculated?.Invoke();

                //Debug.WriteLine(Matrix.ToString());
            });

            t.ContinueWith(end =>
            {
                MatrixFinished?.Invoke();
            });
        }

        public void ClearMatrix()
        {
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    Matrix[i, j] = 0;
                }
            }

            this.OnPropertyChanged("matrix");
        }

        public void SetTesseract(ITesseract tesseract)
        {
            this.tesseract = tesseract;
            this.imageToSudokuService = new ImageToSudokuService(tesseract);
        }
    }
}