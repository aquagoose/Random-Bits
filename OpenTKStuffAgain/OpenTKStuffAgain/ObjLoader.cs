using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpenTKStuffAgain
{
    public class ObjLoader
    {
        public float[] Vertices;
        public float[] TexCoords;
        public float[] Indices;
        public ObjLoader(string path)
        { 
            using (var reader = new StreamReader(path, Encoding.UTF8))
            {
                List<float> vertices = new List<float>();
                List<float> texCoords = new List<float>();
                List<float> indices = new List<float>();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var splitLine = line.Split(' ');
                    if (splitLine[0] == "v") { foreach (var vertex in splitLine.Skip(1)) { vertices.Add(float.Parse(vertex)); } }
                    if (splitLine[0] == "vt") { foreach (var coord in splitLine.Skip(1)) { texCoords.Add(float.Parse(coord)); } }
                    if (splitLine[0] == "f") { 
                        foreach (var num in splitLine.Skip(1))
                        {
                            var splitNum = num.Split('/');
                            foreach(var num2 in splitNum) { indices.Add(float.Parse(num2)); }
                        }
                    }
                }
                Vertices = vertices.ToArray();
                TexCoords = texCoords.ToArray();
                Indices = indices.ToArray();
            }
        }
    }
}
