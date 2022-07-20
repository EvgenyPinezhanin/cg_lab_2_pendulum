using System;
using System.Drawing;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

using ObjLoad;

namespace Pendulum
{
    sealed class Program : GameWindow
    {
        private float[] verticesNormalsText;
        private int[] elements;

        private int VertexBufferObject;
        private int VertexArrayObject;
        private int ElementsBufferObject;

        private Shader ShaderProgram;
        private Texture diffuseMap;
        private Texture specularMap;

        private Camera camera;
        private Vector3 startCameraPos = new Vector3(0.0f, 0.0f, 7.0f);
        private const float cameraSpeed = 2.5f;
        private const float sensitivity = 0.1f;
        private bool firstMove = true;
        private Vector2 lastMousePos;

        private readonly float startAngle = 0.0f;
        private readonly float startSpeed = 150.0f;
        private readonly float k = 2.0f;
        private bool isStop = false;
        private bool isPressF = false;

        private float angle;
        private float time;

        private readonly float[] lamp = {
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
        };

        private Shader ShaderProgramLamp;

        private Vector3 lampPos = new Vector3(0.0f, 2.0f, 0.0f);

        private int VertexArrayObjectLamp;
        private int VertexBufferObjectLamp;

        public Program(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) {
            ClientSize = new Size(width, height);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Obj obj = new Obj(AppDomain.CurrentDomain.BaseDirectory + "resource/pendulum.obj");
            obj.Load();
            Model3D Model = obj.GetModel3D();
            
            verticesNormalsText = Model.GetVerticesNormalsText();
            elements = Model.GetTriangles();

            // Загрузка диффурной карты
            diffuseMap = new Texture(AppDomain.CurrentDomain.BaseDirectory + "resource/texture.png");
            diffuseMap.Use(TextureUnit.Texture0);
            diffuseMap.TextParameterWrap((int)TextureWrapMode.Repeat, (int)TextureWrapMode.Repeat);
            diffuseMap.TextParameterFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
            diffuseMap.GenerateTexture();

            // Загрузка карты бликов
            specularMap = new Texture(AppDomain.CurrentDomain.BaseDirectory + "resource/texture_mirror.png");
            specularMap.Use(TextureUnit.Texture1);
            specularMap.TextParameterWrap((int)TextureWrapMode.Repeat, (int)TextureWrapMode.Repeat);
            specularMap.TextParameterFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
            specularMap.GenerateTexture();

            // Сборка шейдерной программы (компиляция и линковка двух шейдеров)
            ShaderProgram = new Shader(AppDomain.CurrentDomain.BaseDirectory + "shaders/shader.vert",
                                       AppDomain.CurrentDomain.BaseDirectory + "shaders/shader.frag");

            ShaderProgramLamp = new Shader(AppDomain.CurrentDomain.BaseDirectory + "shaders/shader_lamp.vert",
                                           AppDomain.CurrentDomain.BaseDirectory + "shaders/shader_lamp.frag");

            ShaderProgram.SetInt("material.diffuse", 0);
            ShaderProgram.SetInt("material.specular", 0);

            // Создание Vertex Array Object и его привязка
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            // Создание объекта буфера вершин/нормалей/текстур, его привязка и заполнение
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, verticesNormalsText.Length * sizeof(float), verticesNormalsText, BufferUsageHint.StaticDraw);

            // Указание OpenGL, где искать вершины в буфере вершин/нормалей/текстур
            int positionLocation = ShaderProgram.GetAttribLocation("vPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            // Указание OpenGL, где искать нормали в буфере вершин/нормалей/текстур
            int normalLocation = ShaderProgram.GetAttribLocation("vNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            // Указание OpenGL, где искать текстуры в буфере вершин/нормалей/текстур
            int textLocation = ShaderProgram.GetAttribLocation("vTexCoord");
            GL.EnableVertexAttribArray(textLocation);
            GL.VertexAttribPointer(textLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

            // Создание, привязка и заполнение объекта-буфера элементов для треугольников
            ElementsBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementsBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, elements.Length * sizeof(int), elements, BufferUsageHint.StaticDraw);

            // Аналогичные действия для точечного источника света 
            VertexArrayObjectLamp = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObjectLamp);

            VertexBufferObjectLamp = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObjectLamp);
            GL.BufferData(BufferTarget.ArrayBuffer, lamp.Length * sizeof(float), lamp, BufferUsageHint.StaticDraw);

            int vertexLocation = ShaderProgramLamp.GetAttribLocation("vPos");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            // Set the clear color to gray
            GL.ClearColor(0.15f, 0.15f, 0.15f, 0.0f);
            // Включение теста глубины во избежание наложений
            GL.Enable(EnableCap.DepthTest);

            // Установка стартового положения камеры
            camera = new Camera(startCameraPos, (float)Size.Width / Size.Height);

            // Захват и сокрытие курсора
            CursorGrabbed = true;
            CursorVisible = false;

            // Задание стартовых значений
            angle = startAngle;
            time = 0.0f;
        }

        protected override void OnUnload(EventArgs e)
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.UseProgram(0);

            // Delete all the resources.
            GL.DeleteVertexArray(VertexArrayObject);
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteBuffer(ElementsBufferObject);
            ShaderProgram.Dispose();
            diffuseMap.Dispose();
            specularMap.Dispose();

            GL.DeleteVertexArray(VertexArrayObjectLamp);
            GL.DeleteBuffer(VertexBufferObjectLamp);
            ShaderProgramLamp.Dispose();

