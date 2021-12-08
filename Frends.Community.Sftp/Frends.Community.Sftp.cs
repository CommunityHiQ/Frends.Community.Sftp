using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Renci.SshNet;

#pragma warning disable 1591

namespace Frends.Community.Sftp
{
    public class Sftp
    {
        private readonly Lazy<ISftpService> _sftpService = new Lazy<ISftpService>(() =>
        {
            var serviceProvider = new ServiceCollection()
                                        .AddTransient<ISftpService, SftpService>()
                                        .BuildServiceProvider();

            return serviceProvider.GetService<ISftpService>();
        });

        /// <summary>
        /// List files and directories.
        /// Documentation: https://github.com/CommunityHiQ/Frends.Community.Sftp
        /// </summary>
        /// <param name="input">Connection information.</param>
        /// <param name="options">Optional parameters.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List [ Object { string FullPath, bool IsDirectory, bool IsFile, long Length, string Name, DateTime LastWriteTimeUtc, DateTime LastAccessTimeUtc, DateTime LastWriteTime, DateTime LastAccessTime } ]</returns>
        public static List<IFileResult> ListDirectory([PropertyTab] Parameters input, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
            return new Sftp().ListDirectoryInternal(input, options, cancellationToken);
        }

        

        internal List<IFileResult> ListDirectoryInternal(Parameters input, Options options, CancellationToken cancellationToken)
        {
            var connectionInfo = GetConnectionInfo(input, options);
            var regexStr = string.IsNullOrEmpty(options.FileMask) ? string.Empty : WildCardToRegex(options.FileMask);
            var result = new List<IFileResult>();

            var sftp = _sftpService.Value;
            using (sftp.Connect(connectionInfo))
            {
                result = ListDirectoryRecursiveInternal(regexStr, sftp, input.Directory, options, cancellationToken);
                sftp.Disconnect();
            }

            return result;
        }

        internal List<IFileResult> ListDirectoryRecursiveInternal(string regexStr, ISftpService sftp, string directory, Options options, CancellationToken cancellationToken)
        {
            List<IFileResult> directoryList = new List<IFileResult>();

            var files = sftp.ListDirectory(directory);
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (options.IncludeType == IncludeType.Both
                    || (file.IsDirectory && options.IncludeType == IncludeType.Directory)
                    || (file.IsFile && options.IncludeType == IncludeType.File))
                {
                    if (Regex.IsMatch(file.Name, regexStr, RegexOptions.IgnoreCase))
                        directoryList.Add(file);
                }
                if (file.IsDirectory && options.IncludeSubdirectories)
                {
                    directoryList.AddRange(ListDirectoryRecursiveInternal(regexStr, sftp, file.FullPath, options, cancellationToken));
                }
            }
            return directoryList;
        }

        public static void Delete([PropertyTab] PathInput input, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
            new Sftp().DeleteInternal(input, options, cancellationToken);

        }
        internal void DeleteInternal(PathInput input,Options options, CancellationToken cancellation)
        {
            var connectionInfo = GetConnectionInfo((Parameters) input, (Options) options);
            var sftp = _sftpService.Value;
            using (sftp.Connect(connectionInfo))
            {
                sftp.Delete(input.Path);
                sftp.Disconnect();
            }
        }

        public static void WriteBytes([PropertyTab] WriteBytesInput input, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
            new Sftp().WriteInternal((byte[] ) input.ContentBytes,input.Path, (Parameters) input, options, cancellationToken);
        }

        public static void WriteText([PropertyTab] WriteTextInput input, [PropertyTab] TextOption options, CancellationToken cancellationToken)
        {
            var encoding = GetEncodingString(options);
            new Sftp().WriteInternal(System.Text.Encoding.GetEncoding(encoding).GetBytes(input.Content), input.Path, (Parameters) input, options, cancellationToken);
        }

        internal void WriteInternal(byte[] bytes, string path, Parameters input, Options options, CancellationToken cancellation)
        {
            var connectionInfo = GetConnectionInfo((Parameters)input, (Options)options);
            var sftp = _sftpService.Value;
            using (sftp.Connect(connectionInfo))
            {
                sftp.WriteBytes(bytes, path);
                sftp.Disconnect();
            }
        }

