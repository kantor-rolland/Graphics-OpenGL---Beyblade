using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Projekt_Beyblade.Figures
{
    internal class GlObject
    {
        public uint Vao { get; } // vertex array object 
        public uint Vertices { get; }
        public uint Colors { get; }
        public uint Indices { get; }
        public uint IndexArrayLength { get; }

        public Vector3D<float> Position { get; set; }

        private GL Gl;

        public GlObject(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, Vector3D<float> position, GL gl)
        {
            Vao = vao;
            Vertices = vertices;
            Colors = colors;
            Indices = indeces;
            IndexArrayLength = indexArrayLength;
            Position = position;
            Gl = gl;
        }

        internal void ReleaseGlObject()
        {
            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(Vertices);
            Gl.DeleteBuffer(Colors);
            Gl.DeleteBuffer(Indices);
            Gl.DeleteVertexArray(Vao);
        }
    }
}
