using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class LocalUIManager : NetworkBehaviour
{
    public GameObject dot;
    public GameObject crosshair;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner){
            dot.SetActive(false);
            crosshair.SetActive(false);
            enabled = false;
        }
    }
 
    void Update()
    {
        if (GetComponent<PlayerInteraction>().pistolEquipped){
            dot.SetActive(false);
            crosshair.SetActive(true);
        } else { 
            dot.SetActive(true);
            crosshair.SetActive(false);
        }
    }
}
