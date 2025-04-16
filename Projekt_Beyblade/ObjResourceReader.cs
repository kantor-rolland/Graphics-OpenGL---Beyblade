using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Globalization;
using StbImageSharp;
using Projekt_Beyblade.Figures;
using Projekt_Beyblade.Figures;

namespace Projekt_Beyblade
{
    internal class ObjResourceReader
    {
        public static unsafe GlObject CreateObjectWithColor(GL Gl, float[] faceColor, String objectName)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<int[]> objFaces;
            List<float[]> objNormals;

            ReadObjDataForObject(out objVertices, out objFaces, out objNormals, in objectName);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(faceColor, objVertices, objFaces, objNormals, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices);
        }

        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.Count;

            Vector3D<float> position = new Vector3D<float>(0, 0, 0);

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, position, Gl);
        }

        private static unsafe void CreateGlArraysFromObjArrays(float[] faceColor, List<float[]> objVertices, List<int[]> objFaces, List<float[]> objNormals, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            bool useNormals = objNormals.Count > 0;

            foreach (var objFace in objFaces)
            {
                Vector3D<float> normal = default;

                if (!useNormals)
                {
                    var aObjVertex = objVertices[objFace[0] - 1];
                    var a = new Vector3D<float>(aObjVertex[0], aObjVertex[1], aObjVertex[2]);
                    var bObjVertex = objVertices[objFace[1] - 1];
                    var b = new Vector3D<float>(bObjVertex[0], bObjVertex[1], bObjVertex[2]);
                    var cObjVertex = objVertices[objFace[2] - 1];
                    var c = new Vector3D<float>(cObjVertex[0], cObjVertex[1], cObjVertex[2]);

                    normal = Vector3D.Normalize(Vector3D.Cross(b - a, c - a));
                }

                // process 3 vertices
                for (int i = 0; i < objFace.Length; ++i)
                {
                    var objVertex = objVertices[objFace[i] - 1];
                    List<float> glVertex = new List<float>();
                    glVertex.AddRange(objVertex);

                    if (useNormals)
                    {
                        var objNormal = objNormals[objFace[i] - 1];
                        glVertex.AddRange(objNormal);
                    }
                    else
                    {
                        glVertex.Add(normal.X);
                        glVertex.Add(normal.Y);
                        glVertex.Add(normal.Z);
                    }

                    var glVertexStringKey = string.Join(" ", glVertex);
                    if (!glVertexIndices.ContainsKey(glVertexStringKey))
                    {
                        glVertices.AddRange(glVertex);
                        glColors.AddRange(faceColor);
                        glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                    }

                    glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                }
            }
        }

        private static unsafe void ReadObjDataForObject(out List<float[]> objVertices, out List<int[]> objFaces, out List<float[]> objNormals, in String nameOfObject)
        {
            objVertices = new List<float[]>();
            objFaces = new List<int[]>();
            objNormals = new List<float[]>();
            var resourceName = $"Projekt_Beyblade.Resources.{nameOfObject}.obj";
            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream(resourceName))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;

                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                    switch (lineClassifier)
                    {
                        case "v":
                            float[] vertex = new float[3];
                            for (int i = 0; i < vertex.Length; ++i)
                                vertex[i] = float.Parse(lineData[i]);
                            objVertices.Add(vertex);
                            break;
                        case "vn":
                            float[] normal = new float[3];
                            for (int i = 0; i < normal.Length; ++i)
                                normal[i] = float.Parse(lineData[i]);
                            objNormals.Add(normal);
                            break;
                        case "f":
                            int[] face = new int[3];
                            for (int i = 0; i < face.Length; ++i)
                            {
                                var faceData = lineData[i].Split('/');
                                face[i] = int.Parse(faceData[0], CultureInfo.InvariantCulture);
                            }
                            objFaces.Add(face);
                            break;
                        default:
                            throw new Exception("Unhandled obj structure.");
                    }
                }
            }
        }

        public static unsafe GlObjectWithTexture CreateObjectWithTexture(GL Gl, float[] faceColor, string objectName, string textureId)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<float[]> objNormals;
            List<float[]> textCoords;
            List<int[][]> objFaces;

            ReadObjDataForObjectWithTexture(out objVertices, out objNormals, out textCoords, out objFaces, objectName);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<float> glTextures = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArraysWithTexture(faceColor, objVertices, objFaces, objNormals, textCoords, glVertices, glColors, glTextures, glIndices);

            return CreateOpenGlObjectWithTexture(Gl, vao, glVertices, glColors, glTextures, glIndices, textureId);
        }

        // WithTexture
        private static unsafe GlObjectWithTexture CreateOpenGlObjectWithTexture(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<float> glTextures, List<uint> glIndices, string textureId)
        {

            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = 6 * sizeof(float);//offsetTexture + (2 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);


            uint texture = Gl.GenTexture();
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2D, texture);

            var imageResult = ReadTextureImageWithTexture(textureId);
            var textureBytes = (ReadOnlySpan<byte>)imageResult.Data.AsSpan();
            Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)imageResult.Width,
                (uint)imageResult.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureBytes);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Gl.BindTexture(TextureTarget.Texture2D, 0);

            uint textures = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, textures);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glTextures.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(3);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.Count;

            Vector3D<float> position = new Vector3D<float>(0, 0, 0);

            return new GlObjectWithTexture(vao, vertices, colors, indices, indexArrayLength, position, Gl, texture);
        }

        // WithTexture
        private static unsafe void CreateGlArraysFromObjArraysWithTexture(float[] faceColor, List<float[]> objVertices, List<int[][]> objFaces, List<float[]> objNormals, List<float[]> textCoords, List<float> glVertices, List<float> glColors, List<float> glTextures, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            // adatok feldolgozasa, mindegyik vektor kulon kulon
            foreach (var objFace in objFaces)
            {
                var aObjNormal = objNormals[objFace[0][2] - 1];
                var aNormal = new Vector3D<float>(aObjNormal[0], aObjNormal[1], aObjNormal[2]);
                var bObjNormal = objNormals[objFace[1][2] - 1];
                var bNormal = new Vector3D<float>(bObjNormal[0], bObjNormal[1], bObjNormal[2]);
                var cObjNormal = objNormals[objFace[2][2] - 1];
                var cNormal = new Vector3D<float>(cObjNormal[0], cObjNormal[1], cObjNormal[2]);

                // normal vektor kiszamitasa
                var normal = new Vector3D<float>((aNormal.X + bNormal.X + cNormal.X) / 3, (aNormal.Y + bNormal.Y + cNormal.Y) / 3, (aNormal.Z + bNormal.Z + cNormal.Z) / 3);

                // process 3 vertices
                for (int i = 0; i < objFace.Length; ++i)
                {
                    var objVertex = objVertices[objFace[i][0] - 1];
                    var objTexture = textCoords[objFace[i][1] - 1];
                    
                    // felepitjuk a gl vektorokat
                    List<float> glVertex = new List<float>();
                    List<float> glTexture = new List<float>();
                    glTexture.AddRange(objTexture);
                    glVertex.AddRange(objVertex);
                    glVertex.Add(normal.X);
                    glVertex.Add(normal.Y);
                    glVertex.Add(normal.Z);

                    // check if vertex exists
                    var glVertexStringKey = string.Join(" ", glVertex);
                    if (!glVertexIndices.ContainsKey(glVertexStringKey))
                    {
                        glVertices.AddRange(glVertex);
                        glColors.AddRange(faceColor);
                        glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                        glTextures.AddRange(glTexture);
                    }

                    // add vertex to triangle indices
                    glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                }
            }
        }

        // WithTexture
        private static unsafe void ReadObjDataForObjectWithTexture(out List<float[]> objVertices, out List<float[]> objNormals, out List<float[]> textCoords, out List<int[][]> objFaces, in string objectName)
        {
            objVertices = new List<float[]>();
            objFaces = new List<int[][]>();
            objNormals = new List<float[]>();
            textCoords = new List<float[]>();
            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream("Projekt_Beyblade.Resources." + objectName))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;
                    
                    // osztaloyzas  
                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                    switch (lineClassifier)
                    {
                        case "v": // vertex - csucsok
                            float[] vertex = new float[3];
                            for (int i = 0; i < vertex.Length; ++i)
                                vertex[i] = float.Parse(lineData[i]);
                            objVertices.Add(vertex);
                            break;
                        case "vn": // vertex normal - normalvektorok
                            float[] normal = new float[3];
                            for (int i = 0; i < normal.Length; ++i)
                                normal[i] = float.Parse(lineData[i]);
                            objNormals.Add(normal);
                            break;
                        case "vt": // textura
                            float[] texCoord = new float[2];
                            for (int i = 0; i < texCoord.Length; ++i)
                                texCoord[i] = float.Parse(lineData[i]);
                            textCoords.Add(texCoord);
                            break;
                        case "f": // face - vonalak
                            // faces looks like 1/1/1 2/2/2 3/3/3
                            int[][] face = new int[3][];
                            for (int i = 0; i < face.Length; ++i)
                            {
                                face[i] = new int[3];
                                var faceData = lineData[i].Split('/');
                                for (int j = 0; j < face[i].Length; ++j)
                                    face[i][j] = int.Parse(faceData[j]);
                            }
                            objFaces.Add(face);
                            break;
                    }
                }
            }
        }

        // WithTexture
        private static unsafe ImageResult ReadTextureImageWithTexture(string textureResource)
        {
            ImageResult result;
            using (Stream skyeboxStream
                = typeof(GlCube).Assembly.GetManifestResourceStream("Projekt_Beyblade.Resources." + textureResource))
                result = ImageResult.FromStream(skyeboxStream, ColorComponents.RedGreenBlueAlpha);

            return result;
        }

    }
}

