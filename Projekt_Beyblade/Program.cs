using Projekt_Beyblade.Camera;
using Projekt_Beyblade.Figures.ArrangementModels;
using Projekt_Beyblade.Figures;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

using System;
using System.Media;

using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using NAudio.Wave;

namespace Projekt_Beyblade
{
    internal static class Program
    {

        private static CameraDescriptor _cameraDescriptor = new();

        private static bool _internalView = false;

        private static BladeArrangementModel _arrangementModel = new();

        private static IWindow _window;

        private static IInputContext _inputContext;

        private static GL _Gl;

        private static ImGuiController _controller;

        private static uint _program;

        private static List<BeyBlade> beyBlades;

        private static BeyBlade bigBoss;

        // private static BeyBladeBigBoss bigBoss;

        private static Vector3D<float> bladePosition;

        private static GlCube plato;

        private static GlCube _skyBox;

        // first is left, second is right, third is forward, fourth is backward
        private static bool[] directions = { false, false, false, false };

        private static int numberOfHits = 0;

        private static float Shininess = 50;

        // tavolsagok - ertekek
        private static float AmbientStrength = 0.5f;
        private static float DiffuseStrength = 0.15f;
        private static float SpecularStrength = 0.75f;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string TextureUniformVariableName = "uTexture";

        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";

        private const string AmbientStrengthVariableName = "ambientStrength";
        private const string DiffuseStrengthVariableName = "diffuseStrength";
        private const string SpecularStrengthVariableName = "specularStrength";

        private const int n = 2;

        // zene
        // SoundPlayer soundPlayer = new SoundPlayer();

        // NAudio
        private static WaveOutEvent _outputDevice;
        private static AudioFileReader _audioFile;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "BeyBlades";
            windowOptions.Size = new Vector2D<int>(1000, 1000);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            _window = Window.Create(windowOptions);

            _window.Load += Window_Load;
            _window.Update += Window_Update;
            _window.Render += Window_Render;
            _window.Closing += Window_Closing;


            // audio resz

            /*
            //using (var audioFile = new AudioFileReader("Projekt_Beyblade.Resources.example.wav"))
            using (var audioFile = new AudioFileReader("example.wav"))
            using (var outputDevice = new WaveOutEvent()) 
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();

                Console.WriteLine("Playing...");
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }*/

            // Hang lejátszás külön szálon
            //Thread audioThread = new Thread(PlayAudio);
            //audioThread.Start();

            //PlayAudio2();
            PlayBaseAudio();

            _window.Run();
        }
        private static void Window_Load()
        {
            _inputContext = _window.CreateInput();
            foreach (var keyboard in _inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
                keyboard.KeyUp += Keyboard_KeyUp;
            }

            _Gl = _window.CreateOpenGL();

            _controller = new ImGuiController(_Gl, _window, _inputContext);

            _window.FramebufferResize += s =>
            {
                _Gl.Viewport(s);
            };

            _Gl.ClearColor(System.Drawing.Color.Black);

            SetUpObjects(n);

            _arrangementModel.StartRotation(); // bb-k forognak a tengelyuk korul

            _cameraDescriptor.SetExternalView();

            bladePosition = new Vector3D<float>(0, 0, 0);

            LinkProgram();

            _Gl.Enable(EnableCap.DepthTest);
            _Gl.DepthFunc(DepthFunction.Lequal);
            _Gl.Enable(EnableCap.Blend);
            _Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // _controller.SetWindowSize()
            // ImGui.SetWindowSize();
        }

