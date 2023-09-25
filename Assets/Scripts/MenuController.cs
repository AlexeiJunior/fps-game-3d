using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;

public class MenuController : MonoBehaviour {
    public static MenuController instance = null;

    private int port = 8800;

    //Intro menu UI
    public GameObject startMenuUI;
    public InputField usernameInput;
    public InputField ipInput;
    public GameObject connectCamera;
    public Button startButton;
    public GameObject connectingText;

    //Log UI
    public Text log;

    private void Awake() {
        if (instance != null && instance != this){
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void connectToServer() {
        ipInput.text = "127.0.0.1";
        // if (Application.isEditor) ipInput.text = "127.0.0.1";
        setInteractableStart(false);

        if (!Regex.Match(ipInput.text, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}").Success) {
            setLog("Ip doesn't match the format!");
            setInteractableStart(true);
            ipInput.text = "";
            return;
        }

        Client.instance.connectToServer(ipInput.text, port, usernameInput.text);
    }

    public void hideStartMenuUI() {
        startMenuUI.SetActive(false);
        ipInput.interactable = false;
        usernameInput.interactable = false;
        Destroy(connectCamera);
    }

    public void setInteractableStart(bool interactable){
        if (interactable){
            ThreadManager.ExecuteOnMainThread(() => {
                Invoke("enableInteractable", 1);
            });
        } else {
            startButton.interactable = false;
            connectingText.SetActive(true);
        } 
    }

    public void enableInteractable() {
        startButton.interactable = true;
        connectingText.SetActive(false);
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

    public void clearLog() {
        log.text = "";
    }

    public void quitGame(){
        Application.Quit();
    }

    private void OnApplicationQuit() {
        Client.instance.disconnect();
    }
}
