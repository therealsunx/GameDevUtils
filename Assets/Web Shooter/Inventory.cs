using UnityEngine;

public class Inventory : MonoBehaviour {
    public Slot[] slots;
    [SerializeField] Transform selectFrame;
    [SerializeField] float pickUpRange;
    [SerializeField] LayerMask pickUpLayer;

    [HideInInspector] public int selectedSlot = 0;
    PlayerController playerController;

    void Start(){
        playerController = GetComponent<PlayerController>();
        selectFrame.position = slots[selectedSlot].transform.position;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.G))
            PickUpObject();
        if(Input.GetKeyDown(KeyCode.Q))
            slots[selectedSlot].Throw(playerController.facingDir * playerController.throwForce, transform.position);
    }

    void PickUpObject(){
        Collider2D _res = Physics2D.OverlapCircle(transform.position, pickUpRange, pickUpLayer);
        if(_res is null) return;

        slots[selectedSlot].AddObject(_res.gameObject);
    }

    public void SelectSlot(int i){
        selectedSlot = i;
        selectFrame.position = slots[selectedSlot].transform.position;
    }
}
