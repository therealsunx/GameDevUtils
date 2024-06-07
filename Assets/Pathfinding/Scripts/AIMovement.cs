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
                velocity.x *= 0.8f;
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

        if(grounded){
            if(del.y > 0f){
                if(Mathf.Abs(del.x) <= 1f){
                    velocity.x = del.x < 0f? speedX : -speedX;
                }else {
                    velocity.y = Mathf.Sqrt(-2.8f * del.y * Physics2D.gravity.y);
                    if(Mathf.Abs(del.x) < 1.5f) velocity.x = 0f;
                    else velocity.x = -0.8f * del.x * Physics2D.gravity.y / velocity.y;
                }
            }else{
                velocity.x = del.x > 0f? speedX : -speedX;
            }
        }else if(del.y < -0.1f && Mathf.Abs(del.x)<1.5f){
            velocity.x = del.x > 0f? speedX : -speedX;
        }

//      if(grounded && del.y > 0f){
//          if(Mathf.Abs(del.x)<1.1f) {
//              velocity.x = del.x < 0f? speedX : -speedX;
//              return;
//          }
//          velocity.y = Mathf.Sqrt(-3f * del.y * Physics2D.gravity.y);
//          if(Mathf.Abs(del.x) > 1.9f){
//              float tan = 8f * del.y / del.x;
//              velocity.x = velocity.y / tan;
//          }else velocity.x = 0f;
//      } else {
//          if(del.y>0f) return;
//          if(Mathf.Abs(del.x) < 1.1f)
//              velocity.x = del.x > 0f? 2f : -2f;
//          else
//              velocity.x = del.x > 0f? speedX : -speedX;
//          return;
//      }
    }

    void JumpDownHandler(){
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheck.localScale, 0f, groundLayer);
        Vector2 del = target - current;

        if(!grounded) return;
        if(del.y < -1.5f) velocity.x = del.x * Mathf.Sqrt(0.6f * Physics2D.gravity.y / del.y);
        else if (Mathf.Abs(del.x) > 3f){
            velocity.x = Mathf.Sqrt(Mathf.Abs(del.x) * Physics2D.gravity.y * -0.4f);
            velocity.y = velocity.x;
            if(del.x < 0f) velocity.x = -velocity.x;
        } else velocity.x = del.x > 0f? speedX : -speedX;
    }

    void JumpHorizontalHandler(){
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheck.localScale, 0f, groundLayer);
        Vector2 del = target - current;
        if(grounded) {
            velocity.x = Mathf.Sqrt(Mathf.Abs(del.x) * Physics2D.gravity.y * -0.5f);
            velocity.y = velocity.x;
            if(del.x < 0f) velocity.x = -velocity.x;
            // if(velocity.magnitude > speedY) velocity = velocity.normalized * speedY;
        }
    }

    void WalkStateHandler(){
        bool grounded = Physics2D.OverlapBox(groundCheck.position, groundCheck.localScale, 0f, groundLayer);
        if(!grounded) return;

        Vector2 del = target - current;
        if(Mathf.Abs(velocity.x)<.5f || Mathf.Abs(del.x) > 0.2f)
            velocity.x = del.x > 0f? speedX : -speedX;
    }

    bool _NoWayCheck(){
        if(path is null || curWayPoint >= (path.Count-1)){
            mvState = MVSTATE.IDLE;
            return true;
        }
        return false;
    }

    void PathStateCheck(){
        if(_NoWayCheck()) return;

        target = path[curWayPoint+1].position; current = transform.position;
        Vector2 del = target-current;

        System.Action _DetermineNextState = ()=>{
            curWayPoint++;
            if(_NoWayCheck()) return;

            Node cur = path[curWayPoint];
            target = path[curWayPoint+1].position; current = transform.position;
            del = target - cur.position;

            if(Mathf.Abs(del.y) < .1f){
                if(del.x > 0f){
                    mvState = (cur.type & (uint)NodeType.REDGE) > 0 ? MVSTATE.JUMPHOR : MVSTATE.WALK ;
                }else{
                    mvState = (cur.type & (uint)NodeType.LEDGE) > 0 ? MVSTATE.JUMPHOR : MVSTATE.WALK ;
                }
            } else if(del.y > 0f){
                mvState = MVSTATE.JUMPUP;
            } else {
                mvState = Mathf.Abs(del.x) < 1.1f ? MVSTATE.WALK : MVSTATE.JUMPDOWN;
            }
        };
        
        System.Action _AdjustToTarget = ()=>{
            // first check for overshoot state
            if((curWayPoint+2)<path.Count){
                Node trg = path[curWayPoint+1], ntrg = path[curWayPoint+2];
                Vector2 _dl = ntrg.position-trg.position;
                if(Mathf.Abs(_dl.y)<0.1f){
                    float _min, _max;
                    if(trg.position.x > ntrg.position.x){
                        _min = ntrg.position.x;
                        _max = trg.position.x;
                    }else{
                        _min = trg.position.x;
                        _max = ntrg.position.x;
                    }

                    if(current.x > _min && current.x < _max)
                        _DetermineNextState();
                    else if(Mathf.Abs(ntrg.position.x - current.x) < 0.6f*Mathf.Abs(trg.position.x - current.x))
                        _DetermineNextState();
                }
            }else{
                mvState = MVSTATE.WALK;
            }
        };

        if( Mathf.Abs(del.y) < .1f && Mathf.Abs(del.x) < 0.1f)
            _DetermineNextState();
        else if ( Mathf.Abs(del.y) < 1f && Mathf.Abs(del.x) < 1f)
            _AdjustToTarget();
        else if (mvState == MVSTATE.IDLE) mvState = MVSTATE.WALK;
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
