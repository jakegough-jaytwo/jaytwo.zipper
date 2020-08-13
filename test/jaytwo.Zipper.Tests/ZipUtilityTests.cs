using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using jaytwo.DisappearingFiles;
using Xunit;

namespace jaytwo.Zipper.Tests
{
    public class ZipUtilityTests
    {
        [Fact]
        public void Simple_full_loop_works()
        {
            // arrange
            using (var workspace = DisappearingDirectory.CreateInTempPath("ziptest."))
            {
                var subDirectory = workspace.CreateSubdirectoryWithPrefix("originals.");
                var subSubDirectory = subDirectory.CreateSubdirectory("sub folder");
                var extractToDirectory = workspace.CreateNewSubdirectory("unzipped.");

                EmbeddedResources.SaveTo("zipper1.jpg", subDirectory);
                EmbeddedResources.SaveTo("sub folder.zipper2.png", subSubDirectory, "zipper2.png");

                var zipFile = workspace.CreateNewFile("zippers.zip");

                // act
                ZipUtility.CreateZipArchiveFromDirectory(subDirectory, zipFile);
                ZipUtility.ExtractZipArchiveToDirectory(zipFile, extractToDirectory);

                // assert
                Assert.Equal(subDirectory.GetFiles("*.*", SearchOption.AllDirectories).Count(), extractToDirectory.GetFiles("*.*", SearchOption.AllDirectories).Count());

                Assert.All(subDirectory.GetFiles("*.*", SearchOption.AllDirectories), originalFile =>
                {
                    var relativePath = originalFile.FullName.Substring(subDirectory.FullName.Length + 1);
                    var matchingExtractedFile = extractToDirectory.GetFiles(relativePath).Single();
                    Assert.Equal(originalFile.Name, matchingExtractedFile.Name);
                    Assert.Equal(originalFile.Length, matchingExtractedFile.Length);
                    Assert.Equal(GetChecksum(originalFile), GetChecksum(matchingExtractedFile));
                });
            }
        }

        [Fact]
        public void Simple_full_loop_works_with_paths()
        {
            // arrange
            using (var workspace = DisappearingDirectory.CreateInTempPath("ziptest."))
            {
                var subDirectory = workspace.CreateSubdirectoryWithPrefix("originals.");
                var subSubDirectory = subDirectory.CreateSubdirectory("sub folder");
                var extractToDirectory = workspace.CreateNewSubdirectory("unzipped.");

                EmbeddedResources.SaveTo("zipper1.jpg", subDirectory);
                EmbeddedResources.SaveTo("sub folder.zipper2.png", subSubDirectory, "zipper2.png");

                var zipFile = workspace.CreateNewFile("zippers.zip");

                // act
                ZipUtility.CreateZipArchiveFromDirectory(subDirectory.FullName, zipFile.FullName);
                ZipUtility.ExtractZipArchiveToDirectory(zipFile.FullName, extractToDirectory.FullName);

                // assert
                Assert.Equal(subDirectory.GetFiles("*.*", SearchOption.AllDirectories).Count(), extractToDirectory.GetFiles("*.*", SearchOption.AllDirectories).Count());

                Assert.All(subDirectory.GetFiles("*.*", SearchOption.AllDirectories), originalFile =>
                {
                    var relativePath = originalFile.FullName.Substring(subDirectory.FullName.Length + 1);
                    var matchingExtractedFile = extractToDirectory.GetFiles(relativePath).Single();
                    Assert.Equal(originalFile.Name, matchingExtractedFile.Name);
                    Assert.Equal(originalFile.Length, matchingExtractedFile.Length);
                    Assert.Equal(GetChecksum(originalFile), GetChecksum(matchingExtractedFile));
                });
            }
        }

        [Fact]
        public void Simple_full_loop_works_with_streams()
        {
            // arrange
            using (var workspace = DisappearingDirectory.CreateInTempPath("ziptest."))
            {
                var subDirectory = workspace.CreateSubdirectoryWithPrefix("originals.");
                var subSubDirectory = subDirectory.CreateSubdirectory("sub folder");
                var extractToDirectory = workspace.CreateNewSubdirectory("unzipped.");

                EmbeddedResources.SaveTo("zipper1.jpg", subDirectory);
                EmbeddedResources.SaveTo("sub folder.zipper2.png", subSubDirectory, "zipper2.png");

                var zipFile = workspace.CreateNewFile("zippers.zip");

                // act
                ZipUtility.CreateZipArchiveFromDirectory(subDirectory, zipFile);
                ZipUtility.ExtractZipArchiveToDirectory(zipFile.OpenRead(), extractToDirectory);

                // assert
                Assert.Equal(subDirectory.GetFiles("*.*", SearchOption.AllDirectories).Count(), extractToDirectory.GetFiles("*.*", SearchOption.AllDirectories).Count());

                Assert.All(subDirectory.GetFiles("*.*", SearchOption.AllDirectories), originalFile =>
                {
                    var relativePath = originalFile.FullName.Substring(subDirectory.FullName.Length + 1);
                    var matchingExtractedFile = extractToDirectory.GetFiles(relativePath).Single();
                    Assert.Equal(originalFile.Name, matchingExtractedFile.Name);
                    Assert.Equal(originalFile.Length, matchingExtractedFile.Length);
                    Assert.Equal(GetChecksum(originalFile), GetChecksum(matchingExtractedFile));
                });
            }
        }

        [Theory]
        [InlineData("zippers.from7zip.zip")]
        [InlineData("zippers.fromwindows.zip")]
        public void ExtractZipArchiveToDirectory_works_with_commercial_zips(string embeddedResourceKey)
        {
            // arrange
            using (var workspace = DisappearingDirectory.CreateInTempPath("ziptest.7zip."))
            {
                var subDirectory = workspace.CreateSubdirectoryWithPrefix("originals.");
                var subSubDirectory = subDirectory.CreateSubdirectory("sub folder");
                var extractToDirectory = workspace.CreateNewSubdirectory("unzipped.");

                EmbeddedResources.SaveTo("zipper1.jpg", subDirectory);
                EmbeddedResources.SaveTo("sub folder.zipper2.png", subSubDirectory, "zipper2.png");

                var zipFile = EmbeddedResources.SaveTo(embeddedResourceKey, workspace.GetDirectoryInfo());

                // act
                ZipUtility.ExtractZipArchiveToDirectory(zipFile, extractToDirectory);

                // assert
                Assert.Equal(subDirectory.GetFiles("*.*", SearchOption.AllDirectories).Count(), extractToDirectory.GetFiles("*.*", SearchOption.AllDirectories).Count());

                Assert.All(subDirectory.GetFiles("*.*", SearchOption.AllDirectories), originalFile =>
                {
                    var relativePath = originalFile.FullName.Substring(subDirectory.FullName.Length + 1);
                    var matchingExtractedFile = extractToDirectory.GetFiles(relativePath).Single();
                    Assert.Equal(originalFile.Name, matchingExtractedFile.Name);
                    Assert.Equal(originalFile.Length, matchingExtractedFile.Length);
                    Assert.Equal(GetChecksum(originalFile), GetChecksum(matchingExtractedFile));
                });
            }
        }

        private static string GetChecksum(FileInfo file)
        {
            using (var fileStream = file.OpenRead())
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(fileStream);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
