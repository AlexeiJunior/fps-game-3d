using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
    
    public ParticleSystem shootPS;

    public void shoot() {
        shootPS.Play();
    }

    public void setVisible(bool visible) {
        gameObject.SetActive(visible);
    }
}
