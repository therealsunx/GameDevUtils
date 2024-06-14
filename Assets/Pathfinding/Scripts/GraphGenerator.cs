using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class GraphGenerator : MonoBehaviour {

    public static GraphGenerator instance = null;

    [SerializeField] Tilemap tilemap;
    [HideInInspector] public Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();

    void OnDrawGizmos(){
        //GenerateGraph();
//      foreach(Node node in nodes.Values){
//          Gizmos.color = Color.green;
//          Gizmos.DrawSphere(node.position,  0.2f);
//          foreach(Node n in node.neighbours){
//              Gizmos.color = Color.grey;
//              Gizmos.DrawLine(node.position, n.position);
//          }
//      }
    }

    void Awake(){
        if(instance == null){
            instance = this;
            GenerateGraph();
        }
    }

    void GenerateGraph(){
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] tiles = tilemap.GetTilesBlock(bounds);

        GenerateNodes(bounds, tiles);
        FilterNodes();
        GenerateEdges(bounds);
        Debug.Log("Graph generated");
    }

    void FilterNodes(){
        nodes = nodes.Where(i => i.Value.type != 0).ToDictionary(i => i.Key, i=>i.Value);
    }

    void GenerateEdges(BoundsInt bounds){
        for(int y = bounds.y+1; y < bounds.yMax; y++){
            var nps = nodes.Keys.Where(k => k.y == y).OrderBy(i=>i.x).ToList();
            for(int i=0; i<nps.Count-1; i++){
                Node c = nodes[nps[i]];
                Node n = nodes[nps[i+1]];

                if((c.type & (uint)NodeType.RCRNR) > 0) continue;
                if((n.type & (uint)NodeType.LCRNR) > 0) continue;
                if((c.type & (uint)NodeType.REDGE) > 0 && (n.type & (uint)NodeType.LEDGE) > 0)
                    if((nps[i+1].x - nps[i].x) > 5) continue;

                c.neighbours.Add(n);
                n.neighbours.Add(c);
            }
        }
    }

    void GenerateNodes(BoundsInt bounds, TileBase[] tiles){
        Vector2Int pos = Vector2Int.zero;

        System.Action<int, int, uint> addNode = (int x, int y, uint type) => {
            pos.x = bounds.x+x;
            pos.y = bounds.y+y;
            nodes.TryAdd(pos, new Node(tilemap.CellToWorld((Vector3Int)pos) + tilemap.tileAnchor, type));
        };

        System.Action<int, int, bool> addNodeDepthCheck = (int x, int y, bool right)=>{
            bool exit = false;
            Vector2Int p = Vector2Int.zero, _p = new Vector2Int(bounds.x+x, bounds.y+y);
            for(int d=1; d <= 4; d++){
                exit = true;
                for(int w=1; w<=6-d/2; w++){
                    p.y = bounds.y+y-d; p.x = bounds.x + x+ (right?w:-w);
                    if(nodes.ContainsKey(p)){
                        nodes[p].neighbours.Add(nodes[_p]);
                        nodes[_p].neighbours.Add(nodes[p]);
                        if(nodes[p].type == (uint)NodeType.NONE) nodes[p].type = (uint)NodeType.JMPPT;
                        break;
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
                i += bounds.size.x;
                if(tiles[i]) continue; // check for ON-GROUND state

                uint type = 0;
                if(tiles[i-1]) type |= (uint) NodeType.LCRNR;
                if(tiles[i+1]) type |= (uint) NodeType.RCRNR;
                i -= bounds.size.x;
                if(!tiles[i-1]) type |= (uint) NodeType.LEDGE;
                if(!tiles[i+1]) type |= (uint) NodeType.REDGE;

                if((type & (uint) NodeType.REDGE) > 0) {
                    addNode(x, y+1, type);
                    addNodeDepthCheck(x, y+1, true);
                }
                if((type & (uint) NodeType.LEDGE) > 0) {
                    addNode(x, y+1, type);
                    addNodeDepthCheck(x, y+1, false);
                }
                if(type >= (uint)NodeType.LEDGE) continue;
                addNode(x, y+1, type);
            }
        }
    }
}
