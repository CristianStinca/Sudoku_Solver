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
using System.Security.Cryptography.X509Certificates;
using TesseractOcrMaui;

namespace SudokuSolverApp.ViewModels
{
    [QueryProperty("CameraImagePath", "ImagePath")]
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
        
        public event LeftToCameraPageHandler LeftToCameraPage;
        public delegate void LeftToCameraPageHandler();
        
        public event RemoveFocusHandler RemoveFocus;
        public delegate void RemoveFocusHandler();

        public event MatrixChangedHandler MatrixChanged;
        public delegate void MatrixChangedHandler(object obj, EventArgs e, int i, int j);

        [ObservableProperty]
        int[,] matrix = new int[9, 9];

        int[,] temp_matrix = new int[9, 9];

        [ObservableProperty]
        string text;

        [ObservableProperty]
        string cameraImagePath;

        //public byte? number;
        public class Cell
        {
            public Cell(int i, int j)
            {
                this.i = i;
                this.j = j;
            }

            public int i;
            public int j;
        }

        public Cell selectedCell = null;

        ITesseract tesseract = null;
        ImageToSudokuService imageToSudokuService = null;

        Stack<int[,]> previousStates = new();
        Stack<int[,]> forwardStates = new();

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

        private int[,] GetPresentIntArrClone()
        {
            int[,] clone = new int[9, 9];
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    clone[i, j] = Matrix[i, j];
                }
            }

            return clone;
        }

        private void RegisterMatrixChange()
        {
            previousStates.Push(GetPresentIntArrClone());
        }

        [RelayCommand]
        void OnBack()
        {
            if (previousStates.TryPop(out var forw))
            {
                forwardStates.Push(GetPresentIntArrClone());
                Matrix = forw;
                this.OnPropertyChanged("matrix");
                RemoveFocus.Invoke();
            }

        }

        [RelayCommand]
        void OnForward()
        {
            if (forwardStates.TryPop(out var prev))
            {
                previousStates.Push(GetPresentIntArrClone());
                Matrix = prev;
                this.OnPropertyChanged("matrix");
                RemoveFocus.Invoke();
            }

        }

        public void OnMatrixCalculated()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                RegisterMatrixChange();

                Matrix = temp_matrix;

                this.OnPropertyChanged("matrix");
            });
        }

        public void BoardClicked(int i, int j)
        {
            selectedCell = new(i, j);
        }

        public void NumberClicked(int n)
        {
            if (selectedCell == null) return;
            if (n < 0 || n > 9) return;
            RegisterMatrixChange();

            int i = selectedCell.i, j = selectedCell.j;
            Matrix[i, j] = (byte)n;

            this.MatrixChanged?.Invoke(this, new PropertyChangedEventArgs("matrix"), i, j);
        }

        [RelayCommand]
        void CleanCell()
        {
            if (selectedCell == null) return;
            RegisterMatrixChange();

            Matrix[selectedCell.i, selectedCell.j] = 0;
            this.MatrixChanged?.Invoke(this, new PropertyChangedEventArgs("matrix"), selectedCell.i, selectedCell.j);
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

        [RelayCommand]
        async Task OpenCamera()
        {
            LeftToCameraPage.Invoke();
            await Shell.Current.GoToAsync(nameof(CameraPage));
        }

        public void PullFromMemory()
        {
            Task.Run(async () =>
            {
                string cachePath = Path.Combine(FileSystem.Current.CacheDirectory, "camera-view-cache.jpg");
                FileResult result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Pick a sudoku photo"
                });

                if (result == null)
                {
                    MatrixFailedReadImg?.Invoke();
                }

                var stream = await result.OpenReadAsync();
                using var localFileStream = File.Create(cachePath);
                stream.CopyTo(localFileStream);

                localFileStream.Dispose();
                stream.Dispose();

                SetMatrixFromPathOCR(cachePath);
            });
        }

        public void PullFromCamera()
        {
            SetMatrixFromPathOCR(CameraImagePath);
        }

        private void SetMatrixFromPathOCR(string cachePath)
        {
            var t = Task.Run(async () =>
            {
                int[,] arr = null;
                try
                {
                    arr = await imageToSudokuService.GetFromPathAsync(cachePath);
                }
                catch (ArgumentException)
                {
                    MatrixFailedCalculated?.Invoke();
                }

                temp_matrix = arr;

                MatrixCalculated?.Invoke();
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