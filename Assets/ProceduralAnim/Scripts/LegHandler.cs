using UnityEngine;
using UnityEngine.U2D;

public class LegHandler : MonoBehaviour {

    public static int steppableLayer = 1 << 6;

    [SerializeField] private bool _isBackLeg = false;
    [SerializeField] private float _height = 2f, _maxLen = 3f;
    [SerializeField] private Vector3 off = new Vector2(-1f, 0.3f), idlePos;

    Spline _spline;

    void Start(){
        _spline = GetComponent<SpriteShapeController>().spline;
        idlePos = _spline.GetPosition(2);
    }

    void Update(){
        //UpdateFoot(Vector2.right * 2f);
    }

    public void UpdateFoot(Vector2 vel, bool grounded, bool facingRight){
        Vector2 foot = _spline.GetPosition(2);
        if(grounded){
            Collider2D _res = Physics2D.OverlapCircle((Vector2)transform.position + foot, 0.05f, steppableLayer); 
            if(_res != null){
                if(vel.x < 0f) vel.x = -vel.x;
                foot -= vel * Time.deltaTime;
                if(foot.magnitude > _maxLen){
                    RaycastHit2D _rcst = Physics2D.Raycast(transform.position, idlePos, _maxLen, steppableLayer);
                    if(_rcst.collider != null){
                        foot = _rcst.point - (Vector2) transform.position;
                    }else foot = Vector2.down * _height;
                }
            }else{
                RaycastHit2D _rcst = Physics2D.Raycast(transform.position, idlePos, _height, steppableLayer);
                if(_rcst.collider != null){
                    foot = _rcst.point - (Vector2) transform.position;
                }else foot = Vector2.down * _height;
            }
        }
        PlaceFoot(foot);
    }

    void PlaceFoot(Vector2 pos){
        Vector2 p1 = Vector2.Perpendicular(_isBackLeg?-pos:pos).normalized * (_maxLen - pos.magnitude) + pos/2f;
        _spline.SetPosition(1, p1);
        _spline.SetPosition(2, pos);
        _spline.SetRightTangent(0, p1*0.2f);
        _spline.SetRightTangent(1, pos*0.2f);
        _spline.SetLeftTangent(1, -pos*0.2f);
        _spline.SetLeftTangent(2, (p1 - pos)*0.5f);
    }
}
