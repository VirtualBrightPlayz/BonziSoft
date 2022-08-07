using System;
using NAudio.Wave;
using Silk.NET.OpenAL;

public class AudioBuffer<T> : IDisposable
    where T : unmanaged
{
    private uint _handle;
    public uint Handle => _handle;
    private AL _al;

    public AudioBuffer(AL al, string path)
    {
        _al = al;
        var stream = FileManager.LoadStream(path);
        if (stream == null)
        {
            _handle = _al.GenBuffer();
            _al.BufferData(_handle, BufferFormat.Mono8, new byte[0], 0);
        }
        else
        {
            using var file = new Mp3FileReader(FileManager.LoadStream(path));
            byte[] buffer = new byte[file.Length];
            file.Read(buffer);
            BufferFormat format = BufferFormat.Mono8;
            if (file.WaveFormat.BitsPerSample == 8)
            {
                if (file.WaveFormat.Channels == 1)
                    format = BufferFormat.Mono8;
                else
                    format = BufferFormat.Stereo8;
            }
            else
            {
                if (file.WaveFormat.Channels == 1)
                    format = BufferFormat.Mono16;
                else
                    format = BufferFormat.Stereo16;
            }
            _handle = _al.GenBuffer();
            _al.BufferData(_handle, format, buffer, file.WaveFormat.SampleRate);
        }
    }

    public AudioBuffer(AL al, T[] data, BufferFormat format, int freq)
    {
        _al = al;
        _handle = _al.GenBuffer();
        _al.BufferData(_handle, format, data, freq);
    }

    public void Dispose()
    {
        _al.DeleteBuffer(_handle);
    }
}