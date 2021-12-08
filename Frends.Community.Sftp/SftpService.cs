using System;
using System.Collections.Generic;
using System.Linq;
using Renci.SshNet;

namespace Frends.Community.Sftp
{
    internal class SftpService : ISftpService
    {
        private SftpClient _client;

        public ISftpService Connect(ConnectionInfo connectionInfo)
        {
            _client = new SftpClient(connectionInfo);
            _client.Connect();

            return this;
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public IEnumerable<IFileResult> ListDirectory(string path, Action<int> listCallback = null)
        {
            return _client.ListDirectory(path, listCallback).Select(f => new FileResult(f));
        }

        public void WriteBytes(byte[] bytes, string path, Action<int> listCallback = null)
        {
            _client.WriteAllBytes(path, bytes);
        }
        public byte[] ReadBytes(string path, Action<int> listCallback = null)
        {
            return _client.ReadAllBytes(path);
        }
        public void Delete(string path,Action<int> listCallback = null)
        {
            _client.Delete(path);
        }
    }
}
