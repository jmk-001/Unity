using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Component.Animating;
using Unity.VisualScripting;
using System;
using FishNet.Component.Transforming;
using System.Runtime.InteropServices;

public class PistolManager : NetworkBehaviour
{
    public int damage = 1;
    public int[] chamber = new int[] { 0, 0, 0, 0, 0, 0 };
    public GameObject[] chamberLoc = new GameObject[6];
    public GameObject bullet_live;
    public GameObject bullet_blank;
    public GameObject bullet_shell;
    public int chamberPointer = 0;
    public int ownerId;
    public float fireRate = 0.5f;
    private NetworkAnimator _networkAnimator;
    public GameObject pistolPos;
    public GameObject chamberBody;
    [SerializeField] private ParticleSystem muzzleFlash;

    public AudioSource audio_firelive;
    public AudioSource audio_fireblank;
    public AudioSource audio_hammerclick;
    public AudioSource audio_bulletinsert;
    public AudioSource audio_chamberopen;
    public AudioSource audio_chamberclose;
    public AudioSource audio_chamberspin;

    void Awake()
    {
        if (TryGetComponent(out NetworkAnimator netAnim))
            _networkAnimator = netAnim;
    }

    public void setPos(GameObject holder){
        pistolPos = holder;
    }

    public int handleFire(){
        int currentBullet = chamber[chamberPointer];
        ClearChamberServer(transform.gameObject, this);
        return currentBullet;
    }

    public void aimMyself(bool reverse=false){
        ServerAimMyself(reverse);
    }

