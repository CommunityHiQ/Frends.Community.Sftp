using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using static Frends.Community.Sftp.Tests.Helpers;

namespace Frends.Community.Sftp.Tests
{
    [TestFixture]
    internal class SftpTests
    {
        private Parameters _dummyParams = new Parameters { Server = "foo.bar.com", Port = 1234, UserName = "demo", Password = "demo", Directory = "/" };

        #region List directory

        [TestCase(null, IncludeType.Both, new[] { "image01.jpg", "image02.jpg", "image03.jpg", "importantDoc.docx", "subdir" })]
        [TestCase("", IncludeType.Both, new[] { "image01.jpg", "image02.jpg", "image03.jpg", "importantDoc.docx", "subdir" })]
        [TestCase("subdir", IncludeType.Both, new[] { "subdir" })]
        [TestCase("*.jpg", IncludeType.Both, new[] { "image01.jpg", "image02.jpg", "image03.jpg" })]
        [TestCase(null, IncludeType.File, new[] { "image01.jpg", "image02.jpg", "image03.jpg", "importantDoc.docx" })]
        [TestCase("", IncludeType.File, new[] { "image01.jpg", "image02.jpg", "image03.jpg", "importantDoc.docx" })]
        [TestCase("subdir", IncludeType.File, new string[] { })]
        [TestCase("*.jpg", IncludeType.File, new[] { "image01.jpg", "image02.jpg", "image03.jpg" })]
        [TestCase(null, IncludeType.Directory, new[] { "subdir" })]
        [TestCase("", IncludeType.Directory, new[] { "subdir" })]
        [TestCase("subdir", IncludeType.Directory, new[] { "subdir" })]
        [TestCase("*.jpg", IncludeType.Directory, new string[] { })]
        public void GetDirectoryList(string fileMask, IncludeType includeType, IEnumerable<string> expectedFileNames)
        {
            var options = new Options { FileMask = fileMask, IncludeType = includeType };
            var result = CreateSftpTaskInstance().ListDirectoryInternal(_dummyParams, options, new System.Threading.CancellationToken());

            Assert.That(result.Select(f => f.Name), Is.EquivalentTo(expectedFileNames));
        }

        [TestCase(null, IncludeType.Both, new[] { "11/16/2020 12:00:00", "11/16/2020 13:01:00", "11/16/2020 17:00:11", "11/16/2020 22:00:00", "11/16/2020 23:00:00" })]
        public void GetDirectoryListUTCLWTs(string fileMask, IncludeType includeType, IEnumerable<string> expectedLWTs)
        {
            var options = new Options { FileMask = fileMask, IncludeType = includeType };
            var result = CreateSftpTaskInstance().ListDirectoryInternal(_dummyParams, options, new System.Threading.CancellationToken());

            Assert.That(result.Select(f => f.LastWriteTimeUtc.ToString(CultureInfo.InvariantCulture)), Is.EquivalentTo(expectedLWTs));
        }

        [TestCase(null, IncludeType.Both, new[] { "12/08/2020 11:00:00", "12/08/2020 12:00:00", "12/08/2020 13:00:00", "12/08/2020 14:00:00", "12/08/2020 15:00:00" })]
        public void GetDirectoryListUTCLATs(string fileMask, IncludeType includeType, IEnumerable<string> expectedLATs)
        {
            var options = new Options { FileMask = fileMask, IncludeType = includeType };
            var result = CreateSftpTaskInstance().ListDirectoryInternal(_dummyParams, options, new System.Threading.CancellationToken());

            Assert.That(result.Select(f => f.LastAccessTimeUtc.ToString(CultureInfo.InvariantCulture)), Is.EquivalentTo(expectedLATs));
        }

        [TestCase(null, IncludeType.Both, new[] { "11/16/2020 12:00:00", "11/16/2020 13:01:00", "11/16/2020 17:00:11", "11/16/2020 22:00:00", "11/16/2020 23:00:00" })]
        public void GetDirectoryListLWTs(string fileMask, IncludeType includeType, IEnumerable<string> expectedLWTs)
        {
            var options = new Options { FileMask = fileMask, IncludeType = includeType };
            var result = CreateSftpTaskInstance().ListDirectoryInternal(_dummyParams, options, new System.Threading.CancellationToken());

            Assert.That(result.Select(f => f.LastWriteTime.ToString(CultureInfo.InvariantCulture)), Is.EquivalentTo(expectedLWTs));
        }

