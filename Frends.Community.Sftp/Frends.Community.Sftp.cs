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
        List<IFileResult> resultInclSubdirs = new List<IFileResult>();

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
            if (options.IncludeSubdirectories is true)
            {
                return new Sftp().ListDirectoryInclSubdirsInternal(input, options, cancellationToken);
            }
            return new Sftp().ListDirectoryInternal(input, options, cancellationToken);
        }

        internal List<IFileResult> ListDirectoryInternal(Parameters input, Options options, CancellationToken cancellationToken)
        {
            var result = new List<IFileResult>();
            var connectionInfo = GetConnectionInfo(input, options);
            var regexStr = string.IsNullOrEmpty(options.FileMask) ? string.Empty : WildCardToRegex(options.FileMask);

            var sftp = _sftpService.Value;
            using (sftp.Connect(connectionInfo))
            {
                var files = sftp.ListDirectory(input.Directory);
                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (options.IncludeType == IncludeType.Both
                        || (file.IsDirectory && options.IncludeType == IncludeType.Directory)
                        || (file.IsFile && options.IncludeType == IncludeType.File))
                    {
                        if (Regex.IsMatch(file.Name, regexStr, RegexOptions.IgnoreCase))
                            result.Add(file);
                    }
                }

                sftp.Disconnect();
            }

            return result;
        }

        internal List<IFileResult> ListDirectoryInclSubdirsInternal(Parameters input, Options options, CancellationToken cancellationToken)
        {
            var connectionInfo = GetConnectionInfo(input, options);
            var regexStr = string.IsNullOrEmpty(options.FileMask) ? string.Empty : WildCardToRegex(options.FileMask);

            var sftp = _sftpService.Value;
            using (sftp.Connect(connectionInfo))
            {
                ListDirectoryRecursiveInternal(regexStr, sftp, input.Directory, options, cancellationToken);
                sftp.Disconnect();
            }

            return resultInclSubdirs;
        }

        internal void ListDirectoryRecursiveInternal(string regexStr, ISftpService sftp, string directory, Options options, CancellationToken cancellationToken)
        {
            var files = sftp.ListDirectory(directory);
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (options.IncludeType == IncludeType.Both
                    || (file.IsDirectory && options.IncludeType == IncludeType.Directory)
                    || (file.IsFile && options.IncludeType == IncludeType.File))
                {
                    if (Regex.IsMatch(file.Name, regexStr, RegexOptions.IgnoreCase))
                        resultInclSubdirs.Add(file);
                }
                if (file.IsDirectory)
                {
                    ListDirectoryRecursiveInternal(regexStr, sftp, file.FullPath, options, cancellationToken);
                }
            }
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
