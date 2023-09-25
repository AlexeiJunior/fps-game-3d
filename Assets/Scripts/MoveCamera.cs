using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour {

    public static MoveCamera instance = null;

    public Transform player;
    public Vector3 desyncOffeset;

    private void Awake() {
        if (instance != null && instance != this){
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }


    public float teste = 0;
    void Update() {
        transform.position = player.transform.position;
        // transform.position = Vector3.Lerp(player.transform.position, player.transform.position + desyncOffeset, Time.deltaTime * teste);
        // desyncOffeset = Vector3.zero;
    }
}
