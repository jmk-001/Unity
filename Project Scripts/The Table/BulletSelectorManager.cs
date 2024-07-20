using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using JetBrains.Annotations;
using UnityEngine;

public class BulletSelectorManager : NetworkBehaviour
{
    public int[] bullets = new int[6];
    public Transform[] spawnLocs = new Transform[6];
    public GameObject[] spawnedBullets = new GameObject[6];
    public GameObject bulletLive;
    public GameObject bulletBlank;
    public int pointer;
    public int maxNum;
    public int bulletLiveNum;
    public int bulletBlankNum;

    [ServerRpc(RequireOwnership = false)]
    public void initialise(int num){
        initialiseObserver(num, this);
    }
    [ObserversRpc]
    public void initialiseObserver(int num, BulletSelectorManager script){
        script.maxNum = num-1;
        script.bulletLiveNum = 1;
        script.bulletBlankNum = num-1;
        for (int i = 0; i <= script.maxNum; i++){
            GameObject spawned;
            if (i == 0) {
                spawned = Instantiate(bulletLive, script.spawnLocs[i].transform.position, script.spawnLocs[script.pointer].rotation);
                spawned.transform.SetParent(script.spawnLocs[i]);
                ServerManager.Spawn(spawned);
                script.spawnedBullets[i] = spawned;
            } else {
                spawned = Instantiate(bulletBlank, script.spawnLocs[i].transform.position, script.spawnLocs[script.pointer].rotation);
                spawned.transform.SetParent(script.spawnLocs[i]);
                ServerManager.Spawn(spawned);
                script.spawnedBullets[i] = spawned;
            }
            script.pointer = 0;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void increment(){
        incrementObserver(this);
    }

    [ObserversRpc]
    void incrementObserver(BulletSelectorManager script){
        if (script.pointer + 1 > script.maxNum){
            return;
        }
        else {
            script.pointer += 1;
            Destroy(spawnedBullets[script.pointer]);
            ServerManager.Despawn(script.spawnedBullets[script.pointer]);

            GameObject spawned = Instantiate(bulletLive, script.spawnLocs[script.pointer].position, script.spawnLocs[script.pointer].rotation);
            spawned.transform.SetParent(script.spawnLocs[script.pointer]);
            ServerManager.Spawn(spawned);

            script.spawnedBullets[script.pointer] = spawned;
            script.bulletLiveNum += 1;
            script.bulletBlankNum -= 1;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void decrement(){
        decrementObserver(this);
    }

    [ObserversRpc]
    void decrementObserver(BulletSelectorManager script){
        if (script.pointer - 1 < 0){
            return;
        }
        else {
            Destroy(script.spawnedBullets[script.pointer]);
            ServerManager.Despawn(script.spawnedBullets[script.pointer]);

            GameObject spawned = Instantiate(bulletBlank, script.spawnLocs[script.pointer].position, script.spawnLocs[script.pointer].rotation);
            spawned.transform.SetParent(script.spawnLocs[script.pointer]);
            ServerManager.Spawn(spawned);

            script.spawnedBullets[script.pointer] = spawned;
            script.pointer -= 1;
            script.bulletBlankNum += 1;
            script.bulletLiveNum -= 1;
        }
    }

    public (int, int) reportSelection(){
        return (bulletLiveNum, bulletBlankNum);
    }
}
