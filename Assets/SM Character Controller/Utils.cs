using System.Collections.Generic;
using UnityEngine;

public enum STATE{
    IDLE = 0,
    WALKING = 0b1,
    RUNNING = 0b10,
    DASHING = 0b100,
    JUMPING = 0b1000,
    CLIMBING = 0b10000,
    SWIMMING = 0b100000
}

public enum NodeType {
    NONE=0, EDGE, CORNER
}

public class Node {
    public Vector2 position;
    public List<Node> neighbours;

    public Node(Vector2 _position){
        position = _position;
        neighbours = new List<Node>();
    }
}