    public void animateFire(int bulletType, bool dequip, bool aimingMyself, PlayerInteraction script){
        ServerAnimatePistol(bulletType, chamberPointer, dequip, aimingMyself, script);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClearChamberServer(GameObject pistol, PistolManager script){
        ClearChamberObserver(pistol, script);
    }
    [ObserversRpc]
    private void ClearChamberObserver(GameObject pistol, PistolManager script){
        GameObject currentChamber = chamberLoc[chamberPointer];
        if (script.chamber[chamberPointer] > 0){
            ServerManager.Despawn(currentChamber.transform.GetChild(0).gameObject);
            GameObject shell = Instantiate(bullet_shell, currentChamber.transform.position, currentChamber.transform.rotation);
            shell.transform.SetParent(chamberLoc[chamberPointer].transform);
            ServerManager.Spawn(shell);
        }
        
        script.chamber[chamberPointer] = 0;
        script.chamberPointer += 1;
        if (script.chamberPointer >= 6){
            script.chamberPointer = 0;
        }
    }

    // Shuffle Chamber
    public void shuffle(){
        ServerShuffle(this);
    }
    [ServerRpc(RequireOwnership = false)]
    void ServerShuffle(PistolManager script){
        int chamberNum = UnityEngine.Random.Range(0, 6);
        ObserverShuffle(chamberNum, script);
    }
    [ObserversRpc]
    void ObserverShuffle(int newNum, PistolManager script){
        script.chamberPointer = newNum;
        script.chamberBody.transform.Rotate(0, 0, chamberPointer * 60);
    }


    public void checkChamber(PlayerInteraction caller){
        if (ownerId != caller.OwnerId) ServerCheckChamber(caller, this);
    }
    [ServerRpc(RequireOwnership = false)]
    void ServerCheckChamber(PlayerInteraction caller, PistolManager script){
        string openAnimName = "pistol_open" + chamberPointer;
        string closeAnimName = "pistol_close" + chamberPointer;
        ObserverCheckChamber(openAnimName, closeAnimName, caller, script);
    }
    [ObserversRpc]
    void ObserverCheckChamber(string openAnimName, string closeAnimName, PlayerInteraction caller, PistolManager script){
        StartCoroutine(CheckChamberCoroutine(openAnimName, closeAnimName, caller, script));
    }
    IEnumerator CheckChamberCoroutine(string openAnimName, string closeAnimName, PlayerInteraction caller, PistolManager script){
        audio_chamberopen.Play(0);
        GetComponent<Animator>().Play(openAnimName);
        audio_chamberclose.PlayDelayed(1.5f);
        yield return new WaitForSeconds(2f);
        GetComponent<Animator>().Play(closeAnimName);
        audio_chamberspin.PlayDelayed(0.45f);
        shuffle();
        caller.isChecking = false;
    }

    // Loading bullets into the chamber
    public void load(int[] bullets){
        if (!IsOwner) return;
        LoadServer(gameObject, this, bullets);
    }
    [ServerRpc(RequireOwnership = false)]
    private void LoadServer(GameObject pistol, PistolManager script,  int[] bullets){
        for (int i = 0; i < chamber.Length; i++){
            if (bullets[i] == 1){
                    GameObject bullet = Instantiate(bullet_blank, script.chamberLoc[i].transform.position, script.chamberLoc[i].transform.rotation);
                    bullet.transform.SetParent(script.chamberLoc[i].transform);
                    ServerManager.Spawn(bullet);
            }
            else if (bullets[i] == 2){
                GameObject bullet = Instantiate(bullet_live, script.chamberLoc[i].transform.position, script.chamberLoc[i].transform.rotation);
                bullet.transform.SetParent(script.chamberLoc[i].transform);
                ServerManager.Spawn(bullet);
            }
        }
        LoadObserver(pistol, script, bullets);
    }
    [ObserversRpc]
    private void LoadObserver(GameObject pistol, PistolManager script, int[] bullets){
        for (int i = 0; i < chamber.Length; i++){
            script.chamber[i] = bullets[i]; // Update chamber intergers
        }
    }

    // Pistol Animation upon pulling the trigger
    [ServerRpc(RequireOwnership = false)]
    private void ServerAnimatePistol(int bulletType, int nextChamber, bool dequip, bool aimingMyself, PlayerInteraction script){
        ObserverAnimatePistol(bulletType, nextChamber, dequip, aimingMyself, script);
    }
    [ObserversRpc]
    private void ObserverAnimatePistol(int bulletType, int nextChamber, bool dequip, bool aimingMyself, PlayerInteraction script){
        bool playHandRecoil = false;
        bool liveBulletFired = false;
        if (bulletType == 2 ){
            playHandRecoil = true;
            liveBulletFired = true;
            audio_firelive.Play(0);
        }
        else if (bulletType == 1) audio_fireblank.Play(0);
        else audio_hammerclick.Play(0);

        string fireAnimName = "";
        string chamberSpinAnimName = "chamber_spin_" + nextChamber.ToString() + ((nextChamber+1)%6).ToString();
        if (bulletType == 0) fireAnimName = "pistol_tick";
        if (bulletType == 1) fireAnimName = "pistol_muzzle_only";
        if (bulletType == 2) fireAnimName = "pistol_recoil";
        StartCoroutine(AnimationCoroutine(fireAnimName, chamberSpinAnimName, playHandRecoil, dequip, aimingMyself, liveBulletFired, script));
    }
    IEnumerator AnimationCoroutine(string fireAnimName, string chamberSpinAnimName, bool playHandRecoil, bool dequip, bool aimingMyself, bool liveBulletFired, PlayerInteraction script){
        if (playHandRecoil && !aimingMyself) {
            pistolPos.GetComponent<Animator>().enabled = true;
            pistolPos.GetComponent<Animator>().Play("holder_recoil");
        } else if (playHandRecoil && aimingMyself) {
            pistolPos.GetComponent<Animator>().enabled = true;
            pistolPos.GetComponent<Animator>().Play("holder_recoil_myself");
        }
        if (!playHandRecoil && aimingMyself) pistolPos.GetComponent<Animator>().Play("holder_norecoil_aimmyself");
        GetComponent<Animator>().Play(fireAnimName);

        yield return new WaitForSeconds(1f);

        GetComponent<Animator>().Play(chamberSpinAnimName);
        yield return new WaitForSeconds(1f);
        if (aimingMyself){
            script.AimMySelf(reverse:true);
            yield return new WaitForSeconds(0.7f);
        }
        if (liveBulletFired) {
            script.LiveFired();
        }
        if (!liveBulletFired && dequip) {
            script.unequipPistol();
            script.firedAtEnemy = true;
        }
        script.canInteract = true;
        if (playHandRecoil) pistolPos.GetComponent<Animator>().enabled = false;
    }

    // Aim Myself
    [ServerRpc(RequireOwnership = false)]
    void ServerAimMyself(bool reverse){
        ObserverAimMyself(reverse);
    }
    [ObserversRpc]
    void ObserverAimMyself(bool reverse){
        if (!reverse){
            pistolPos.GetComponent<Animator>().enabled=true;
            GetComponent<Animator>().Play("pistol_aimmyself");
            pistolPos.GetComponent<Animator>().Play("holder_aimmyself");
        }
        else {
            GetComponent<Animator>().Play("pistol_unaimmyself");
            pistolPos.GetComponent<Animator>().Play("holder_unaimmyself");
            StartCoroutine(disableHolderAnimDelayed());
        }
    }
    IEnumerator disableHolderAnimDelayed(){
        yield return new WaitForSeconds(0.7f);
        pistolPos.GetComponent<Animator>().enabled=false;
    }

    public void ejectBullet(){
        ServerEjectBullet(this);
    }
    [ServerRpc(RequireOwnership = false)]
    void ServerEjectBullet(PistolManager script){
        ObserverEjectBullet(script);
    }
    [ObserversRpc]
    void ObserverEjectBullet(PistolManager script){
        foreach(GameObject chamber in script.chamberLoc){
            Transform bullet;
            if (chamber.transform.childCount >= 1) bullet = chamber.transform.GetChild(0);
            else continue;
            if (bullet != null){
                bullet.SetParent(null);
                bullet.GetComponent<MeshCollider>().enabled = true;
                bullet.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }
}
