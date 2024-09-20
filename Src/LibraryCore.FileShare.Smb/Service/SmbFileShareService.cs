using LibraryCore.FileShare.Smb.ExtensionMethods;
using LibraryCore.FileShare.Smb.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SMBLibrary;
using SMBLibrary.Client;

namespace LibraryCore.FileShare.Smb.Service;

/// <summary>
/// Service class to interact with the SMB file share. This service will support Smb 2.0 on a linux or windows device / server.
/// </summary>
/// <param name="settings">Settings which contain the server and authentication account</param>
/// <param name="smbClient">The smb client</param>
public class SmbFileShareService(ISMBClient smbClient, IOptions<SmbFileShareSettings> settings) : ISmbFileShareService, IDisposable
{
    private bool Disposed { get; set; }
    private ISMBClient SmbClient { get; } = smbClient;
    private IOptions<SmbFileShareSettings> Settings { get; } = settings;
    private static MemoryCache Cache { get; } = new(new MemoryCacheOptions());

    /// <summary>
    /// Get a list of file shares for a given server
    /// </summary>
    /// <returns>List of file share names</returns>
    public IEnumerable<string> ListShares()
    {
        ConnectAndLogIn();

        var shares = SmbClient.ListShares(out var status);

        status.ThrowIfNotSuccessful();

        return shares;
    }

    /// <summary>
    /// Get a list of directories and files for a given share
    /// </summary>
    /// <param name="shareName">The share name to connect to</param>
    /// <param name="fileName">File name to search for or use the default of a star to search for everything</param>
    /// <param name="path">Search on a specific path. Format should be @"MySubFolder\MySubSubFolder. Or you can leave it blank to search from the root of the file share</param>
    /// <returns>List of files and directories which you can parse through</returns>
    public IEnumerable<QueryDirectoryFileInformation> ListFileAndDirectories(string shareName, string fileName = "*", string path = "")
    {
        ConnectAndLogIn();

        var fileStore = SmbClient.TreeConnect(shareName, out var status);

        status.ThrowIfNotSuccessful();

        try
        {
            fileStore.CreateFile(out var directoryHandle, out var fileStatus, path, AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null)
                .ThrowIfNotSuccessful();

            fileStore.QueryDirectory(out var fileList, directoryHandle, fileName, FileInformationClass.FileDirectoryInformation)
                .ThrowIfNotStatus([NTStatus.STATUS_NO_MORE_FILES, .. ThrowUtilities.OnlySuccess]);

            fileStore.CloseFile(directoryHandle)
                .ThrowIfNotSuccessful();

            return fileList;
        }
        finally
        {
            fileStore.Disconnect();
        }
    }

    /// <summary>
    /// Read a file from a share and path
    /// </summary>
    /// <param name="shareName">The share name to connect to</param>
    /// <param name="filePath">Full Path to the file to read</param>
    /// <returns>The file in a byte array</returns>
    public byte[] ReadFile(string shareName, string filePath)
    {
        ConnectAndLogIn();

        var fileStore = SmbClient.TreeConnect(shareName, out var status);
        object? fileHandle = null;

        try
        {
            status.ThrowIfNotSuccessful();

            fileStore.CreateFile(out fileHandle, out var fileStatus, filePath, AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null)
                   .ThrowIfNotSuccessful();

            using var stream = new MemoryStream();

            long bytesRead = 0;

            while (true)
            {
                var tempReadFileStatus = fileStore.ReadFile(out var buffer, fileHandle, bytesRead, (int)SmbClient.MaxReadSize);

                tempReadFileStatus.ThrowIfNotStatus([NTStatus.STATUS_END_OF_FILE, .. ThrowUtilities.OnlySuccess]);

                //end of file
                if (tempReadFileStatus == NTStatus.STATUS_END_OF_FILE || buffer.Length == 0)
                {
                    break;
                }

                bytesRead += buffer.Length;
                stream.Write(buffer, 0, buffer.Length);
            }

            return stream.ToArray();
        }
        finally
        {
            if (fileHandle is not null)
            {
                _ = fileStore.CloseFile(fileHandle);
            }

            fileStore.Disconnect();
        }
    }

