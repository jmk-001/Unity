using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IO.Swagger.Model;
using UnityEngine;

public class ChamberLoaderManager : NetworkBehaviour
{
    public int[] bullets = new int[6];
    public Transform[] spawnLocs = new Transform[6];
    public GameObject[] spawnedBullets = new GameObject[6];
    public GameObject bulletLive;
    public GameObject bulletBlank;
    public GameObject chamber;
    public int pointer;
    public int maxTotal;
    public int maxLive;
    public int maxBlank;
    public int liveRemaining;
    public int blankRemaining;

    [ServerRpc(RequireOwnership = false)]
    public void initialise(int live, int blank){
        initialiseObserver(live, blank, this);
    }
    [ObserversRpc]
    public void initialiseObserver(int live, int blank, ChamberLoaderManager script){
        script.maxTotal = live + blank;
        script.maxLive = live;
        script.maxBlank = blank;
        script.liveRemaining = script.maxLive;
        script.blankRemaining = script.maxBlank;
    }

    [ServerRpc(RequireOwnership = false)]
    public void insertBullet(int bulletType){
        insertBulletObserver(bulletType, this);
    }
    [ObserversRpc]
    public void insertBulletObserver(int bulletType, ChamberLoaderManager script){
        int bulletRemaining = bulletType == 2 ? script.liveRemaining : script.blankRemaining;
        if (script.bullets[script.pointer] == bulletType || 
            script.bullets[script.pointer] != 0 ||
            bulletRemaining <= 0){
            return;
        }
        GameObject spawned = bulletType == 1 ? Instantiate(bulletBlank, script.spawnLocs[script.pointer].transform.position, script.spawnLocs[script.pointer].rotation):
                                                Instantiate(bulletLive, script.spawnLocs[script.pointer].transform.position, script.spawnLocs[script.pointer].rotation);
        spawned.transform.SetParent(script.spawnLocs[script.pointer]);
        ServerManager.Spawn(spawned);
        script.spawnedBullets[script.pointer] = spawned;
        script.bullets[script.pointer] = bulletType;
        if (bulletType == 1) script.blankRemaining -= 1;
        else script.liveRemaining -= 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void removeBullet(){
        removeBulletObserver(this);
    }
    [ObserversRpc]
    public void removeBulletObserver( ChamberLoaderManager script){
        if (script.bullets[script.pointer] == 0) return;
        
        Destroy(script.spawnedBullets[script.pointer]);
        ServerManager.Despawn(script.spawnedBullets[script.pointer]);

        if (script.bullets[script.pointer] == 1){
            script.blankRemaining += 1;
        } else script.liveRemaining += 1;
        script.bullets[script.pointer] = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void rotateRight(){
        rotateRightObserver(this);
    }
    [ObserversRpc]
    public void rotateRightObserver(ChamberLoaderManager script){
        script.pointer = (script.pointer + 1) % 6;
        script.chamber.transform.Rotate(0, 0, -60);
    }

    [ServerRpc(RequireOwnership = false)]
    public void rotateLeft(){
        rotateLeftObserver(this);
    }
    [ObserversRpc]
    public void rotateLeftObserver(ChamberLoaderManager script){
        script.pointer = script.pointer - 1 < 0 ? 5 : script.pointer - 1;
        script.chamber.transform.Rotate(0, 0, 60);
    }

    public int[] reportSelection(){
        return bullets;
    }
}
