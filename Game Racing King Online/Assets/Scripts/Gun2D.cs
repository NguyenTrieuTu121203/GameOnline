using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun2D : MonoBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    
    public float speedBullet;
    public float time = 0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        time += Time.deltaTime;
        if (time >= 2f)
        {
            Fire();
            time = 0f;
        }
    }

    private void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Rigidbody2D bulletRigidbody = bullet.GetComponent<Rigidbody2D>();
        bulletRigidbody.velocity = bulletSpawnPoint.right * speedBullet;
    }
}
