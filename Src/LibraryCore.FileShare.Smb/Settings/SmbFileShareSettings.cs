using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;

namespace LibraryCore.FileShare.Smb.Settings;

/// <summary>
/// Contains the app settings needed to run the smb file share service
/// </summary>
public class SmbFileShareSettings
{
    [Required]
    public string AuthenticationUserName { get; set; } = null!;

    [Required]
    public string AuthenticationPassword { get; set; } = null!;

    /// <summary>
    /// The domain authentication to use. 
    /// </summary>
    public string? DomainAuthentication { get; set; }

    /// <summary>
    /// Use full dns such as server.mycompany.org
    /// </summary>
    [Required]
    public string ServerName { get; set; } = null!;

    /// <summary>
    /// The ip address resolution for the server name. You can override this if you want to use a different ip address
    /// </summary>
    /// <returns>Ip address which will be used to connect to the shared</returns>
    public virtual IPAddress ResolveIpAddress()
    {
        var temp = Dns.GetHostAddresses(ServerName);

        //try to get the ip4 address first
        return temp.FirstOrDefault(t => t.AddressFamily == AddressFamily.InterNetwork) ?? temp.First();
    }
}
