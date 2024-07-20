using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
 
//This is made by Bobsi Unity - Youtube
public class PlayerMovement : NetworkBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float mouseSensitivity = 2.0f;
    public float lookXLimit = 45.0f;
    public Transform sp_02;
    public GameObject localUI;
    public GameObject pickuppos;
    [SerializeField] private int playerSelfLayer = 8;
 
    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    float rotationY = 0;
 
    [HideInInspector]
    public bool canMove = true;
 
    [SerializeField]
    private float cameraYOffset = 0.4f;
    private Camera playerCamera;
    public float initialY;
 
    public override void OnStartClient()
    {
        base.OnStartClient();
        GameManager.PlayerMovements.Add(OwnerId, this);
        if (base.IsOwner)
        {
            playerCamera = Camera.main;
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z + -3);
            playerCamera.transform.SetParent(transform);
            this.GetComponent<PlayerInteraction>().cameraTransform = playerCamera.transform;

            sp_02 = GameObject.Find("sp_02").transform;
            if (transform.position == sp_02.position){
                transform.Rotate(0, 180, 0);
            }

            gameObject.layer = playerSelfLayer;
            foreach (Transform child in transform){
                child.gameObject.layer = playerSelfLayer;
            }
            initialY = transform.rotation.y;
        }
        else
        {
            gameObject.GetComponent<PlayerMovement>().enabled = false;
        }
    }
 
    void Start()
    {
        characterController = GetComponent<CharacterController>();
 
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
 
    void Update()
    {
        // Player and Camera rotation
        if (canMove && playerCamera != null)
        {
            float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            
            rotationX += mouseY;
            rotationY += mouseX;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            rotationY = Mathf.Clamp(rotationY, initialY-40f, initialY+40f);

            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
            //transform.localRotation *= Quaternion.Euler(0, rotationY, 0);
        }
    }
}