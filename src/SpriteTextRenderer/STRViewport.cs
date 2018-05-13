namespace SpriteTextRenderer
{
    /// <summary>
    /// The SpriteTextRenderer uses a library-independent viewport structure because the base lib must not depend on either SlimDX or SharpDX
    /// </summary>
    public class STRViewport
    {
        public float Width { get; set; }
        public float Height { get; set; }
    }
}
