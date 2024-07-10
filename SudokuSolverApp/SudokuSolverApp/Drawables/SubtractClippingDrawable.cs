using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolverApp.Drawables
{
    internal class SubtractClippingDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            float abstract_unit = dirtyRect.Width / 12f;
            float clip_rect_side = abstract_unit * 10f;

            float stroke_thikness = 4f;
            float stroke_side = stroke_thikness / 2f;

            float width_mid = dirtyRect.Width / 2f;
            float height_mid = dirtyRect.Height / 2f;

            float x0 = width_mid - (clip_rect_side / 2f);
            float y0 = height_mid - (clip_rect_side / 2f);
            float x1 = width_mid - (clip_rect_side / 6f);
            float x2 = width_mid + (clip_rect_side / 6f);
            float y1 = height_mid - (clip_rect_side / 6f);
            float y2 = height_mid + (clip_rect_side / 6f);

            float sqare_side = (clip_rect_side / 3f) - stroke_side;

            canvas.SubtractFromClip(x0, y0, sqare_side, sqare_side);
            canvas.SubtractFromClip(x1, y0, sqare_side, sqare_side);
            canvas.SubtractFromClip(x2, y0, sqare_side, sqare_side);
            canvas.SubtractFromClip(x0, y1, sqare_side, sqare_side);
            canvas.SubtractFromClip(x1, y1, sqare_side, sqare_side);
            canvas.SubtractFromClip(x2, y1, sqare_side, sqare_side);
            canvas.SubtractFromClip(x0, y2, sqare_side, sqare_side);
            canvas.SubtractFromClip(x1, y2, sqare_side, sqare_side);
            canvas.SubtractFromClip(x2, y2, sqare_side, sqare_side);

            canvas.FillColor = (Color)App.Current.Resources["Black"];

            canvas.FillRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);
        }
    }
}
