using UnityEngine.UI;
using UnityEngine;

public class GameController : MonoBehaviour {
    public static GameController instance = null;

    private int port = 8800;

    //Game UI
    public GameObject damage;
    public Text health;
    public Text block;

    //Log UI
    public Text log;

    private void Awake() {
        if (instance != null && instance != this){
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    private void Start() {
        Physics.autoSimulation = false;
    }

    public void setLog(string text) {
        setLog(text, 1);
    }

    public void setLog(string text, int duration) {
        ThreadManager.ExecuteOnMainThread(() => {
            log.text = text;
            Invoke("clearLog", duration);
        });
    }

    public void setHealth(float health) {
        this.health.text = "Health " + health;
    }

    public void setBlock(float block) {
        this.block.text = "Block " + block;
    }

    public void clearLog() {
        log.text = "";
    }

    public void quitGame(){
        Application.Quit();
    }

    private void OnApplicationQuit() {
        Client.instance.disconnect();
    }

    public void enableDamage() {
        damage.SetActive(true);
        Invoke("disableDamage", 0.3f);
    }

    private void disableDamage() {
        damage.SetActive(false);
    }
}
