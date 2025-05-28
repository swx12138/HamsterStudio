using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Toolkits
{
    public static class FileTools
    {
        public static async Task<bool> SaveToFile(this Stream stream, int offest = 0)
        {
            using var fileStream = File.Create("test.png");

            var pos = stream.Position;
            stream.Seek(offest, SeekOrigin.Begin);

            await stream.CopyToAsync(fileStream);

            stream.Seek(pos, SeekOrigin.Begin);
            return true;
        }
    }
}
