using System;
using Silk.NET.OpenGL;

public unsafe class VertexArrayObject<TVert, TIndex> : IDisposable
    where TVert : unmanaged
    where TIndex : unmanaged
{
    private uint handle;
    private GL gl;

    public VertexArrayObject(GL gl, BufferObject<TVert> vbo, BufferObject<TIndex> ebo)
    {
        this.gl = gl;

        handle = gl.GenVertexArray();
        Bind();
        vbo.Bind();
        ebo.Bind();
    }

    public void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offset)
    {
        Bind();
        gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVert), (void*)(offset * sizeof(TVert)));
        gl.EnableVertexAttribArray(index);
    }

    public void Bind()
    {
        gl.BindVertexArray(handle);
    }

    public void Dispose()
    {
        gl.DeleteVertexArray(handle);
    }
}