using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading;
using UnityEngine.UI;



public class Unit : MonoBehaviour
{
    public int unitID;
    public bool isPlayersUnit;
  
    public float moveSpeed = 5f;
    public float JumpForce = 15.0f;
    public Rigidbody2D rb;
    public Animator animator;
    public float Horizol;
    public bool IsGround;
    public bool IsDie;
    public bool isFaceRight = true;
    public Vector3 tempScale;
    public int ValueFlip;
    public bool IsKey;

    public GameObject cherry;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        tempScale = transform.localScale;
        cherry = GameObject.FindWithTag("Cherry");
    }

    private void Update()
    {
        PlayerMove();
        PlayerIsDied();
        CheckIsKey();
    }



    private void PlayerMove()
    {
        Horizol = Input.GetAxis("Horizontal");
        
        if (Horizol != 0 && isPlayersUnit)
        {
            animator.SetFloat("Speed",Mathf.Abs(Horizol));
            Vector3 movement = new Vector3(Horizol, 0f) * moveSpeed * Time.deltaTime;
            transform.Translate(movement, Space.Self);
            PlayerFlip();
            float x = transform.position.x;
            float y = transform.position.y;
            float animationValue = 1.0f;
            PlayerControls.client.Send("Moving|" + unitID + "|" + x + "|" + y + "|" + animationValue+ "|" + ValueFlip);
            
        }
        if(Horizol == 0 && isPlayersUnit)
        {
            animator.SetFloat("Speed", Mathf.Abs(Horizol));
            PlayerFlip();
            float x = transform.position.x;
            float y = transform.position.y;
            float animationValue = 0;
            PlayerControls.client.Send("Moving|" + unitID + "|" + x + "|" + y + "|" + animationValue + "|" + ValueFlip);
            
        }
        if (Input.GetKeyDown(KeyCode.Space) && isPlayersUnit && IsGround)
        {
            rb.velocity = new Vector2(rb.velocity.x, JumpForce);
            float x = transform.position.x;
            float y = transform.position.y;
            float z = transform.position.z;
            PlayerControls.client.Send("Moving|" + unitID + "|" + x + "|" + y + "|" + z + "|" + 1 + "|" + ValueFlip);
            
        }
    }

    //Kiểm tra xem có chạm đất
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) IsGround = true;
        if(collision.gameObject.CompareTag("Obstacle")) IsDie = true;
       
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) IsGround = false;
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Bullet")) IsDie = false;
        
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cherry")) IsKey = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cherry")) IsKey = true;
    }

    private void CheckIsKey()
    {
        if (IsKey && !IsDie)
        {
            cherry.SetActive(false);
        }
        if(IsKey && IsDie)
        {
            cherry.SetActive(true);
            IsKey = false;
        }
    }

    private void PlayerIsDied()
    {
        if (IsDie && isPlayersUnit)
        {
            transform.position = new Vector3(0.1f, -6.09f, 0);
            float x = transform.position.x;
            float y = transform.position.y;
            float z = transform.position.z;
            PlayerControls.client.Send("Moving|" + unitID + "|" + x + "|" + y + "|" + z + "|" + ValueFlip);
        }
    }



    private void PlayerFlip()
    {
        if (Horizol < 0 && isFaceRight)
        {
            Flip();
            ValueFlip = 1;
            /*PlayerControls.client.Send("Flipping|" + unitID + "|" + 0);*/
        }
        if(Horizol>0 && !isFaceRight)
        {
            Flip();
            ValueFlip = 0;
            /*PlayerControls.client.Send("Flipping|" + unitID + "|" + 1);*/
        }
    }

    private void Flip()
    {
        Vector2 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
        isFaceRight=!isFaceRight;
    }





    public void MoveTo(Vector3 pos)
    {
        
        pos.z = 0;
        rb.MovePosition(pos);
        /*Thread.Sleep(25);*/
    }

    public void FlipDirec(int index)
    {
        
        if (index == 1)
        {
            Vector3 ScaleTemp = new Vector3(-2.5f,2.5f,0f);
            transform.localScale = ScaleTemp;
        }
        else
        {
            Vector3 ScaleTemp = new Vector3(2.5f, 2.5f, 0f);
            transform.localScale = ScaleTemp;
        }
    }

    public void SetAnimtionState(float ValueAnim)
    {
        
        if (ValueAnim != 0)
            animator.SetFloat("Speed", 1f);
        else
            animator.SetFloat("Speed", 0f);
        
    }
}



