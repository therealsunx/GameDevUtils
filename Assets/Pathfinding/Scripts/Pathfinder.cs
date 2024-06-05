using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Pathfinder : MonoBehaviour {
    public Transform seeker, target;

    [SerializeField] GraphGenerator graph;

    public List<Node> FindPath(Transform seeker, Transform target){
        Node start, finish;
        if(!FindClosestNode(seeker.position, out start)) return null;
        if(!FindClosestNode(target.position, out finish)) return null;

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        HashSet<Node> openSet = new HashSet<Node>();
        openSet.Add(start);

        Dictionary<Node, float> gValues = new Dictionary<Node, float>();
        foreach(Node n in graph.nodes.Values){
            gValues[n] = float.MaxValue;
        }
        gValues[start] = 0;

        Dictionary<Node, float> fValues = new Dictionary<Node, float>();
        foreach(Node n in graph.nodes.Values){
            fValues[n] = float.MaxValue;
        }
        fValues[start] = Vector2.Distance(start.position, finish.position);

        Node current;
        while(openSet.Count > 0){
            current = openSet.First();
            foreach(Node n in openSet){
                if(fValues[n] < fValues[current]) current = n;
            }

            if(current == finish) return ReconstructPath(cameFrom, current);
            openSet.Remove(current);

            foreach(Node n in current.neighbours){
                float tg = gValues[current] + Vector2.Distance(current.position, n.position);
                if(tg < gValues[n]){
                    cameFrom[n] = current;
                    gValues[n] = tg;
                    fValues[n] = tg + Vector2.Distance(n.position, finish.position);
                    if(!openSet.Contains(n)){
                        openSet.Add(n);
                    }
                }
            }
        }
        Debug.Log("nooo");
        return null;
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

    void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Node _seek, _trgt;
        if(FindClosestNode(seeker.position, out _seek)) Gizmos.DrawSphere(_seek.position, 0.2f);
        if(FindClosestNode(target.position, out _trgt)) Gizmos.DrawSphere(_trgt.position, 0.2f);

        List<Node> path = FindPath(seeker, target);
        if(path.Count == 0) return;
        for(int i=0; i < path.Count-1; i++)
            Gizmos.DrawLine((Vector3) path[i].position, (Vector3) path[i+1].position);
    }

    bool FindClosestNode(Vector3 position, out Node node){
        Vector2Int pos = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
        Vector2Int off = Vector2Int.zero;

        for(int d=0; d >= -5; d--){
            for(int w=0; w <= 5; w++){
                off.Set(w, d);
                if(graph.nodes.ContainsKey(pos + off)){
                    node = graph.nodes[pos+off];
                    return true;
                }
                if(w>0){ 
                    off.Set(-w, d);
                    if(graph.nodes.ContainsKey(pos + off)){
                        node = graph.nodes[pos+off];
                        return true;
                    }
                }
            }
        }
        node = null;
        return false;
    }
}
