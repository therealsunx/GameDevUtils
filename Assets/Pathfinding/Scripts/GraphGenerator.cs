using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class GraphGenerator : MonoBehaviour {

    [SerializeField] Tilemap tilemap;
    Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();

    void OnDrawGizmos(){
        if(nodes.Count == 0) GenerateGraph();
        foreach(Node node in nodes.Values){
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(node.position, node.type == NodeType.NONE? 0.1f:0.2f);
            foreach(Node n in node.neighbours){
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(node.position, n.position);
            }
        }
    }

    void GenerateGraph(){

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] tiles = tilemap.GetTilesBlock(bounds);

        GenerateNodes(bounds, tiles);
        FilterNodes();
        GenerateEdges(bounds);
    }

    void FilterNodes(){
        nodes = nodes.Where(i => i.Value.type != NodeType.NONE).ToDictionary(i => i.Key, i=>i.Value);
    }

    void GenerateEdges(BoundsInt bounds){
        for(int y = bounds.y+1; y < bounds.yMax; y++){
            var nps = nodes.Keys.Where(k => k.y == y).OrderBy(i=>i.x).ToList();
            bool brk = true;
            for(int i=0; i<nps.Count; i++){
                if(brk) brk = false;
                else {
                    nodes[nps[i]].neighbours.Add(nodes[nps[i-1]]);
                    nodes[nps[i-1]].neighbours.Add(nodes[nps[i]]);
                    if(nodes[nps[i]].type != NodeType.WAYPOINT) brk = true;
                }
            }
        }
    }

    void GenerateNodes(BoundsInt bounds, TileBase[] tiles){
        Vector2Int pos = Vector2Int.zero;

        System.Action<int, int, NodeType> addNode = (int x, int y, NodeType type) => {
            pos.x = bounds.x+x;
            pos.y = bounds.y+y;
            nodes.TryAdd(pos, new Node(tilemap.CellToWorld((Vector3Int)pos) + tilemap.tileAnchor, type));
        };

        System.Action<int, int, bool> addNodeDepthCheck = (int x, int y, bool right)=>{
            addNode(x,y, NodeType.EDGE);
            bool exit = false;
            Vector2Int p = Vector2Int.zero, _p = new Vector2Int(bounds.x+x, bounds.y+y);
            for(int d=1; d <= 4; d++){
                exit = true;
                int n = 3;
                for(int w=0; w<=5-d; w++){
                    p.y = bounds.y+y-d; p.x = bounds.x + x+ (right?w:-w);
                    if(n<=0) break;
                    if(nodes.ContainsKey(p)){
                        nodes[p].neighbours.Add(nodes[_p]);
                        nodes[_p].neighbours.Add(nodes[p]);
                        if(nodes[p].type == NodeType.NONE) nodes[p].type = NodeType.WAYPOINT;
                        n--;
                    }else exit = false;
                }
                if(exit) break;
            }
        };

        for(int y=0; y< bounds.size.y; y++){
            for(int x = 0; x < bounds.size.x; x++){
                if(x==0 || y == (bounds.size.y-1) || x == (bounds.size.x-1)) continue; // ignore edge layers

                int i = x + y * bounds.size.x;
                if(!tiles[i])continue; // only select cells on land

                if(tiles[i+bounds.size.x]) continue; // check for ON-GROUND state
                bool skip = false;
                if(!tiles[i+1]) {
                    addNodeDepthCheck(x+1, y+1, true);
                    addNode(x, y+1, NodeType.WAYPOINT);
                    skip = true;
                }
                if(!tiles[i-1]) {
                    addNodeDepthCheck(x-1, y+1, false);
                    addNode(x, y+1, NodeType.WAYPOINT);
                    skip = true;
                }
                if(skip) continue;

                i += bounds.size.x;
                if(tiles[i+1] || tiles[i-1]) addNode(x, y+1, NodeType.CORNER);
                else addNode(x,y+1, NodeType.NONE);
            }
        }
    }
}
