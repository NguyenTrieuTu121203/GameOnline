using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
   
    private void Awake()
    {
        
    }
    // Update is called once per frame
    private void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        
        if(col.gameObject.CompareTag("Obstacle")|| col.gameObject.CompareTag("Unjump")|| col.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
       
    }
}
