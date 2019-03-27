using System;

namespace Ccf.Ck.SysPlugins.Data.FileUpload.BaseClasses
{
    internal interface IImageEditor : IDisposable
    {
        void Resize(double width, double height);

        void Save(string path);
    }
}