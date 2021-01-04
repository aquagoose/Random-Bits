using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace OpenTKStuffAgain
{
    // This is where all OpenGL code will be written.
    // OpenTK allows for several functions to be overriden to extend functionality; this is how we'll be writing code.
    public class Window : GameWindow
    {
        // Create the vertices for our triangle/square/h. These are listed in normalised device coordinates (NDC)
        // OpenGL only supports rendering in 3D, so to create a flat triangle, the Z coordinate will be kept at 0.

        private readonly float[] _vertices =
        {
            -0.5f,0.5f,0,
            -0.5f,-0.5f,0,
            0.5f,-0.5f,0,
            0.5f,0.5f,0,

            -0.5f,0.5f,1,
            -0.5f,-0.5f,1,
            0.5f,-0.5f,1,
            0.5f,0.5f,1,

            0.5f,0.5f,0,
            0.5f,-0.5f,0,
            0.5f,-0.5f,1,
            0.5f,0.5f,1,

            -0.5f,0.5f,0,
            -0.5f,-0.5f,0,
            -0.5f,-0.5f,1,
            -0.5f,0.5f,1,

            -0.5f,0.5f,1,
            -0.5f,0.5f,0,
            0.5f,0.5f,0,
            0.5f,0.5f,1,

            -0.5f,-0.5f,1,
            -0.5f,-0.5f,0,
            0.5f,-0.5f,0,
            0.5f,-0.5f,1
        };

        private readonly float[] _texCoords =
        {
            0,0,
            0,1,
            1,1,
            1,0,
            0,0,
            0,1,
            1,1,
            1,0,
            0,0,
            0,1,
            1,1,
            1,0,
            0,0,
            0,1,
            1,1,
            1,0,
            0,0,
            0,1,
            1,1,
            1,0,
            0,0,
            0,1,
            1,1,
            1,0
        }; 

        // Indicies array
        private readonly uint[] _indices =
        {
            0,1,3,
            3,1,2,
            4,5,7,
            7,5,6,
            8,9,11,
            11,9,10,
            12,13,15,
            15,13,14,
            16,17,19,
            19,17,18,
            20,21,23,
            23,21,22
        };

        // THese are the handles to OpenGL Objects.
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _elementBufferObject;
        private int _textureBuffer;

        // The shader
        private Shader _shader;

        // Texture
        private Texture _texture;
        private Texture _texture2;

        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;

        //private ObjLoader _objLoader;
        //private float[] _vertices;
        //private float[] _texCoords;
        //private float[] _indices;
        

        // How long since the program was opened.
        private double _time;

        public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        protected override void OnLoad(EventArgs e)
        {
            //_objLoader = new ObjLoader("Resources/cube.obj");
            //_vertices = _objLoader.Vertices;
            //_texCoords = _objLoader.TexCoords;
            //_indices = _objLoader.Indices;

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest); // Enable depth testing.

            // Create the VBO Buffer
            _vertexBufferObject = GL.GenBuffer();

            // Bind the VBO buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Upload the vertices to the buffer
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // Create and bind the EBO
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // Create the shader
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

            // Enable the shader
            _shader.Use();

            // Create texture
            _texture = new Texture("Resources/container.png");
            _texture.Use();
            //_texture2 = new Texture("Resources/gordon-ramsay-1-e1523056498302.jpg");
            //_texture2.Use(TextureUnit.Texture1);

            _shader.SetInt("texture0", 0);
            //_shader.SetInt("texture1", 1);

            // Create the VAO object
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Bind the VBO again so that the VAO will bind that as well.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Bind the EBO, again.
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            // Set up the vertex shader
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            _textureBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _textureBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _texCoords.Length * sizeof(float), _texCoords, BufferUsageHint.StaticDraw);
            //GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vector2.SizeInBytes, 0);

            // Texture setup
            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            _camera = new Camera(Vector3.UnitZ * 3, Width / (float)Height);

            base.OnLoad(e);
        }

        // Render loop.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _time += 40.0 * e.Time;

            // Clear the color buffer bit
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Bind the VAO
            GL.BindVertexArray(_vertexArrayObject);

            // Create transformation matrix
            //var transform = Matrix4.Identity; // Creates a simple identity matrix

            //transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(20f));
            //transform *= Matrix4.CreateScale(1.1f);
            //transform *= Matrix4.CreateTranslation(0.1f, 0.1f, 0.0f);

            _texture.Use();
            //_texture2.Use(TextureUnit.Texture1);
            // Bind the shader
            _shader.Use();

            // Create the model matrix
            var model = Matrix4.Identity;
            model *= Matrix4.CreateTranslation(0, 0, -0.5f);
            model *= Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
            // Pass to vertex shader
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            // Draw everything
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            CursorGrabbed = true;
            Cursor = MouseCursor.Empty;

            // Swap to the other buffer (double buffering)
            SwapBuffers();

            base.OnRenderFrame(e);
        }

        // This function runs on every update frame.
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!Focused) return;

            // Gets the KeyboardState for this frame. KeyboardState allows us to check the status of keys.
            var input = Keyboard.GetState();

            // Check if the Escape button is currently being pressed.
            if(input.IsKeyDown(Key.Escape))
            {
                // If it is, exit the window.
                Exit();
            }

            const float cameraSpeed = 0.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Key.W)) _camera.Position += _camera.Front * cameraSpeed * (float)e.Time;
            if (input.IsKeyDown(Key.S)) _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time;
            if (input.IsKeyDown(Key.A)) _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time;
            if (input.IsKeyDown(Key.D)) _camera.Position += _camera.Right * cameraSpeed * (float)e.Time;
            if (input.IsKeyDown(Key.C)) _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time;
            if (input.IsKeyDown(Key.Space)) _camera.Position += _camera.Up * cameraSpeed * (float)e.Time;

            var mouse = Mouse.GetState();
            if(_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (Focused) Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
            base.OnMouseMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            // Resize the viewport
            GL.Viewport(0, 0, Width, Height);
            _camera.AspectRatio = Width / (float)Height;
            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            // Unbind all the resources
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete the resources
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_vertexArrayObject);

            GL.DeleteProgram(_shader.Handle);
            GL.DeleteTexture(_texture.Handle);
            //GL.DeleteTexture(_texture2.Handle);

            base.OnUnload(e);
        }
    }
}
