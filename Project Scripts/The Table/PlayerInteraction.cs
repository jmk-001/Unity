using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEditor;
using TMPro;
using FishNet.Component.Animating;
using Unity.VisualScripting.FullSerializer;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class PlayerInteraction : NetworkBehaviour
{
    public bool canInteract = true;
    public bool aimingMyself = false;
    public LayerMask pistolMask;
    public LayerMask bulletSelectorMask;
    public LayerMask chamberLoaderMask;
    public LayerMask obsMask;
    private float lastFireTime;
    public LayerMask myselfMask;
    public LayerMask enemyMask;
    public bool canFire;
    public bool isChecking;
    public bool firedAtEnemy = false;
    public Transform cameraTransform;
    public GameObject pistolLook;
    public GameObject objInHand;
    public Image crosshair;
    public Image circle;
    [SerializeField] GameObject pickUpPos;
    public bool pistolEquipped = false;
    public bool liveBulletFired = false;
    private Transform worldObjectHolder;
    private Transform prevPistol;
    private Color32 COLOR_GREEN = new Color32(0, 255, 0, 100);
    private Color32 COLOR_RED = new Color32(255, 0, 0, 100);

    public override void OnStartClient()
    {
        base.OnStartClient();
        GameManager.PlayerInteractions.Add(OwnerId, this);
        if (!base.IsOwner)
        {
            enabled = false;
        }
        cameraTransform = Camera.main.transform;
        pistolLook = GameObject.FindGameObjectWithTag("PistolLook");
        worldObjectHolder = GameObject.FindGameObjectWithTag("WorldObjects").transform;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        GameManager.PlayerInteractions.Remove(OwnerId);
    }

    private void Update()
    {
        if (canInteract){
            // Highlight pistol
            if (cameraTransform != null &&
                Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, Mathf.Infinity, pistolMask)){
                hit.collider.GetComponent<Highlight>().ToggleHighlight(true);
                prevPistol = hit.transform;
            } else { 
                if (prevPistol != null){
                    prevPistol.GetComponent<Highlight>().ToggleHighlight(false);
                } 
            }

            if (Input.GetKeyDown(KeyCode.E) && !pistolEquipped){
                equipPistol();
            }

            if (pistolEquipped){
                // Unequip
                if (Input.GetKeyDown(KeyCode.E)){
                    unequipPistol();
                }

                // Pistol Fired
                if (Input.GetKeyDown(KeyCode.Mouse0) && canFire && !isChecking){
                    if (Physics.Raycast(pistolLook.transform.position, pistolLook.transform.forward, out RaycastHit eny, Mathf.Infinity, enemyMask)){
                        Fire(true);
                    } else Fire();
                }

                // Pistol Aim Myself
                if (Input.GetKeyDown(KeyCode.Mouse2)){
                    if (!aimingMyself) AimMySelf();
                    else AimMySelf(reverse:true);
                }

                if (Physics.Raycast(pistolLook.transform.position, pistolLook.transform.forward, out RaycastHit enemy, Mathf.Infinity, enemyMask)){
                    circle.enabled = false;
                    crosshair.enabled = true;
                    crosshair.color = COLOR_RED;
                    canFire = true;
                }
                else if (Physics.Raycast(pistolLook.transform.position, pistolLook.transform.forward, out RaycastHit myself, Mathf.Infinity, myselfMask)){
                    crosshair.enabled = false;
                    circle.enabled = true;
                    canFire = true;
                }
                else {
                    crosshair.enabled = true;
                    circle.enabled = false;
                    crosshair.color = COLOR_GREEN;
                    canFire = false;
                }

                // Check chamber
                if (Input.GetKeyDown(KeyCode.C) && !aimingMyself){
                    isChecking = true;
                    objInHand.GetComponent<PistolManager>().checkChamber(this);
                }
            }

            // Bullet Loader
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit bSelector, 10f, bulletSelectorMask)){
                    bSelector.collider.GetComponent<BulletSelectorManager>().decrement();
                }
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit selector, 10f, bulletSelectorMask)){
                    selector.collider.GetComponent<BulletSelectorManager>().increment();
                } 
            }
            handleChamberLoader();
        }
    }

    public void equipPistol(GameObject pistol=null){
        if (pistol != null){
            SetObjectInHandServer(pistol.transform.gameObject, pickUpPos.transform.position, gameObject.transform.rotation, pickUpPos, this);
        }
        else {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, Mathf.Infinity, pistolMask)){
                Debug.Log(hit);
                SetObjectInHandServer(hit.transform.gameObject, pickUpPos.transform.position, gameObject.transform.rotation, pickUpPos, this);
            }
        }
    }

    public void unequipPistol(){
        DropObjectInHandServer(objInHand, worldObjectHolder, this);
    }

    public void AimMySelf(bool reverse=false){
        aimingMyself = !aimingMyself;
        objInHand.GetComponent<PistolManager>().aimMyself(reverse);
        if (!reverse) pistolLook.GetComponent<Animator>().Play("pl_aimmyself");
        else pistolLook.GetComponent<Animator>().Play("pl_unaimmyself");
    }

    public void Fire(bool atEnemy=false)
    {
        // Handle fire rate
        if (Time.time < lastFireTime + objInHand.GetComponent<PistolManager>().fireRate) return;
        lastFireTime = Time.time;

        // Disable Interactions
        canInteract = false;

        // Get chambered bullet
        int bulletType = objInHand.GetComponent<PistolManager>().handleFire();

        // Calculate (decode) damage
        int damage = bulletType;
        if (damage > 0) damage -= 1;

        // Raycast
        if (Physics.Raycast(pistolLook.transform.position, pistolLook.transform.forward, out RaycastHit myself, Mathf.Infinity, myselfMask)){
            if (bulletType == 2){
            }
            if (myself.transform.TryGetComponent(out PlayerHealth hp)){
            hp.TakeDamage(damage);
        }}
        if (Physics.Raycast(pistolLook.transform.position, pistolLook.transform.forward, out RaycastHit enemy, Mathf.Infinity, enemyMask)){
            if (bulletType == 2){
            }
            if (enemy.transform.TryGetComponent(out PlayerHealth health)){
            health.TakeDamage(damage);
        }}
        // AnimateWeapon
        objInHand.GetComponent<PistolManager>().animateFire(bulletType, atEnemy, aimingMyself, this);
    }

    // Server: Pick-up
    [ServerRpc(RequireOwnership = false)]
    void SetObjectInHandServer(GameObject obj, Vector3 position, Quaternion rotation, GameObject pickUpPoint, PlayerInteraction script){
        SetObjectInHandObserver(obj, position, rotation, pickUpPoint, script);
    }
    [ObserversRpc]
    void SetObjectInHandObserver(GameObject obj, Vector3 position, Quaternion rotation, GameObject pickUpPoint, PlayerInteraction script){
        obj.GetComponent<Rigidbody>().isKinematic = true;
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.SetParent(pickUpPoint.transform);
        obj.GetComponent<NetworkObject>().SetParent(pickUpPoint.GetComponent<NetworkObject>());
        obj.GetComponent<PistolManager>().setPos(GameObject.FindGameObjectWithTag("pickuppos"));

        script.pistolEquipped = true;
        script.objInHand = obj;
        script.pistolEquipped = true;

        Debug.Log("Picked up the pistol");
    }

    // Server: Drop-off
    [ServerRpc(RequireOwnership = false)]
    void DropObjectInHandServer(GameObject obj, Transform worldObjects, PlayerInteraction script){
        DropObjectInHandObserver(obj, worldObjects, script);
    }
    [ObserversRpc]
    void DropObjectInHandObserver(GameObject obj, Transform worldObjects, PlayerInteraction script){
        obj.GetComponent<Rigidbody>().isKinematic = false;
        obj.transform.parent = null;
        obj.GetComponent<NetworkObject>().UnsetParent();
        script.objInHand = null;
        script.pistolEquipped = false;
    }

    // Enabling/Disabling player interactions
    public static void TogglePlayerInteractions(int clientID, bool toggle){
        if (!GameManager.PlayerInteractions.TryGetValue(clientID, out PlayerInteraction interaction))
            return;
        interaction.TogglePlayerInteractionsServer(toggle);
    }
    [ServerRpc(RequireOwnership = false)]
    public void TogglePlayerInteractionsServer(bool toggle){
        TogglePlayerInteractionsObserver(toggle);
    }

    [ObserversRpc]
    private void TogglePlayerInteractionsObserver(bool toggle){
        canInteract = toggle;
    }

    void handleChamberLoader(){
        // Chamber Loader
        if (Input.GetKeyDown(KeyCode.Q) &&
            Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit cLoader, Mathf.Infinity, chamberLoaderMask))
        {
            cLoader.collider.GetComponent<ChamberLoaderManager>().rotateLeft();
        }
        if (Input.GetKeyDown(KeyCode.E) &&
            Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit cloader, Mathf.Infinity, chamberLoaderMask))
        {
            cloader.collider.GetComponent<ChamberLoaderManager>().rotateRight();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1) &&
            Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit cl, Mathf.Infinity, chamberLoaderMask))
        {
            cl.collider.GetComponent<ChamberLoaderManager>().insertBullet(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) &&
            Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit cld, Mathf.Infinity, chamberLoaderMask))
        {
            cld.collider.GetComponent<ChamberLoaderManager>().insertBullet(2);
        }
        if (Input.GetKeyDown(KeyCode.Space) &&
            Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit c, Mathf.Infinity, chamberLoaderMask))
        {
            c.collider.GetComponent<ChamberLoaderManager>().removeBullet();
        }
    }

    public void LiveFired(){
        ServerLiveFired(objInHand, this);
    }
    [ServerRpc(RequireOwnership = false)]
    void ServerLiveFired(GameObject objInHand, PlayerInteraction script){
        ObserverLiveFired(objInHand, script);
    }
    [ObserversRpc]
    void ObserverLiveFired(GameObject objInHand, PlayerInteraction script){
        StartCoroutine(LiveFiredCoroutine(objInHand, script));
    }
    IEnumerator LiveFiredCoroutine(GameObject objInHand, PlayerInteraction script){
        pickUpPos.GetComponent<Animator>().enabled = true;
        objInHand.GetComponent<Animator>().Play("pistol_ejectBullet");
        yield return new WaitForSeconds(2f);
        objInHand.GetComponent<PistolManager>().ejectBullet();
        yield return new WaitForSeconds(2f);
        Destroy(objInHand);
        ServerManager.Despawn(objInHand);
        script.objInHand = null;
        script.pistolEquipped = false;
        script.liveBulletFired = true;
        pickUpPos.GetComponent<Animator>().enabled = false;
    }
}
