using System;
using Silk.NET.OpenGL;

public unsafe class BufferObject<T> : IDisposable
    where T : unmanaged
{
    private uint handle;
    private BufferTargetARB type;
    private GL gl;

    public BufferObject(GL gl, Span<T> data, BufferTargetARB type)
    {
        this.gl = gl;
        this.type = type;

        handle = gl.GenBuffer();
        Bind();
        fixed (void* d = data)
        {
            gl.BufferData(type, (nuint)(data.Length * sizeof(T)), d, BufferUsageARB.StaticDraw);
        }
    }

    public void Bind()
    {
        gl.BindBuffer(type, handle);
    }

    public void Dispose()
    {
        gl.DeleteBuffer(handle);
    }
}