        public static byte[] ReadBytes([PropertyTab] PathInput input, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
            return new Sftp().ReadInternal(input, options, cancellationToken);
        }

        public static string ReadText([PropertyTab] PathInput input, [PropertyTab] TextOption options, CancellationToken cancellationToken)
        {
            var encoding = GetEncodingString(options);

            return System.Text.Encoding.GetEncoding(encoding).GetString(
                new Sftp().ReadInternal(input, (Options) options, cancellationToken));
        }


        private static string GetEncodingString(TextOption options)
        {
            switch (options.FileEncoding)
            {
                case FileEncoding.UTF8:
                    return "utf-8";
                    break;
                case FileEncoding.ANSI:
                    return "iso-8859-1";
                    break;
                case FileEncoding.ASCII:
                    return "us-ascii";
                    break;
                case FileEncoding.Unicode:
                    return "utf-16";
                    break;
                case FileEncoding.Other:
                    return options.EncodingInString;
                    break;
                default:
                    return "";
                    break;
            }
        }


        internal byte[] ReadInternal(PathInput input, Options options, CancellationToken cancellation)
        {
            var connectionInfo = GetConnectionInfo((Parameters)input, (Options)options);
            var sftp = _sftpService.Value;
            byte[] rval;
            using (sftp.Connect(connectionInfo))
            {
                rval = sftp.ReadBytes(input.Path);
                sftp.Disconnect();
            }
            return rval;
        }



        private static ConnectionInfo GetConnectionInfo(Parameters input, Options options)
        {
            if (string.IsNullOrEmpty(options.PrivateKeyFileName))
            {
                if (!options.UseKeyboardInteractiveAuthenticationMethod)
                    return new PasswordConnectionInfo(input.Server, input.Port, input.UserName, input.Password);

                var keyboardInteractiveAuth = Sftp.GetKeyboardInteractiveAuthentication(input.UserName, input.Password);

                PasswordAuthenticationMethod pauth = new PasswordAuthenticationMethod(input.UserName, input.Password);

                return new ConnectionInfo(input.Server, input.Port, input.UserName, pauth, keyboardInteractiveAuth);
            }

            if (string.IsNullOrEmpty(options.Passphrase))
            {
                if (options.UseKeyboardInteractiveAuthenticationMethod)
                {
                    var keyboardInteractiveAuth = Sftp.GetKeyboardInteractiveAuthentication(input.UserName, input.Password);

                    var privateKeyAuth = new PrivateKeyAuthenticationMethod(input.UserName,
                                                            new PrivateKeyFile(options.PrivateKeyFileName));

                    return new ConnectionInfo(input.Server, input.Port, input.UserName, privateKeyAuth, keyboardInteractiveAuth);
                }

                return new PrivateKeyConnectionInfo(input.Server, input.Port, input.UserName, new PrivateKeyFile(options.PrivateKeyFileName));
            }

            if (options.UseKeyboardInteractiveAuthenticationMethod)
            {
                var keyboardInteractiveAuth = Sftp.GetKeyboardInteractiveAuthentication(input.UserName, input.Password);

                var privateKeyAuth = new PrivateKeyAuthenticationMethod(input.UserName,
                                                        new PrivateKeyFile(options.PrivateKeyFileName, options.Passphrase));

                return new ConnectionInfo(input.Server, input.Port, input.UserName, privateKeyAuth, keyboardInteractiveAuth);
            }

            return new PrivateKeyConnectionInfo(input.Server, input.Port, input.UserName, new PrivateKeyFile(options.PrivateKeyFileName, options.Passphrase));
        }

        private static KeyboardInteractiveAuthenticationMethod GetKeyboardInteractiveAuthentication(string username, string password)
        {
            var keyboardInteractiveAuth = new KeyboardInteractiveAuthenticationMethod(username);
            keyboardInteractiveAuth.AuthenticationPrompt += (sender, args) =>
            {
                foreach (var authenticationPrompt in args.Prompts)
                    authenticationPrompt.Response = password;
            };

            return (keyboardInteractiveAuth);
        }

        private static string WildCardToRegex(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
    }
}
