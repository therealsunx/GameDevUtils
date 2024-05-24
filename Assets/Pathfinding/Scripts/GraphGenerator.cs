using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GraphGenerator : MonoBehaviour {

    [SerializeField] Tilemap tilemap;
    List<Node> nodes = new List<Node>();


    void OnDrawGizmos(){
        if(nodes.Count == 0) GenerateNodes();
        foreach(Node node in nodes){
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(node.position, 0.2f);
            foreach(Node n in node.neighbours){
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(node.position, n.position);
            }
        }
    }

    void GenerateNodes(){
        // Gizmos.color = Color.green;
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] tiles = tilemap.GetTilesBlock(bounds);

        Vector3Int pos = Vector3Int.zero;
        Node last;

        for(int y=0; y< bounds.size.y; y++){
            last = null;
            for(int x = 0; x < bounds.size.x; x++){
                if(x==0 || y == (bounds.size.y-1) || x == (bounds.size.x-1)) continue; // ignore edge layers

                int i = x + y * bounds.size.x;
                if(!tiles[i]) {
                    last = null;
                    continue; // only select cells on land
                }

                i += bounds.size.x;
                if(tiles[i]){
                    last = null;
                    continue; // check for ON-GROUND state
                }

                pos.y=bounds.y+y+1; pos.x = bounds.x + x;
                // Gizmos.DrawSphere(tilemap.CellToWorld(pos)+tilemap.tileAnchor, 0.1f);

                if(!(tiles[i-1] || tiles[i+1])){
                    i -= bounds.size.x;
                    if(tiles[i-1] && tiles[i+1]) continue;
                }

                Node n = new Node(tilemap.CellToWorld(pos) + tilemap.tileAnchor);
                nodes.Add(n);

                if(last is not null){
                    // Gizmos.DrawLine((Vector3)last?.position, n.position);
                    n.neighbours.Add(last);
                    last.neighbours.Add(n);
                }
                last = n;
                // Gizmos.DrawSphere(tilemap.CellToWorld(pos)+tilemap.tileAnchor, 0.2f);
            }
        }
    }
}
