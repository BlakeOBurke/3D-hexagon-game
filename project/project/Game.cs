using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;

namespace project
{
    class Hex
    {
        static (int, int)[] axial_addjacent = new (int, int)[]
        {
                (1,0),
                (1,-1),
                (0,-1),
                (-1,0),
                (-1,1),
                (0,1),
        };


        public static Random rand = new Random();
        public static List<Hex> hexes = new List<Hex>();

        //axial coords

        public int q;
        public int r;
        public Hex(int Q, int R)
        {
            q = Q;
            r = R;
        }
        public Hex((int, int) a)
        {
            q = a.Item1;
            r = a.Item2;
        }

        static public bool isEqual(Hex a, Hex b)
        {
            return (a.q == b.q && a.r == b.r);
        }
        public bool isEqual(Hex a)
        {
            return (a.q == this.q && a.r == this.r);
        }
        static (int, int) add((int, int) A, (int, int) B)
        {
            return (A.Item1 + B.Item1, A.Item2 + B.Item2);
        }
        (int, int) add((int, int) B)
        {
            return (this.q + B.Item1, this.r + B.Item2);
        }
        static (int, int) subtract((int, int) A, (int, int) B)
        {
            return (A.Item1 - B.Item1, A.Item2 - B.Item2);
        }
        int distance((int, int) A, (int, int) B)
        {
            return (Math.Abs(A.Item1 - B.Item1) + Math.Abs(A.Item1 + A.Item2 - B.Item1 - B.Item2) + Math.Abs(A.Item2 - B.Item2)) / 2;
        }

        static (int, int) direction(int direction)
        {//in range 0->5
            return axial_addjacent[direction];
        }
        static (int, int) neighbor(Hex A, (int, int) direction)
        {
            return A.add(direction);
        }
        (int, int) neighbor((int, int) direction)
        {
            return this.add(direction);
        }
        public static List<(int, int)> RangeN((int, int) centre, int N)
        {
            List<(int, int)> A = new List<(int, int)>();
            for (int i = -N; i <= N; i++)
            {
                for (int j = Math.Max(-N, -i - N); j <= Math.Min(N, -i + N); j++)
                {
                    A.Add(add(centre, (i, j)));
                }
            }
            return A;
        }
    }

    public class Game : GameWindow
    {
        //self explanatory static variables


        static int screenHeight;
        static int screenWidth;

        static int mouseX, mouseY;

        int randomSeed;
        Random rnd;
        int mouseScroll = 0;
        static bool quickMov = false;
        int totalframeCount;

        Game.Camera camera = new Game.Camera(0, 0, 0);

        static Shader shader;
        static Shader shader2;

        //in the constructors there are many arbitrary re-assignments. this is because some of these are static to the Game class and would persist between games, reseting them fixes this.
        public Game(int width, int height) : base(width, height, GraphicsMode.Default, "game")
        {
            screenHeight = width; screenWidth = height; quickMov = false; mouseX = 0; mouseY = 0;
            rnd = new Random();
            camera = new Game.Camera(0, 0, 0);
            Shape.models.Add(new Shape("Cube.obj", randomColor()));
            Shape.models.Last().centre += new Vector3(-5, -5, -10);
        }


        Color randomColor()        //self explanatory
        {
            return Color.FromArgb(255,rnd.Next(50, 205), rnd.Next(50, 205), rnd.Next(50, 205));
        }




        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(Color.Coral);

            shader = new Shader("shader.vert.txt", "shader.frag.txt");
            shader2 = new Shader("shader.vert.txt", "shader2.frag.txt");


            GL.Enable(EnableCap.DepthTest);

            mouseX = 0; mouseY = 0;


            for (int i = 0; i < Hex.hexes.Count; i++)
            {
                if (Hex.hexes[i].q == 0 && Hex.hexes[i].r == 0)
                {
                    Hexagon.Hexagons.Add(new Hexagon("hex.obj", Color.Red, (Hex.hexes[i].q, Hex.hexes[i].r)));
                }
                else
                {
                    Hexagon.Hexagons.Add(new Hexagon("hex.obj", randomColor(), (Hex.hexes[i].q, Hex.hexes[i].r)));
                }
                Hexagon.Hexagons.Last().centre = new Vector3((Hex.hexes[i].q + Hex.hexes[i].r / 2f)*2 , -10, (float)Math.Sqrt(3) * (Hex.hexes[i].r));
                Hexagon.Hexagons.Last().angle = new Vector3(0, (float)Math.PI / 2f, 0);
                Hexagon.Hexagons.Last().scale *= 10/9f;
            }

        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            shader.Dispose();
            shader2.Dispose();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            //totalframeCount++;