        private static void PlayAudio()
        {
            using (var audioFile = new AudioFileReader("example.wav"))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();

                Console.WriteLine("Playing...");
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        private static async Task PlayAudio2(string file)
        {
            using (var audioFile = new AudioFileReader(file))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();

                Console.WriteLine("Playing...");
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    //System.Threading.Thread.Sleep(1000);
                    await Task.Delay(1000);
                }
            }
        }

        private static async Task PlayBaseAudio()
        {
            while (true)
            {
                try
                {
                    _audioFile = new AudioFileReader("example.wav");
                    _outputDevice = new WaveOutEvent();
                    _outputDevice.Init(_audioFile);
                    _outputDevice.Play();

                    Console.WriteLine("Playing...");
                    while (_outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        await Task.Delay(1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error playing audio: " + ex.Message);
                }
                finally
                {
                    _audioFile?.Dispose();
                    _outputDevice?.Dispose();
                }

                // Kis késleltetés a loop újraindítása előtt
                await Task.Delay(500);
            }
        }

        private static void PlayAudio22()
        {
            try
            {
                _audioFile = new AudioFileReader("Projekt_Beyblade.Resources.example.wav");
                _outputDevice = new WaveOutEvent();
                _outputDevice.Init(_audioFile);
                _outputDevice.Play();

                Console.WriteLine("Playing...");
                while (_outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error playing audio: " + ex.Message);
            }
            finally
            {
                _audioFile?.Dispose();
                _outputDevice?.Dispose();
            }
        }

        private static void LinkProgram()
        {
            uint vshader = _Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = _Gl.CreateShader(ShaderType.FragmentShader);

            _Gl.ShaderSource(vshader, ReadShader("VertexShader.vert"));
            _Gl.CompileShader(vshader);
            _Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + _Gl.GetShaderInfoLog(vshader));

            _Gl.ShaderSource(fshader, ReadShader("FragmentShader.frag"));
            _Gl.CompileShader(fshader);

            _program = _Gl.CreateProgram();
            _Gl.AttachShader(_program, vshader);
            _Gl.AttachShader(_program, fshader);
            _Gl.LinkProgram(_program);
            _Gl.GetProgram(_program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {_Gl.GetProgramInfoLog(_program)}");
            }
            _Gl.DetachShader(_program, vshader);
            _Gl.DetachShader(_program, fshader);
            _Gl.DeleteShader(vshader);
            _Gl.DeleteShader(fshader);
        }

        private static string ReadShader(string shaderFileName)
        {
            using (Stream shaderStream = typeof(Program).Assembly.GetManifestResourceStream("Projekt_Beyblade.Shaders." + shaderFileName))
            using (StreamReader shaderReader = new StreamReader(shaderStream))
                return shaderReader.ReadToEnd();
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            // mozgasok + kamera
            switch (key)
            {
                case Key.Left:
                    Console.WriteLine("Kamera balra");
                    _cameraDescriptor.DecreaseZYAngle();
                    break;
                case Key.Right:
                    Console.WriteLine("Kamera jobbra");
                    _cameraDescriptor.IncreaseZYAngle();
                    break;
                case Key.Down:
                    Console.WriteLine("Kamera le");
                    _cameraDescriptor.IncreaseDistance();
                    break;
                case Key.Up:
                    Console.WriteLine("Kamera fel");
                    _cameraDescriptor.DecreaseDistance();
                    break;
                case Key.M:
                    _cameraDescriptor.IncreaseZXAngle();
                    break;
                case Key.N:
                    _cameraDescriptor.DecreaseZXAngle();
                    break;
                case Key.A:
                    directions[0] = true;
                    break;
                case Key.D:
                    directions[1] = true;
                    break;
                case Key.S:
                    directions[2] = true;
                    break;
                case Key.W:
                    directions[3] = true;
                    break;
            }
        }

        private static void Keyboard_KeyUp(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.A:
                    directions[0] = false;
                    break;
                case Key.D:
                    directions[1] = false;
                    break;
                case Key.S:
                    directions[2] = false;
                    break;
                case Key.W:
                    directions[3] = false;
                    break;
                    /*
                case Key.Left:
                    _cameraDescriptor.DecreaseZYAngle();
                    break;
                    ;
                case Key.Right:
                    _cameraDescriptor.IncreaseZYAngle();
                    break;
                case Key.Down:
                    _cameraDescriptor.IncreaseDistance();
                    break;
                case Key.Up:
                    _cameraDescriptor.DecreaseDistance();
                    break;*/
                /*
                 * case Key.U:
                    _cameraDescriptor.IncreaseZXAngle();
                    break;
                case Key.D:
                    _cameraDescriptor.DecreaseZXAngle();
                    break;
                */
            }
        }

        private static void Window_Update(double deltaTime)
        {
            MoveBigBoss();

            if (_internalView)
            {
                _cameraDescriptor.SetInternalView(bigBoss.beyBlade.Position);
                //_cameraDescriptor.SetInternalView(bigBoss.beyBladeBigBoss.Position);
            }
            else
            {
                _cameraDescriptor.SetExternalView();
            }

            beyBlades.ForEach(beyBlade => beyBlade.UpdatePosition((float)deltaTime));

            bigBoss.beyBlade.Position = bladePosition;

            _arrangementModel.UpdateRotation(deltaTime);

            _controller.Update((float)deltaTime);
        }

        private static void MoveBigBoss()
        {
            // figura mozgatass a 4 iranyban
            if (directions[0])
            {
                bladePosition.X -= 0.2f;
            }
            if (directions[1])
            {
                bladePosition.X += 0.2f;
            }
            if (directions[2])
            {
                bladePosition.Z += 0.2f;
            }
            if (directions[3])
            {
                bladePosition.Z -= 0.2f;
            }
        }

        private static unsafe void Window_Render(double deltaTime)
        {
            _Gl.Clear(ClearBufferMask.ColorBufferBit);
            _Gl.Clear(ClearBufferMask.DepthBufferBit);


            _Gl.UseProgram(_program);

            SetViewMatrix();
            SetProjectionMatrix();

            // alap fgvk 
            SetLightColor();
            SetLightPosition();
            SetViewerPosition();
            SetShininess();
            SetLigthingParams();

            // baybladek 
            DrawBeyBlades((float)deltaTime);
            DrawBigBoss((float)deltaTime);

            //DrawPlato(); // ground
            DrawPlato2();

            DrawSkyBox(); // skybox vilag

            CheckBigBossCollision(); // talalat/utkozes

            if (ImGuiNET.ImGui.Button("Internal View"))
            {
                _internalView = true;
            }
            if (ImGuiNET.ImGui.Button("External View"))
            {
                _internalView = false;
                _cameraDescriptor.SetExternalView();
            }
            ImGuiNET.ImGui.Text($"Number of hits: {numberOfHits}");

            if (numberOfHits == n)
            {
                ImGuiNET.ImGui.Text("WIN!!!!");
            }

            ImGuiNET.ImGui.End();

            _controller.Render();
        }

        private static void CheckBigBossCollision()
        {
            // ha utkoztunk egy masik figuraval

            for (int i = 0; i < beyBlades.Count; i++)
            {
                if (beyBlades[i].gata)
                {
                    continue;
                }

                if (Math.Abs(beyBlades[i].beyBlade.Position.X - bladePosition.X) < 3f && Math.Abs(beyBlades[i].beyBlade.Position.Z - bladePosition.Z) < 3f)
                {
                    numberOfHits++; //noveljuk a szamot
                    beyBlades[i].gata = true; //beallitjuk "meghaltnak"

                    PlayAudio2("eat.wav");
                }
            }
        }

        private static unsafe void DrawSkyBox()
        {
            // kirajzoljuk a vilagot
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(400f);
            SetModelMatrix(modelMatrix);
            _Gl.BindVertexArray(_skyBox.Vao);

            int textureLocation = _Gl.GetUniformLocation(_program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            _Gl.Uniform1(textureLocation, 0);

            _Gl.ActiveTexture(TextureUnit.Texture0);
            _Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            _Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            _Gl.BindTexture(TextureTarget.Texture2D, _skyBox.Texture.Value);

            _Gl.DrawElements(GLEnum.Triangles, _skyBox.IndexArrayLength, GLEnum.UnsignedInt, null);
            _Gl.BindVertexArray(0);

            CheckError();
            _Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void DrawPlato2()
        {
            var modelMatrix = Matrix4X4.CreateScale(2f, 2f, 2f);
            SetModelMatrix(modelMatrix);
            _Gl.BindVertexArray(plato.Vao);



            // Set the texture uniform
            int textureLocation = _Gl.GetUniformLocation(_program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            _Gl.Uniform1(textureLocation, 0);



            // Activate and bind the texture
            _Gl.ActiveTexture(TextureUnit.Texture0);
            _Gl.BindTexture(TextureTarget.Texture2D, plato.Texture.Value);



            _Gl.DrawElements(GLEnum.Triangles, plato.IndexArrayLength, GLEnum.UnsignedInt, null);
            _Gl.BindVertexArray(0);



            CheckError();
            _Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void DrawBeyBlades(float delta)
        {
            // a kisebb figurak
            for (int i = 0; i < beyBlades.Count; i++)
            {
                if (beyBlades[i].gata)
                {
                    continue;
                }

                // skalazas
                var scale = Matrix4X4.CreateScale(0.7f);
                var rotLocY = Matrix4X4.CreateRotationY((float)_arrangementModel.SliceRotationAngle); // y tengely szerinti
                var translationMatrix = Matrix4X4.CreateTranslation(beyBlades[i].beyBlade.Position); // eltolas 
                var modelMatrix = scale * rotLocY * translationMatrix;

                SetModelMatrix(modelMatrix);

                _Gl.BindVertexArray(beyBlades[i].beyBlade.Vao);

                int textureLocation = _Gl.GetUniformLocation(_program, TextureUniformVariableName);
                if (textureLocation == -1)
                {
                    throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
                }

                _Gl.Uniform1(textureLocation, 0);

                _Gl.ActiveTexture(TextureUnit.Texture0);
                _Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
                _Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
                _Gl.BindTexture(TextureTarget.Texture2D, beyBlades[i].beyBlade.Texture.Value);

                _Gl.DrawElements(GLEnum.Triangles, beyBlades[i].beyBlade.IndexArrayLength, GLEnum.UnsignedInt, null);
                _Gl.BindVertexArray(0);

                CheckError();
                _Gl.BindTexture(TextureTarget.Texture2D, 0);
                CheckError();
            }
        }

        private static unsafe void DrawBigBoss(float delta)
        {
            // a sajat figurank, ammit mozgatunk

            var scale = Matrix4X4.CreateScale(1.6f);
            var rotLocY = Matrix4X4.CreateRotationY((float)_arrangementModel.SliceRotationAngle);
            var translationMatrix = Matrix4X4.CreateTranslation(bladePosition);
            var modelMatrix = scale * rotLocY * translationMatrix;

            SetModelMatrix(modelMatrix);

            _Gl.BindVertexArray(bigBoss.beyBlade.Vao);

            int textureLocation = _Gl.GetUniformLocation(_program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }

            _Gl.Uniform1(textureLocation, 0);

            _Gl.ActiveTexture(TextureUnit.Texture0);
            _Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            _Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            _Gl.BindTexture(TextureTarget.Texture2D, bigBoss.beyBlade.Texture.Value);

            _Gl.DrawElements(GLEnum.Triangles, bigBoss.beyBlade.IndexArrayLength, GLEnum.UnsignedInt, null);
            _Gl.BindVertexArray(0);

            CheckError();
            _Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void DrawPlato()
        {
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(400f);
            SetModelMatrix(modelMatrix);
            _Gl.BindVertexArray(plato.Vao);

            int textureLocation = _Gl.GetUniformLocation(_program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            _Gl.Uniform1(textureLocation, 0);

            _Gl.ActiveTexture(TextureUnit.Texture0);
            _Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            _Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            _Gl.BindTexture(TextureTarget.Texture2D, plato.Texture.Value);

            _Gl.DrawElements(GLEnum.Triangles, plato.IndexArrayLength, GLEnum.UnsignedInt, null);
            _Gl.BindVertexArray(0);

            CheckError();
            _Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void SetLightColor()
        {
            int location = _Gl.GetUniformLocation(_program, LightColorVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightColorVariableName} uniform not found on shader.");
            }

            _Gl.Uniform3(location, 1f, 1f, 1f);
            CheckError();
        }

        private static unsafe void SetLightPosition()
        {
            int location = _Gl.GetUniformLocation(_program, LightPositionVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightPositionVariableName} uniform not found on shader.");
            }

            _Gl.Uniform3(location, -50f, 30f, 0f);
            CheckError();
        }

        private static unsafe void SetViewerPosition()
        {
            int location = _Gl.GetUniformLocation(_program, ViewPosVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewPosVariableName} uniform not found on shader.");
            }

            _Gl.Uniform3(location, _cameraDescriptor.Position.X, _cameraDescriptor.Position.Y, _cameraDescriptor.Position.Z);
            CheckError();
        }

        private static unsafe void SetShininess()
        {
            int location = _Gl.GetUniformLocation(_program, ShininessVariableName);

            if (location == -1)
            {
                throw new Exception($"{ShininessVariableName} uniform not found on shader.");
            }

            _Gl.Uniform1(location, Shininess);
            CheckError();
        }

        private static unsafe void SetLigthingParams()
        {
            int ambientLoc = _Gl.GetUniformLocation(_program, AmbientStrengthVariableName);
            int diffuseLoc = _Gl.GetUniformLocation(_program, DiffuseStrengthVariableName);
            int specularLoc = _Gl.GetUniformLocation(_program, SpecularStrengthVariableName);

            if (ambientLoc == -1 || diffuseLoc == -1 || specularLoc == -1)
            {
                throw new Exception($" uniform not found on shader.");
            }

            _Gl.Uniform1(ambientLoc, AmbientStrength);
            _Gl.Uniform1(diffuseLoc, DiffuseStrength);
            _Gl.Uniform1(specularLoc, SpecularStrength);
            CheckError();
        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            int location = _Gl.GetUniformLocation(_program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            _Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();

            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
            location = _Gl.GetUniformLocation(_program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            _Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 1000);
            int location = _Gl.GetUniformLocation(_program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            _Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }

        private static unsafe void SetViewMatrix()
        {
            var viewMatrix = Matrix4X4.CreateLookAt(_cameraDescriptor.Position, _cameraDescriptor.Target, _cameraDescriptor.UpVector);
            int location = _Gl.GetUniformLocation(_program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            _Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        private static unsafe void SetUpObjects(int n)
        {
            // a kisebb bb-k
            beyBlades = new List<BeyBlade>();
            for (int i = 0; i < n; i++)
            {
                beyBlades.Add(new BeyBlade(_Gl, "dragon2.jpg"));
            }

            // a sajat bb
            // bigBoss = new BeyBlade(_Gl, "arabbit.jpg");
            // bigBoss = new BeyBlade(_Gl, "dragon_boss.png"); 
            bigBoss = new BeyBlade(_Gl, "dragon_boss3.jpg");

            float[] tableColor = { 0f, 0f, 0.5f, 1f };

            _skyBox = GlCube.CreateInteriorCube(_Gl, "skybox3.png");
            
            plato = GlCube.CreateSquareWithTexture(_Gl, tableColor, "ground2.jpg");
        }

        private static void Window_Closing()
        {
            _skyBox.ReleaseGlObject();
            beyBlades.ForEach(beyBlade => beyBlade.ReleaseBeyBlade());

            // sound
            _outputDevice?.Stop();
            _audioFile?.Dispose();
            _outputDevice?.Dispose();

        }

        public static void CheckError()
        {
            var error = (ErrorCode)_Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }

    }
}