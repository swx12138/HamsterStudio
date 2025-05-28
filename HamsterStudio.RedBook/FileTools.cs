using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.RedBook
{
    public static class FileTools
    {
        public static async Task<bool> SaveToFile(this Stream stream, string filename)
        {
            using var fileStream = File.Create(filename);
            await stream.CopyToAsync(fileStream);
            return true;
        }
    }
}
