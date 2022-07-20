using System;
using System.Collections.Generic;
using System.IO;

namespace ObjLoad
{
    public struct Vertex
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    
        public Vertex(float _X, float _Y, float _Z)
        {
            X = _X;
            Y = _Y;
            Z = _Z;
        }
    
        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }

    public struct Vertex_index
    {
        public int V { get; set; }
        public int VT { get; set; }
        public int VN { get; set; }

        public Vertex_index(int _V, int _VT, int _VN)
        {
            V = _V;
            VT = _VT;
            VN = _VN;
        }

        public override string ToString()
        {
            return $"({V}, {VT}, {VN})";
        }
    }

    public struct Normal
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    
        public Normal(float _X, float _Y, float _Z)
        {
            X = _X;
            Y = _Y;
            Z = _Z;
        }
    
        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }

    public struct Texture
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Texture(float _X, float _Y)
        {
            X = _X;
            Y = _Y;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }

    public struct Triangle
    {
        public Vertex_index V1 { get; set; }
        public Vertex_index V2 { get; set; }
        public Vertex_index V3 { get; set; }

        public Triangle(Vertex_index _V1, Vertex_index _V2, Vertex_index _V3)
        {
            V1 = _V1;
            V2 = _V2;
            V3 = _V3;
        }

        public override string ToString()
        {
            return $"({V1.V}/{V1.VT}/{V1.VN}, {V2.V}/{V2.VT}/{V2.VN}, {V3.V}/{V3.VT}/{V3.VN})";
        }
    }
    public class Model3D
    {
        public Vertex[] Vertices { get; set; }
        public Normal[] Normals { get; set; }
        public Triangle[] Triangles { get; set; }
        public Texture[] Textures { get; set; }

        public Model3D() {}
        public Model3D(Vertex[] _Vertices, Normal[] _Normals, Triangle[] _Triangles, Texture[] _Textures)
        {
            Vertices = _Vertices;
            Normals = _Normals;
            Triangles = _Triangles;
            Textures = _Textures;
        }

        public float[] GetVerticesNormalsText()
        {
            float[] result = new float[Vertices.Length * 8];
            for (int i = 0, j = 0; i < Vertices.Length; i++, j += 8)
            {
                result[j] = Vertices[i].X;
                result[j + 1] = Vertices[i].Y;
                result[j + 2] = Vertices[i].Z;

                result[j + 3] = Normals[i].X;
                result[j + 4] = Normals[i].Y;
                result[j + 5] = Normals[i].Z;

                result[j + 6] = Textures[i].X;
                result[j + 7] = Textures[i].Y;
            }
            return result;
        }

        public int[] GetTriangles()
        {
            int[] result = new int[Triangles.Length * 3];
            for (int i = 0, j = 0; i < Triangles.Length; i++, j += 3)
            {
                result[j] = Triangles[i].V1.V;
                result[j + 1] = Triangles[i].V2.V;
                result[j + 2] = Triangles[i].V3.V;
            }
            return result;
        }
    }

    public class Obj
    {
        private string Path_to_file { get; set; }
        private readonly Model3D Model;

        public Obj(string _Path_to_file)
        {
            Model = new Model3D();
            Path_to_file = _Path_to_file;
        }

        public Model3D GetModel3D()
        {
            return Model;
        }

        public void Load()
        {
            List<Vertex> objVertices = new List<Vertex>();
            List<Normal> objNormals = new List<Normal>();
            List<Texture> objTextures = new List<Texture>();
            List<Triangle> objTriangles = new List<Triangle>();

            string fileText = File.ReadAllText(Path_to_file);
            string[] fileLines = fileText.Split('\n');

            int i = 0;
            for (; i < fileLines.Length; i++)
            {
                string line = fileLines[i];
                string[] lineParts = line.Split(' ');


                if (line.StartsWith("vn"))
                {
                    float x = float.Parse(lineParts[1].Replace('.', ','));
                    float y = float.Parse(lineParts[2].Replace('.', ','));
                    float z = float.Parse(lineParts[3].Replace('.', ','));
                    objNormals.Add(new Normal(x, y, z));
                } 
                else if (line.StartsWith("vt")) 
                {
                    float x = float.Parse(lineParts[1].Replace('.', ','));
                    float y = float.Parse(lineParts[2].Replace('.', ','));
                    objTextures.Add(new Texture(x, y));
                }
                else if (line.StartsWith("v"))
                {
                    float x = float.Parse(lineParts[1].Replace('.', ','));
                    float y = float.Parse(lineParts[2].Replace('.', ','));
                    float z = float.Parse(lineParts[3].Replace('.', ','));
                    objVertices.Add(new Vertex(x, y, z));
                }
                else if (line.StartsWith("f")) break;
            }

            List<int> numberVertex = new List<int>();
            for (int l = 0; l < objVertices.Count; l++)
            {
                numberVertex.Add(-1);
            }
            List<int> numberNormal = new List<int>();
            for (int l = 0; l < objVertices.Count; l++)
            {
                numberNormal.Add(-1);
            }
            List<int> numberTexture = new List<int>();
            for (int l = 0; l < objVertices.Count; l++)
            {
                numberTexture.Add(-1);
            }
            List<Vertex> newObjVertices = new List<Vertex>();
            for (int l = 0; l < objVertices.Count; l++)
            {
                newObjVertices.Add(new Vertex());
            }
            List<Normal> newObjNormals = new List<Normal>();
            for (int l = 0; l < objVertices.Count; l++)
            {
                newObjNormals.Add(new Normal());
            }
            List<Texture> newObjTextures = new List<Texture>();
            for (int l = 0; l < objVertices.Count; l++)
            {
                newObjTextures.Add(new Texture());
            }

            bool b;
            int countVertex = objVertices.Count;

            // Считывание треугольников
            for (; i < fileLines.Length; i++)
            {
                string line = fileLines[i];
                string[] lineParts = line.Split(' ');

                int[] v = new int[3];
                int[] vn = new int[3];
                int[] vt = new int[3];

                if (line.StartsWith("f"))
                {
                    string[] s1 = lineParts[1]
                        .Split(new string[] { "/" }, StringSplitOptions.None);
                    string[] s2 = lineParts[2]
                        .Split(new string[] { "/" }, StringSplitOptions.None);
                    string[] s3 = lineParts[3]
                        .Split(new string[] { "/" }, StringSplitOptions.None);

                    v[0] = int.Parse(s1[0]) - 1;
                    vt[0] = int.Parse(s1[1]) - 1;
                    vn[0] = int.Parse(s1[2]) - 1;

                    v[1] = int.Parse(s2[0]) - 1;
                    vt[1] = int.Parse(s2[1]) - 1;
                    vn[1] = int.Parse(s2[2]) - 1;

                    v[2] = int.Parse(s3[0]) - 1;
                    vt[2] = int.Parse(s3[1]) - 1;
                    vn[2] = int.Parse(s3[2]) - 1;

                    // Изменение массивов с нормалями, текстурами, вершинами
                    for (int j = 0; j < 3; j++)
                    {
                        b = false;
                        for (int k = 0; k < newObjVertices.Count; k++)
                        {
                            if (v[j] == numberVertex[k])
                            {
                                b = true;
                                break;
                            }
                        }
                        if (b)
                        {
                            newObjVertices.Add(objVertices[v[j]]);
                            newObjTextures.Add(objTextures[vt[j]]);
                            newObjNormals.Add(objNormals[vn[j]]);
                            v[j] = countVertex;
                            vt[j] = countVertex;
                            vn[j] = countVertex;
                            numberVertex.Add(v[j]);
                            numberTexture.Add(vt[j]);
                            numberNormal.Add(vn[j]);
                            countVertex++;
                        }
                        else
                        {
                            newObjVertices[v[j]] = objVertices[v[j]];
                            newObjTextures[v[j]] = objTextures[vt[j]];
                            newObjNormals[v[j]] = objNormals[vn[j]];
                            numberVertex[v[j]] = v[j];
                            numberTexture[v[j]] = vt[j];
                            numberNormal[v[j]] = vn[j];
                        }
                    }

                    objTriangles.Add(new Triangle(
                        new Vertex_index(v[0], vt[0], vn[0]), new Vertex_index(v[1], vt[1], vn[1]), new Vertex_index(v[2], vt[2], vn[2])));
                }
            }
            Model.Vertices = newObjVertices.ToArray();
            Model.Normals = newObjNormals.ToArray();
            Model.Textures = newObjTextures.ToArray();
            Model.Triangles = objTriangles.ToArray();
        }
    }
}
