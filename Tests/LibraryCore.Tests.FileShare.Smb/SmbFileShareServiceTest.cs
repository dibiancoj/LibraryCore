using LibraryCore.FileShare.Smb.CustomExceptions;
using LibraryCore.FileShare.Smb.Service;
using Microsoft.Extensions.Options;
using SMBLibrary;
using SMBLibrary.Client;
using System.Net;

namespace LibraryCore.Tests.FileShare.Smb;

public class SmbFileShareServiceTest
{
    public SmbFileShareServiceTest()
    {
        MockISmbClient = new Mock<ISMBClient>();
        MappedDriveSettingsTestToUse = Options.Create(new MappedDriveSettingsTest
        {
            AuthenticationPassword = "password",
            AuthenticationUserName = "username",
            ServerName = "MyServerName",
            DomainAuthentication = "mycorp.org"
        });
        SmbFileShareServiceToUse = new SmbFileShareService(MockISmbClient.Object, MappedDriveSettingsTestToUse);
    }

    private Mock<ISMBClient> MockISmbClient { get; }
    private IOptions<MappedDriveSettingsTest> MappedDriveSettingsTestToUse { get; }
    private ISmbFileShareService SmbFileShareServiceToUse { get; }

    #region Setup

    private void SetupConnectAndLogin(bool isConnectedResult = true)
    {
        MockISmbClient.Setup(x => x.Connect(It.Is<IPAddress>(t => Equals(t, MappedDriveSettingsTest.IpAddressToUse)), SMBTransportType.DirectTCPTransport))
            .Callback(() => MockISmbClient.Setup(x => x.IsConnected).Returns(isConnectedResult))
            .Returns(true);

        MockISmbClient.Setup(x => x.Login("mycorp.org", "username", "password"))
            .Returns(NTStatus.STATUS_SUCCESS);
    }

    #endregion

    #region Tests

    [Fact]
    public void ConnectionOnlyOncePerScopedInstance()
    {
        SetupConnectAndLogin();

        NTStatus status = NTStatus.STATUS_SUCCESS;

        MockISmbClient.Setup(x => x.ListShares(out status))
            .Returns(["Share1", "Share2"]);

        var result = SmbFileShareServiceToUse.ListShares();
        _ = SmbFileShareServiceToUse.ListShares();

        Assert.Equal(2, result.Count());

        MockISmbClient.Verify(x => x.Connect(It.Is<IPAddress>(t => Equals(t, MappedDriveSettingsTest.IpAddressToUse)), SMBTransportType.DirectTCPTransport), Times.Once);
        MockISmbClient.Verify(x => x.Login("mycorp.org", "username", "password"), Times.Once);
        MockISmbClient.Verify(x => x.ListShares(out status), Times.Exactly(2));
    }

    [Fact]
    public void ListShares()
    {
        SetupConnectAndLogin();

        NTStatus status;

        MockISmbClient.Setup(x => x.ListShares(out status))
            .Returns(["Share1", "Share2"]);

        var result = SmbFileShareServiceToUse.ListShares();

        Assert.Equal(2, result.Count());
        Assert.Contains("Share1", result);
        Assert.Contains("Share2", result);

        MockISmbClient.Verify(x => x.Connect(It.Is<IPAddress>(t => Equals(t, MappedDriveSettingsTest.IpAddressToUse)), SMBTransportType.DirectTCPTransport), Times.Once);
        MockISmbClient.Verify(x => x.Login("mycorp.org", "username", "password"), Times.Once);
        MockISmbClient.Verify(x => x.ListShares(out status), Times.Once);
    }

