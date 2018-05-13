using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.DirectWrite;
using SlimDX.Direct2D;
using SlimDX.DXGI;
using SlimDX.Direct3D10;
using SlimDX;
using System.Globalization;
using System.IO;
using System.Drawing;

namespace SpriteTextRenderer.SlimDX
{
    /// <summary>
    /// This class is responsible for rendering arbitrary text using SlimDX. Every TextRenderer is specialized for a specific font and relies on
    /// a SpriteRenderer for rendering the text.
    /// </summary>
    public class TextBlockRenderer : SpriteTextRenderer.TextBlockRenderer
    {
        private RenderTargetProperties rtp;

        /// <summary>
        /// Creates a new text renderer for a specific font.
        /// </summary>
        /// <param name="sprite">The sprite renderer that is used for rendering</param>
        /// <param name="fontName">Name of font. The font has to be installed on the system. 
        /// If no font can be found, a default one is used.</param>
        /// <param name="fontSize">Size in which to prerender the text. FontSize should be equal to render size for best results.</param>
        /// <param name="fontStretch">Font stretch parameter</param>
        /// <param name="fontStyle">Font style parameter</param>
        /// <param name="fontWeight">Font weight parameter</param>
        public TextBlockRenderer(SpriteRenderer sprite, String fontName, global::SlimDX.DirectWrite.FontWeight fontWeight,
            global::SlimDX.DirectWrite.FontStyle fontStyle, FontStretch fontStretch, float fontSize)
            : base(sprite, fontSize)
        {           
            System.Threading.Monitor.Enter(sprite.Device);
            try
            {
                rtp = new RenderTargetProperties()
                {
                    HorizontalDpi = 96,
                    VerticalDpi = 96,
                    Type = RenderTargetType.Default,
                    PixelFormat = new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied),
                    MinimumFeatureLevel = FeatureLevel.Direct3D10
                };

                font = ((global::SlimDX.DirectWrite.Factory)WriteFactory).CreateTextFormat(fontName, fontWeight, fontStyle, fontStretch, fontSize, CultureInfo.CurrentCulture.Name);
            }
            finally
            {
                System.Threading.Monitor.Exit(sprite.Device);
            }

            CreateCharTable(0);
        }

        // ### Public draw interface ###

        /// <summary>
        /// Draws the string in the specified coordinate system.
        /// </summary>
        /// <param name="text">The text to draw</param>
        /// <param name="position">A position in the chosen coordinate system where the top left corner of the first character will be</param>
        /// <param name="realFontSize">The real font size in the chosen coordinate system</param>
        /// <param name="color">The color in which to draw the text</param>
        /// <param name="coordinateType">The chosen coordinate system</param>
        /// <returns>The StringMetrics for the rendered text</returns>
        public StringMetrics DrawString(string text, Vector2 position, float realFontSize, Color4 color, CoordinateType coordinateType)
        {
            return base.DrawString(text, position.ToSTRVector(), realFontSize, color.ToSTRColor(), coordinateType);
        }

        /// <summary>
        /// Draws the string untransformed in absolute coordinate system.
        /// </summary>
        /// <param name="text">The text to draw</param>
        /// <param name="position">A position in absolute coordinates where the top left corner of the first character will be</param>
        /// <param name="color">The color in which to draw the text</param>
        /// <returns>The StringMetrics for the rendered text</returns>
        public StringMetrics DrawString(string text, Vector2 position, Color4 color)
        {
            return base.DrawString(text, position.ToSTRVector(), color.ToSTRColor());
        }

        /// <summary>
        /// Draws the string in the specified coordinate system aligned in the given rectangle. The text is not clipped or wrapped.
        /// </summary>
        /// <param name="text">The text to draw</param>
        /// <param name="rect">The rectangle in which to align the text</param>
        /// <param name="align">Alignment of text in rectangle</param>
        /// <param name="realFontSize">The real font size in the chosen coordinate system</param>
        /// <param name="color">The color in which to draw the text</param>
        /// <param name="coordinateType">The chosen coordinate system</param>
        /// <returns>The StringMetrics for the rendered text</returns>
        public StringMetrics DrawString(string text, RectangleF rect, TextAlignment align, float realFontSize, Color4 color, CoordinateType coordinateType)
        {
            return base.DrawString(text, rect, align, realFontSize, color.ToSTRColor(), coordinateType);
        }

