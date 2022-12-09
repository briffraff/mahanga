using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using mahanga.Core.Interface;

namespace mahanga.Core
{
    public class Texture : ITexture
    {
        internal string Path;
        internal string Name;
        internal bool isLatest;
        internal DateTime LastWriteTime;

        internal Texture(string path, string name, bool islatest)
        {
            Path = path;
            Name = name;
            isLatest = islatest;
            LastWriteTime = File.GetLastWriteTime(path + "\\" + name);
        }
    }
}
