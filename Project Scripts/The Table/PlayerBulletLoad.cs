using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using FishNet;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBulletLoad : NetworkBehaviour
{
    public int[] myDiceResult;
    public bool turn = false;
    public bool selectorSpawned = false;
    public bool done = false;
    public GameObject bulletSelector;
    public int bulletLiveNum = 0;
    public int bulletBlankNum = 0;
    public GameObject spawnedSelector;
    public GameObject spawnedSelectorModel;
    public int currentRound;

    public override void OnStartClient()
    {
        base.OnStartClient();
        GameManager.PlayerBulletLoads.Add(OwnerId, this);
        if (!base.IsOwner)
        {
            enabled = false;
        }
    }

    void Update()
    {
        if (turn && !selectorSpawned){
            ServerSpawnSelector(bulletSelector, GameObject.Find("componentHolder").transform, this);
            selectorSpawned = true;
        }
        if (turn && selectorSpawned){
            if (Input.GetKeyDown(KeyCode.Return)){
                ServerSendBulletType(spawnedSelectorModel.GetComponent<BulletSelectorManager>().reportSelection().Item1,
                                        spawnedSelectorModel.GetComponent<BulletSelectorManager>().reportSelection().Item2, this);
                ServerDespawnObj(this);
                selectorSpawned = false;
                done = true;
            }
        }
    }

    // This will be called first to save inherited variables before starting the state
    public void StartLoad(bool isHost, int round){
        ServerStartBulletLoad(isHost, round, this);
    }

    [ServerRpc(RequireOwnership = false)]
    void ServerSpawnSelector(GameObject obj, Transform spawnLoc, PlayerBulletLoad script){
        GameObject spawned = Instantiate(obj, spawnLoc.position, spawnLoc.rotation);
        spawned.transform.Rotate(-90f, 180f, 0f);
        ServerManager.Spawn(spawned);
        SetSpanwedSelector(spawned, spawnLoc, script);
    }
    [ObserversRpc]
    void SetSpanwedSelector(GameObject spawned, Transform spawnLoc, PlayerBulletLoad script){
        script.spawnedSelector = spawned;
        script.spawnedSelectorModel = spawned.transform.GetChild(0).gameObject;
        script.spawnedSelectorModel.GetComponent<BulletSelectorManager>().initialise(myDiceResult[currentRound]);
        script.spawnedSelector.transform.SetParent(spawnLoc);
    }

    // Server Start State
    [ServerRpc(RequireOwnership = false)]
    public void ServerStartBulletLoad(bool isHost, int round, PlayerBulletLoad script){
        ObserverStartBulletLoad(isHost, round, script);
    }
    [ObserversRpc]
    public void ObserverStartBulletLoad(bool isHost, int round, PlayerBulletLoad script){
        if (isHost) script.myDiceResult = GameManager.hostDiceResult;
        else script.myDiceResult = GameManager.clientDiceResult;
        script.currentRound = round;
        script.turn = true;
    }

    // Server Send user-chosen bullet types
    [ServerRpc(RequireOwnership = false)]
    public void ServerSendBulletType(int live, int blank, PlayerBulletLoad script){
        ObserverSendBulletType(live, blank, script);
    }
    [ObserversRpc]
    public void ObserverSendBulletType(int live, int blank, PlayerBulletLoad script){
        script.bulletLiveNum = live;
        script.bulletBlankNum = blank;
    }

    [ServerRpc(RequireOwnership = false)]
    void ServerDespawnObj(PlayerBulletLoad script){
        ServerManager.Despawn(spawnedSelector);
        SetDespanwedObject(script);
    }

    [ObserversRpc]
    void SetDespanwedObject(PlayerBulletLoad script){
        script.spawnedSelector = null;
    }
}
