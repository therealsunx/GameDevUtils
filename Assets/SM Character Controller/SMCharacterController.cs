using System.Collections;
using UnityEngine;

public class SMCharacterController : MonoBehaviour {
    
    [Header("Movement Modifiers")]
    [SerializeField] float maxSpeed;
    [SerializeField] float walkForce, jumpForce, dashForce, dashTime;
    Vector2 direction;
    float speed;
    bool grounded, onwater;

    [Header("Constants")]
    [SerializeField] float runThreshold;

    [Header("LayerMasks")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask waterLayer;
    [SerializeField] LayerMask grabbableLayer;

    [Header("Refs")]
    [SerializeField] Transform groundCheckPos;
    [SerializeField] Transform aboveHeadCheck;

    STATE curState = STATE.IDLE;
    STATE nextState;

    Rigidbody2D rb;
    Camera cam;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        grounded = Physics2D.OverlapCircle(groundCheckPos.position, 0.1f, groundLayer);
        onwater = Physics2D.OverlapCircle(transform.position, 0.5f, waterLayer);

        if(onwater) curState = STATE.SWIMMING;
        else if(grounded) curState = STATE.IDLE;
        else curState = STATE.JUMPING;
    }

    void Update(){
        speed = Mathf.Abs(rb.velocity.x);
        grounded = Physics2D.OverlapCircle(groundCheckPos.position, 0.1f, groundLayer);
        onwater = Physics2D.OverlapCircle(transform.position, 0.5f, waterLayer);
        direction = cam.ScreenToWorldPoint(Input.mousePosition);

        StateTransition();
    }

    void StateTransition(){
        float xin = Input.GetAxisRaw("Horizontal");
        float yin = Input.GetAxisRaw("Vertical");
        bool jumpTrigger = Input.GetKeyDown(KeyCode.Space);
        bool dashTrigger = Input.GetKeyDown(KeyCode.LeftShift);
        bool grabTrigger = Input.GetKeyDown(KeyCode.E);
        bool grabbableNearby = Physics2D.OverlapCircle(transform.position, 0.5f, grabbableLayer);

        switch(curState){
            case STATE.IDLE:
                if(xin != 0f){
                    HorizontalMovement(xin);
                    nextState = STATE.WALKING;
                }
                if(jumpTrigger && grounded) Jump();
                break;

            case STATE.WALKING:
                if(xin == 0f) nextState = STATE.IDLE;
                else HorizontalMovement(xin);
                
                if(speed > runThreshold) nextState = STATE.RUNNING;
                
                if(jumpTrigger && grounded) Jump();
                break;

            case STATE.RUNNING:
                if(xin == 0f) nextState = STATE.IDLE;
                else HorizontalMovement(xin);

                if(jumpTrigger && grounded) Jump();
                break;

            case STATE.JUMPING:
                if(rb.velocity.y < 0f) rb.gravityScale = 2.6f;
                else rb.gravityScale = 1f;

                if(onwater) {
                    rb.gravityScale = -0.01f;
                    nextState = STATE.SWIMMING;
                } else if(grounded) {
                    rb.gravityScale = 1f;
                    nextState = STATE.IDLE;
                }
                break;

            case STATE.SWIMMING:
                bool onSurface = !Physics2D.Raycast(aboveHeadCheck.position, Vector2.up, 0.1f);
                if(onSurface && jumpTrigger) {
                    nextState = STATE.JUMPING;
                    rb.gravityScale = 1f;
                    Jump();
                }else if(!onwater){
                    nextState = STATE.JUMPING;
                    rb.gravityScale = 1f;
                }

                HorizontalMovement(xin);
                VerticalMovement(yin);
                break;

            case STATE.CLIMBING:
                if(grabTrigger || !grabbableNearby){
                    if(grounded) nextState = STATE.IDLE;
                    else nextState = STATE.JUMPING;
                }
                ClimbingMovement(yin);
                break;
        }
        
        if(curState != STATE.CLIMBING){
            if(dashTrigger && curState != STATE.DASHING){
                StartCoroutine(Dash());
                rb.gravityScale = 1f;
            }
            else if(grabTrigger){
                if(grabbableNearby) nextState = STATE.CLIMBING;
                rb.gravityScale = 1f;
            }
        }
    }

    void LateUpdate(){
        curState = nextState;
    }

    void ClimbingMovement(float i){
        rb.velocity = Vector2.up * i * maxSpeed * 0.5f;
    }

    void HorizontalMovement(float i){
        if(Mathf.Abs(rb.velocity.x) < maxSpeed) {
            float mul = i <= 0f? i*(rb.velocity.x>0f?2:1):(rb.velocity.x<0f?2:1);
            rb.AddForce(Vector2.right * walkForce * mul);
        }
    }

    void VerticalMovement(float i){
        if(Mathf.Abs(rb.velocity.y) < maxSpeed) {
            float mul = i <= 0f? i*(rb.velocity.y>0f?2:1):(rb.velocity.y<0f?2:1);
            rb.AddForce(Vector2.up* walkForce * mul);
        }
    }

    void Jump(){
        nextState = STATE.JUMPING;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    IEnumerator Dash(){
        nextState = STATE.DASHING;
        rb.AddForce(Vector2.right * (direction.x < 0f?-dashForce:dashForce), ForceMode2D.Impulse);
        yield return new WaitForSeconds(dashTime);
        rb.velocity = Vector2.zero;
        
        if(grounded) nextState = STATE.IDLE;
        else nextState = STATE.JUMPING;
    }

//    void OnTriggerEnter2D(Collider2D other){
//        if(other.gameObject.layer == 4) {
//            onwater = true;
//        }
//    }
//
//    void OnTriggerExit2D(Collider2D other){
//        if(other.gameObject.layer == 4){
//            onwater = false;
//            Debug.Log("Chapaaak");
//        }
//    }
}

//
//    void DashCheck(){
//        if(nextState != STATE.DASHING && Input.GetKeyDown(KeyCode.W)){
//            StartCoroutine(Dash());
//        }
//    }
//
//    void VerticalMovement(){
//
//    }
//
//
//}
