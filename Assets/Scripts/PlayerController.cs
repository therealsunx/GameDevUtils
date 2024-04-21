using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("Movement")]
    [SerializeField] float speed;
    Vector2 velocity, facingDir;

    [Header("Jump")]
    [SerializeField] float jumpSpeed;
    [SerializeField] float coyoteTime;
    [SerializeField] LayerMask jumpableLayer;
    float jumpWindowTimer;
    int jumps;

    [Header("Web")]
    [SerializeField] float maxWebLength;
    [SerializeField] LayerMask webbableLayer;
    [SerializeField] Web web;
    bool autoWebTug = true;

    Rigidbody2D rb;
    Camera mainCam;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
    }

    void Update(){
        facingDir = (mainCam.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        velocity = rb.velocity;

        HandleMovement();

        if(Input.GetMouseButtonDown(0)){
            ShootWeb();
        }else if (Input.GetMouseButton(0)){
            if(Input.GetKey(KeyCode.R))
                web.targetLength += 0.1f;
            else if(autoWebTug)
                web.targetLength *= 0.9f;
        }else if(Input.GetMouseButtonUp(0))
            web.enabled = false;

        if(Input.GetKeyDown(KeyCode.E))
            autoWebTug = !autoWebTug;

        rb.velocity =  velocity;
    }

    void HandleMovement(){
        facingDir = (mainCam.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        float x = Input.GetAxisRaw("Horizontal");

        if(x != 0f) velocity.x = x * speed;

        if(velocity.y < 0f) rb.gravityScale = 3.8f;
        else rb.gravityScale = 1f;

        if(Physics2D.Raycast(transform.position, Vector2.down, 0.6f, jumpableLayer)){
            jumpWindowTimer = coyoteTime;
            jumps = 2;
        }else{
            if(jumpWindowTimer < 0f) jumps = Mathf.Min(1, jumps);
            else jumpWindowTimer -= Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.Space) && jumps > 0){
            velocity.y = jumpSpeed;
            jumps--;
        }

    }

    void ShootWeb(){
        RaycastHit2D _hit = Physics2D.Raycast(transform.position, facingDir, maxWebLength, webbableLayer);
        if(_hit.collider is null) return;

        if(_hit.rigidbody is not null && _hit.rigidbody.bodyType == RigidbodyType2D.Dynamic){ 
            web.spring.connectedBody = _hit.rigidbody;
        }else{
            web.spring.connectedBody = null;
        }
        web.targetPoint = _hit.point;
        web.targetLength = _hit.distance * 0.9f;
        web.enabled = true;
    }

}
