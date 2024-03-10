using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using UnityEngine;
public enum RelayConnectionStatus
    {
        Success,
        Failed,
        Connecting,
        StartingServer
    }

    /// <summary>
    /// RelayHostData represents the necessary information for creating a relay allocation
    /// </summary>
    public struct RelayHostData
    {
        public string JoinCode;
        public string IpV4Address;
        public ushort Port;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] Key;
    }

    /// <summary>
    /// RelayClientData represents the necessary information for joining a relay server
    /// </summary>
    public struct RelayClientData
    {
        public string IpV4Address;
        public ushort Port;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }

/// <summary>
/// Wrapper for all the interactions with the Relay API.
/// </summary>
public class RelayAPIInterface
{
    public RelayHostData HostData;
    public RelayClientData ClientData;
    public RelayConnectionStatus RelayConnectionStatus { get; private set; }

    public async Task<bool> StartRelayServer(int maxConnections, bool debug = false)
    {
        if (debug)
            Debug.Log("Starting Relay Server...");
        RelayConnectionStatus = RelayConnectionStatus.StartingServer;

        var allocation = await Relay.Instance.CreateAllocationAsync(maxConnections, "us-central1");
        HostData = new RelayHostData()
        {
            Key = allocation.Key,
            Port = (ushort) allocation.RelayServer.Port,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            IpV4Address = allocation.RelayServer.IpV4
        };

        HostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

        var utp = (UnityTransport) NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        utp.SetHostRelayData(HostData.IpV4Address, HostData.Port, HostData.AllocationIDBytes, HostData.Key,
            HostData.ConnectionData);

        RelayConnectionStatus = RelayConnectionStatus.Success;
        if (debug)
            Debug.Log($"Started Relay Server: {HostData.JoinCode}");
        return true;
    }

    public async Task JoinRelayServer(string joinCode)
    {
        RelayConnectionStatus = RelayConnectionStatus.Connecting;
        var allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        ClientData = new RelayClientData()
        {
            Key = allocation.Key,
            Port = (ushort) allocation.RelayServer.Port,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData,
            IpV4Address = allocation.RelayServer.IpV4
        };

        var utp = (UnityTransport) NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        utp.SetClientRelayData(ClientData.IpV4Address, ClientData.Port, ClientData.AllocationIDBytes,
            ClientData.Key, ClientData.ConnectionData, ClientData.HostConnectionData);
        RelayConnectionStatus = RelayConnectionStatus.Success;
    }
}