            KeyboardState input = Keyboard.GetState();
            camera.FreeMouse();
            camera.FreeCam(input);


            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }
            Console.WriteLine(camera.pos.X);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {





            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 camMatrix = camera.CameraMatrix();

            //draw it ALL!

            List<(int,int)> A = Hex.RangeN((0, 0), 6);

            List<Hexagon> B = Hexagon.Hexagons.Where(x => A.Contains((x.qr.Item1, x.qr.Item2))).ToList();


            shader2 = new Shader("shader.vert.txt", "shader2.frag.txt");

            for (int i = 0; i < B.Count; i++)
            {
                B[i].highlightDraw(camMatrix);
            }
            B[B.Count-1].highlightDraw(camMatrix);

            shader = new Shader("shader.vert.txt", "shader.frag.txt");

            for (int i = 0; i < Hexagon.Hexagons.Count(); i++)
            {
                if (!B.Contains(Hexagon.Hexagons[i]))
                {
                    Hexagon.Hexagons[i].drawObj(camMatrix);
                }
            }

            for (int i = 0; i < Shape.models.Count(); ++i)
            {
                Shape.models[i].drawObj(camMatrix);
            }


            //reset the shader
            shader.Dispose();
            shader2.Dispose();

            this.SwapBuffers();
        }


        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, this.Width, this.Height);
        }

        struct vertex // bit self expanatory
        {
            public Vector3 pos;
            public Color color;
            public vertex(float x, float y, float z, Color Colors)
            {
                this.pos = new Vector3(x, y, z);
                this.color = Colors;
            }
            public vertex(Vector3 inp, Color Colors)
            {
                this.pos = inp;
                this.color = Colors;
            }

            public vertex(Vector4 a, Color b)
            {
                this.pos = new Vector3(a);
                this.color = b;
            }

        }
        class Camera // camera to get matrices for 3D 
        {
            public Vector3 pos;
            public Vector3 direction;
            public float fov;

            public Camera(float x, float y, float z)
            {
                this.pos = new Vector3(x, y, z);
                this.direction = new Vector3(0, 0, 0);
                ZoomReset();
            }

            public Vector3 CamForward() // the direction the camera is pointing, changing direction and forward affect this
            {
                Matrix4 rotation = Matrix4.CreateRotationZ(-direction[2]) * Matrix4.CreateRotationY(-direction[1]) * Matrix4.CreateRotationX(-direction[0]);
                Vector4 ouut = (rotation * new Vector4(0,0,1,1));
                return new Vector3(ouut);
            }
            //these three change FOV based on the speed and slow powerups
            public void ZoomFast()
            {
                fov = (float)(0.0174533 * 110);
            }
            public void ZoomReset()
            {
                fov = (float)(0.0174533 * 90);
            }
            public void ZoomSlow()
            {
                fov = (float)(0.0174533 * 75);
            }

            public Matrix4 CameraMatrix() // the projection matrix, if the game is over its slightly different as it looks to the centre of the world
            {
                Vector3 forw = this.CamForward();
                forw[0] += this.pos[0]; forw[1] += this.pos[1]; forw[2] += this.pos[2];
                Matrix4 camer = Matrix4.LookAt(new Vector3(this.pos[0], this.pos[1], this.pos[2]), new Vector3(forw[0], forw[1], forw[2]), new Vector3(0, 1, 0));
                camer *= Matrix4.CreatePerspectiveFieldOfView(fov, screenWidth / (float)screenHeight, 0.5f,200f);
                return camer;
            }
            public void FreeCam(KeyboardState input)
            {
                if (input.IsKeyDown(Key.W))
                {
                    Vector3 mov = this.CamForward();

                    if (quickMov)
                    {
                        mov = new Vector3(mov[0] * 10, mov[1] * 10, mov[2] * 10);
                    }


                    this.pos += new Vector3(mov[0], mov[1], mov[2]);
                }
                if (input.IsKeyDown(Key.S))
                {
                    Vector3 mov = this.CamForward();

                    if (quickMov)
                    {
                        mov = new Vector3(mov[0] * 10, mov[1] * 10, mov[2] * 10);
                    }

                    this.pos += new Vector3(-mov[0], -mov[1], -mov[2]);

                }
                if (input.IsKeyDown(Key.A))
                {
                    Vector3 mov = Vector3.Cross(this.CamForward(), new Vector3(0, -1, 0));

                    if (quickMov)
                    {
                        mov = new Vector3(mov[0] * 10, mov[1] * 10, mov[2] * 10);
                    }

                    this.pos += (new Vector3(mov[0], mov[1], mov[2]));

                }
                if (input.IsKeyDown(Key.D))
                {
                    Vector3 mov = Vector3.Cross(this.CamForward(), new Vector3(0, -1, 0));

                    if (quickMov)
                    {
                        mov = new Vector3(mov[0] * 10, mov[1] * 10, mov[2] * 10);
                    }

                    this.pos += new Vector3(-mov[0], -mov[1], -mov[2]);
                }

                if (input.IsKeyDown(Key.ShiftLeft))
                {
                    quickMov = true;
                }
                else if (input.IsKeyUp(Key.ShiftLeft))
                {
                    quickMov = false;
                }

                if (input.IsKeyDown(Key.T))
                {
                    this.fov -= 0.00174533f * 4;
                }
                else if (input.IsKeyDown(Key.G))
                {
                    this.fov += 0.00174533f * 4;
                }
            } 
            public void FreeMouse()
            {
                MouseState moose = Mouse.GetState();
                this.direction[1] -= (moose.X - mouseX) * 0.001f;
                mouseX = moose.X;

                this.direction[0] += (moose.Y - mouseY) * 0.001f;
                mouseY = moose.Y;

                if (this.direction[0] > Math.PI / 2 - 0.05f)
                {
                    this.direction[0] = (float)Math.PI / 2 - 0.05f;
                }
                else if (this.direction[0] < -Math.PI / 2 + 0.05f)
                {
                    this.direction[0] = (float)-Math.PI / 2 + 0.05f;
                }
            }
        }
        class Shader
        {
            public int Handle;

            public Shader(string vertexPath, string fragmentPath)
            {
                int VertexShader, FragmentShader;
                string VertexShaderSource = File.ReadAllText(vertexPath);
                //string VertexShaderSource = "#version 330 core\nlayout (location = 0) in vec3 aPos;   // the position variable has attribute position 0\nlayout(location = 1) in vec3 aColor; // the color variable has attribute position 1\nout vec3 ourColor; // output a color to the fragment shader\nuniform mat4 projection;\nvoid main()\n{\n    gl_Position = vec4(aPos, 1) * projection;\n    ourColor = aColor; // set ourColor to the input color we got from the vertex data\n}";

                string FragmentShaderSource = File.ReadAllText(fragmentPath);
                //string FragmentShaderSource = "#version 330 core\r\nout vec4 FragColor;  \r\nin vec3 ourColor;\r\n  \r\nvoid main()\r\n{\r\n    FragColor = vec4(ourColor, 0.2f);\r\n}";

                VertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(VertexShader, VertexShaderSource);

                FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(FragmentShader, FragmentShaderSource);


                GL.CompileShader(VertexShader);

                GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
                if (success == 0)
                {
                    string infoLog = GL.GetShaderInfoLog(VertexShader);
                    Console.WriteLine(infoLog);
                }

                GL.CompileShader(FragmentShader);

                GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out int ssuccess);
                if (ssuccess == 0)
                {
                    string infoLog = GL.GetShaderInfoLog(FragmentShader);
                    Console.WriteLine(infoLog);
                }




                Handle = GL.CreateProgram();

                GL.AttachShader(Handle, VertexShader);
                GL.AttachShader(Handle, FragmentShader);

                GL.LinkProgram(Handle);

                GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int sssuccess);
                if (sssuccess == 0)
                {
                    string infoLog = GL.GetProgramInfoLog(Handle);
                    Console.WriteLine(infoLog);
                }

                GL.DetachShader(Handle, VertexShader);
                GL.DetachShader(Handle, FragmentShader);
                GL.DeleteShader(FragmentShader);
                GL.DeleteShader(VertexShader);
            }
            public void Use()
            {
                GL.UseProgram(Handle);
            }
            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    GL.DeleteProgram(Handle);

                    disposedValue = true;
                }
            }

            ~Shader()
            {
                if (disposedValue == false)
                {
                    Console.WriteLine("GPU Resource leak");
                }
            }


            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        class Hexagon : Shape
        {
            public static List<Hexagon> Hexagons = new List<Hexagon>();
            public (int, int) qr;
            public Hexagon(string path, Color color, (int, int) qr) : base(path, color)
            {
                this.qr = qr;
            }

            public void highlightDraw(Matrix4 proj)
            {
                Matrix4 model = this.modelMat();
                //get the model matrix

                Matrix4 aproj = Matrix4.CreateScale(this.scale) * model * proj;
                //using matrix multiplication to apply the transformations

                int uniID = GL.GetUniformLocation(shader2.Handle, "projection");

                //Give the matrix to the GPU
                GL.UniformMatrix4(uniID, true, ref aproj);



                //instruct the GPU on what data to draw
                GL.BindVertexArray(this.vertexArrayObject);

                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBufferObject);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBufferObject);


                GL.VertexAttribPointer(this.vertexBufferObject, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexAttribArray(this.vertexBufferObject);

                GL.VertexAttribPointer(this.elementBufferObject, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(this.elementBufferObject);

                shader2.Use();

                GL.DrawElements(PrimitiveType.Triangles, this.count, DrawElementsType.UnsignedInt, 0);
            }
            public override void drawObj(Matrix4 proj)
            {//proj is the projection matrix, the same for all objects
                Matrix4 model = this.modelMat();
                //get the model matrix

                Matrix4 aproj = Matrix4.CreateScale(this.scale) * model * proj;
                //using matrix multiplication to apply the transformations

                int uniID = GL.GetUniformLocation(shader.Handle, "projection");

                //Give the matrix to the GPU
                GL.UniformMatrix4(uniID, true, ref aproj);



                //instruct the GPU on what data to draw
                GL.BindVertexArray(this.vertexArrayObject);

                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBufferObject);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBufferObject);


                GL.VertexAttribPointer(this.vertexBufferObject, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexAttribArray(this.vertexBufferObject);

                GL.VertexAttribPointer(this.elementBufferObject, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(this.elementBufferObject);

                shader.Use();

                GL.DrawElements(PrimitiveType.Triangles, this.count, DrawElementsType.UnsignedInt, 0);
            } //draw stuff
        }

        class Shape // Shape, many functions. Buffers are RAM on the GPU for storing triangles
        {
            public Vector3 angle;
            public int count;
            public vertex[] verts;
            uint[] triangle;

            //default direction and scales
            public Vector3 direction = new Vector3(0, 0, 1);
            public Vector3 scale = new Vector3(1, 1, 1);

            public Vector3 centre;

            public static List<int> shapes = new List<int>();
            public static List<Shape> models = new List<Shape>();


            public int vertexBufferObject;
            public int elementBufferObject;
            public int vertexArrayObject;

            public virtual void drawObj(Matrix4 proj)
            {//proj is the projection matrix, the same for all objects
                Matrix4 model = this.modelMat();
                //get the model matrix

                Matrix4 aproj = Matrix4.CreateScale(this.scale) * model * proj;
                //using matrix multiplication to apply the transformations

                int uniID = GL.GetUniformLocation(3, "projection");

                //Give the matrix to the GPU
                GL.UniformMatrix4(uniID, true, ref aproj);



                //instruct the GPU on what data to draw
                GL.BindVertexArray(this.vertexArrayObject);

                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBufferObject);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBufferObject);


                GL.VertexAttribPointer(this.vertexBufferObject, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexAttribArray(this.vertexBufferObject);

                GL.VertexAttribPointer(this.elementBufferObject, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(this.elementBufferObject);





                //gpu things
                shader.Use();
                GL.DrawElements(PrimitiveType.Triangles, this.count, DrawElementsType.UnsignedInt, 0);
            } //draw stuff

            //deconstructs .obj files
            public Shape(string path, Color color)
            {

                string[] inp = File.ReadAllLines(path);

                List<vertex> ver = new List<vertex>();

                List<uint> tria = new List<uint>();


                for (int i = 0; i < inp.Count(); i++)
                {
                    if (inp[i].Substring(0, 2) == "v ")
                    {
                        string[] point = inp[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        ver.Add(new vertex(new Vector3(float.Parse(point[1]), float.Parse(point[2]), float.Parse(point[3])), color ));
                    }
                    else if (inp[i].Substring(0, 2) == "f ")
                    {
                        string[] point = inp[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        tria.Add((uint)(int.Parse(point[1].Split('/')[0]) - 1));
                        tria.Add((uint)(int.Parse(point[2].Split('/')[0]) - 1));
                        tria.Add((uint)(int.Parse(point[3].Split('/')[0]) - 1));
                    }
                }

                this.verts = ver.ToArray();
                this.triangle = tria.ToArray();


                this.centre = avgPos();
                this.count = this.triangle.Count();

                this.angle = new Vector3(0, 0, 0);


                SetPosVerts(centre, angle);

                this.centre = avgPos();
                shapes.Add(count);



                doBuffers();

            }
            

            //allocates and sets data onto GPU
            //resetting buffers
            public void doBuffers()
            {

                this.vertexBufferObject = GL.GenBuffer();
                this.elementBufferObject = GL.GenBuffer();
                this.vertexArrayObject = GL.GenVertexArray();

                resetBuffers();

                verts = null;
                triangle = null;
            }
            public void resetBuffers()
            {
                float[] vertices = GetFloat();
                uint[] indices = triangle;


                GL.BindVertexArray(vertexArrayObject);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);

                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);


                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(1);
            }


            //gets the center possition of a shape as the average
            public Vector3 avgPos()
            {
                double x = 0; double y = 0; double z = 0;
                for (int i = 0; i < this.verts.Count(); i++)
                {
                    x += this.verts[i].pos[0];
                    y += this.verts[i].pos[1];
                    z += this.verts[i].pos[2];
                }
                return new Vector3((float)(x / this.verts.Count()), (float)(y / this.verts.Count()), (float)(z / this.verts.Count()));
            }  

            //moving the position of the object
            public void SetPos(Vector3 movement, Vector3 rotation)
            {
                this.angle[0] += rotation[0];
                this.angle[1] += rotation[1];
                this.angle[2] += rotation[2];

                this.direction = new Vector3(new Vector4(0, 0, 1, 0) * Matrix4.CreateRotationZ(this.angle[2]) * Matrix4.CreateRotationY(this.angle[1]) * Matrix4.CreateRotationX(this.angle[0]));

                this.centre += movement;
            }
            public void SetPos(Vector3 movement)
            {
                this.centre += movement;
            }
            public void SetPosVerts(Vector3 movement, Vector3 rotation)
            {

                Matrix4 moova1 = Matrix4.CreateTranslation(centre);
                Matrix4 moova2 = Matrix4.CreateTranslation(-(centre + movement));
                Matrix4 moverr = Matrix4.CreateRotationZ(rotation[2]) * Matrix4.CreateRotationY(rotation[1]) * Matrix4.CreateRotationX(rotation[0]);

                moverr = moova1 * moverr * moova2;
                angle += rotation;

                for (int i = 0; i < verts.Count(); i++)
                {
                    verts[i] = new vertex(moverr * (new Vector4(verts[i].pos[0], verts[i].pos[1], verts[i].pos[2], 1)), verts[i].color);
                }

                this.direction = new Vector3(new Vector4(0, 0, 1, 0) * Matrix4.CreateRotationZ(this.angle[2]) * Matrix4.CreateRotationY(this.angle[1]) * Matrix4.CreateRotationX(this.angle[0]));

                centre += movement;
            }
            public virtual void Scale(Vector3 scale)
            {
                this.scale = this.scale * scale;
            }
            public virtual void Scale(float scale)
            {
                this.scale = scale * this.scale;
            }
            public virtual void setScale(float scale)
            {
                this.scale = new Vector3(scale, scale, scale);
            }
            public float[] GetFloat()
            {
                //gets all of the information about the vertices to send to the graphics as a single array
                float[] result = new float[count * 6];
                for (int i = 0; i < verts.Count(); i++)
                {
                    result[i * 6] = verts[i].pos[0];
                    result[i * 6 + 1] = verts[i].pos[1];
                    result[i * 6 + 2] = verts[i].pos[2];
                    result[i * 6 + 3] = verts[i].color.R / 255f;
                    result[i * 6 + 4] = verts[i].color.G / 255f;
                    result[i * 6 + 5] = verts[i].color.B / 255f;
                }

                return result;
            }

            public virtual Matrix4 modelMat()
            {
                Shape shapee = this;
                Matrix4 mov = Matrix4.CreateRotationZ(this.angle[2]) * Matrix4.CreateRotationY(this.angle[1]) * Matrix4.CreateRotationX(this.angle[0]) * Matrix4.CreateTranslation(this.centre);
                return mov;
            }

            public Vector3 getForward()
            {
                Matrix3 rotation = Matrix3.CreateRotationZ(-angle[2]) * Matrix3.CreateRotationY(-angle[1]) * Matrix3.CreateRotationX(-angle[0]);
                Vector3 ouut = (rotation * new Vector3(0, 0, 1));
                return ouut;
            }
        }



        //static void Collide(Terrain terrain, Shape Shape, out float height)
        //{

        //    Collision(terrain, Shape.centre, out float height1, out Vector3 normal);

        //    height = height1;

        //}
        //static float[] Barycentric(Vector3 A, Vector3 B, Vector3 C, Vector3 position)//gets the barycentric coordinates of a point on a triangle for use in the smooth terrain collision
        //{
        //    //since the terrain is scaled, it needs to be resized
        //    A *= Terrain.squareSize;
        //    B *= Terrain.squareSize;
        //    C *= Terrain.squareSize;
        //    //takes into terrain coords
        //    position += (Terrain.squareSize / 2f) * new Vector3(Terrain.gridDimension, 0, Terrain.gridDimension);
        //    float[] barycentrics = new float[3];

        //    float T = (A.X - C.X) * (B.Z - C.Z) - (A.Z - C.Z) * (B.X - C.X);

        //    barycentrics[0] = ((B.Z - C.Z) * (position.X - C.X) + (C.X - B.X) * (position.Z - C.Z)) / T;
        //    barycentrics[1] = ((C.Z - A.Z) * (position.X - C.X) + (A.X - C.X) * (position.Z - C.Z)) / T;
        //    barycentrics[2] = 1f - barycentrics[0] - barycentrics[1];

        //    return barycentrics;
        //}
        //static void Collision(Terrain terrain, Vector3 B, out float Height, out Vector3 normal) //gets height and normal vector of terrain area that the car/ object is currently in
        //{
        //    //matrix method for barycentric coordinates, formulas from wikipedia

        //    //get square on grid for 

        //    //convert shape coordinates to one square on the terrain

        //    //terrain has 
        //    //Terrain.gridDimension
        //    //^2 pieces
        //    //centres at 0,0

        //    //each terrain piece is 5 long
        //    //add (gridDimension/2) to the shapes centre then divide by 5 and it will be in relative terrain world with each corner of a terrain piece being easily indedxable from the array 
        //    //check if outside the range (0,0) to (gridDimension, gridDimension)

        //    float X = B.X / Terrain.squareSize + ((Terrain.gridDimension + 1) / 2);
        //    float Z = B.Z / Terrain.squareSize + ((Terrain.gridDimension + 1) / 2);

        //    int X_ = (int)Math.Floor(X);
        //    int Z_ = (int)Math.Floor(Z);


        //    //decide bottom right or top left triangle
        //    //corners
        //    Vector3[] c = new Vector3[3];
        //    float[] barry;
        //    float X_L = X - X_;
        //    float Z_L = Z - Z_;
        //    if (X_L < Z_L)
        //    {
        //        //bottom right
        //        c[1] = terrain.terrain.verts[X_ * (Terrain.gridDimension + 1) + Z_].pos;
        //        c[2] = terrain.terrain.verts[(X_ + 1) * (Terrain.gridDimension + 1) + (Z_ + 1)].pos;
        //        c[0] = terrain.terrain.verts[X_ * (Terrain.gridDimension + 1) + (Z_ + 1)].pos;

        //        barry = Barycentric(c[0], c[1], c[2], B);
        //        normal = Vector3.Cross(c[2] - c[0], c[1] - c[0]);
        //    }
        //    else
        //    {
        //        //top left
        //        c[2] = terrain.terrain.verts[(X_ + 1) * (Terrain.gridDimension + 1) + (Z_ + 1)].pos;
        //        c[0] = terrain.terrain.verts[(X_ + 1) * (Terrain.gridDimension + 1) + Z_].pos;
        //        c[1] = terrain.terrain.verts[X_ * (Terrain.gridDimension + 1) + Z_].pos;

        //        barry = Barycentric(c[0], c[1], c[2], B);
        //        normal = Vector3.Cross(c[1] - c[0], c[2] - c[0]);
        //    }

        //    Height = c[0].Y * barry[0] + c[1].Y * barry[1] + c[2].Y * barry[2];
        //    Height *= Terrain.squareSize;
        //}


        public class Perlin
        {
            //perlin noise does not work if the only points sampled are integers
            public static float perlinFloatifiser = 2.5f;
            public static float[,] DoPerlin(float[,] c, float xOffset, float yOffset, int octaves)
            {
                float[,] a = new float[c.GetLength(0), c.GetLength(1)];
                for (int i = 0; i < a.GetLength(0); i++)
                {
                    for (int j = 0; j < a.GetLength(1); j++)
                    {
                        float x = xOffset + perlinFloatifiser * i / (float)a.GetLength(0);
                        float y = yOffset + perlinFloatifiser * j / (float)a.GetLength(1);

                        //half amplitude and double frequency of each octave
                        a[i, j] = SampleNoise(x, y, 1, 1, octaves, 2f);

                        //function that manipulates the output to make it nicer
                        a[i, j] = (float)Math.Pow(Math.E, a[i, j]);

                    }
                }

                return a;
            }

            static float SampleNoise(float x, float y, float amplitude, float frequency, int octaves, float amplitudeModifier)
            {
                float value = 0;
                //add multiple 'octaves' to make it looks fancy
                for (int i = 0; i < octaves; i++)
                {
                    value += amplitude * perlin(x * frequency, y * frequency);
                    amplitude /= amplitudeModifier;
                    frequency *= amplitudeModifier;
                }
                //divide by 2 - (1/2)^octaves to return to range (-1 -> 1)
                value = value / (float)(2 - Math.Pow(.5f, octaves - 1));
                return value;
            }

            public static float perlin(float x, float y)
            {
                //corners of a grid 
                int xFloor = (int)Math.Floor(x);
                int xCeiling = xFloor + 1;
                int yFloor = (int)Math.Floor(y);
                int yCeiling = yFloor + 1;

                //0-1 how far along from the bottom left side is the point
                float x_Offset = x - (float)xFloor;
                float y_Offset = y - (float)yFloor;


                //dot products find the value for the botton line of the square
                float bottomLeft = Dot_position_random(xFloor, yFloor, x, y);
                float bottomRight = Dot_position_random(xCeiling, yFloor, x, y);
                float interpolate1 = Lerp(bottomLeft, bottomRight, x_Offset);

                //dot products find the value for the botton line of the square
                float topLeft = Dot_position_random(xFloor, yCeiling, x, y);
                float topRight = Dot_position_random(xCeiling, yCeiling, x, y);
                float interpolate2 = Lerp(topLeft, topRight, x_Offset);

                float value = Lerp(interpolate1, interpolate2, y_Offset);

                //from range -1 -> 1   to 0 -> 1
                return (value / 2f + 0.5f);
            }

            static Vector2 RandomGradient(int ix, int iy)
            {
                //PSEUDO random direction vector -> same ix and iy = same vector 

                Random randx = new Random(ix);
                Random randy = new Random(iy);
                Random rar = new Random(randx.Next() * randy.Next());

                Vector2 v = new Vector2((float)Math.Cos(rar.Next() * 180 / Math.PI), (float)(Math.Sin(rar.Next() * 180 / Math.PI)));
                return v;
            }

            static float Lerp(float a0, float a1, float w)
            {
                //lerp means linear interpolate, it isnt linearly interpolating cause I changed my mind
                if (0.0 > w) return a0;
                if (1.0 < w) return a1;


                //meant to be smoother instead of a linear mapping 

                return (float)((a1 - a0) * (3.0 - w * 2.0) * w * w + a0);
            }

            static float Dot_position_random(int x_Offset, int y_Offset, float x, float y)
            {
                Vector2 gradient = RandomGradient(x_Offset, y_Offset);
                return Vector2.Dot(new Vector2(x - x_Offset, y - y_Offset), gradient);
            }


        }
    }
}


