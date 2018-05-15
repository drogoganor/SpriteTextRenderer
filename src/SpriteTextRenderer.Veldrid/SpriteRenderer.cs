using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace SpriteTextRenderer.Veldrid
{
    /// <summary>
    /// This class is responsible for rendering 2D sprites using Veldrid. Typically, only one instance of this class is necessary.
    /// </summary>
    public class SpriteRenderer : SpriteTextRenderer.SpriteRenderer
    {
        private GraphicsDevice device;
        /// <summary>
        /// Returns the Veldrid device that this SpriteRenderer was created for.
        /// </summary>
        public GraphicsDevice Device { get { return device; } }

        protected override object DeviceRef { get { return device; } }
        
        // TODO: Reimplement these
        protected override object CurrentBlendState
        {
            get { return null; }
            set { }
        }

        protected override object CurrentDepthStencilState
        {
            get { return null; }
            set { }
        }
        
        public SpriteRenderer(GraphicsDevice device, int bufferSize = 128)
            : base(bufferSize)
        {
            this.device = device;

            Initialize();
        }

        #region ### Private Veldrid members
        private CommandList commandList;
        private DeviceBuffer vertexBuffer;
        private DeviceBuffer indexBuffer;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private Pipeline pipeline;

        public DeviceBuffer _projectionBuffer;
        public ResourceSet ResourceSet;
        private ResourceSet _worldTextureSet;
        public ResourceLayout ResourceLayout;

        private VertexLayoutDescription layout;

        private Texture _surfaceTexture;
        private TextureView _surfaceTextureView;


        #endregion

        #region ### Public draw interface ###

        /// <summary>
        /// Draws a complete texture on the screen.
        /// </summary>
        /// <param name="texture">The shader resource view of the texture to draw</param>
        /// <param name="position">Position of the top left corner of the texture in the chosen coordinate system</param>
        /// <param name="size">Size of the texture in the chosen coordinate system. The size is specified in the screen's coordinate system.</param>
        /// <param name="coordinateType">A custom coordinate system in which to draw the texture</param>
        public void Draw(object texture, Vector2 position, Vector2 size, CoordinateType coordinateType)
        {
            base.Draw(texture, position.ToSTRVector(), size.ToSTRVector(), coordinateType);
        }

        /// <summary>
        /// Draws a complete texture on the screen.
        /// </summary>
        /// <param name="texture">The shader resource view of the texture to draw</param>
        /// <param name="position">Position of the top left corner of the texture in the chosen coordinate system</param>
        /// <param name="size">Size of the texture in the chosen coordinate system. The size is specified in the screen's coordinate system.</param>
        /// <param name="center">Specify the texture's center in the chosen coordinate system. The center is specified in the texture's local coordinate system. E.g. for <paramref name="coordinateType"/>=CoordinateType.SNorm, the texture's center is defined by (0, 0).</param>
        /// <param name="rotationAngle">The angle in radians to rotate the texture. Positive values mean counter-clockwise rotation. Rotations can only be applied for relative or absolute coordinates. Consider using the Degrees or Radians helper structs.</param>
        /// <param name="coordinateType">A custom coordinate system in which to draw the texture</param>
        public void Draw(object texture, Vector2 position, Vector2 size, Vector2 center, double rotationAngle, CoordinateType coordinateType)
        {
            base.Draw(texture, position.ToSTRVector(), size.ToSTRVector(), center.ToSTRVector(), rotationAngle, coordinateType);
        }

        /// <summary>
        /// Draws a complete texture on the screen.
        /// </summary>
        /// <param name="texture">The shader resource view of the texture to draw</param>
        /// <param name="position">Position of the top left corner of the texture in the chosen coordinate system</param>
        /// <param name="size">Size of the texture in the chosen coordinate system. The size is specified in the screen's coordinate system.</param>
        /// <param name="coordinateType">A custom coordinate system in which to draw the texture</param>
        /// <param name="color">The color with which to multiply the texture</param>
        public void Draw(object texture, Vector2 position, Vector2 size, RgbaFloat color, CoordinateType coordinateType)
        {
            base.Draw(texture, position.ToSTRVector(), size.ToSTRVector(), color.ToSTRColor(), coordinateType);
        }

        /// <summary>
        /// Draws a complete texture on the screen.
        /// </summary>
        /// <param name="texture">The shader resource view of the texture to draw</param>
        /// <param name="position">Position of the top left corner of the texture in the chosen coordinate system</param>
        /// <param name="size">Size of the texture in the chosen coordinate system. The size is specified in the screen's coordinate system.</param>
        /// <param name="center">Specify the texture's center in the chosen coordinate system. The center is specified in the texture's local coordinate system. E.g. for <paramref name="coordinateType"/>=CoordinateType.SNorm, the texture's center is defined by (0, 0).</param>
        /// <param name="rotationAngle">The angle in radians to rotate the texture. Positive values mean counter-clockwise rotation. Rotations can only be applied for relative or absolute coordinates. Consider using the Degrees or Radians helper structs.</param>
        /// <param name="coordinateType">A custom coordinate system in which to draw the texture</param>
        /// <param name="color">The color with which to multiply the texture</param>
        public void Draw(object texture, Vector2 position, Vector2 size, Vector2 center, double rotationAngle, RgbaFloat color, CoordinateType coordinateType)
        {
            base.Draw(texture, position.ToSTRVector(), size.ToSTRVector(), center.ToSTRVector(), rotationAngle, color.ToSTRColor(), coordinateType);
        }

        /// <summary>
        /// Draws a region of a texture on the screen.
        /// </summary>
        /// <param name="texture">The shader resource view of the texture to draw</param>
        /// <param name="position">Position of the center of the texture in the chosen coordinate system</param>
        /// <param name="size">Size of the texture in the chosen coordinate system. The size is specified in the screen's coordinate system.</param>
        /// <param name="center">Specify the texture's center in the chosen coordinate system. The center is specified in the texture's local coordinate system. E.g. for <paramref name="coordinateType"/>=CoordinateType.SNorm, the texture's center is defined by (0, 0).</param>
        /// <param name="rotationAngle">The angle in radians to rotate the texture. Positive values mean counter-clockwise rotation. Rotations can only be applied for relative or absolute coordinates. Consider using the Degrees or Radians helper structs.</param>
        /// <param name="coordinateType">A custom coordinate system in which to draw the texture</param>
        /// <param name="color">The color with which to multiply the texture</param>
        /// <param name="texCoords">Texture coordinates for the top left corner</param>
        /// <param name="texCoordsSize">Size of the region in texture coordinates</param>
        public void Draw(object texture, Vector2 position, Vector2 size, Vector2 center, double rotationAngle, Vector2 texCoords, Vector2 texCoordsSize, RgbaFloat color, CoordinateType coordinateType)
        {
            base.Draw(texture, position.ToSTRVector(), size.ToSTRVector(), center.ToSTRVector(), rotationAngle, texCoords.ToSTRVector(), texCoordsSize.ToSTRVector(), color.ToSTRColor(), coordinateType);
        }
        
        /// <summary>
        /// Draws a region of a texture on the screen.
        /// </summary>
        /// <param name="texture">The shader resource view of the texture to draw</param>
        /// <param name="position">Position of the center of the texture in the chosen coordinate system</param>
        /// <param name="size">Size of the texture in the chosen coordinate system. The size is specified in the screen's coordinate system.</param>
        /// <param name="coordinateType">A custom coordinate system in which to draw the texture</param>
        /// <param name="color">The color with which to multiply the texture</param>
        /// <param name="texCoords">Texture coordinates for the top left corner</param>
        /// <param name="texCoordsSize">Size of the region in texture coordinates</param>
        public void Draw(object texture, Vector2 position, Vector2 size, Vector2 texCoords, Vector2 texCoordsSize, RgbaFloat color, CoordinateType coordinateType)
        {
            base.Draw(texture, position.ToSTRVector(), size.ToSTRVector(), STRVector.Zero, 0.0, texCoords.ToSTRVector(), texCoordsSize.ToSTRVector(), color.ToSTRColor(), coordinateType);
        }
        #endregion

        #region ### Template method hooks ###

        protected override STRViewport QueryViewport()
        {
            var framebuffer = device.SwapchainFramebuffer;
            var vp = new Viewport(0, 0, framebuffer.Width, framebuffer.Height, 0, 1);
            return vp.ToSTRViewport();
        }

        protected override void CompileEffectAndGetVariable(string hlslSource, string variableName)
        {
            var factory = device.ResourceFactory;

            layout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));

            string shaderName = "Color";
            _vertexShader = LoadShader(ShaderStages.Vertex, shaderName);
            _fragmentShader = LoadShader(ShaderStages.Fragment, shaderName);
            
            _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            ResourceLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Projection", ResourceKind.UniformBuffer, ShaderStages.Vertex))
                    );

            ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
                ResourceLayout,
                _projectionBuffer));
            
            // Create pipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            pipelineDescription.ResourceLayouts = new[] { ResourceLayout }; //System.Array.Empty<ResourceLayout>();
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { layout },
                shaders: new Shader[] { _vertexShader, _fragmentShader });
            pipelineDescription.Outputs = device.SwapchainFramebuffer.OutputDescription;

            pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

            commandList = factory.CreateCommandList();
        }

        protected override void CreateInputLayout(STRInputElement[] elements)
        {
            //var specificElements = elements.Select(e => e.ToVeldridInputElement()).ToArray();
            //layout = new VertexLayoutDescription(specificElements);
        }

        protected override void CreateVertexBuffer(int elementByteSize, int elements)
        {
            var factory = device.ResourceFactory;

            BufferDescription vbDescription = new BufferDescription(
                4 * VertexPositionColor.SizeInBytes,
                BufferUsage.VertexBuffer);
            vertexBuffer = factory.CreateBuffer(vbDescription);

            BufferDescription ibDescription = new BufferDescription(
                4 * sizeof(ushort),
                BufferUsage.IndexBuffer);
            indexBuffer = factory.CreateBuffer(ibDescription);
        }

        protected override void CreateDepthStencilAndBlendState()
        {

        }

        protected override void UpdateVertexBufferData<T>(T[] vertices)
        {
            VertexPositionColor[] quadVertices =
            {
                new VertexPositionColor(new Vector2(175f, 175f), RgbaFloat.Green),
                new VertexPositionColor(new Vector2(10, 175f), RgbaFloat.Red),
                new VertexPositionColor(new Vector2(175f, 10), RgbaFloat.Yellow),
                new VertexPositionColor(new Vector2(10, 10), RgbaFloat.Blue)
            };
            ushort[] quadIndices = { 0, 1, 2, 3 };
            device.UpdateBuffer(vertexBuffer, 0, quadVertices);
            device.UpdateBuffer(indexBuffer, 0, quadIndices);
        }

        protected override void InitRendering()
        {
            var fb = device.SwapchainFramebuffer;

            // Begin() must be called before commands can be issued.
            commandList.Begin();

            // We want to render directly to the output window.
            commandList.SetFramebuffer(fb);
            commandList.SetFullViewports();
            commandList.UpdateBuffer(_projectionBuffer, 0, Matrix4x4.CreateOrthographicOffCenter(0, fb.Width, fb.Height, 0, 0, 1));
            commandList.SetPipeline(pipeline);
            commandList.SetGraphicsResourceSet(0, ResourceSet);
        }

        protected override void Draw(object texture, int count, int offset)
        {
            // Set all relevant state to draw our quad.
            commandList.SetVertexBuffer(0, vertexBuffer);
            commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);

            // Issue a Draw command for a single instance with 4 indices.
            commandList.DrawIndexed(
                indexCount: (uint)4,
                instanceCount: 1,
                indexStart: (uint)offset,
                vertexOffset: 0,
                instanceStart: 0);

            // End() must be called before commands can be submitted for execution.
            commandList.End();
            Device.SubmitCommands(commandList);
        }

        protected override void DisposeOfResources()
        {
            /*
            fx.Dispose();
            inputLayout.Dispose();
            vb.Dispose();
            */
        }
        #endregion

        protected override void UpdateAlphaBlend()
        {
            /*
            this.pass = fx.GetTechniqueByIndex((int)AlphaBlendMode).GetPassByIndex(0);
            */
        }



        private Shader LoadShader(ShaderStages stage, string name)
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
            string path = Path.Combine(System.AppContext.BaseDirectory, "Shaders", $"{name}-{stage.ToString()}.{extension}");
            byte[] shaderBytes = File.ReadAllBytes(path);
            return device.ResourceFactory.CreateShader(new ShaderDescription(stage, shaderBytes, entryPoint));
        }
    }
}
