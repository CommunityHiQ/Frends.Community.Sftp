using System;
using System.Collections.Generic;
using Renci.SshNet;

namespace Frends.Community.Sftp.Tests.Mocks
{
    internal class MockSftpService : ISftpService
    {
        public ISftpService Connect(ConnectionInfo connectionInfo)
        {
            return this;
        }

        public void Disconnect()
        {
        }

        public void Dispose()
        {
        }

        public IEnumerable<IFileResult> ListDirectory(string path, Action<int> listCallback = null)
        {
            if (path == "/")
            {
                return new IFileResult[] {
                    new MockFileResult { FullPath = "/documents/image01.jpg", IsDirectory = false, IsFile = true, Length = 12345, Name = "image01.jpg", LastWriteTimeUtc = new DateTime(2020,11,16,12,0,0), LastAccessTimeUtc = new DateTime(2020,12,8,11,0,0), LastWriteTime = new DateTime(2020,11,16,12,0,0), LastAccessTime = new DateTime(2020,12,8,11,0,0) },
                    new MockFileResult { FullPath = "/documents/image02.jpg", IsDirectory = false, IsFile = true, Length = 24512, Name = "image02.jpg", LastWriteTimeUtc = new DateTime(2020,11,16,13,1,0), LastAccessTimeUtc = new DateTime(2020,12,8,12,0,0), LastWriteTime = new DateTime(2020,11,16,13,1,0), LastAccessTime = new DateTime(2020,12,8,12,0,0) },
                    new MockFileResult { FullPath = "/documents/image03.jpg", IsDirectory = false, IsFile = true, Length = 54321, Name = "image03.jpg", LastWriteTimeUtc = new DateTime(2020,11,16,17,0,11), LastAccessTimeUtc = new DateTime(2020,12,8,13,0,0), LastWriteTime = new DateTime(2020,11,16,17,0,11), LastAccessTime = new DateTime(2020,12,8,13,0,0) },
                    new MockFileResult { FullPath = "/documents/importantDoc.docx", IsDirectory = false, IsFile = true, Length = 12345, Name = "importantDoc.docx", LastWriteTimeUtc = new DateTime(2020,11,16,22,0,0), LastAccessTimeUtc = new DateTime(2020,12,8,14,0,0), LastWriteTime = new DateTime(2020,11,16,22,0,0), LastAccessTime = new DateTime(2020,12,8,14,0,0) },
                    new MockFileResult { FullPath = "/documents/subdir", IsDirectory = true, IsFile = false, Length = 12345, Name = "subdir", LastWriteTimeUtc = new DateTime(2020,11,16,23,0,0), LastAccessTimeUtc = new DateTime(2020,12,8,15,0,0), LastWriteTime = new DateTime(2020,11,16,23,0,0), LastAccessTime = new DateTime(2020,12,8,15,0,0) }
                };
            }

            return new IFileResult[] {
                    new MockFileResult { FullPath = "/documents/subdir/fileInSubdir.jpg", IsDirectory = false, IsFile = true, Length = 12345, Name = "fileInSubdir.jpg", LastWriteTimeUtc = new DateTime(2020,11,16,12,0,0), LastAccessTimeUtc = new DateTime(2020,12,8,11,0,0), LastWriteTime = new DateTime(2020,11,16,12,0,0), LastAccessTime = new DateTime(2020,12,8,11,0,0) },
                    new MockFileResult { FullPath = "/documents/subdir/anotherFileInSubdir.docx", IsDirectory = false, IsFile = true, Length = 24512, Name = "anotherFileInSubdir.docx", LastWriteTimeUtc = new DateTime(2020,11,16,13,1,0), LastAccessTimeUtc = new DateTime(2020,12,8,12,0,0), LastWriteTime = new DateTime(2020,11,16,13,1,0), LastAccessTime = new DateTime(2020,12,8,12,0,0) }
                };
        }
        public void Delete(string path, Action<int> listCallback = null)
        {

        }
        public void WriteBytes(byte[] bytes, string path, Action<int> listCallback = null)
        {

        }
        public byte[] ReadBytes(string path, Action<int> listCallback = null)
        {
            return System.Text.Encoding.GetEncoding("utf-8").GetBytes("1231232");
        }

    }
}