            base.OnUnload(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // Clear the color buffer and depth.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Bind the VAO
            GL.BindVertexArray(VertexArrayObject);
            // Use/Bind the program
            ShaderProgram.Use();

            // Матрица модели с заданным масштабированием и поворотом
            Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(angle));
            ShaderProgram.SetMatrix4("model", model);
            // Матрица перехода в пространство вида - "eye space"
            ShaderProgram.SetMatrix4("view", camera.GetViewMatrix());
            // Матрица проекции на систему координат от -1 до 1 по x и y
            ShaderProgram.SetMatrix4("projection", camera.GetProjectionMatrix());
            //(позиция наблюдателя)
            ShaderProgram.SetVector3("viewPos", camera.Position);

            // Параметры света
            ShaderProgram.SetVector3("light.position", lampPos);
            ShaderProgram.SetVector3("light.ambient", new Vector3(0.2f, 0.2f, 0.2f));
            ShaderProgram.SetVector3("light.diffuse", new Vector3(0.5f, 0.5f, 0.5f));
            ShaderProgram.SetVector3("light.specular", new Vector3(1.0f, 1.0f, 1.0f));
            ShaderProgram.SetFloat("light.constant", 1.0f);
            ShaderProgram.SetFloat("light.linear", 0.09f);
            ShaderProgram.SetFloat("light.quadratic", 0.032f);

            // Свойства материала ()
            //Цвет блика
            ShaderProgram.SetVector3("material.specular", new Vector3(0.508273f, 0.508273f, 0.508273f));
            //Сила блеска
            ShaderProgram.SetFloat("material.shininess", 1.0f);

            // This draws the triangles
            GL.DrawElements(PrimitiveType.Triangles, elements.Length, DrawElementsType.UnsignedInt, 0);

            //Draw the lamp
            GL.BindVertexArray(VertexArrayObjectLamp);

            ShaderProgramLamp.Use();
            
            Matrix4 modelLamp = Matrix4.CreateScale(0.2f) * Matrix4.CreateTranslation(lampPos);
            
            ShaderProgramLamp.SetMatrix4("model", modelLamp);
            ShaderProgramLamp.SetMatrix4("view", camera.GetViewMatrix());
            ShaderProgramLamp.SetMatrix4("projection", camera.GetProjectionMatrix());

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3 * lamp.Length);

            // Swap the front/back buffers so what we just rendered to the back buffer is displayed in the window.
            Context.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (!Focused)
            {
                return;
            }

            KeyboardState inputKey = Keyboard.GetState();

            if (inputKey.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            // Обновление значений угла отклонения маятника
            if (!isStop)
            {
                time += (float)e.Time;
                MathHelper.DegreesToRadians(startAngle);
                angle = (float)(MathHelper.DegreesToRadians(startAngle) * Math.Cos(Math.Sqrt(k) * time)
                      + startSpeed / Math.Sqrt(k) * Math.Sin(Math.Sqrt(k) * time));
            }

            if (inputKey.IsKeyDown(Key.W))
                camera.Position += camera.Front * cameraSpeed * (float)e.Time;
            if (inputKey.IsKeyDown(Key.S))
                camera.Position -= camera.Front * cameraSpeed * (float)e.Time;
            if (inputKey.IsKeyDown(Key.A))
                camera.Position -= camera.Right * cameraSpeed * (float)e.Time;
            if (inputKey.IsKeyDown(Key.D))
                camera.Position += camera.Right * cameraSpeed * (float)e.Time;
            if (inputKey.IsKeyDown(Key.LShift))
                camera.Position -= camera.Up * cameraSpeed * (float)e.Time;
            if (inputKey.IsKeyDown(Key.Space))
                camera.Position += camera.Up * cameraSpeed * (float)e.Time;

            Vector3 rl = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 fb = new Vector3(0.0f, 0.0f, 1.0f);
            Vector3 ud = new Vector3(0.0f, 1.0f, 0.0f);
            if (inputKey.IsKeyDown(Key.Up))
                lampPos -= fb * cameraSpeed * (float)e.Time;
            if (inputKey.IsKeyDown(Key.Down))
                lampPos += fb * cameraSpeed * (float)e.Time;
            if (inputKey.IsKeyDown(Key.Left))
                lampPos -= rl * cameraSpeed * (float)e.Time;
            if (inputKey.IsKeyDown(Key.Right))
                lampPos += rl * cameraSpeed * (float)e.Time;
            if (inputKey.IsKeyDown(Key.E))
                lampPos -= ud * cameraSpeed * (float)e.Time;
            if (inputKey.IsKeyDown(Key.Q))
                lampPos += ud * cameraSpeed * (float)e.Time;

            // Остановить/запустить маятник
            if (inputKey.IsKeyDown(Key.F))
            {
                isPressF = true;
            }
            if (inputKey.IsKeyUp(Key.F))
            {
                if (isPressF == true)
                {
                    isPressF = false;
                    isStop = !isStop;
                }
            }

            MouseState inputMouse = Mouse.GetState();
            
            if (firstMove)
            {
                lastMousePos = new Vector2(inputMouse.X, inputMouse.Y);
                firstMove = false;
            }
            else
            {
                var deltaX = inputMouse.X - lastMousePos.X;
                var deltaY = inputMouse.Y - lastMousePos.Y;
                lastMousePos = new Vector2(inputMouse.X, inputMouse.Y);
            
                camera.Yaw += deltaX * sensitivity;
                camera.Pitch -= deltaY * sensitivity;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            camera.Fov -= e.DeltaPrecise;
            if (camera.Fov >= 90.0f)
            {
                camera.Fov = 90.0f;
            }
            else if (camera.Fov <= 10.0f)
            {
                camera.Fov = 10.0f;
            }

            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (Focused)
            {
                Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
            }

            base.OnMouseMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.Width, Size.Height);
            camera.AspectRatio = (float)Size.Width / Size.Height;
        }
    }
}