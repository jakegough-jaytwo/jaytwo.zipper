using System;
using System.IO;
using System.IO.Compression;

namespace jaytwo.Zipper
{
    public class ZipUtility
    {
        private const CompressionLevel DefaultCompressionLevel = CompressionLevel.Fastest;

        public static void ExtractZipArchiveToDirectory(string zipFilePath, string extractToDirectory)
            => ExtractZipArchiveToDirectory(new FileInfo(zipFilePath), new DirectoryInfo(extractToDirectory));

        public static void ExtractZipArchiveToDirectory(FileInfo zipFile, DirectoryInfo extractToDirectory)
        {
            using (var zipStream = zipFile.OpenRead())
            {
                ExtractZipArchiveToDirectory(zipStream, extractToDirectory);
            }
        }

        public static void ExtractZipArchiveToDirectory(Stream zipStream, string extractToDirectory)
            => ExtractZipArchiveToDirectory(zipStream, new DirectoryInfo(extractToDirectory));

        public static void ExtractZipArchiveToDirectory(Stream zipStream, DirectoryInfo extractToDirectory)
        {
            using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                ExtractZipArchiveToDirectory(zipArchive, extractToDirectory);
            }
        }

        public static void ExtractZipArchiveToDirectory(ZipArchive zipArchive, string extractToDirectory)
            => ExtractZipArchiveToDirectory(zipArchive, new DirectoryInfo(extractToDirectory));

        public static void ExtractZipArchiveToDirectory(ZipArchive zipArchive, DirectoryInfo extractToDirectory)
        {
            foreach (var zipEntry in zipArchive.Entries)
            {
                var isDirectory = zipEntry.Length == 0 && (zipEntry.FullName.EndsWith("/") || zipEntry.FullName.EndsWith("\\"));
                if (!isDirectory)
                {
                    var extractToFileName = Path.Combine(extractToDirectory.FullName, zipEntry.FullName);
                    var extractToSubDirectory = Path.GetDirectoryName(extractToFileName);

                    if (!Directory.Exists(extractToSubDirectory))
                    {
                        Directory.CreateDirectory(extractToSubDirectory);
                    }

                    zipEntry.ExtractToFile(extractToFileName, false);
                }
            }
        }

        public static void CreateZipArchiveFromDirectory(string sourceDirectoryPath, string targetArchivePath)
            => CreateZipArchiveFromDirectory(sourceDirectoryPath, targetArchivePath, DefaultCompressionLevel);

        public static void CreateZipArchiveFromDirectory(DirectoryInfo directory, FileInfo targetFile)
            => CreateZipArchiveFromDirectory(directory, targetFile, DefaultCompressionLevel);

        public static void CreateZipArchiveFromDirectory(string sourceDirectoryPath, string targetArchivePath, CompressionLevel compressionLevel)
            => CreateZipArchiveFromDirectory(new DirectoryInfo(sourceDirectoryPath), new FileInfo(targetArchivePath), compressionLevel);

        public static void CreateZipArchiveFromDirectory(DirectoryInfo directory, FileInfo targetFile, CompressionLevel compressionLevel)
        {
            using (var zipFile = targetFile.Create())
            using (var zipArchive = new ZipArchive(zipFile, ZipArchiveMode.Create, false))
            {
                AddDirectoryToArchive(zipArchive, directory, string.Empty, compressionLevel);
            }
        }

        private static void AddDirectoryToArchive(ZipArchive archive, DirectoryInfo currentDirectory, string currentDirectoryRelativePath, CompressionLevel compressionLevel)
        {
            if (!string.IsNullOrEmpty(currentDirectoryRelativePath))
            {
                // zip archives like zero-length entries for directories
                archive.CreateEntry(currentDirectoryRelativePath);
            }

            foreach (var subDirectory in currentDirectory.GetDirectories())
            {
                // directories need to be added before the files in the directory
                // directory slashes need to end in the slash
                //    -- and zip archives only like forward slashes for some reason
                var subDirectoryRelativePath = currentDirectoryRelativePath + subDirectory.Name + "/";
                AddDirectoryToArchive(archive, subDirectory, subDirectoryRelativePath, compressionLevel);
            }

            foreach (var file in currentDirectory.GetFiles())
            {
                var fileRelativeName = currentDirectoryRelativePath + file.Name;
                archive.CreateEntryFromFile(file.FullName, fileRelativeName, compressionLevel);
            }
        }
    }
}