        [TestCase(null, IncludeType.Both, new[] { "12/08/2020 11:00:00", "12/08/2020 12:00:00", "12/08/2020 13:00:00", "12/08/2020 14:00:00", "12/08/2020 15:00:00" })]
        public void GetDirectoryListLATs(string fileMask, IncludeType includeType, IEnumerable<string> expectedLATs)
        {
            var options = new Options { FileMask = fileMask, IncludeType = includeType };
            var result = CreateSftpTaskInstance().ListDirectoryInternal(_dummyParams, options, new System.Threading.CancellationToken());

            Assert.That(result.Select(f => f.LastAccessTime.ToString(CultureInfo.InvariantCulture)), Is.EquivalentTo(expectedLATs));
        }

        #endregion List directory

        #region List directory, include subdirs

        [TestCase(null, IncludeType.Both, new[] { "image01.jpg", "image02.jpg", "image03.jpg", "importantDoc.docx", "subdir", "fileInSubdir.jpg", "anotherFileInSubdir.docx" })]
        [TestCase("", IncludeType.Both, new[] { "image01.jpg", "image02.jpg", "image03.jpg", "importantDoc.docx", "subdir", "fileInSubdir.jpg", "anotherFileInSubdir.docx" })]
        [TestCase("subdir", IncludeType.Both, new[] { "subdir" })]
        [TestCase("*.jpg", IncludeType.Both, new[] { "image01.jpg", "image02.jpg", "image03.jpg", "fileInSubdir.jpg" })]
        [TestCase(null, IncludeType.File, new[] { "image01.jpg", "image02.jpg", "image03.jpg", "importantDoc.docx", "fileInSubdir.jpg", "anotherFileInSubdir.docx" })]
        [TestCase("", IncludeType.File, new[] { "image01.jpg", "image02.jpg", "image03.jpg", "importantDoc.docx", "fileInSubdir.jpg", "anotherFileInSubdir.docx" })]
        [TestCase("subdir", IncludeType.File, new string[] { })]
        [TestCase("*.jpg", IncludeType.File, new[] { "image01.jpg", "image02.jpg", "image03.jpg", "fileInSubdir.jpg" })]
        [TestCase(null, IncludeType.Directory, new[] { "subdir" })]
        [TestCase("", IncludeType.Directory, new[] { "subdir" })]
        [TestCase("subdir", IncludeType.Directory, new[] { "subdir" })]
        [TestCase("*.jpg", IncludeType.Directory, new string[] { })]
        public void GetDirectoryListIncludeSubdirs(string fileMask, IncludeType includeType, IEnumerable<string> expectedFileNames)
        {
            var options = new Options { FileMask = fileMask, IncludeType = includeType, IncludeSubdirectories = true };
            var result = CreateSftpTaskInstance().ListDirectoryInternal(_dummyParams, options, new System.Threading.CancellationToken());

            Assert.That(result.Select(f => f.Name), Is.EquivalentTo(expectedFileNames));
        }

        [TestCase(null, IncludeType.Both, new[] { "11/16/2020 12:00:00", "11/16/2020 13:01:00", "11/16/2020 17:00:11", "11/16/2020 22:00:00", "11/16/2020 23:00:00", "11/16/2020 12:00:00", "11/16/2020 13:01:00" })]
        public void GetDirectoryListUTCLWTsIncludeSubdirs(string fileMask, IncludeType includeType, IEnumerable<string> expectedLWTs)
        {
            var options = new Options { FileMask = fileMask, IncludeType = includeType, IncludeSubdirectories = true };
            var result = CreateSftpTaskInstance().ListDirectoryInternal(_dummyParams, options, new System.Threading.CancellationToken());

            Assert.That(result.Select(f => f.LastWriteTimeUtc.ToString(CultureInfo.InvariantCulture)), Is.EquivalentTo(expectedLWTs));
        }

