using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Pathfinder : MonoBehaviour {
    public delegate void _OnPathUpdate();
    public _OnPathUpdate OnPathUpdate;

    [HideInInspector] public Node _lastNode = null, _lastTargetNode = null;
    [HideInInspector] public List<Node> path = null;
    [HideInInspector] public bool updateReady = false;

    public Transform target;
    [SerializeField] private float _refreshTime;
    float __rtimer = 0f;

    void Update(){
        if(__rtimer > 0f) {
            __rtimer -= Time.deltaTime;
        }else {
            updateReady = true;
        }
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.red;
        if(path is null) return;
        for(int i=0; i<path.Count-1; i++){
            Gizmos.DrawLine(path[i].position, path[i+1].position);
        }
    }

    public void UpdatePath(){
        if(!FindClosestNode(transform.position, ref _lastNode)) return;
        if(!FindClosestNode(target.position, ref _lastTargetNode)) return;

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        HashSet<Node> openSet = new HashSet<Node>();
        openSet.Add(_lastNode);

        Dictionary<Node, float> gValues = new Dictionary<Node, float>();
        foreach(Node n in GraphGenerator.instance.nodes.Values){
            gValues[n] = float.MaxValue;
        }
        gValues[_lastNode] = 0;

        Dictionary<Node, float> fValues = new Dictionary<Node, float>();
        foreach(Node n in GraphGenerator.instance.nodes.Values){
            fValues[n] = float.MaxValue;
        }
        fValues[_lastNode] = Vector2.Distance(_lastNode.position, _lastTargetNode.position);

        Node current;
        while(openSet.Count > 0){
            current = openSet.First();
            foreach(Node n in openSet){
                if(fValues[n] < fValues[current]) current = n;
            }

            if(current == _lastTargetNode){
                path = ReconstructPath(cameFrom, current);
                {
                    updateReady = false;
                    __rtimer = _refreshTime;
                    OnPathUpdate();
                }
                return;
            }
            openSet.Remove(current);

            foreach(Node n in current.neighbours){
                float tg = gValues[current] + Vector2.Distance(current.position, n.position);
                if(tg < gValues[n]){
                    cameFrom[n] = current;
                    gValues[n] = tg;
                    fValues[n] = tg + Vector2.Distance(n.position, _lastTargetNode.position);
                    if(!openSet.Contains(n)){
                        openSet.Add(n);
                    }
                }
            }
        }
    }

    List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current){
        List<Node> path = new List<Node>();
        path.Add(current);
        while(cameFrom.ContainsKey(current)){
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    bool FindClosestNode(Vector3 position, ref Node node){
        Vector2Int pos = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
        Vector2Int off = Vector2Int.zero;

        for(int d=0; d >= -5; d--){
            for(int w=0; w <= 5; w++){
                off.Set(w, d);
                if(GraphGenerator.instance.nodes.ContainsKey(pos + off)){
                    node = GraphGenerator.instance.nodes[pos+off];
                    return true;
                }
                if(w>0){ 
                    off.Set(-w, d);
                    if(GraphGenerator.instance.nodes.ContainsKey(pos + off)){
                        node = GraphGenerator.instance.nodes[pos+off];
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
