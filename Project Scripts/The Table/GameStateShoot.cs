using System.Collections;
using System.Collections.Generic;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using UnityEditor;
using UnityEngine;

public class GameStateShoot : NetworkBehaviour
{
    [HideInInspector] public GameObject spawnedPistol;
    private Dictionary<int, PlayerShoot> ChildScripts;
    private Dictionary<int, PlayerInteraction> InteractionStates;
    private GameManager manager;

    // Update is called once per frame
    void Update()
    {
        if (ChildScripts != null){
            if (ChildScripts[0].initialLoadDone) {
                ServerInitialLoadDone(true, this);
                // InteractionStates[1].canInteract = true;
                // ChildScripts[0].initialLoadDone = false;
            }
            if (ChildScripts[1].initialLoadDone) {
                ServerInitialLoadDone(false, this);
                // InteractionStates[0].canInteract = true;
                // ChildScripts[1].initialLoadDone = false;
            }
            if (InteractionStates[0].firedAtEnemy){
                InteractionStates[0].canInteract = false;
                InteractionStates[1].canInteract = true;
                InteractionStates[0].firedAtEnemy = false;
            }
            if (InteractionStates[1].firedAtEnemy){
                InteractionStates[1].canInteract = false;
                InteractionStates[0].canInteract = true;
                InteractionStates[1].firedAtEnemy = false;
            }
            if (ChildScripts[0].isActive && (InteractionStates[0].liveBulletFired || InteractionStates[1].liveBulletFired)){
                ServerNextTurn(this);
                ChildScripts[1].startTurn(GameManager.clientLiveBulletNum, GameManager.clientBlankBulletNum);
                ChildScripts[0].isActive = false;
            }
            if (ChildScripts[1].isActive && (InteractionStates[0].liveBulletFired || InteractionStates[1].liveBulletFired)){
                ChildScripts[1].isActive = false;
                InteractionStates[0].liveBulletFired = false;
                InteractionStates[1].liveBulletFired = false;
                manager.doneShooting = true;
                manager.round += 1;
                ServerSyncExitVars(this);
            }
        }
    }

    public void begin(Dictionary<int, PlayerShoot> shoots, Dictionary<int, PlayerInteraction> interactions, GameManager mgr)
    {
        ChildScripts = shoots;
        InteractionStates = interactions;
        manager = mgr;

        InteractionStates[1].canInteract = false;
        ChildScripts[0].startTurn(GameManager.hostLiveBulletNum, GameManager.hostBlankBulletNum);
    }

    // Sync Next Turn Transition Vars
    [ServerRpc(RequireOwnership = false)]
    void ServerNextTurn(GameStateShoot script){
        ObserverNextTurn(script);
    }
    [ObserversRpc]
    void ObserverNextTurn(GameStateShoot script){
        script.ChildScripts[0].isActive = false;
        script.InteractionStates[1].canInteract = true;
        script.InteractionStates[0].liveBulletFired = false;
        script.InteractionStates[1].liveBulletFired = false;
    }

    // Sync Next Round Transition Vars
    [ServerRpc(RequireOwnership = false)]
    void ServerNextRound(GameStateShoot script){
        ObserverNextRound(script);
    }
    [ObserversRpc]
    void ObserverNextRound(GameStateShoot script){
        ChildScripts[1].isActive = false;
        InteractionStates[0].liveBulletFired = false;
        InteractionStates[1].liveBulletFired = false;
        manager.doneShooting = true;
        manager.round += 1;
    }

    // Sync Initial Load Vars
    [ServerRpc(RequireOwnership = false)]
    void ServerInitialLoadDone(bool isHost, GameStateShoot script){
        ObserverInitialLoadDone(isHost, script);
    }
    [ObserversRpc]
    void ObserverInitialLoadDone(bool isHost, GameStateShoot script){
        if (isHost)
        {script.InteractionStates[1].canInteract = true;
        script.ChildScripts[0].initialLoadDone = false;}
        else
        {InteractionStates[0].canInteract = true;
        ChildScripts[1].initialLoadDone = false;}
    }

    // Sync Initial Load Vars
    [ServerRpc(RequireOwnership = false)]
    void ServerSyncExitVars(GameStateShoot script){
        ObserverSyncExitVars(script);
    }
    [ObserversRpc]
    void ObserverSyncExitVars(GameStateShoot script){
        script.ChildScripts[0].isActive = false;
        script.ChildScripts[1].isActive = false;
        script.InteractionStates[0].liveBulletFired = false;
        script.InteractionStates[1].liveBulletFired = false;
        script.manager.doneShooting = true;
    }
}
