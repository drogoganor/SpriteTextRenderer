using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sample
{
    class VeldridSample
    {
        Veldrid.Direct3D11.Device device;
        SwapChain swapChain;
        RenderTargetView renderView;
        SpriteTextRenderer.Veldrid.SpriteRenderer sprite;
        public void Run()
        {
            var form = new RenderForm(Helpers.GetFormTitle("Veldrid"));
            form.Resize += form_Resize;
            
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };
            
            FeatureLevel[] levels = {
                            FeatureLevel.Level_11_0,
                            FeatureLevel.Level_10_1,
                            FeatureLevel.Level_10_0
                        };
            Veldrid.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware,
                Veldrid.Direct3D11.DeviceCreationFlags.None, levels, desc, out device, out swapChain);
            
            device.QueryInterface<Veldrid.DXGI.Device>().GetParent<Veldrid.DXGI.Adapter>().GetParent<Veldrid.DXGI.Factory>().MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            Resize(form.ClientSize);

            sprite = new SpriteTextRenderer.Veldrid.SpriteRenderer(device);
            var textBlock = new SpriteTextRenderer.Veldrid.TextBlockRenderer(sprite, "Arial", FontWeight.Bold, Veldrid.DirectWrite.FontStyle.Normal, FontStretch.Normal, 16);

            var sdxTexture = LoadTextureFromFile(device, "sdx.png");
            var srvTexture = new ShaderResourceView(device, sdxTexture);
            FrameMonitor frameMonitor = new FrameMonitor();
            form.Left += form.Width;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            RenderLoop.Run(form, () =>
            {
                double totalMillisec = stopWatch.Elapsed.TotalMilliseconds;
                frameMonitor.Tick();
                device.ImmediateContext.ClearRenderTargetView(renderView, Color.DarkBlue);
                textBlock.DrawString("ABCDEFGHIJKLMNOPQRSTUVWXYZ" + Environment.NewLine + "abcdefghijklmnopqrstuvwxyz", Vector2.Zero, new Color4(1.0f, 1.0f, 0.0f,1.0f));
                textBlock.DrawString("SDX SpriteTextRenderer sample" + Environment.NewLine + "(using Veldrid)", new System.Drawing.RectangleF(0, 0, form.ClientSize.Width, form.ClientSize.Height),
                    SpriteTextRenderer.TextAlignment.Right | SpriteTextRenderer.TextAlignment.Bottom, new Color4(1.0f, 1.0f, 0.0f, 1.0f));

                textBlock.DrawString(frameMonitor.FPS.ToString("f2") + " FPS", new System.Drawing.RectangleF(0, 0, form.ClientSize.Width, form.ClientSize.Height),
                   SpriteTextRenderer.TextAlignment.Right | SpriteTextRenderer.TextAlignment.Top, new Color4(1.0f, 1.0f, 1.0f, 1.0f));

                sprite.Draw(srvTexture, new Vector2(300, 170), new Vector2(150, 150), new Vector2(75, 75), new Radians(totalMillisec / 1000.0), CoordinateType.Absolute);
                sprite.Draw(srvTexture, new Vector2(380, 320), new Vector2(150, 150), new Vector2(50, 50), new Degrees(-totalMillisec / 8), CoordinateType.Absolute);


                sprite.Flush();

                swapChain.Present(1, PresentFlags.None);
            });
			
            srvTexture.Dispose();
            sdxTexture.Dispose();
            textBlock.Dispose();
            renderView.Dispose();
            swapChain.Dispose();
            device.Dispose();
        }


        void form_Resize(object sender, EventArgs e)
        {
            RenderForm renderForm = (sender as RenderForm);
            Resize(renderForm.ClientSize);
        }

        void Resize(System.Drawing.Size size)
        {
            Resize(new Vector2(size.Width, size.Height));
        }

        void Resize(Vector2 size)
        {
            if (renderView != null)
                renderView.Dispose();

            swapChain.ResizeBuffers(1, (int)size.X, (int)size.Y, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
            Texture2D backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
            renderView = new RenderTargetView(device, backBuffer);
            backBuffer.Dispose();

            device.ImmediateContext.Rasterizer.SetViewport(new Viewport(0, 0, (int)size.X, (int)size.Y, 0.0f, 1.0f));
            device.ImmediateContext.OutputMerger.SetTargets(renderView);

            if (sprite != null)
                sprite.RefreshViewport();
        }

        public static Texture2D LoadTextureFromFile(Veldrid.Direct3D11.Device aDevice, string aFullPath)
        {
            Texture2D result = null;
            ImagingFactory fac = new ImagingFactory();

            BitmapDecoder bc = new Veldrid.WIC.BitmapDecoder(fac, aFullPath, DecodeOptions.CacheOnLoad);
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
            fTextureDesc.Format = Veldrid.DXGI.Format.B8G8R8A8_UNorm;
            fTextureDesc.Width = width;
            fTextureDesc.Height = height;
            fTextureDesc.Usage = ResourceUsage.Default;
            fTextureDesc.MipLevels = 1;
            fTextureDesc.ArraySize = 1;
            fTextureDesc.OptionFlags = ResourceOptionFlags.None;
            fTextureDesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
            fTextureDesc.SampleDescription = new Veldrid.DXGI.SampleDescription(1, 0);

            result = new Texture2D(aDevice,fTextureDesc, new DataBox[] { db });
            handle.Free();

            return result;
        }
    }
}
