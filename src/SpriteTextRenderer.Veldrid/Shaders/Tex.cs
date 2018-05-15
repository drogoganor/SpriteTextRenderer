using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("Tex", "SpriteTextRenderer.Veldrid.Shaders.Tex.VS", "SpriteTextRenderer.Veldrid.Shaders.Tex.FS")]

namespace SpriteTextRenderer.Veldrid.Shaders
{
    public class Tex
    {
        [ResourceSet(0)]
        public Matrix4x4 Projection;
        [ResourceSet(1)]
        public Texture2DResource SurfaceTexture;
        [ResourceSet(1)]
        public SamplerResource SurfaceSampler;

        [VertexShader]
        public FragmentInput VS(VertexInput input)
        {
            FragmentInput output;
            var worldPosition = Mul(Projection, new Vector4(input.Position, 0, 1));
            output.SystemPosition = worldPosition;
            output.Color = input.Color;
            output.TexCoords = input.TexCoords;

            return output;
        }

        [FragmentShader]
        public Vector4 FS(FragmentInput input)
        {
            //return input.Color;
            return Sample(SurfaceTexture, SurfaceSampler, input.TexCoords);
        }

        public struct VertexInput
        {
            [PositionSemantic] public Vector2 Position;
            [TextureCoordinateSemantic] public Vector2 TexCoords;
            [ColorSemantic] public Vector4 Color;
        }

        public struct FragmentInput
        {
            [SystemPositionSemantic] public Vector4 SystemPosition;
            [TextureCoordinateSemantic] public Vector2 TexCoords;
            [ColorSemantic] public Vector4 Color;
        }
    }
}
