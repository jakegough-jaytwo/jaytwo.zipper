using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace jaytwo.Zipper.Tests
{
    public static class EmbeddedResources
    {
        public static FileInfo SaveTo(string key, DirectoryInfo directory)
            => SaveTo(key, directory, key);

        public static FileInfo SaveTo(string key, DirectoryInfo directory, string fileName)
        {
            var fileInfo = new FileInfo(Path.Combine(directory.FullName, fileName));
            return SaveTo(key, fileInfo);
        }

        public static FileInfo SaveTo(string key, FileInfo target)
        {
            using (var resourceStream = GetStream(key))
            using (var fileStream = target.Create())
            {
                resourceStream.CopyTo(fileStream);
            }

            target.Refresh();
            return target;
        }

        public static string GetContentAsString(string key)
        {
            using (var stream = GetStream(key))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static byte[] GetContentAsBytes(string key)
        {
            using (var stream = GetStream(key))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static Stream GetStream(string key)
            => typeof(EmbeddedResources).Assembly.GetManifestResourceStream(GetFullKey(key));

        private static string GetFullKey(string key)
        {
            var allResourceNames = typeof(EmbeddedResources).Assembly.GetManifestResourceNames();
            var result = allResourceNames
                .Where(x => x.Equals($"{typeof(EmbeddedResources).FullName}.{key.Replace(" ", "_")}", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            return result;
        }
    }
}