        [TestCase(null, IncludeType.Both, new[] { "12/08/2020 11:00:00", "12/08/2020 12:00:00", "12/08/2020 13:00:00", "12/08/2020 14:00:00", "12/08/2020 15:00:00", "12/08/2020 11:00:00", "12/08/2020 12:00:00" })]
        public void GetDirectoryListUTCLATsIncludeSubdirs(string fileMask, IncludeType includeType, IEnumerable<string> expectedLATs)
        {
            var options = new Options { FileMask = fileMask, IncludeType = includeType, IncludeSubdirectories = true };
            var result = CreateSftpTaskInstance().ListDirectoryInternal(_dummyParams, options, new System.Threading.CancellationToken());

            Assert.That(result.Select(f => f.LastAccessTimeUtc.ToString(CultureInfo.InvariantCulture)), Is.EquivalentTo(expectedLATs));
        }

        [TestCase(null, IncludeType.Both, new[] { "11/16/2020 12:00:00", "11/16/2020 13:01:00", "11/16/2020 17:00:11", "11/16/2020 22:00:00", "11/16/2020 23:00:00", "11/16/2020 12:00:00", "11/16/2020 13:01:00" })]
        public void GetDirectoryListLWTsIncludeSubdirs(string fileMask, IncludeType includeType, IEnumerable<string> expectedLWTs)
        {
            var options = new Options { FileMask = fileMask, IncludeType = includeType, IncludeSubdirectories = true };
            var result = CreateSftpTaskInstance().ListDirectoryInternal(_dummyParams, options, new System.Threading.CancellationToken());

            Assert.That(result.Select(f => f.LastWriteTime.ToString(CultureInfo.InvariantCulture)), Is.EquivalentTo(expectedLWTs));
        }

        [TestCase(null, IncludeType.Both, new[] { "12/08/2020 11:00:00", "12/08/2020 12:00:00", "12/08/2020 13:00:00", "12/08/2020 14:00:00", "12/08/2020 15:00:00", "12/08/2020 11:00:00", "12/08/2020 12:00:00" })]
        public void GetDirectoryListLATsIncludeSubdirs(string fileMask, IncludeType includeType, IEnumerable<string> expectedLATs)
        {
            var options = new Options { FileMask = fileMask, IncludeType = includeType, IncludeSubdirectories = true };
            var result = CreateSftpTaskInstance().ListDirectoryInternal(_dummyParams, options, new System.Threading.CancellationToken());

            Assert.That(result.Select(f => f.LastAccessTime.ToString(CultureInfo.InvariantCulture)), Is.EquivalentTo(expectedLATs));
        }

        #endregion List directory, include subdirs

        [Test]
        [Ignore("This is used to test with an actual SFTP server.")]
        public void GetActualDirectoryList()
        {
            var input = new Parameters
            {
                Server = "",
                Port = 22,
                UserName = "",
                Password = "",
                Directory = "."
            };

            var options = new Options
            {
                IncludeType = IncludeType.Both,
                IncludeSubdirectories = true
            };

            IEnumerable<string> expectedFileNames = new[] { "test.txt" };
            var result = Sftp.ListDirectory(input, options, new System.Threading.CancellationToken());
            IEnumerable<string> actualFileNames = result.Select(f => f.Name);

            Assert.That(actualFileNames, Is.EquivalentTo(expectedFileNames));
        }


        [Test]
        [Ignore("This is used to test with an actual SFTP server.")]
        public void ReadFiles()
        {
            var input = new PathInput
            {
                Server = "localhost",
                Port = 22,
                UserName = "sftpuser",
                Password = "",
                Directory = "",
                Path = "sftpuser/data.json"
            };

            var text_options = new TextOption
            {
                IncludeType = IncludeType.Both,
                IncludeSubdirectories = true,
                FileEncoding = FileEncoding.UTF8
            };

            var result = Sftp.ReadText(input, text_options, new System.Threading.CancellationToken());
            Assert.That(result.Substring(0,1), Is.EqualTo("["));

            var options = new Options
            {
                IncludeType = IncludeType.Both,
                IncludeSubdirectories = true,
            };


            var byteresult = Sftp.ReadBytes(input, options, new System.Threading.CancellationToken());
            Assert.That(byteresult.GetType().IsArray, Is.EqualTo(true));
            var strres = System.Text.Encoding.GetEncoding("utf-8").GetString(byteresult);
            Assert.That(strres.Substring(0, 1), Is.EqualTo("["));

        }

