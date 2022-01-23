using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using EasyJoystick;
using UnityStandardAssets.CrossPlatformInput;

public class Wormy : MonoBehaviour
{
    public Rigidbody2D bulletPrefab;
    public Transform currentGun;

    public float wormySpeed = 1;
    public float maxRelativeVelocity;
    public float misileForce = 5;

    PhotonView view;
    //Joystick control
    [SerializeField] private float speed;
    [SerializeField] private EasyJoystick.Joystick joystick;

    public bool IsTurn { get { return WormyManager.singleton.IsMyTurn(wormId); } }

    public int wormId;
    WormyHealth wormyHealth;
    SpriteRenderer ren;

    private void Start()
    {
        wormyHealth = GetComponent<WormyHealth>();
        ren = GetComponent<SpriteRenderer>();
        view = GetComponent<PhotonView>();
    }

    private void Update()
    {

        if (view.IsMine)
        {
            if (!IsTurn)
                return;

            RotateGun();

            var hor = CrossPlatformInputManager.GetAxis("Horizontal");
            if (hor == 0)
            {
                currentGun.gameObject.SetActive(true);

                ren.flipX = currentGun.eulerAngles.z < 180;

                if (CrossPlatformInputManager.GetButtonDown("Fire"))
                {
                    var p = Instantiate(bulletPrefab, currentGun.position - currentGun.right, currentGun.rotation);
                    p.AddForce(-currentGun.right * misileForce, ForceMode2D.Impulse);

                    if (IsTurn)
                        WormyManager.singleton.NextWorm();
                }
            }
            else
            {
                currentGun.gameObject.SetActive(false);
                transform.position += Vector3.right * hor * Time.deltaTime * wormySpeed;
                ren.flipX = CrossPlatformInputManager.GetAxis("Horizontal") > 0;
            }
        }

    }

    void RotateGun()
    {
        //Input.mousePosition
        //var diff = Camera.main.ScreenToWorldPoint() - transform.position;
        //diff.Normalize();

        float rot_z = Mathf.Atan2(joystick.Vertical(), joystick.Horizontal()) * Mathf.Rad2Deg;
        currentGun.rotation = Quaternion.Euler(0f, 0f, rot_z + 180);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.magnitude > maxRelativeVelocity)
        {
            wormyHealth.ChangeHealth(-3);
            if (IsTurn)
                WormyManager.singleton.NextWorm();
        }  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Explosion"))
        {
            wormyHealth.ChangeHealth(-10);
            if (IsTurn)
                WormyManager.singleton.NextWorm();
        }
    }
}
