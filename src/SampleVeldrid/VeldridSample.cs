using SpriteTextRenderer;
using System;
using System.IO;
using System.Numerics;
using Veldrid;
using Veldrid.StartupUtilities;

namespace Sample
{
    class VeldridSample
    {
        GraphicsDevice device;
        SpriteTextRenderer.Veldrid.SpriteRenderer sprite;

        public void Run()
        {
            var windowCI = new WindowCreateInfo()
            {
                X = 100,
                Y = 100,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowTitle = "Veldrid"
            };
            var window = VeldridStartup.CreateWindow(ref windowCI);
            device = VeldridStartup.CreateGraphicsDevice(window);
            
            var factory = device.ResourceFactory;
            
            sprite = new SpriteTextRenderer.Veldrid.SpriteRenderer(device);

            /*
            var sdxTexture = LoadTextureFromFile(device, "sdx.png");
            var srvTexture = new ShaderResourceView(device, sdxTexture);
            FrameMonitor frameMonitor = new FrameMonitor();
            form.Left += form.Width;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            */
            
            while (window.Exists)
            {
                window.PumpEvents();

                if (window.Exists)
                {
                    Draw();
                }
            }

            //form.Resize += form_Resize;
            //device.QueryInterface<SharpDX.DXGI.Device>().GetParent<SharpDX.DXGI.Adapter>().GetParent<SharpDX.DXGI.Factory>().MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);
            //Resize(form.ClientSize);

            /*
            sprite = new SpriteTextRenderer.Veldrid.SpriteRenderer(device);
            var textBlock = new SpriteTextRenderer.Veldrid.TextBlockRenderer(sprite, "Arial", FontWeight.Bold, SharpDX.DirectWrite.FontStyle.Normal, FontStretch.Normal, 16);
            
            var sdxTexture = LoadTextureFromFile(device, "sdx.png");
            var srvTexture = new ShaderResourceView(device, sdxTexture);
            FrameMonitor frameMonitor = new FrameMonitor();
            form.Left += form.Width;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            
            RenderLoop.Run(form, () =>
            {
                textBlock.DrawString("ABCDEFGHIJKLMNOPQRSTUVWXYZ" + Environment.NewLine + "abcdefghijklmnopqrstuvwxyz", Vector2.Zero, new Color4(1.0f, 1.0f, 0.0f,1.0f));
                textBlock.DrawString("SDX SpriteTextRenderer sample" + Environment.NewLine + "(using SharpDX)", new System.Drawing.RectangleF(0, 0, form.ClientSize.Width, form.ClientSize.Height),
                    SpriteTextRenderer.TextAlignment.Right | SpriteTextRenderer.TextAlignment.Bottom, new Color4(1.0f, 1.0f, 0.0f, 1.0f));

                textBlock.DrawString(frameMonitor.FPS.ToString("f2") + " FPS", new System.Drawing.RectangleF(0, 0, form.ClientSize.Width, form.ClientSize.Height),
                   SpriteTextRenderer.TextAlignment.Right | SpriteTextRenderer.TextAlignment.Top, new Color4(1.0f, 1.0f, 1.0f, 1.0f));
            });
            */

            DisposeResources();
        }

        private void Draw()
        {
            var dummyTexture = "fart";
            sprite.Draw(dummyTexture, new Vector2(100, 100), new Vector2(150, 150), new Vector2(75, 75), 0, CoordinateType.Absolute);
            //sprite.Draw(null, new Vector2(380, 320), new Vector2(150, 150), new Vector2(50, 50), new Degrees(-55 / 8), CoordinateType.Absolute);

            //sprite.Flush();

            device.SwapBuffers();
        }
        
        public static Texture LoadTextureFromFile(GraphicsDevice aDevice, string aFullPath)
        {
            Texture result = null;

            /*
            ImagingFactory fac = new ImagingFactory();

            BitmapDecoder bc = new SharpDX.WIC.BitmapDecoder(fac, aFullPath, DecodeOptions.CacheOnLoad);
            BitmapFrameDecode bfc = bc.GetFrame(0);
            FormatConverter fc = new FormatConverter(fac);
            System.Guid desiredFormat = PixelFormat.Format32bppBGRA;
            fc.Initialize(bfc, desiredFormat);

            float[] buffer = new float[fc.Size.Width * fc.Size.Height];

            bool canConvert = fc.CanConvert(bfc.PixelFormat, desiredFormat);

            fc.CopyPixels<float>(buffer);
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            float sizeOfPixel = PixelFormat.GetBitsPerPixel(desiredFormat) / 8;
            if (sizeOfPixel != 4.0f)
                throw new System.Exception("Unknown error");

            DataBox db = new DataBox(handle.AddrOfPinnedObject(), fc.Size.Width * (int)sizeOfPixel, fc.Size.Width * fc.Size.Height * (int)sizeOfPixel);
            
            int width = fc.Size.Width;
            int height = fc.Size.Height;

            Texture2DDescription fTextureDesc = new Texture2DDescription();
            fTextureDesc.CpuAccessFlags = CpuAccessFlags.None;
            fTextureDesc.Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            fTextureDesc.Width = width;
            fTextureDesc.Height = height;
            fTextureDesc.Usage = ResourceUsage.Default;
            fTextureDesc.MipLevels = 1;
            fTextureDesc.ArraySize = 1;
            fTextureDesc.OptionFlags = ResourceOptionFlags.None;
            fTextureDesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
            fTextureDesc.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);

            result = new Texture2D(aDevice,fTextureDesc, new DataBox[] { db });
            handle.Free();
            */
            return result;
        }
        
        private Shader LoadShader(ShaderStages stage)
        {
            string extension = null;
            switch (device.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                    extension = "hlsl.bytes";
                    break;
                case GraphicsBackend.Vulkan:
                    extension = "spv";
                    break;
                case GraphicsBackend.OpenGL:
                    extension = "glsl";
                    break;
                case GraphicsBackend.Metal:
                    extension = "metallib";
                    break;
                default: throw new System.InvalidOperationException();
            }

            string entryPoint = stage == ShaderStages.Vertex ? "VS" : "FS";
            string path = Path.Combine(System.AppContext.BaseDirectory, "Shaders", $"{stage.ToString()}.{extension}");
            byte[] shaderBytes = File.ReadAllBytes(path);
            return device.ResourceFactory.CreateShader(new ShaderDescription(stage, shaderBytes, entryPoint));
        }

        private void DisposeResources()
        {
            device.Dispose();
        }
    }
}
