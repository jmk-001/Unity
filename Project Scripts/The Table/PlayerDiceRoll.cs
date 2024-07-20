using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDiceRoll : NetworkBehaviour
{
    public GameObject diceGroup;
    public Transform spawnLoc;
    [HideInInspector] public GameObject spawnedObject;
    public bool done = false;
    public int[] diceResult = new int[5];
    public bool diceSpawned = false;
    public bool turn = false;

    public override void OnStartClient()
    {
        base.OnStartClient();
        GameManager.PlayerDiceRolls.Add(OwnerId, this);
        if (!base.IsOwner)
        {
            enabled = false;
        }
    }

    void Update()
    {
        if (turn && Input.GetKeyDown(KeyCode.R) && !diceSpawned){
            ServerSpawnObj(diceGroup, transform.Find("componentHolder"), this);
            diceSpawned = true;
            
            // enable UI message (press something to roll, etc.)
            Debug.Log(OwnerId + ": Press R to roll the dice.");

            startRoll();
        }
    }

    public void startRoll(){
        // Show UI, press button, retrieve dice numbers and return.
        ServerStartRoll(this, spawnedObject);
    }

    // Start Rolling
    [ServerRpc(RequireOwnership = false)]
    public void ServerStartRoll(PlayerDiceRoll script, GameObject diceGroup){
        ObserverStartRoll(script, diceGroup);
    }

    [ObserversRpc]
    public void ObserverStartRoll(PlayerDiceRoll script, GameObject diceGroup){
        StartCoroutine(RollCoroutine(script, diceGroup));
    }

    // Dice-rolling Coroutine
    public IEnumerator RollCoroutine(PlayerDiceRoll script, GameObject diceGroup){
        yield return new WaitForSeconds(3f);
        
        script.diceResult = script.spawnedObject.GetComponent<DiceGroupManager>().reportDiceResult();
        ServerDespawnObj(this);
        done = true;
    }

    // Dice Spawn
    [ServerRpc(RequireOwnership = false)]
    void ServerSpawnObj(GameObject obj, Transform spawnLoc, PlayerDiceRoll script){
        GameObject spawned = Instantiate(obj, spawnLoc.position, Quaternion.identity);

        foreach(Transform dice in spawned.transform){
            dice.rotation = Random.rotation;
        }

        ServerManager.Spawn(spawned);

        SetSpanwedObject(spawned, script);
    }

    [ObserversRpc]
    void SetSpanwedObject(GameObject spanwed, PlayerDiceRoll script){
        script.spawnedObject = spanwed;
    }

    // Dice Despawn
    [ServerRpc(RequireOwnership = false)]
    void ServerDespawnObj(PlayerDiceRoll script){
        ServerManager.Despawn(spawnedObject);
        SetDespanwedObject(script);
    }

    [ObserversRpc]
    void SetDespanwedObject(PlayerDiceRoll script){
        script.spawnedObject = null;
    }


    [ServerRpc(RequireOwnership = false)]
    void ServerSetSpawnPoint(Transform point, PlayerDiceRoll script){
        ObserverSetSpawnPoint(point, script);
    }

    [ObserversRpc]
    void ObserverSetSpawnPoint(Transform point, PlayerDiceRoll script){
        script.spawnLoc = point;
    }
}
