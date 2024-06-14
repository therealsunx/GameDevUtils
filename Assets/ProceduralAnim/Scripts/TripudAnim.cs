using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripudAnim : MonoBehaviour {

    [SerializeField] LegHandler[] legs;
    [Range(0f, 40f)]
    [SerializeField] float kp, kd;

    private int groundLayer = 1 << 6;
    private float _targetRange = 2f, _prevDiff = 0f;
    private Rigidbody2D _rigidbody;
    private Vector2 _velocity;
    private bool _right = true;

    void Start(){
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update(){
        _velocity = _rigidbody.velocity;
        if(Input.GetKeyDown(KeyCode.Space)) _velocity.y = 18f;
        _velocity.x = Input.GetAxis("Horizontal") * 3f;

        if(_velocity.x != 0f && (_velocity.x > 0f ^ _right)){
            _right = !_right;
            transform.Rotate(Vector3.up * 180f);
        }

        RaycastHit2D _res1 = Physics2D.Raycast(transform.position - transform.right*.3f, Vector3.down, _targetRange*2f, groundLayer);
        RaycastHit2D _res2 = Physics2D.Raycast(transform.position + transform.right*.3f, Vector3.down, _targetRange*2f, groundLayer);

        float trg = _targetRange, dis = 0f;
        if(_res1.collider == null) {
            trg -= _targetRange*0.5f;
        }else dis += _res1.distance;
        if(_res2.collider == null){
            trg -= _targetRange*0.5f;
        }else dis += _res2.distance;
        dis *= 0.5f;

        if(dis>0.1f){
            float d = trg - dis;
            _rigidbody.gravityScale = -kp * d/_targetRange - kd*(d - _prevDiff)/_targetRange;
            _prevDiff = d;
        }else{
            _rigidbody.gravityScale = 1f;
        }

        foreach(LegHandler leg in legs){
            leg.UpdateFoot(_velocity, _res1.collider != null, _right);
        }
    }

    void LateUpdate(){
        _rigidbody.velocity = _velocity;
    }
}
