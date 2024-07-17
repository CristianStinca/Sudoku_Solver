using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        public event MatrixChangedHandler MatrixChanged;
        public delegate void MatrixChangedHandler(object obj, EventArgs e, int i, int j);

        [ObservableProperty]
        int[,] matrix = new int[9, 9];

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

        public async Task<bool> PullFromMemory()
        {
            string cachePath = Path.Combine(FileSystem.Current.CacheDirectory, "camera-view-cache.jpg");

            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Pick a sudoku photo"
            });

            if (result == null)
            {
                return false; // shall never happen
            }

            var stream = await result.OpenReadAsync();
            using var localFileStream = File.Create(cachePath);
            stream.CopyTo(localFileStream);

            localFileStream.Dispose();
            stream.Dispose();

            sbyte[,] arr;
            try
            {
                arr = await imageToSudokuService.GetFromPathAsync(cachePath);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            //for (int i = 0; i < arr.GetLength(0); i++)
            //{
            //    for (int j = 0; j < arr.GetLength(1); j++)
            //    {
            //        Matrix[i, j] = (int)arr[i, j];
            //        //Debug.WriteLine($"current ind: i = {i}, j = {j}");
            //    }
            //}

            //Debug.WriteLine(Matrix.ToString());
            return true;
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