    [Fact]
    public void ListFileAndDirectories()
    {
        const string shareName = "Share1";

        object handle = default!;
        NTStatus status;
        FileStatus fileStatus;
        List<QueryDirectoryFileInformation> queryDirectoryFiles;

        SetupConnectAndLogin();

        var mockTreeConnect = new Mock<ISMBFileStore>();

        MockISmbClient.Setup(x => x.TreeConnect(shareName, out status))
           .Returns(mockTreeConnect.Object);

        mockTreeConnect.Setup(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4", AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null))
            .Returns(NTStatus.STATUS_SUCCESS);

        mockTreeConnect.Setup(x => x.QueryDirectory(out queryDirectoryFiles, handle, "*", FileInformationClass.FileDirectoryInformation))
            .Callback((out List<QueryDirectoryFileInformation> result, object handle, string fileName, FileInformationClass information) =>
            {
                result = ([new FileDirectoryInformation
                {
                    FileName = "FolderA",
                    FileAttributes = SMBLibrary.FileAttributes.Directory
                },
                    new FileDirectoryInformation
                    {
                        FileName = "FileA",
                        FileAttributes = SMBLibrary.FileAttributes.Archive
                    }]);
            })
            .Returns(NTStatus.STATUS_SUCCESS);

        mockTreeConnect.Setup(x => x.CloseFile(handle))
            .Returns(NTStatus.STATUS_SUCCESS);

        mockTreeConnect.Setup(x => x.Disconnect());

        var result = SmbFileShareServiceToUse.ListFileAndDirectories(shareName, path: @"MyFolder\QA4");

        Assert.Equal(2, result.Count());
        Assert.Contains(result.Cast<FileDirectoryInformation>(), x => x.FileName == "FolderA" && x.FileAttributes == SMBLibrary.FileAttributes.Directory);
        Assert.Contains(result.Cast<FileDirectoryInformation>(), x => x.FileName == "FileA" && x.FileAttributes == SMBLibrary.FileAttributes.Archive);

        MockISmbClient.Verify(x => x.TreeConnect(shareName, out status), Times.Once);
        mockTreeConnect.Verify(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4", AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null), Times.Once);
        mockTreeConnect.Verify(x => x.QueryDirectory(out queryDirectoryFiles, handle, "*", FileInformationClass.FileDirectoryInformation), Times.Once);
        mockTreeConnect.Verify(x => x.CloseFile(handle), Times.Once());
        mockTreeConnect.Verify(x => x.Disconnect(), Times.Once());
    }

    [Fact]
    public void ReadFileNoBuffering()
    {
        const string shareName = "Share1";
        object handle = default!;
        NTStatus status;
        FileStatus fileStatus;
        byte[] fileBytes = [];

        SetupConnectAndLogin();

        var mockTreeConnect = new Mock<ISMBFileStore>();

        MockISmbClient.SetupGet(x => x.MaxReadSize)
            .Returns(65536);

        MockISmbClient.Setup(x => x.TreeConnect(shareName, out status))
           .Returns(mockTreeConnect.Object);

        mockTreeConnect.Setup(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4\test.txt", AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null))
          .Returns(NTStatus.STATUS_SUCCESS);

        mockTreeConnect.Setup(x => x.ReadFile(out fileBytes, handle, 0, 65536))
            .Returns(NTStatus.STATUS_SUCCESS)
            .Callback((out byte[] data, object handle, long offset, int maxCount) =>
            {
                data = [0, 1, 2];
            });

        mockTreeConnect.Setup(x => x.ReadFile(out fileBytes, handle, 3, 65536))
         .Returns(NTStatus.STATUS_END_OF_FILE);

        mockTreeConnect.Setup(x => x.Disconnect());

        var result = SmbFileShareServiceToUse.ReadFile(shareName, @"MyFolder\QA4\test.txt");

        Assert.Equal(3, result.Length);
        Assert.Equal(result, [0, 1, 2]);

        MockISmbClient.Verify(x => x.TreeConnect(shareName, out status), Times.Once);
        mockTreeConnect.Verify(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4\test.txt", AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null), Times.Once);
        mockTreeConnect.Verify(x => x.ReadFile(out fileBytes, handle, 0, 65536), Times.Once);
        mockTreeConnect.Verify(x => x.ReadFile(out fileBytes, handle, 3, 65536), Times.Once);
        mockTreeConnect.Verify(x => x.Disconnect(), Times.Once());
    }

    [Fact]
    public void ReadFileWithBuffering()
    {
        const string shareName = "Share1";
        object handle = default!;
        NTStatus status;
        FileStatus fileStatus;
        byte[] fileBytes = [];

        SetupConnectAndLogin();

        var mockTreeConnect = new Mock<ISMBFileStore>();

        MockISmbClient.SetupGet(x => x.MaxReadSize)
            .Returns(65536);

        MockISmbClient.Setup(x => x.TreeConnect(shareName, out status))
           .Returns(mockTreeConnect.Object);

        mockTreeConnect.Setup(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4\test.txt", AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null))
          .Returns(NTStatus.STATUS_SUCCESS);

        mockTreeConnect.Setup(x => x.ReadFile(out fileBytes, handle, 0, 65536))
            .Returns(NTStatus.STATUS_SUCCESS)
            .Callback((out byte[] data, object handle, long offset, int maxCount) =>
            {
                data = [0, 1, 2];
            });

        mockTreeConnect.Setup(x => x.ReadFile(out fileBytes, handle, 3, 65536))
           .Returns(NTStatus.STATUS_SUCCESS)
           .Callback((out byte[] data, object handle, long offset, int maxCount) =>
           {
               data = [3, 4, 5];
           });

        mockTreeConnect.Setup(x => x.ReadFile(out fileBytes, handle, 6, 65536))
         .Returns(NTStatus.STATUS_END_OF_FILE);

        mockTreeConnect.Setup(x => x.Disconnect());

        var result = SmbFileShareServiceToUse.ReadFile(shareName, @"MyFolder\QA4\test.txt");

        Assert.Equal(6, result.Length);
        Assert.Equal(result, [0, 1, 2, 3, 4, 5]);

        MockISmbClient.Verify(x => x.TreeConnect(shareName, out status), Times.Once);
        mockTreeConnect.Verify(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4\test.txt", AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null), Times.Once);
        mockTreeConnect.Verify(x => x.ReadFile(out fileBytes, handle, 0, 65536), Times.Once);
        mockTreeConnect.Verify(x => x.ReadFile(out fileBytes, handle, 3, 65536), Times.Once);
        mockTreeConnect.Verify(x => x.ReadFile(out fileBytes, handle, 6, 65536), Times.Once);
        mockTreeConnect.Verify(x => x.Disconnect(), Times.Once());
    }

    private static bool IsMatchingByteArray(byte[] bytes, byte[] expectedBytes) => bytes.SequenceEqual(expectedBytes);

    [Fact]
    public void CreateFileWithNoBuffer()
    {
        const string shareName = "Share1";
        object handle = default!;
        NTStatus status;
        FileStatus fileStatus;
        int bytesWritten = 0;
        byte[] fileBytes = [0, 1, 2, 3, 4, 5];

        SetupConnectAndLogin();

        MockISmbClient.SetupGet(x => x.MaxWriteSize)
         .Returns(30000);

        var mockTreeConnect = new Mock<ISMBFileStore>();

        MockISmbClient.Setup(x => x.TreeConnect(shareName, out status))
           .Returns(mockTreeConnect.Object);

        mockTreeConnect.Setup(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4\test.txt", AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.None, CreateDisposition.FILE_CREATE, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null))
        .Returns(NTStatus.STATUS_SUCCESS);

        mockTreeConnect.Setup(x => x.WriteFile(out bytesWritten, handle, 0, It.Is<byte[]>(t => IsMatchingByteArray(t, fileBytes))))
          .Returns(NTStatus.STATUS_SUCCESS)
          .Callback((out int numberOfBytesWritten, object handle, long offset, byte[] data) =>
          {
              numberOfBytesWritten = 6;
          });

        mockTreeConnect.Setup(x => x.Disconnect());

        using var streamWithMyFile = new MemoryStream(fileBytes);

        SmbFileShareServiceToUse.CreateFile(shareName, @"MyFolder\QA4\test.txt", streamWithMyFile);

        MockISmbClient.Verify(x => x.TreeConnect(shareName, out status), Times.Once);
        mockTreeConnect.Verify(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4\test.txt", AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.None, CreateDisposition.FILE_CREATE, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null), Times.Once);
        mockTreeConnect.Verify(x => x.WriteFile(out bytesWritten, handle, 0, It.Is<byte[]>(t => IsMatchingByteArray(t, fileBytes))), Times.Once);
        mockTreeConnect.Verify(x => x.Disconnect(), Times.Once());
    }

    [Fact]
    public void CreateFileWithBuffer()
    {
        const string shareName = "Share1";
        object handle = default!;
        NTStatus status;
        FileStatus fileStatus;
        int bytesWritten = 0;
        byte[] fileBytes = [0, 1, 2, 3, 4, 5];

        SetupConnectAndLogin();

        MockISmbClient.SetupGet(x => x.MaxWriteSize)
         .Returns(3);

        var mockTreeConnect = new Mock<ISMBFileStore>();

        MockISmbClient.Setup(x => x.TreeConnect(shareName, out status))
           .Returns(mockTreeConnect.Object);

        mockTreeConnect.Setup(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4\test.txt", AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.None, CreateDisposition.FILE_CREATE, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null))
        .Returns(NTStatus.STATUS_SUCCESS);

        mockTreeConnect.Setup(x => x.WriteFile(out bytesWritten, handle, 0, It.Is<byte[]>(t => IsMatchingByteArray(t, fileBytes.Take(3).ToArray()))))
          .Returns(NTStatus.STATUS_SUCCESS)
          .Callback((out int numberOfBytesWritten, object handle, long offset, byte[] data) =>
            {
                numberOfBytesWritten = 3;
            });

        mockTreeConnect.Setup(x => x.WriteFile(out bytesWritten, handle, 3, It.Is<byte[]>(t => IsMatchingByteArray(t, fileBytes.Skip(3).Take(3).ToArray()))))
            .Returns(NTStatus.STATUS_SUCCESS)
            .Callback((out int numberOfBytesWritten, object handle, long offset, byte[] data) =>
             {
                 numberOfBytesWritten = 6;
             });

        mockTreeConnect.Setup(x => x.Disconnect());

        using var streamWithMyFile = new MemoryStream([0, 1, 2, 3, 4, 5]);

        SmbFileShareServiceToUse.CreateFile(shareName, @"MyFolder\QA4\test.txt", streamWithMyFile);

        MockISmbClient.Verify(x => x.TreeConnect(shareName, out status), Times.Once);
        mockTreeConnect.Verify(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4\test.txt", AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.None, CreateDisposition.FILE_CREATE, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null), Times.Once);
        mockTreeConnect.Verify(x => x.WriteFile(out bytesWritten, handle, 0, It.Is<byte[]>(t => IsMatchingByteArray(t, fileBytes.Take(3).ToArray()))), Times.Once);
        mockTreeConnect.Verify(x => x.WriteFile(out bytesWritten, handle, 3, It.Is<byte[]>(t => IsMatchingByteArray(t, fileBytes.Skip(3).Take(3).ToArray()))), Times.Once);
        mockTreeConnect.Verify(x => x.Disconnect(), Times.Once());
    }

    [Fact]
    public void DeleteFile()
    {
        const string shareName = "Share1";
        object handle = default!;
        NTStatus status;
        FileStatus fileStatus;

        SetupConnectAndLogin();

        var mockTreeConnect = new Mock<ISMBFileStore>();

        MockISmbClient.Setup(x => x.TreeConnect(shareName, out status))
            .Returns(mockTreeConnect.Object);

        mockTreeConnect.Setup(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4\test.txt", AccessMask.GENERIC_WRITE | AccessMask.DELETE | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.None, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null))
            .Returns(NTStatus.STATUS_SUCCESS);

        mockTreeConnect.Setup(x => x.SetFileInformation(handle, It.Is<FileDispositionInformation>(t => t.DeletePending)))
            .Returns(NTStatus.STATUS_SUCCESS);

        mockTreeConnect.Setup(x => x.Disconnect());

        SmbFileShareServiceToUse.DeleteFile(shareName, @"MyFolder\QA4\test.txt");

        MockISmbClient.Verify(x => x.TreeConnect(shareName, out status), Times.Once);
        mockTreeConnect.Verify(x => x.CreateFile(out handle, out fileStatus, @"MyFolder\QA4\test.txt", AccessMask.GENERIC_WRITE | AccessMask.DELETE | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.None, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null), Times.Once);
        mockTreeConnect.Verify(x => x.SetFileInformation(handle, It.Is<FileDispositionInformation>(t => t.DeletePending)), Times.Once());
        mockTreeConnect.Verify(x => x.Disconnect(), Times.Once());
    }

    [Fact]
    public void MoveFile()
    {
        const string shareName = "Share1";
        object handle = default!;
        NTStatus status;
        FileStatus fileStatus;

        SetupConnectAndLogin();

        var mockTreeConnect = new Mock<ISMBFileStore>();

        var currentFilePath = Path.Combine("MyFolder", "QA4", "test.txt");
        var newFilePath = Path.Combine("MyFolder", "NewFolder", "test.txt");

        MockISmbClient.Setup(x => x.TreeConnect(shareName, out status))
            .Returns(mockTreeConnect.Object);

        var shareAccess = ShareAccess.Read | ShareAccess.Write | ShareAccess.Delete;

        mockTreeConnect.Setup(x => x.CreateFile(out handle, out fileStatus, currentFilePath, AccessMask.DELETE, 0, shareAccess, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null))
            .Returns(NTStatus.STATUS_SUCCESS);

        mockTreeConnect.Setup(x => x.SetFileInformation(handle, It.Is<FileRenameInformationType2>(t => t.FileName == newFilePath)))
            .Returns(NTStatus.STATUS_SUCCESS);

        mockTreeConnect.Setup(x => x.Disconnect());

        SmbFileShareServiceToUse.MoveFile(shareName, currentFilePath, newFilePath);

        MockISmbClient.Verify(x => x.TreeConnect(shareName, out status), Times.Once);
        mockTreeConnect.Verify(x => x.CreateFile(out handle, out fileStatus, currentFilePath, AccessMask.DELETE, 0, shareAccess, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE, null), Times.Once);
        mockTreeConnect.Verify(x => x.SetFileInformation(handle, It.Is<FileRenameInformationType2>(t => t.FileName == newFilePath)), Times.Once());
        mockTreeConnect.Verify(x => x.Disconnect(), Times.Once());
    }

    [Fact]
    public void DisposeWhenNotConnected()
    {
        SmbFileShareServiceToUse.Dispose();
    }

    [Fact]
    public void DisposeWhenConnected()
    {
        MockISmbClient.Setup(x => x.IsConnected)
            .Returns(true);

        MockISmbClient.Setup(x => x.Logoff())
          .Returns(NTStatus.STATUS_SUCCESS);

        MockISmbClient.Setup(x => x.Disconnect());

        SmbFileShareServiceToUse.Dispose();

        MockISmbClient.Verify(x => x.Logoff(), Times.Once);
        MockISmbClient.Verify(x => x.Disconnect(), Times.Once);
    }

    [Fact]
    public void ThrowOnNotSuccessful()
    {
        SetupConnectAndLogin();

        NTStatus status = NTStatus.STATUS_ACCOUNT_EXPIRED;

        MockISmbClient.Setup(x => x.ListShares(out status))
            .Returns(["Share1", "Share2"]);

        var exceptionThrown = Assert.Throws<SmbStatusException>(() =>
        {
            _ = SmbFileShareServiceToUse.ListShares();
        });

        Assert.Equal("SMB Status Exception: StatusId = STATUS_ACCOUNT_EXPIRED - Expression = status", exceptionThrown.ToString());
    }

    #endregion

}