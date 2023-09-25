using UnityEngine;

public class PlayerGenerator : MonoBehaviour {
    public static PlayerGenerator instance = null;

    public GameObject playerPrefab;

    private void Awake() {
        if (instance != null && instance != this){
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void newPlayer(Packet packet, string username) {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();
        Quaternion camRotation = packet.ReadQuaternion();

        ThreadManager.ExecuteOnMainThread(() => {
            PlayerGenerator.instance.instantiatePlayer(id, username, position, rotation, camRotation);
        });

        Packet packetSend = new Packet();
        packetSend.Write("newEnemyTS");
        packetSend.Write(id);
        packetSend.Write(username);
        Client.instance.sendTcpData(packetSend);
    }

    public void instantiatePlayer(int id, string myUsername, Vector3 position, Quaternion rotation, Quaternion camRotation) {
        GameObject playerGO = Instantiate(playerPrefab, position, rotation);
        PlayerController playerController = playerGO.GetComponentInChildren<PlayerController>();
        playerController.setId(id);
        playerController.setUsername(myUsername);
        Client.instance.addPlayers(id, playerController);
    }

    public void newEnemy(Packet packet) {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();
        Quaternion camRotation = packet.ReadQuaternion();

        ThreadManager.ExecuteOnMainThread(() => {
            PlayerGenerator.instance.instantiatePlayerEnemy(id, username, position, rotation, camRotation);
        });
    }

    public void instantiatePlayerEnemy(int id, string username, Vector3 position, Quaternion rotation, Quaternion camRotation) {
        GameObject playerGO = Instantiate(playerPrefab, position, rotation);
        PlayerController playerController = playerGO.GetComponentInChildren<PlayerController>();
        playerController.setId(id);
        playerController.setUsername(username);
        playerController.makeItEnemy();
        Client.instance.addPlayers(id, playerController);
        MenuController.instance.setLog("Player [id: "+id+" username: "+username+"] has connect!");
    }
}
