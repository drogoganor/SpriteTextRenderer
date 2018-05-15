using System;
using System.Numerics;
using Veldrid;

namespace SpriteTextRenderer.Veldrid
{
    public struct VertexPositionColor
    {
        public const uint SizeInBytes = 24;
        public Vector2 Position;
        public RgbaFloat Color;
        public VertexPositionColor(Vector2 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
    }

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
        public static VertexElementDescription ToVeldridInputElement(this STRInputElement element)
        {
            VertexElementFormat format;
            switch (element.Format)
            {
                case STRFormat.B8G8R8A8_UNorm:
                    format = VertexElementFormat.Float2;
                    break;
                case STRFormat.R32G32_Float:
                    format = VertexElementFormat.Float4;
                    break;
                default:
                    throw new NotImplementedException("The input element format " + element + " cannot be translated to a Veldrid format");
            }
            return new VertexElementDescription(element.Semantic, VertexElementSemantic.Position, VertexElementFormat.Float2);
        }

        /// <summary>
        /// Converts a Veldrid Viewport to a library-independent viewport.
        /// </summary>
        /// <param name="vp">The Veldrid viewport</param>
        /// <returns>The library-independent viewport</returns>
        public static STRViewport ToSTRViewport(this Viewport vp)
        {
            return new STRViewport() { Width = vp.Width, Height = vp.Height };
        }

        /// <summary>
        /// Converts a Veldrid vector to a library-independent vector
        /// </summary>
        /// <param name="v">The Veldrid vector</param>
        /// <returns>The library-independent vector</returns>
        public static STRVector ToSTRVector(this Vector2 v)
        {
            return new STRVector(v.X, v.Y);
        }

        /// <summary>
        /// Converts a library-independent vector to a Veldrid vector.
        /// </summary>
        /// <param name="v">The library-independent vector</param>
        /// <returns>The Veldrid vector</returns>
        public static Vector2 ToVector(this STRVector v)
        {
            return new Vector2(v.X, v.Y);
        }

        /// <summary>
        /// Converts a Veldrid color to a library-independent color.
        /// </summary>
        /// <param name="c">The Veldrid color</param>
        /// <returns>The library-independent color</returns>
        public static STRColor ToSTRColor(this RgbaFloat c)
        {
            return new STRColor(c.A, c.R, c.G, c.B);
        }

        /*
        /// <summary>
        /// Converts a Veldrid TextLayout to a library-independent layout.
        /// </summary>
        /// <param name="layout">The Veldrid layout</param>
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
        */
    }
}
