using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AIMovement : MonoBehaviour {
    public enum AISTATE {IDLE, FOLLOW, ATTACK}
    public enum MVSTATE {IDLE, WALK, JUMPHOR, JUMPUP, JUMPDOWN}

    [SerializeField] Pathfinder pathfinder;
    [SerializeField] float speedX, speedY, refreshTime;
    [SerializeField] Transform player, groundCheck;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] TMP_Text stateText;

    private float _refreshTimer = 0f;
    private int curWayPoint = 0;
    private List<Node> path = null;
    private Vector2 target, current;
    private Vector2 velocity;
    private MVSTATE mvState = MVSTATE.IDLE;
    private AISTATE state = AISTATE.FOLLOW;
    private Rigidbody2D rb;


    void Start(){
        rb = GetComponent<Rigidbody2D>();
    }

    void Update(){
        velocity = rb.velocity;
        UpdatePath();
        StateHandle();
        rb.velocity = velocity;
    }

    void StateHandle(){
        switch(state){
            case AISTATE.FOLLOW:
                if(path is not null) FollowStateHandle();
                break;
        }
    }

    void FollowStateHandle(){
        PathStateCheck();
        stateText.text = mvState.ToString();
        switch(mvState){
            case MVSTATE.IDLE:
                break;
            case MVSTATE.WALK:
                WalkStateHandler();
                break;
            case MVSTATE.JUMPHOR:
                JumpHorizontalHandler();
                break;
            case MVSTATE.JUMPDOWN:
                JumpDownHandler();
                break;
            case MVSTATE.JUMPUP:
                JumpUpHandler();
                break;
        }
    }

    void JumpUpHandler(){
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheck.localScale, 0f, groundLayer);
        Vector2 del = target - current;

        if(grounded && del.y > 0f){
            if(Mathf.Abs(del.x)<1.1f) {
                velocity.x = del.x < 0f? speedX : -speedX;
                return;
            }
            velocity.y = Mathf.Sqrt(-3f * del.y * Physics2D.gravity.y);
            if(Mathf.Abs(del.x) > 1.9f){
                float tan = 8f * del.y / del.x;
                velocity.x = velocity.y / tan;
            }else velocity.x = 0f;
        } else {
            if(del.y>0f) return;
            if(Mathf.Abs(del.x) < 1.1f)
                velocity.x = del.x > 0f? 2f : -2f;
            else
                velocity.x = del.x > 0f? speedX : -speedX;
            return;
        }

    }

    void JumpDownHandler(){
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheck.localScale, 0f, groundLayer);
        Vector2 del = target - current;

        if(!grounded) return;
        if(del.y < -1.5f) velocity.x = del.x * Mathf.Sqrt(0.6f * Physics2D.gravity.y / del.y);
        else velocity.x = del.x > 0f? speedX : -speedX;
    }

    void JumpHorizontalHandler(){
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheck.localScale, 0f, groundLayer);
        Vector2 del = target - current;
        if(grounded) {
            velocity.x = del.x * Physics2D.gravity.y * -0.1f;
            velocity.y = Mathf.Abs(velocity.x);
            if(velocity.magnitude > speedY) velocity = velocity.normalized * speedY;
        }
    }

    void WalkStateHandler(){
        Vector2 del = target - current;
        velocity.x = del.x > 0f? speedX:-speedX;
    }

    void PathStateCheck(){
        if((curWayPoint + 1) >= path.Count) target = player.position;
        else target = path[curWayPoint+1].position;
        current = (Vector2) transform.position;
        Vector2 del = target - current;

        // update current waypoint on graph based on next neighbours
        Debug.DrawRay(current, del, del.magnitude < 1f?Color.green:Color.yellow);
        if(del.magnitude > .1f){
            mvState = mvState == MVSTATE.IDLE? MVSTATE.WALK : mvState;
            return;
        }
        curWayPoint++;

        if((curWayPoint+1) >= path.Count){
            target = player.position;
            del = target - (Vector2) transform.position;
            mvState = del.y >= .1f? MVSTATE.JUMPUP : MVSTATE.WALK;
            Debug.Log("Path finished");
            return;
        }

        Node trg = path[curWayPoint+1], cur = path[curWayPoint];
        target = trg.position;
        del = target - cur.position;

        if(Mathf.Abs(del.y) < 0.1f){
            if(del.x > 0f){
                mvState = (cur.type & (uint)NodeType.REDGE) > 0 ? MVSTATE.JUMPHOR : MVSTATE.WALK ;
            }else{
                mvState = (cur.type & (uint)NodeType.LEDGE) > 0 ? MVSTATE.JUMPHOR : MVSTATE.WALK ;
            }
        }else if(del.y < 0f){
            mvState = (del.y >= -2.1f && Mathf.Abs(del.x) > Mathf.Abs(del.y))?MVSTATE.JUMPHOR : MVSTATE.JUMPDOWN;
        }else{
            mvState = MVSTATE.JUMPUP;
        }
    }

    void UpdatePath(){
      if(_refreshTimer > 0f){
          _refreshTimer -= Time.deltaTime;
      }else{
          _refreshTimer = refreshTime;
          curWayPoint = 0;
          path = pathfinder.FindPath(transform, player);
      }
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(current, .4f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(target, .4f);
    }
}
