using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour {
    public static Client instance = null;
    public bool isConnected = false;

    private int id = -1;
    private string username = "";

    private string ip = "";
    private int port = -1;

    private Tcp tcp;
    private Udp udp;

    Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();

    private void Awake() {
        if (instance != null && instance != this){
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(instance);
    }

    public void connectToServer(string ipText, int port, string usernameInput) {
        this.username = usernameInput;
        this.port = port;
        this.ip = ipText;

        isConnected = true;
        connectToTcp();
    }

    public void connectToTcp() {
        TcpClient socketTcp = new TcpClient();
        tcp = new Tcp(socketTcp, ip, port); 
        tcp.connect();
    }

    public void connectByTcpFS(Packet packet) {
        int id = packet.ReadInt();
        this.id = id;

        connectToUdp();
    }

    public void connectToUdp() {
        UdpClient socketUdp = new UdpClient(((IPEndPoint) tcp.getSocket().Client.LocalEndPoint).Port);
        udp = new Udp(socketUdp, ip, port);
        udp.connect();

        Packet packet = new Packet();
        packet.Write("connectByUdpTS");
        packet.Write(id);
        sendUdpData(packet);
    }

    public void connectByUdpFS(Packet packet) {
        int id = packet.ReadInt();

        ThreadManager.ExecuteOnMainThread(() => { SceneManager.LoadScene("Mapa1");});

        requestSpawnPlayer();
    }

    public void requestSpawnPlayer() {
        Packet packet = new Packet();
        packet.Write("requestSpawnPlayerTS");
        packet.Write(id);
        packet.Write(username);
        sendTcpData(packet);
    }

    public void requestSpawnPlayerFS(Packet packet) {
        PlayerGenerator.instance.newPlayer(packet, username);
        //passar v3 do map aq
        MapGenerator.instance.newMap();
    }

    public void newEnemyFS(Packet packet) {
        PlayerGenerator.instance.newEnemy(packet);
    }

    public void playerPositionFS(Packet packet) {
        int id = packet.ReadInt();
        PlayerController player = getPlayerById(id);
        if(player == null) return;

        player.playerPosition(packet);
    }

    public void shootImpactLocationFS(Packet packet) {
        int id = packet.ReadInt();
        PlayerController player = getPlayerById(id);
        if(player == null) return;

        player.creatShootImpact(packet);
    }

    public void killPlayerFS(Packet packet) {
        int id = packet.ReadInt();
        PlayerController player = getPlayerById(id);
        if(player == null) return;

        ThreadManager.ExecuteOnMainThread(() => { player.killPlayer(packet);});
    }

    public void respawnPlayerFS(Packet packet) {
        int id = packet.ReadInt();
        PlayerController player = getPlayerById(id);
        if(player == null) return;

        ThreadManager.ExecuteOnMainThread(() => { player.respawnPlayer(packet);});
    }

    public void playerDisconnect(Packet packet) {
        int id = packet.ReadInt();
        
        PlayerController player = getPlayerById(id);
        if(player == null) return;

        player.removePlayer();
    }

    public void disconnect() {
        if (!isConnected) return;
        isConnected = false;

        PlayerController player = getPlayerById(id);
        if(player == null) return;

        player.removePlayer();
        removeFromPlayers(id);
        
        tcp.disconnect();
        udp.disconnect();
        tcp = null;
        udp = null;

        MenuController.instance.setLog("Disconnected from server!");
    }

    public void sendTcpData(Packet packet) {
        if(tcp == null) return;
        packet.WriteLength();
        tcp.sendData(packet);
    }

    public void sendUdpData(Packet packet) {
        if(udp == null) return;
        packet.WriteLength();
        udp.sendData(packet);
    }

    public void addPlayers(int id, PlayerController player) {
        players.Add(id, player);
    }

    public void removeFromPlayers(int id) {
        players.Remove(id);
    }

    public List<PlayerController> getPlayers() {
        return players.Select(player => player.Value).ToList();
    }

    public PlayerController getPlayerById(int id) {
        PlayerController value;
        if(players.TryGetValue(id, out value)) return value;
        return null;
    }
}