    /// <summary>
    /// Create a file in a share and folder path
    /// </summary>
    /// <param name="shareName">The share name to connect to</param>
    /// <param name="filePath">Full Path to the file to create</param>
    /// <param name="fileStream">The stream of the file you want to create</param>
    public void CreateFile(string shareName, string filePath, Stream fileStream)
    {
        ConnectAndLogIn();

        var fileStore = SmbClient.TreeConnect(shareName, out var status);
        object? fileHandle = null;

        try
        {
            status.ThrowIfNotSuccessful();

            fileStore.CreateFile(out fileHandle, out var fileStatus, filePath, AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.None, CreateDisposition.FILE_CREATE, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null)
                .ThrowIfNotSuccessful();

            int writePosition = 0;
            int maxWriteSize = (int)SmbClient.MaxWriteSize;

            while (fileStream.Position < fileStream.Length)
            {
                var buffer = new byte[maxWriteSize];

                int bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                if (bytesRead < maxWriteSize)
                {
                    Array.Resize(ref buffer, bytesRead);
                }

                fileStore.WriteFile(out var numberOfBytesWritten, fileHandle, (uint)writePosition, buffer)
                    .ThrowIfNotSuccessful();

                writePosition += numberOfBytesWritten;
            }
        }
        finally
        {
            if (fileHandle is not null)
            {
                _ = fileStore.CloseFile(fileHandle);
            }

            fileStore.Disconnect();
        }
    }

    /// <summary>
    /// Delete a file from a share and folder path
    /// </summary>
    /// <param name="shareName">The share name to connect to</param>
    /// <param name="filePath">Full Path to the file to delete</param>
    public void DeleteFile(string shareName, string filePath)
    {
        ConnectAndLogIn();

        var fileStore = SmbClient.TreeConnect(shareName, out var status);
        object? fileHandle = null;

        try
        {
            status.ThrowIfNotSuccessful();

            fileStore.CreateFile(out fileHandle, out var fileStatus, filePath, AccessMask.GENERIC_WRITE | AccessMask.DELETE | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.None, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null)
                .ThrowIfNotSuccessful();

            FileDispositionInformation fileDispositionInformation = new()
            {
                DeletePending = true
            };

            fileStore.SetFileInformation(fileHandle, fileDispositionInformation)
               .ThrowIfNotSuccessful();
        }
        finally
        {
            if (fileHandle is not null)
            {
                _ = fileStore.CloseFile(fileHandle);
            }

            fileStore.Disconnect();
        }
    }

    /// <summary>
    /// Move a file from a folder to another folder in a share
    /// </summary>
    /// <param name="shareName">Share name</param>
    /// <param name="currentFilePath">The current path to the file. Ie: FolderA/File.pdf</param>
    /// <param name="destinationFilePath">The destination to move it to. Ie: FolderB/File.pdf</param>
    public void MoveFile(string shareName, string currentFilePath, string destinationFilePath)
    {
        ConnectAndLogIn();

        var fileStore = SmbClient.TreeConnect(shareName, out var status);
        object? fileHandle = null;

        try
        {
            status.ThrowIfNotSuccessful();

            var shareAccess = ShareAccess.Read | ShareAccess.Write | ShareAccess.Delete;

            fileStore.CreateFile(out fileHandle, out var fileStatus, currentFilePath, AccessMask.DELETE, 0, shareAccess, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null)
                   .ThrowIfNotSuccessful();

            var renameInfo = new FileRenameInformationType2
            {
                FileName = destinationFilePath
            };

            status = fileStore.SetFileInformation(fileHandle, renameInfo);
        }
        finally
        {
            if (fileHandle is not null)
            {
                _ = fileStore.CloseFile(fileHandle);
            }

            fileStore.Disconnect();
        }
    }

    private void ConnectAndLogIn()
    {
        if (!SmbClient.IsConnected)
        {
            //no lock...just basic get me the ip address
            var ipAddress = Cache.GetOrCreate(Settings.Value.ServerName, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return Settings.Value.ResolveIpAddress();
            });

            if (!SmbClient.Connect(ipAddress, SMBTransportType.DirectTCPTransport))
            {
                throw new Exception($"Can't Connect To Shared Drive On Server = {ipAddress}");
            }

            SmbClient.Login(Settings.Value.DomainAuthentication, Settings.Value.AuthenticationUserName, Settings.Value.AuthenticationPassword)
                .ThrowIfNotSuccessful();
        }
    }

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                if (SmbClient.IsConnected)
                {
                    SmbClient.Logoff();
                    SmbClient.Disconnect();
                }
            }

            Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
