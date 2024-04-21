using UnityEngine;
using System.Collections;

public class Web : MonoBehaviour {
    [HideInInspector] public Vector3 targetPoint;
    [HideInInspector] public float targetLength{
        get{return spring.distance;}
        set{spring.distance=value;}
    }

    [SerializeField] int precision;
    [SerializeField] AnimationCurve offsetDir, offsetMag;
    public SpringJoint2D spring;
    private LineRenderer _lr;

    private Coroutine webSCor;

    void Awake(){
        _lr = GetComponent<LineRenderer>();
    }

    void OnEnable(){
        if(spring.connectedBody is not null){
            spring.connectedAnchor = Vector3.zero;
        }else{
            spring.connectedAnchor = targetPoint;
        }
        _lr.enabled = true;
        webSCor = StartCoroutine(ShootWeb());
    }

    IEnumerator ShootWeb(){
        _lr.positionCount = precision+1;
        Vector2 web = targetPoint - transform.position;
        float progress = 0f;

        while(progress < 1f){
            progress += 0.1f;
            SetWebPoints(progress, web);
            yield return new WaitForSeconds(0.01f);
        }
        spring.enabled = true;

        _lr.positionCount = 2;
        _lr.SetPosition(1, targetPoint);
    }

    void SetWebPoints(float progress, Vector2 web){
        Vector3 webVec = progress * web;
        Vector3 normal = new Vector3(-webVec.y, webVec.x, 0f);

        float t=0f;
        for(int i=0; i <= precision; i++){
            t = (float)i / precision;
            _lr.SetPosition(i, transform.position +webVec * t + normal * offsetMag.Evaluate(progress) * offsetDir.Evaluate(t)); 
        }
    }

    void OnDisable(){
        StopCoroutine(webSCor);
        _lr.enabled = false;
        spring.enabled = false;
    }

    void Update(){
        Draw();
    }

    void Draw(){
        _lr.SetPosition(0, transform.position);
        if(spring.connectedBody is not null)
            _lr.SetPosition(1, spring.connectedBody.transform.position);
    }
}
