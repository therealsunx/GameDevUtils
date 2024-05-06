using System.Collections;
using TMPro;
using UnityEngine;

public class SMCharacterController : MonoBehaviour {
    
    [Header("Movement Modifiers")]
    [SerializeField] float walkSpeed;
    [SerializeField] float runMultiplier, jumpVelocity, dashVelocity, dashTime;
    Vector2 mouseDir, velocity, input;

    [Header("LayerMasks")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask waterLayer;
    [SerializeField] LayerMask grabbableLayer;

    [Header("Refs")]
    [SerializeField] Transform groundCheckPos;
    [SerializeField] Transform aboveHeadCheck;
    [SerializeField] TMP_Text stateText;

    STATE curState = STATE.IDLE;
    STATE nextState;

    Rigidbody2D rb;
    Camera cam;

    // Triggers
    bool t_jump, dash_t, run_t, grab_t, grabbableNearby, grounded, onWater;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        grounded = Physics2D.OverlapCircle(groundCheckPos.position, 0.1f, groundLayer);
        onWater = Physics2D.OverlapCircle(transform.position, 0.5f, waterLayer);

        if(onWater) curState = STATE.SWIMMING;
        else if(grounded) curState = STATE.IDLE;
        else curState = STATE.JUMPING;

        stateText.text = curState.ToString();
    }

    void Update(){
        GetInputs();
        StateTransition();

        stateText.text = curState.ToString();
    }

    void LateUpdate(){
        curState = nextState;
        rb.velocity = velocity;
    }

    void GetInputs(){
        velocity = rb.velocity;
        grounded = Physics2D.OverlapCircle(groundCheckPos.position, 0.1f, groundLayer);
        onWater = Physics2D.OverlapCircle(transform.position, 0.5f, waterLayer);
        mouseDir = (cam.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        input.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        t_jump = Input.GetKeyDown(KeyCode.Space);
        dash_t = Input.GetKeyDown(KeyCode.Q);
        run_t = Input.GetKey(KeyCode.LeftShift);
        grab_t = Input.GetKeyDown(KeyCode.E);
        grabbableNearby = Physics2D.OverlapCircle(transform.position, 0.5f, grabbableLayer);
    }

    void StateTransition(){

        switch(curState){
            case STATE.IDLE:
                if(dash_t) StartCoroutine(Dash());
                else if(input.x != 0f){
                    HorizontalMovement(input.x);
                    nextState = STATE.WALKING;
                }
                if(grab_t && grabbableNearby) nextState = STATE.CLIMBING;
                else if(grounded){
                    if(t_jump) Jump();
                }else nextState = STATE.JUMPING;
                break;

            case STATE.WALKING:
                if(input.x == 0f) nextState = STATE.IDLE;
                else if(run_t) {
                    HorizontalMovement(input.x * runMultiplier);
                    nextState = STATE.RUNNING;
                }
                else HorizontalMovement(input.x);

                if(dash_t) StartCoroutine(Dash());
                else if(grab_t && grabbableNearby) nextState = STATE.CLIMBING;
                else if(grounded){
                    if(t_jump) Jump();
                }else nextState = STATE.JUMPING;

                break;

            case STATE.RUNNING:
                if(input.x == 0f) nextState = STATE.IDLE;
                else if(run_t) HorizontalMovement(input.x * runMultiplier);
                else {
                    nextState = STATE.WALKING;
                    HorizontalMovement(input.x);
                }

                if(dash_t) StartCoroutine(Dash());
                else if(grab_t && grabbableNearby) nextState = STATE.CLIMBING;
                else if(grounded){
                    if(t_jump) Jump();
                }else nextState = STATE.JUMPING;
                break;

            case STATE.JUMPING:
                if(velocity.y < 0f) rb.gravityScale = 2.6f;
                else rb.gravityScale = 1f;
                HorizontalMovement(input.x);
                if(dash_t) StartCoroutine(Dash());
                else if(grab_t && grabbableNearby) nextState = STATE.CLIMBING;
                if(onWater) {
                    rb.gravityScale = 0.1f;
                    nextState = STATE.SWIMMING;
                } else if(grounded) {
                    rb.gravityScale = 1f;
                    nextState = STATE.IDLE;
                }
                break;

            case STATE.SWIMMING:
                bool onSurface = !Physics2D.Raycast(aboveHeadCheck.position, Vector2.up, 0.1f);


                if(dash_t) StartCoroutine(Dash());
                else if(grab_t && grabbableNearby) nextState = STATE.CLIMBING;
                else if(onSurface && t_jump) {
                    nextState = STATE.JUMPING;
                    rb.gravityScale = 1f;
                    Jump();
                }else if(!onWater){
                    nextState = STATE.JUMPING;
                    rb.gravityScale = 1f;
                }else {
                    HorizontalMovement(input.x * 0.7f);
                    VerticalMovement(input.y * 0.7f);
                }

                break;

            case STATE.CLIMBING:
                if(dash_t) StartCoroutine(Dash());
                if(grab_t || !grabbableNearby){
                    if(grounded) nextState = STATE.IDLE;
                    else nextState = STATE.JUMPING;
                }
                VerticalMovement(input.y * 0.5f);
                break;
        }
    }

    void HorizontalMovement(float i){
        velocity.x = i * walkSpeed;
    }

    void VerticalMovement(float i){
        velocity.y = i * walkSpeed;
    }

    void Jump(){
        nextState = STATE.JUMPING;
        velocity.y = jumpVelocity;
    }

    IEnumerator Dash(){
        nextState = STATE.DASHING;
        // rb.AddForce(Vector2.right * (mouseDir.x < 0f?-dashVelocity:dashVelocity), ForceMode2D.Impulse);
        velocity.x = (input.x!=0f?input.x : mouseDir.x<0f?-1f:1f) * dashVelocity;
        velocity.y = 0f;
        yield return new WaitForSeconds(dashTime);
        velocity.x = 0f;

        if(onWater) nextState = STATE.SWIMMING;
        else if(grounded) nextState = STATE.IDLE;
        else nextState = STATE.JUMPING;
    }

    //    void OnTriggerEnter2D(Collider2D other){
    //        if(other.gameObject.layer == 4) {
    //            onWater = true;
    //        }
    //    }
    //
    //    void OnTriggerExit2D(Collider2D other){
    //        if(other.gameObject.layer == 4){
    //            onWater = false;
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
