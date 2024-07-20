using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class ShootNetwork : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;

    private List<Bullet> spawnedBullets = new List<Bullet>();

    private Transform pistol;

    void Start()
    {
        pistol = GameObject.Find("pistol").transform;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var bullet in spawnedBullets)
        {
            bullet.bulletTransform.position += bullet.Direction * Time.deltaTime * bulletSpeed;
        }

        if (!IsOwner){
            return;
        }
        if (Input.GetKeyDown(KeyCode.Mouse0)){
            shoot();
        }
    }

    private void shoot() {
        Vector3 startPos = pistol.position;
        Vector3 direction = pistol.forward;

        SpawnBulletLocal(startPos, direction);
        SpawnBullet(startPos, direction, TimeManager.Tick);
    }

    private void SpawnBulletLocal (Vector3 startPos, Vector3 direction){
        GameObject bullet = Instantiate(bulletPrefab, startPos, Quaternion.identity);
        spawnedBullets.Add(new Bullet() { bulletTransform = bullet.transform, Direction = direction });
    }

    [ServerRpc]
    private void SpawnBullet(Vector3 startPos, Vector3 direction, uint startTick)
    {
        SpawnBulletObserver(startPos, direction, startTick);
    }

    [ObserversRpc(ExcludeOwner=true)]
    private void SpawnBulletObserver(Vector3 startPos, Vector3 direction, uint startTick){
        float timeDifference = (TimeManager.Tick - startTick) / TimeManager.TickRate;   // how long it takes to receive data
        Vector3 spawnPosition = startPos + direction * bulletSpeed * timeDifference;
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        spawnedBullets.Add(new Bullet() { bulletTransform = bullet.transform, Direction = direction });
    }

    private class Bullet {
        public Transform bulletTransform;
        public Vector3 Direction;
    }
}
