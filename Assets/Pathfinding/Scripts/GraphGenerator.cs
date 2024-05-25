using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GraphGenerator : MonoBehaviour {

    [SerializeField] Tilemap tilemap;
    Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();

    void OnDrawGizmos(){
        if(nodes.Count == 0) GenerateGraph();
        foreach(Node node in nodes.Values){
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(node.position, 0.2f);
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
        // GenerateEdges(bounds, tiles);
    }

    void GenerateEdges(BoundsInt bounds, TileBase[] tiles){
        // linked the edges on same platform which generating nodes
        // next step is to link the adjacent nodes
        foreach(Vector2Int k in nodes.Keys){
            Vector2Int nk = k;

            nk.x-=1;nk.y+=1;
            if(nodes.TryGetValue(nk, out Node n)) {
                nodes[k].neighbours.Add(n);
                n.neighbours.Add(nodes[k]);
            }
            nk.x += 2;
            if(nodes.TryGetValue(nk, out n)) {
                nodes[k].neighbours.Add(n);
                n.neighbours.Add(nodes[k]);
            }
        }
    }

    void GenerateNodes(BoundsInt bounds, TileBase[] tiles){
        Vector2Int pos = Vector2Int.zero;
        for(int y=0; y< bounds.size.y; y++){
            Node last = null;
            for(int x = 0; x < bounds.size.x; x++){
                if(x==0 || y == (bounds.size.y-1) || x == (bounds.size.x-1)) continue; // ignore edge layers

                int i = x + y * bounds.size.x;
                if(!tiles[i]){last = null; continue;} // only select cells on land

                i += bounds.size.x;
                if(tiles[i]) {last = null; continue;} // check for ON-GROUND state

                NodeType _type = NodeType.NONE;
                Node _l=null, _r =null;

                if(tiles[i-1] || tiles[i+1]) _type = NodeType.CORNER;
                
                int d = EdgeCheck(i-bounds.size.x-1, tiles, bounds);
                if(d >= 0){
                    _type = NodeType.EDGE;
                    pos.x = bounds.x+x-1; pos.y = bounds.y+y-d;
                    if(nodes.ContainsKey(pos)) _l = nodes[pos];
                    else {
                        _l = new Node(tilemap.CellToWorld((Vector3Int)pos)+tilemap.tileAnchor);
                        nodes[pos] = _l;
                    }
                }

                d = EdgeCheck(i-bounds.size.x+1, tiles, bounds);
                if(d >= 0){
                    _type = NodeType.EDGE;
                    pos.x = bounds.x+x+1; pos.y = bounds.y+y-d;
                    if(nodes.ContainsKey(pos)) _r = nodes[pos];
                    else {
                        _r = new Node(tilemap.CellToWorld((Vector3Int)pos)+tilemap.tileAnchor);
                        nodes[pos] = _r;
                    }
                }
                if(_type == NodeType.NONE) continue;

                pos.y=bounds.y+y+1; pos.x = bounds.x + x;
                nodes[pos] = new Node(tilemap.CellToWorld((Vector3Int) pos) + tilemap.tileAnchor);
                if(last is not null){
                    last.neighbours.Add(nodes[pos]);
                    nodes[pos].neighbours.Add(last);
                }
                if(_l is not null){
                    _l.neighbours.Add(nodes[pos]);
                    nodes[pos].neighbours.Add(_l);
                }
                if(_r is not null){
                    _r.neighbours.Add(nodes[pos]);
                    nodes[pos].neighbours.Add(_r);
                }
                last = nodes[pos];
            }
        }
    }

    int EdgeCheck(int i, TileBase[] tiles, BoundsInt bounds){
        if(tiles[i]) return -1;
        for(int d=0; d<6; d++){
            i -= bounds.size.x;
            if(i < 0) return -1;
            if(tiles[i]) return d;
        }
        return -1;
    }
}
