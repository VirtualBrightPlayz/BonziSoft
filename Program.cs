using System;
using System.Drawing;
using NAudio.Wave;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenAL;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

public unsafe class Program
{
    public static uint[] Indices = new uint[]
    {
        0, 1, 2,
        2, 0, 3,
    };

    // X Y Z U V
    public static float[] Vertices = new float[]
    {
        1f, 1f, 0.0f, 1f, 0f,
        1f, -1f, 0.0f, 1f, 1f,
        -1f, -1f, 0.0f, 0f, 1f,
        -1f, 1f, 0.0f, 0f, 0f,
    };

    public static int Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Compatability, ContextFlags.Default, new APIVersion(3, 3));
        options.PreferredBitDepth = new Silk.NET.Maths.Vector4D<int>(8, 8, 8, 8);
        options.TransparentFramebuffer = true;
        options.WindowState = WindowState.Maximized;
        options.WindowBorder = WindowBorder.Hidden;
        options.IsVisible = false;
        options.Size = Monitor.GetMainMonitor(null).Bounds.Size;
        Window.PrioritizeGlfw();
        using var window = Window.Create(options);
        using var audioContext = new AudioContext();

        GL gl = null;
        AL al = AL.GetApi(false);
        BufferObject<uint> ebo = null;
        BufferObject<float> vbo = null;
        VertexArrayObject<float, uint> vao = null;
        Shader shader = null;
        AudioSource audioSource = null;
        AudioBuffer<byte> audioBufferOnOpen = null;
        AudioBuffer<byte> audioBufferLoop = null;
        AudioBuffer<byte> audioBufferUnfocus = null;
        AudioBuffer<byte> audioBufferOnClose = null;

        string gifPath = "bonzi.gif";
        string pngPath = "bonzi.png";
        var gif = Image.Load<Rgba32>(FileManager.LoadBytes(gifPath) ?? FileManager.LoadBytes(pngPath));

        Texture[] gifFrames = new Texture[gif.Frames.Count];
        int[] gifFrameDelays = new int[gif.Frames.Count];
        int maxTime = 0;

        window.Load += () =>
        {
            gl = GL.GetApi(window);

            vbo = new BufferObject<float>(gl, Vertices, BufferTargetARB.ArrayBuffer);
            ebo = new BufferObject<uint>(gl, Indices, BufferTargetARB.ElementArrayBuffer);
            vao = new VertexArrayObject<float, uint>(gl, vbo, ebo);
            vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

            shader = new Shader(gl, "shader.vert", "shader.frag");
            for (int i = 0; i < gifFrames.Length; i++)
            {
                gifFrames[i] = new Texture(gl, gif.Frames.CloneFrame(i));
                gifFrameDelays[i] = gif.Frames[i].Metadata.GetGifMetadata().FrameDelay;
                maxTime += gifFrameDelays[i];
            }

            audioBufferOnOpen = new AudioBuffer<byte>(al, "open.mp3");
            audioBufferLoop = new AudioBuffer<byte>(al, "loop.mp3");
            audioBufferUnfocus = new AudioBuffer<byte>(al, "unfocus.mp3");
            audioBufferOnClose = new AudioBuffer<byte>(al, "close.mp3");
            audioSource = new AudioSource(al);

            audioSource.SetBuffer(audioBufferOnOpen);
            audioSource.Play();

            window.WindowState = WindowState.Maximized;
            window.IsVisible = true;
            gl.Viewport(window.Size);
        };

        window.FramebufferResize += (s) =>
        {
            gl.Viewport(s);
        };

        double time = 0;

        window.Render += (obj) =>
        {
            double prevFrameDelayOffset = time % ((double)maxTime / 100);
            time += obj;
            var tex = gifFrames[0];
            double frameDelayOffset = time % ((double)maxTime / 100);
            double elapsed = 0;
            for (int i = 0; i < gifFrames.Length; i++)
            {
                if (frameDelayOffset <= elapsed)
                {
                    tex = gifFrames[i];
                    break;
                }
                elapsed += (double)gifFrameDelays[i] / 100;
            }
            if (prevFrameDelayOffset > frameDelayOffset)
            {
                audioSource.SetBuffer(audioBufferLoop);
                audioSource.Play();
            }
            gl.ClearColor(System.Drawing.Color.FromArgb(0, 0, 0, 0));
            gl.Clear(ClearBufferMask.ColorBufferBit);
            vao.Bind();
            shader.Use();
            tex.Bind(TextureUnit.Texture0);
            shader.SetUniform("uTexture0", 0);
            gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
        };

        window.Closing += () =>
        {
            audioSource.SetBuffer(audioBufferOnClose);
            audioSource.Play();
            while (audioSource.IsPlaying)
            {
            }
            audioSource.Dispose();
            audioBufferLoop.Dispose();
            audioBufferUnfocus.Dispose();
            audioBufferOnClose.Dispose();
            vbo.Dispose();
            ebo.Dispose();
            vao.Dispose();
            shader.Dispose();
            for (int i = 0; i < gifFrames.Length; i++)
            {
                gifFrames[i].Dispose();
            }
            gif.Dispose();
        };

        bool forceFocus = Array.IndexOf(args, "-focus") != -1;
        window.StateChanged += (s) =>
        {
            if (s != WindowState.Maximized)
            {
                audioSource.SetBuffer(audioBufferUnfocus);
                audioSource.Play();
            }
            if (s != WindowState.Maximized && forceFocus)
            {
                window.WindowState = WindowState.Maximized;
            }
        };

        window.FocusChanged += (s) =>
        {
            if (!s)
            {
                audioSource.SetBuffer(audioBufferUnfocus);
                audioSource.Play();
            }
            if (!s && forceFocus)
            {
                window.IsVisible = true;
            }
        };

        window.Run();

        while (audioSource.IsPlaying)
        {
        }

        return 0;
    }
}