using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    Rigidbody2D rb;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float dashSpeed = 10f;
    [SerializeField] PlayerControls playerControls;

    Vector2 moveDirection = Vector2.zero;
    private InputAction move;
    private InputAction dash;

    void Awake(){
        playerControls = new PlayerControls();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable(){
        move = playerControls.Movement.Walk;
        move.Enable();

        dash = playerControls.Movement.Dash;
        dash.Enable();
        dash.performed+=Dash;
    }

    private void OnDisable(){
        move.Disable();

        dash.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = move.ReadValue<Vector2>();
    }

    private void FixedUpdate(){
        rb.velocity = new Vector2(moveDirection.x*moveSpeed, moveDirection.y*moveSpeed);
    }

    private void Dash(InputAction.CallbackContext context){
        //rb.AddForce(new Vector2(moveDirection * dashSpeed, moveDirection * dashSpeed), ForceMode2D.Impulse);
        Debug.Log("Dashed");
    }
}
