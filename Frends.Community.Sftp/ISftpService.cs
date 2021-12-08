using System;
using System.Collections.Generic;
using Renci.SshNet;

#pragma warning disable 1591

namespace Frends.Community.Sftp
{
    public interface ISftpService : IDisposable
    {
        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <param name="connectionInfo">Connection information.</param>
        /// <returns>Instance of SftpService implementation.</returns>
        ISftpService Connect(ConnectionInfo connectionInfo);

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Retrieves list of files in remote directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>List of files.</returns>
        IEnumerable<IFileResult> ListDirectory(string path, Action<int> listCallback = null);

        /// <summary>
        /// Writes array of bytes to a file 
        /// </summary>
        /// <param name="bytes">byte array to be writteb to the file</param>
        /// <param name="path">The path to the file to receive the array of bytes</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>nothing</returns>
        void WriteBytes(byte[] bytes, string path, Action<int> listCallback = null);

        /// <summary>
        /// Reads array of bytes from a file 
        /// </summary>
        /// <param name="path">The path to the file from which to read the array of bytes</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>nothing</returns>
        byte[] ReadBytes(string path, Action<int> listCallback = null);

        /// <summary>
        /// deletes a file 
        /// </summary>
        /// <param name="path">The path to the file to be deleted</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>nothing</returns>
        void Delete(string path, Action<int> listCallback = null);

    }
}
