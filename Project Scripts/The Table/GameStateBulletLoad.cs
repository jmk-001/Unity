using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using UnityEngine;

public class GameStateBulletLoad : NetworkBehaviour
{
    Dictionary<int, PlayerInteraction> InteractionStates;
    Dictionary<int, PlayerBulletLoad> ChildScripts;
    GameManager manager;

    void Update()
    {
        if (ChildScripts != null){
            if (ChildScripts[0].turn && ChildScripts[0].done){
                ChildScripts[1].StartLoad(false, manager.round);
                ChildScripts[0].turn = false;
            }
            if (ChildScripts[1].turn && ChildScripts[1].done){
                ChildScripts[1].turn = false;
                ChildScripts[0].done = false;
                ServerSyncNextState();
                ServerSetBulletNumbers();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ServerSetBulletNumbers(){
        ObserverSetBulletNumbers();
    }

    [ObserversRpc]
    void ObserverSetBulletNumbers(){
        GameManager.hostLiveBulletNum = ChildScripts[0].bulletLiveNum;
        GameManager.clientLiveBulletNum = ChildScripts[1].bulletLiveNum;
        GameManager.hostBlankBulletNum = ChildScripts[0].bulletBlankNum;
        GameManager.clientBlankBulletNum = ChildScripts[1].bulletBlankNum;
    }

    public void begin(Dictionary<int, PlayerBulletLoad> bulletLoads, Dictionary<int, PlayerInteraction> interactions, 
        GameManager script, int[] hostDiceResult, int[] clientDiceResult, int round)
    {
        manager = script;
        ChildScripts = bulletLoads;
        InteractionStates = interactions;
        InteractionStates[0].canInteract = true;
        InteractionStates[1].canInteract = true;
        ChildScripts[0].StartLoad(true, manager.round);
    }

    [ServerRpc(RequireOwnership = false)]
    void ServerSyncNextState(){
        ObserverSyncNextState();
    }
    [ObserversRpc]
    void ObserverSyncNextState(){
        ChildScripts[1].turn = false;
        ChildScripts[1].done = false;
        ChildScripts[0].turn = false;
        ChildScripts[0].done = false;
        manager.doneLoading = true;
    }
}
    
