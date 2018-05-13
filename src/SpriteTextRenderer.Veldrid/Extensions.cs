using SharpDX.Direct3D11;
using System;
using Veldrid;

namespace SpriteTextRenderer.Veldrid
{
    /// <summary>
    /// Provides extension methods to convert Veldrid-specific types to library-independent types and vice versa.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts a library-independent input element to a Veldrid InputElement.
        /// </summary>
        /// <param name="element">The library-independent input element to convert</param>
        /// <returns>The Veldrid InputElement</returns>
        public static InputLayout ToSlimDXInputElement(this STRInputElement element)
        {
            PixelFormat format;
            switch (element.Format)
            {
                case STRFormat.B8G8R8A8_UNorm:
                    format = PixelFormat.B8_G8_R8_A8_UNorm;
                    break;
                case STRFormat.R32G32_Float:
                    format = PixelFormat.R32_G32_Float;
                    break;
                default:
                    throw new NotImplementedException("The input element format " + element + " cannot be translated to a Veldrid format");
            }
            return new VertexLayoutDescription(element.Semantic, 0, format, element.Offset, 0);
        }

        /// <summary>
        /// Converts a SlimDX Viewport to a library-independent viewport.
        /// </summary>
        /// <param name="vp">The SlimDX viewport</param>
        /// <returns>The library-independent viewport</returns>
        public static STRViewport ToSTRViewport(this Viewport vp)
        {
            return new STRViewport() { Width = vp.Width, Height = vp.Height };
        }

        /// <summary>
        /// Converts a SlimDX vector to a library-independent vector
        /// </summary>
        /// <param name="v">The SlimDX vector</param>
        /// <returns>The library-independent vector</returns>
        public static STRVector ToSTRVector(this Vector2 v)
        {
            return new STRVector(v.X, v.Y);
        }

        /// <summary>
        /// Converts a library-independent vector to a SlimDX vector.
        /// </summary>
        /// <param name="v">The library-independent vector</param>
        /// <returns>The SlimDX vector</returns>
        public static Vector2 ToVector(this STRVector v)
        {
            return new Vector2(v.X, v.Y);
        }

        /// <summary>
        /// Converts a SlimDX color to a library-independent color.
        /// </summary>
        /// <param name="c">The SlimDX color</param>
        /// <returns>The library-independent color</returns>
        public static STRColor ToSTRColor(this Color4 c)
        {
            return new STRColor(c.Alpha, c.Red, c.Green, c.Blue);
        }

        /// <summary>
        /// Converts a SlimDX TextLayout to a library-independent layout.
        /// </summary>
        /// <param name="layout">The SlimDX layout</param>
        /// <returns>The library-independent layout</returns>
        public static STRLayout ToSTRLayout(this TextLayout layout)
        {
            return new STRLayout()
            {
                TopLeft = new STRVector(layout.Metrics.Left, layout.Metrics.Top),
                LayoutSize = new STRVector(layout.Metrics.LayoutWidth, layout.Metrics.LayoutHeight),
                Size = new STRVector(layout.Metrics.Width, layout.Metrics.Height),
                OverhangLeft = layout.OverhangMetrics.Left,
                OverhangRight = layout.OverhangMetrics.Right,
                OverhangBottom = layout.OverhangMetrics.Bottom,
                OverhangTop = layout.OverhangMetrics.Top,
                WidthIncludingTrailingWhitespaces = layout.Metrics.WidthIncludingTrailingWhitespace,
                TextLayout = layout
            };
        }
    }
}
