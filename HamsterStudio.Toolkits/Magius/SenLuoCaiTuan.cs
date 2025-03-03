using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Toolkits.Magius
{
    internal class SenLuoCaiTuan
    {
        static string CalculateSHA384(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var sha384 = SHA384.Create())
                {
                    byte[] hash = sha384.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }
        }

        static List<string> TraverseFolder(string folderPath)
        {
            Console.WriteLine(folderPath);
            List<string> files = new List<string>();
            TraverseFolderRecursive(folderPath, files);
            return files;
        }

        static void TraverseFolderRecursive(string folderPath, List<string> files)
        {
            foreach (string file in Directory.GetFiles(folderPath))
            {
                files.Add(file);
            }
            foreach (string subFolder in Directory.GetDirectories(folderPath))
            {
                TraverseFolderRecursive(subFolder, files);
            }
        }

        static void Main0(string[] args)
        {
            string folderPath = @"G:\森萝财团";
            List<string> files = TraverseFolder(folderPath);

            // 使用并行计算文件的SHA-384哈希值
            List<string> hashes = new List<string>();
            Parallel.ForEach(files, (file) =>
            {
                string hash = CalculateSHA384(file);
                if (hashes.Contains(hash))
                {
                    Trace.TraceWarning($"重复的hash:{hash} {file}");
                }
                else
                {
                    hashes.Add(hash);
                }

                Console.Write($"\r{hashes.Count}/{files.Count} : {hash}");

                if (-1 != file.IndexOf("[fbzip.com]"))
                {
                    string nname = file.Replace("[fbzip.com]", "");
                    File.Move(file, nname);
                }

            });
            Console.WriteLine("\norigin hashes compute over.");

            string oldFolderPath = @"D:\xunlei\SLCT";
            List<string> oldFiles = TraverseFolder(oldFolderPath);
            int n = 0;
            foreach (string oldFile in oldFiles)
            {
                ++n;
                string ha = CalculateSHA384(oldFile);
                if (hashes.Contains(ha))
                {
                    /*Console.WriteLine($" {ha} : {oldFile}");*/
                    File.Delete(oldFile);
                    Console.WriteLine($"{n} [-] {oldFile}");
                }
                else
                {
                    Console.WriteLine($"{n} [@] {oldFile}");
                }
            }
        }

    }
}