        [Test]
        [Ignore("This is used to test with an actual SFTP server.")]
        public void WriteAndDeleteFiles()
        {
            var input = new WriteBytesInput
            {
                Server = "localhost",
                Port = 22,
                UserName = "sftpuser",
                Password = "",
                Directory = "",
                Path = "sftpuser/radom.bytes"
            };

            var options = new Options
            {
                IncludeType = IncludeType.Both,
                IncludeSubdirectories = true,
            };

            byte[] arr = new byte[256];
            Random rnd = new Random();
            rnd.NextBytes(arr);
            input.ContentBytes = arr;

            Sftp.WriteBytes(input, options, new System.Threading.CancellationToken());

            var checkinput = new PathInput {
                Server = "localhost",
                Port = 22,
                UserName = "sftpuser",
                Password = "",
                Directory = "",
                Path = "sftpuser/radom.bytes"
            };

            var arr2 = Sftp.ReadBytes(checkinput, options,new System.Threading.CancellationToken());
            Assert.That(arr, Is.EqualTo(arr2));

            Sftp.Delete(checkinput, options, new System.Threading.CancellationToken());

            var ex = Assert.Throws<Renci.SshNet.Common.SftpPathNotFoundException>( () => Sftp.ReadBytes(checkinput, options, new System.Threading.CancellationToken()) );

            var text_options = new TextOption
            {
                IncludeType = IncludeType.Both,
                IncludeSubdirectories = true,
                FileEncoding = FileEncoding.UTF8
            };
            var txtinput = new WriteTextInput
            {
                Server = "localhost",
                Port = 22,
                UserName = "sftpuser",
                Password = "",
                Directory = "",
                Path = "sftpuser/hui.txt"
            };

            txtinput.Content = "Hui";

            Sftp.WriteText(txtinput, text_options, new System.Threading.CancellationToken());
            checkinput = new PathInput
            {
                Server = "localhost",
                Port = 22,
                UserName = "sftpuser",
                Password = "",
                Directory = "",
                Path = "sftpuser/hui.txt"
            };


            var result = Sftp.ReadText(checkinput, text_options, new System.Threading.CancellationToken());
            Assert.That(result,Is.EqualTo(txtinput.Content));

            Sftp.Delete(checkinput, (Options) text_options, new System.Threading.CancellationToken());
            ex = Assert.Throws<Renci.SshNet.Common.SftpPathNotFoundException>(() => Sftp.ReadText(checkinput, text_options, new System.Threading.CancellationToken()));



        }
        [Test]
        [Ignore("This is used to test with an actual SFTP server.")]
        public void TestEncodings()
        {
            foreach (var enc in new FileEncoding[] { FileEncoding.UTF8, FileEncoding.ANSI, FileEncoding.ASCII, FileEncoding.Unicode, FileEncoding.Other })
            {

                

                var txtinput = new WriteTextInput
                {
                    Server = "localhost",
                    Port = 22,
                    UserName = "sftpuser",
                    Password = "",
                    Directory = "",
                    Path = "sftpuser/hui.txt"
                };
                txtinput.Content = "Hui";
                var text_options = new TextOption
                {
                    IncludeType = IncludeType.Both,
                    IncludeSubdirectories = true,
                    FileEncoding = enc
                };
                if (enc == FileEncoding.Other)
                {
                    text_options.EncodingInString = "Ei löydy";
                    Assert.Throws<System.ArgumentException>(() => Sftp.WriteText(txtinput, text_options, new System.Threading.CancellationToken()));
                    text_options.EncodingInString = "utf-7";
                    Sftp.WriteText(txtinput, text_options, new System.Threading.CancellationToken());
                } else
                {
                    Sftp.WriteText(txtinput, text_options, new System.Threading.CancellationToken());
                }
                
                var checkinput = new PathInput
                {
                    Server = "localhost",
                    Port = 22,
                    UserName = "sftpuser",
                    Password = "",
                    Directory = "",
                    Path = "sftpuser/hui.txt"
                };
                Sftp.Delete(checkinput, (Options)text_options, new System.Threading.CancellationToken());
                Assert.Throws<Renci.SshNet.Common.SftpPathNotFoundException>(() => Sftp.ReadText(checkinput, text_options, new System.Threading.CancellationToken()));
            }
        }
    }
}