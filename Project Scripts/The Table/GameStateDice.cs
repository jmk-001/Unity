using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using FishNet.Object;
using UnityEngine;

public class GameStateDice : NetworkBehaviour
{
    Dictionary<int, PlayerDiceRoll> ChildScripts;
    GameManager manager;
    void Update()
    {
        if (ChildScripts != null){
            if (ChildScripts[0].turn && ChildScripts[0].done){
                Debug.Log("host's rolling is done");
                ChildScripts[0].turn = false;
                ChildScripts[1].turn = true;
                ServerSetHostDiceResult();
            }
            if (ChildScripts[1].turn && ChildScripts[1].done){
                ChildScripts[0].turn = false;
                ChildScripts[1].turn = false;
                manager.doneRolling = true;
                ServerSetClientDiceResult();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ServerSetHostDiceResult(){
        ObserverSetHostDiceResult();
    }

    [ObserversRpc]
    void ObserverSetHostDiceResult(){
        GameManager.hostDiceResult = ChildScripts[0].diceResult;
    }

    [ServerRpc(RequireOwnership = false)]
    void ServerSetClientDiceResult(){
        ObserverSetClientDiceResult();
    }

    [ObserversRpc]
    void ObserverSetClientDiceResult(){
        GameManager.clientDiceResult = ChildScripts[1].diceResult;
        manager.doneRolling = true;
        enabled = false;
    }

    public void begin(Dictionary<int, PlayerDiceRoll> diceRolls, Dictionary<int, PlayerInteraction> interactions, GameManager script)
    {
        //ServerSpawnObj(diceGroup, transform, this);
        // interactions[1].TogglePlayerInteractionsServer(false);
        manager = script;
        ChildScripts = diceRolls;
        ChildScripts[0].turn = true;
    }
}
