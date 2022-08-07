using System;
using System.IO;
using System.Reflection;
using System.Text;

public static class FileManager
{
    public static Stream LoadStream(string path)
    {
        var stream = typeof(Program).Assembly.GetManifestResourceStream("BonziSoft." + path);
        string filepath = Path.Combine(Directory.GetCurrentDirectory(), path);
        if (stream == null && File.Exists(filepath))
            stream = File.OpenRead(filepath);
        return stream;
    }

    public static byte[] LoadBytes(string path)
    {
        using var stream = LoadStream(path);
        if (stream == null)
            return null;
        byte[] data = new byte[stream.Length];
        stream.Read(data, 0, data.Length);
        return data;
    }

    public static string LoadString(string path)
    {
        return Encoding.UTF8.GetString(LoadBytes(path));
    }
}