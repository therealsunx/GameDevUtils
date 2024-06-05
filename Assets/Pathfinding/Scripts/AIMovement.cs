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

        Debug.Log(del + " " + grounded);

        if(grounded && del.y > 0f){
            velocity.y = Mathf.Sqrt(-4f * del.y * Physics2D.gravity.y);
            if(Mathf.Abs(del.x) > 1f){
                float tan = 10f * del.y / del.x;
                velocity.x = velocity.y / tan;
            }
        }else if(Mathf.Abs(del.x)<.5f)
            velocity.x = del.x<0f?-1f:1f;
    }

    void JumpDownHandler(){
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheck.localScale, 0f, groundLayer);
        Vector2 del = target - current;
        if(grounded && del.y < -0.5f){
            velocity.x = del.x * Mathf.Sqrt(0.5f * Physics2D.gravity.y / del.y);
        }else{
            //Debug.Log(del + "yeah");
        }
    }

    void JumpHorizontalHandler(){
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheck.localScale, 0f, groundLayer);
        Vector2 del = target - current;
        if(grounded) {
            velocity.x = del.x * Physics2D.gravity.y * -0.1f;
            velocity.y = Mathf.Abs(velocity.x);
        }
    }

    void PathStateCheck(){
        target = path[curWayPoint+1].position;
        current = (Vector2) transform.position;
        Vector2 del = target - current;
        if(del.magnitude > .8f){
            mvState = mvState == MVSTATE.IDLE? MVSTATE.WALK : mvState;
            return;
        }
        curWayPoint++;
        if((curWayPoint+1) >= path.Count){
            target = player.position;
            del = target - (Vector2) transform.position;
            mvState = del.y >= .1f? MVSTATE.JUMPUP : MVSTATE.WALK;
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
            mvState = del.y < -1f? MVSTATE.JUMPDOWN : MVSTATE.WALK;
        }else{
            mvState = MVSTATE.JUMPUP;
        }
    }

    void WalkStateHandler(){
        Vector2 del = path[curWayPoint+1].position - (Vector2) transform.position;
        velocity.x = del.x > 0f? speedX:-speedX;
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
