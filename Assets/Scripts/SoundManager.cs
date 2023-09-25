using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static AudioClip jump, land, dash, shoot, hit;
    static AudioSource audioSrc;

    void Start()
    {
        jump = Resources.Load<AudioClip>("jump");
        land = Resources.Load<AudioClip>("land");
        dash = Resources.Load<AudioClip>("dash");
        shoot = Resources.Load<AudioClip>("shoot");
        hit = Resources.Load<AudioClip>("hit");
        
        if(!audioSrc) audioSrc = GetComponent<AudioSource>();
        audioSrc.volume = 0.45f;
    }
    
    public static void PlaySound(string clip)
    {
        switch(clip){
            case "jump":
                audioSrc.PlayOneShot(jump);
                break;
            case "land":
                audioSrc.PlayOneShot(land);
                break;
            case "dash":
                audioSrc.PlayOneShot(dash);
                break;
            case "shoot":
                audioSrc.PlayOneShot(shoot);
                break;
            case "hit":
                audioSrc.PlayOneShot(hit);
                break;
        }        
    }
}
