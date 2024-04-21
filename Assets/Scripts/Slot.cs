using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour {

    Image image;
    GameObject heldObject;

    void Awake(){
        image = GetComponent<Image>();
    }

    public void Throw(Vector2 force, Vector2 throwPoint){
        if(heldObject is null) return;

        heldObject.SetActive(true);
        heldObject.transform.position = throwPoint;
        heldObject.GetComponent<Rigidbody2D>().AddForce(force);
        image.sprite = null;
        heldObject = null;
    }

    public void AddObject(GameObject _object){
        if(heldObject is not null){
            heldObject.SetActive(true);
            heldObject.transform.position = _object.transform.position;
        }

        heldObject = _object;
        image.sprite = heldObject.GetComponent<SpriteRenderer>().sprite;
        heldObject.SetActive(false);
    }
}
