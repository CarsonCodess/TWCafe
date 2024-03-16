using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using UnityEngine;

public struct RelayHostData
{
    public string JoinCode;
    public string IpV4Address;
    public ushort Port;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] Key;
}

public struct RelayClientData
{
    public string IpV4Address;
    public ushort Port;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] HostConnectionData;
    public byte[] Key;
}

public class RelayAPIInterface
{
    public RelayHostData HostData;
    public RelayClientData ClientData;

    public async Task<bool> StartRelayServer(int maxConnections)
    {
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
        return true;
    }

    public async Task<bool> JoinRelayServer(string joinCode)
    {
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
        return true;
    }
}