using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float hAxis;
    float vAxis;
    public float speed;

    Vector3 moveVec;

    Animator anim;
    bool wDown;

    Rigidbody rigid;
    bool jDown;
    bool isJump;

    Vector3 dodgeVec;
    bool isDodge;

    GameObject nearObject;
    bool iDown;
    public GameObject[] weapons;
    public bool[] hasWeapons;

    GameObject equipWeapon;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool isSwap;
    int equipWeaponIndex = -1;

    public int ammo;
    public int coin;
    public int health;
    public int hasGrenades;
    public int MaxAmmo;
    public int MaxCoin;
    public int MaxHealth;
    public int MaxHasGrenades;

    public GameObject[] grenades;

    void Awake()
    {
        rigid = GetComponentInChildren<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
        Swap();
        Interaction();
    }
    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2= Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
        {
            moveVec = dodgeVec;
        }

        if(isSwap)
        {
            moveVec = Vector3.zero;
        }

        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);
    }
    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }
    void Jump()
    {
        if (jDown && !isJump && moveVec == Vector3.zero && !isDodge && !isSwap)
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }
    void Dodge()
    {
        if (jDown && !isJump && moveVec != Vector3.zero && !isDodge && !isSwap)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.4f);
        }
    }
    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }
    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0)) return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1)) return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2)) return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 1;
        if (sDown2) weaponIndex = 2;
        if (sDown3) weaponIndex = 3;

        if((sDown1 || sDown2 || sDown3) && !isJump && !isSwap && !isDodge)
        {
            if(equipWeapon != null)
            {
                equipWeapon.SetActive(false);
            }
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex];
            equipWeapon.SetActive(true);

            anim.SetTrigger("doSwap");
            isSwap = true;
            Invoke("SwapOut", 0.5f);
        }
    }
    void SwapOut()
    {
        isSwap = false;
    }
    void Interaction()
    {
        if (iDown && nearObject != null && !isJump)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if(MaxAmmo < ammo)
                    {
                        ammo = MaxAmmo;
                    }
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (MaxCoin < coin)
                    {
                        coin = MaxCoin;
                    }
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (MaxHealth < health)
                    {
                        health = MaxHealth;
                    }
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (MaxHasGrenades < hasGrenades)
                    {
                        hasGrenades = MaxHasGrenades;
                    }
                    break;
            }

            Destroy(other.gameObject);
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = other.gameObject;
        }

        Debug.Log(nearObject.name);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }
}
