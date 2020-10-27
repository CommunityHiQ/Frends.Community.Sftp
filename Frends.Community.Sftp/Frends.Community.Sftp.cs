﻿using System;
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
        /// <returns>List of IFileResult objects.</returns>
        public static List<IFileResult> ListDirectory([PropertyTab] Parameters input, [PropertyTab] Options options, CancellationToken cancellationToken)
        {
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

        private static ConnectionInfo GetConnectionInfo(Parameters input, Options options)
        {
            if (string.IsNullOrEmpty(options.PrivateKeyFileName))
                return new PasswordConnectionInfo(input.Server, input.Port, input.UserName, input.Password);

            return new PrivateKeyConnectionInfo(input.Server, input.Port, input.UserName, new PrivateKeyFile(options.PrivateKeyFileName, options.Passphrase));
        }

        private static string WildCardToRegex(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
    }
}
