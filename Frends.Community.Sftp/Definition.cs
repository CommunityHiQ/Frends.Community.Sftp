#pragma warning disable 1591

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Renci.SshNet.Sftp;
using System.Globalization;

namespace Frends.Community.Sftp
{
    /// <summary>
    /// Enumeration to specify if the directory listing should contain files, directories or both.
    /// </summary>
    public enum IncludeType
    {
        File,
        Directory,
        Both
    }

    /// <summary>
    /// Parameters class usually contains parameters that are required.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// Server address
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Server { get; set; }

        /// <summary>
        /// Port number
        /// </summary>
        [DefaultValue(22)]
        public int Port { get; set; } = 22;

        /// <summary>
        /// Username
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [PasswordPropertyText]
        public string Password { get; set; }

        /// <summary>
        /// Directory on the server.
        /// </summary>
        [DefaultValue("/")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Directory { get; set; } = "/";
    }

    /// <summary>
    /// Options class provides additional optional parameters.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Types to include in the directory listing.
        /// </summary>
        [DefaultValue(IncludeType.File)]
        public IncludeType IncludeType { get; set; } = IncludeType.File;

        /// <summary>
        /// Pattern to match.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string FileMask { get; set; }

        /// <summary>
        /// Include subdirectories?
        /// </summary>
        public bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// Full path to private key file.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string PrivateKeyFileName { get; set; }

        /// <summary>
        /// Passphrase for the private key file.
        /// </summary>
        [PasswordPropertyText]
        public string Passphrase { get; set; }

        /// <summary>
        /// If set, it allows you to use keyboard-interactive authentication.
        /// </summary>
        public bool UseKeyboardInteractiveAuthenticationMethod { get; set; }
    }
    public enum FileEncoding { UTF8, ANSI, ASCII, Unicode, Other }
    public class TextOption : Options
    {
        
        /// <summary>
        /// Encoding for the read content. By selecting 'Other' you can use any encoding.
        /// </summary>
        public FileEncoding FileEncoding { get; set; }

        [UIHint(nameof(FileEncoding), "", FileEncoding.UTF8)]
        public bool EnableBom { get; set; }

        /// <summary>
        /// File encoding to be used. A partial list of possible encodings: https://en.wikipedia.org/wiki/Windows_code_page#List
        /// </summary>
        [UIHint(nameof(FileEncoding), "", FileEncoding.Other)]
        public string EncodingInString { get; set; }
    }

    public class PathInput : Parameters
    {
        /// <summary>
        /// Path of the file 
        /// </summary>
        [DefaultValue("")]
        public string Path { get; set; }
    }

    
    public class WriteBytesInput : Parameters
    {
        /// <summary>
        /// Byte array to be written to the file
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]

        public object ContentBytes { get; set; }
        /// <summary>
        /// Path of the file to be read 
        /// </summary>
        [DefaultValue("")]
        public string Path { get; set; }

    }
    public class WriteTextInput : Parameters
    {
        /// <summary>
        /// Text content to be written to the file
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Path of the file to be read 
        /// </summary>
        [DefaultValue("")]
        public string Path { get; set; }

    }



    public class FileResult : IFileResult
    {
        /// <summary>
        /// Full path of directory or file.
        /// </summary>
        public string FullPath { get; }
        public bool IsDirectory { get; }
        public bool IsFile { get; }

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long Length { get; }
        public string Name { get; }

        /// <summary>
        /// Timestamps for last access and write in both UTC and current timezone.
        /// </summary>
        public DateTime LastWriteTimeUtc { get; }
        public DateTime LastAccessTimeUtc { get; }
        public DateTime LastWriteTime { get; }
        public DateTime LastAccessTime { get; }

        public FileResult(SftpFile file)
        {
            this.FullPath = file.FullName;
            this.IsDirectory = file.IsDirectory;
            this.IsFile = file.IsRegularFile;
            this.Length = file.Length;
            this.Name = file.Name;
            this.LastWriteTimeUtc = file.LastWriteTimeUtc;
            this.LastAccessTimeUtc = file.LastAccessTimeUtc;
            this.LastWriteTime = file.LastWriteTime;
            this.LastAccessTime = file.LastAccessTime;
        }
    }
}
