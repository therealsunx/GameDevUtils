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
    NONE=0b0,
    LCRNR = 0b1,
    RCRNR = 0b10,
    LEDGE= 0b100,
    REDGE = 0b1000,
    JMPPT = 0b10000
}

public class Node {
    public Vector2 position;
    public List<Node> neighbours;
    public uint type;

    public Node(Vector2 _position, uint _type){
        position = _position;
        neighbours = new List<Node>();
        type = _type;
    }
}