        /// <summary>
        /// Draws the string unscaled in absolute coordinate system aligned in the given rectangle. The text is not clipped or wrapped.
        /// </summary>
        /// <param name="text">Text to draw</param>
        /// <param name="rect">A position in absolute coordinates where the top left corner of the first character will be</param>
        /// <param name="align">Alignment in rectangle</param>
        /// <param name="color">Color in which to draw the text</param>
        /// <returns>The StringMetrics for the rendered text</returns>
        public StringMetrics DrawString(string text, RectangleF rect, TextAlignment align, Color4 color)
        {
            return base.DrawString(text, rect, align, color.ToSTRColor());
        }

        // ### Template method hooks

        protected override STRLayout GetTextLayout(string s)
        {
            return new TextLayout((global::SlimDX.DirectWrite.Factory)WriteFactory, s, (TextFormat)font).ToSTRLayout();
        }

        protected override IDisposable CreateFontMapTexture(int width, int height, CharRenderCall[] drawCalls)
        {
            var TexDesc = new Texture2DDescription()
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                Height = height,
                Width = width,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.Shared,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            };

            var device10 = (global::SlimDX.Direct3D10_1.Device1)D3DDevice10;
            var texture = new Texture2D(device10, TexDesc);

            var rtv = new RenderTargetView(device10, texture);
            device10.ClearRenderTargetView(rtv, new Color4(0, 1, 1, 1));
            //device10.ClearRenderTargetView(rtv, new Color4(1, 0, 0, 0));
            Surface surface = texture.AsSurface();
            var target = RenderTarget.FromDXGI((global::SlimDX.Direct2D.Factory)D2DFactory, surface, rtp);
            var color = new SolidColorBrush(target, new Color4(1, 1, 1, 1));

            target.BeginDraw();

            foreach (var drawCall in drawCalls)
            {
                target.DrawTextLayout(drawCall.Position, (TextLayout)drawCall.TextLayout, color);
            }
         
            target.EndDraw();
            
            color.Dispose();
            
            //This is a workaround for Windows 8.1 machines. 
            //If these lines would not be present, the shared resource would be empty.
            //TODO: find a nicer solution
            using(var ms = new MemoryStream())
                Texture2D.ToStream(texture, ImageFileFormat.Bmp, ms);

            target.Dispose();
            surface.Dispose();
            rtv.Dispose();
            return texture;
        }

        protected override void CreateDeviceCompatibleTexture(int width, int height, IDisposable texture10, out IDisposable texture11, out IDisposable srv11)
        {
            var texture = (global::SlimDX.Direct3D10.Texture2D)texture10;
            var device11 = ((SpriteRenderer)Sprite).Device;

            lock (device11)
            {
                var dxgiResource = new global::SlimDX.DXGI.Resource(texture);

                global::SlimDX.Direct3D11.Texture2D tex11;
                if (PixCompatible)
                {
                    tex11 = new global::SlimDX.Direct3D11.Texture2D(device11, new global::SlimDX.Direct3D11.Texture2DDescription()
                    {
                        ArraySize = 1,
                        BindFlags = global::SlimDX.Direct3D11.BindFlags.ShaderResource | global::SlimDX.Direct3D11.BindFlags.RenderTarget,
                        CpuAccessFlags = global::SlimDX.Direct3D11.CpuAccessFlags.None,
                        Format = Format.R8G8B8A8_UNorm,
                        Height = height,
                        Width = width,
                        MipLevels = 1,
                        OptionFlags = global::SlimDX.Direct3D11.ResourceOptionFlags.Shared,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = global::SlimDX.Direct3D11.ResourceUsage.Default
                    });
                }
                else
                {
                    tex11 = device11.OpenSharedResource<global::SlimDX.Direct3D11.Texture2D>(dxgiResource.SharedHandle);
                }
                srv11 = new global::SlimDX.Direct3D11.ShaderResourceView(device11, tex11);
                texture11 = tex11;
                dxgiResource.Dispose(); 
            }            
        }

        protected override DeviceDescriptor CreateDevicesAndFactories()
        {
            DeviceDescriptor desc = new DeviceDescriptor();
            desc.D3DDevice10 = new global::SlimDX.Direct3D10_1.Device1(DeviceCreationFlags.BgraSupport, global::SlimDX.Direct3D10_1.FeatureLevel.Level_10_0);
            desc.WriteFactory = new global::SlimDX.DirectWrite.Factory(global::SlimDX.DirectWrite.FactoryType.Shared);
            desc.D2DFactory = new global::SlimDX.Direct2D.Factory(global::SlimDX.Direct2D.FactoryType.SingleThreaded);
            return desc;
        }
    }
}
