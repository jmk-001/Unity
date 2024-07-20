using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 5;
    public bool dead = false;
    public int currentHealth;
    private int healthTracker;
    public GameObject healthBar;
    public GameObject healthBarHolder;
    public Image fill;
    public Gradient gradient;
    public GameObject[] objectsToDisable;

    void Update()
    {
        if (currentHealth != healthTracker && currentHealth > 0){
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>().shake();
            ServerAnimateLight();
            healthTracker = currentHealth;
        }
    }

    void Awake()
    {
        currentHealth = maxHealth;
        healthTracker = currentHealth;

        fill.color = gradient.Evaluate(1f);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        GameManager.PlayerHealths.Add(OwnerId, this);
        if (!base.IsOwner){
            enabled = false;
        }
    }

    public bool TakeDamage(int damage){
        ServerTakeDamage(damage, this);
        Debug.Log("Function Player Health: " + currentHealth);
        if (currentHealth <= 0) return true; 
        else return false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerTakeDamage(int damage, PlayerHealth script){
        currentHealth -= damage;
        //Debug.Log("New Player Health: " + currentHealth);
        OberverUpdateHealth(currentHealth, script);
        if (currentHealth <= 0){
            Die();
        }
    }
    [ObserversRpc]
    void OberverUpdateHealth(int newHealth, PlayerHealth script){
        script.currentHealth = newHealth;
        script.healthBar.GetComponent<Slider>().value = currentHealth;
        script.fill.color = script.gradient.Evaluate(script.healthBar.GetComponent<Slider>().normalizedValue);
    }

    private void Die(){
        //Debug.Log("Player is dead");
        ServerSetDeathStatus(this);
    }

    // Sync Death Status
    [ServerRpc(RequireOwnership = false)]
    void ServerSetDeathStatus(PlayerHealth script){
        ObserverSetDeathStatus(script);
    }
    [ObserversRpc]
    void ObserverSetDeathStatus(PlayerHealth script){
        script.dead = true;
        GameObject.FindGameObjectWithTag("PlayerUI").SetActive(false);
        GameManager.PlayerDead();
    }

    // Animate Light Blink for Both Players
    [ServerRpc(RequireOwnership = false)]
    void ServerAnimateLight(){
        ObserverAnimateLight();
    }
    [ObserversRpc]
    void ObserverAnimateLight(){
        GameObject toplight = GameObject.FindGameObjectWithTag("toplight");
        toplight.GetComponent<Animator>().Play("lightblink");
        toplight.GetComponent<AudioSource>().Play(0);
    }
}
