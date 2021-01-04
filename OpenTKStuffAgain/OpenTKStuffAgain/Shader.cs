using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKStuffAgain
{
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public Shader(string vertPath, string fragPath)
        {
            // Load vertex shader and compile
            var shaderSource = LoadSource(vertPath);

            // GL.CreateShader creates an empty shader. The ShaderType enum denotes which type of shader will be created.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            // Bind the GLSL source code.
            GL.ShaderSource(vertexShader, shaderSource);

            // Compile the shader.
            CompileShader(vertexShader);

            // We do the same for the fragment shader.
            shaderSource = LoadSource(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            // The two shaders must then be merged into a shader program, so OpenGL can use it.
            // Create a program
            Handle = GL.CreateProgram();

            // Attach both shaders
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // Link them together
            LinkProgram(Handle);

            // When the shader program is linked, it no longer needs the individual shaders attached to it.
            // Detach the shaders, and then delete them.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // The shader is now ready, but first we are going to cache all shader uniform locations.
            // Querying this from the shader is very slow, so we do it once on init and reuse those values later.

            // Get the number of active uniforms in the shader.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Allocate the dictionary to hold the locations.
            _uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms.
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // Get the name of this uniform
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // Get the location
                var location = GL.GetUniformLocation(Handle, key);

                // Add it to the dictionary
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if(code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occured whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // Link the program
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if(code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                var infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Error occured whilst linking Program({program}).\n\n{infoLog}");
            }
        }

        // A wrapper function that enables the shader program.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // Use this in the VertexAttribPointer
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        // Loads the entire file into a string.
        private static string LoadSource(string path)
        {
            using (var sr = new StreamReader(path, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        // Uniform setters
        // 1. Bind the program you went to set the uniform on
        // 2 Get a handle to the location of the uniform with GL.GetUniformLocation
        // 3. Use the appropriate GL.Uniform* function to set the uniform.

        /// <summary>Set a uniform int on this shader</summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>Set a uniform float on this shader</summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>Set a uniform int on this shader</summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///     <para>
        ///     The matrix is transposed before being sent to the shader.
        ///     </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }
    }
}
