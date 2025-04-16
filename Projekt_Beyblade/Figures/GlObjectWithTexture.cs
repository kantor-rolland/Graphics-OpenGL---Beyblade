using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Projekt_Beyblade.Figures
{
    internal class GlObjectWithTexture : GlObject
    {
        public uint? Texture { get; private set; }

        public GlObjectWithTexture(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, Vector3D<float> position, GL gl, uint texture) : base(vao, vertices, colors, indeces, indexArrayLength, position, gl)
        {
            Texture = texture; //csak siman megadjuk a texturat
        }

        internal new void ReleaseGlObject()
        {
            base.ReleaseGlObject();
        }
    }
}
