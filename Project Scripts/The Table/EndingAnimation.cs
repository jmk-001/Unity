using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class EndingAnimation : MonoBehaviour
{
    public GameObject toplight;

    public void startAnim(){
        RenderSettings.ambientLight = new Color(0, 0, 0);
        StartCoroutine(EndingAnim());
    }

    IEnumerator EndingAnim(){
        toplight.GetComponent<Animator>().enabled = false;
        yield return new WaitForSeconds(3f);
        toplight.GetComponent<Animator>().enabled = true;
        transform.SetParent(null);
        GetComponent<Animator>().enabled = true;
        GetComponent<Animator>().Play("camera_ending");
        toplight.GetComponent<Animator>().Play("lightblink");
        toplight.GetComponent<AudioSource>().Play(0);
        
        // transition to main screen
    }
}
