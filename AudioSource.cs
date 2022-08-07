using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenAL;

public class AudioSource : IDisposable
{
    private uint _handle;
    private AL _al;

    public bool IsPlaying
    {
        get
        {
            _al.GetSourceProperty(_handle, GetSourceInteger.SourceState, out int val);
            return (SourceState)val == SourceState.Playing;
        }
    }

    public AudioSource(AL al)
    {
        _al = al;

        _handle = _al.GenSource();
        SetParameters();
    }

    public void SetBuffer<T>(AudioBuffer<T> buffer)
        where T : unmanaged
    {
        Stop();
        _al.SetSourceProperty(_handle, SourceInteger.Buffer, buffer.Handle);
    }

    private void SetParameters()
    {
        _al.SetSourceProperty(_handle, SourceFloat.Pitch, 1);
        _al.SetSourceProperty(_handle, SourceFloat.Gain, 1);
        _al.SetSourceProperty(_handle, SourceFloat.MaxDistance, float.PositiveInfinity);
        _al.SetSourceProperty(_handle, SourceVector3.Position, 0f, 0f, 0f);
        _al.SetSourceProperty(_handle, SourceVector3.Velocity, 0f, 0f, 0f);
        _al.SetSourceProperty(_handle, SourceBoolean.Looping, false);
    }

    public void Stop()
    {
        _al.SourceStop(_handle);
    }

    public void Play()
    {
        _al.SourcePlay(_handle);
    }

    public void Dispose()
    {
        _al.DeleteSource(_handle);
    }
}
