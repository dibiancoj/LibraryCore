using SMBLibrary;

namespace LibraryCore.FileShare.Smb.Service;

/// <summary>
/// Service class to interact with the SMB file share. This service will support Smb 2.0 on a linux or windows device / server.
/// </summary>
public interface ISmbFileShareService : IDisposable
{
    /// <summary>
    /// Get a list of file shares for a given server
    /// </summary>
    /// <returns>List of file share names</returns>
    public IEnumerable<string> ListShares();

    /// <summary>
    /// Get a list of directories and files for a given share
    /// </summary>
    /// <param name="shareName">The share name to connect to</param>
    /// <param name="fileName">File name to search for or use the default of a star to search for everything</param>
    /// <param name="path">Search on a specific path. Format should be @"MySubFolder\MySubSubFolder. Or you can leave it blank to search from the root of the file share</param>
    /// <returns>List of files and directories which you can parse through</returns>
    public IEnumerable<QueryDirectoryFileInformation> ListFileAndDirectories(string shareName, string fileName = "*", string path = "");

    /// <summary>
    /// Read a file from a share and path
    /// </summary>
    /// <param name="shareName">The share name to connect to</param>
    /// <param name="filePath">Full Path to the file to read</param>
    /// <returns>The file in a byte array</returns>
    public byte[] ReadFile(string shareName, string filePath);

    /// <summary>
    /// Create a file in a share and folder path
    /// </summary>
    /// <param name="shareName">The share name to connect to</param>
    /// <param name="filePath">Full Path to the file to create</param>
    /// <param name="fileStream">The stream of the file you want to create</param>
    public void CreateFile(string shareName, string filePath, Stream fileStream);

    /// <summary>
    /// Delete a file from a share and folder path
    /// </summary>
    /// <param name="shareName">The share name to connect to</param>
    /// <param name="filePath">Full Path to the file to delete</param>
    public void DeleteFile(string shareName, string filePath);

    /// <summary>
    /// Move a file from a folder to another folder in a share
    /// </summary>
    /// <param name="shareName">Share name</param>
    /// <param name="currentFilePath">The current path to the file. Ie: FolderA/File.pdf</param>
    /// <param name="destinationFilePath">The destination to move it to. Ie: FolderB/File.pdf</param>
    public void MoveFile(string shareName, string currentFilePath, string destinationFilePath);
}
