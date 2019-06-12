using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MainController : MonoBehaviour {

    

    void UpdateNode(GameObject obj)
    {
        //Takes the parents gameObject, Determines array position, and new position;
        GameObject mAdjObj = obj.transform.parent.gameObject;
        int numInArray = int.Parse(mAdjObj.transform.name);
        Vector3 newPos = mAdjObj.transform.localPosition;

        v[numInArray] = newPos;                         //Change the Vector array
        nodeArray[numInArray].pos = newPos;             //Change the Node struct pos
        UpdateNormal(numInArray);                     //Changes relevant normals
        CreateMesh();                                   //Redraws the triangles.
    }

    void UpdateNormal(int p)
    {
        Node n = nodeArray[p];       
        //Recalc normals of all neighbor nodes
        for (int i = 0; i< n.neighbors.Length; i++)
        {
            int loc = n.neighbors[i];
            Node neigh = nodeArray[loc];
            CalculateAvgNormal(loc);
            UpdateNormalLines();
        }
    }

    void CalculateAvgNormal(int p)
    {
        Node node = nodeArray[p];
        int numNeighbors = node.neighbors.Length;
        Vector3 sumVector = new Vector3(0, 0, 0);

        //2 neighbors =     a = self; b = n[0]; c = n[1]
        if (numNeighbors == 2)
        {
           sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[0]].pos, nodeArray[node.neighbors[1]].pos);
            node.norm = sumVector;      //change the Node stuct
            nodeArray[p] = node;
            n[p] = sumVector;           //change the normal array
        }

        //3 neighbors =     a = self; b = n[0]; c = n[1]
        //                  a = self; b = n[1]; c = n[2]
        if (numNeighbors == 3)
        {

            sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[0]].pos, nodeArray[node.neighbors[1]].pos);
            sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[1]].pos, nodeArray[node.neighbors[2]].pos);
            sumVector = sumVector / 2;
            node.norm = sumVector;      //change the Node stuct
            nodeArray[p] = node;
            n[p] = sumVector;           //change the normal array
        }

        //4 neighbors =     a = self; b = n[0]; c = n[1]
        //                  a = self; b = n[1]; c = n[2]
        //                  a = self; b = n[2]; c = n[3]
        if (numNeighbors == 4)
        {
            sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[0]].pos, nodeArray[node.neighbors[1]].pos);
            sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[1]].pos, nodeArray[node.neighbors[2]].pos);
            sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[2]].pos, nodeArray[node.neighbors[3]].pos);
            sumVector = sumVector / 3;
            node.norm = sumVector;      //change the Node stuct
            nodeArray[p] = node;
            n[p] = sumVector;           //change the normal array
        }

        //6 neighbors =     a = self; b = n[0]; c = n[1]
        //                  a = self; b = n[1]; c = n[2]
        //                  a = self; b = n[2]; c = n[3]
        //                  a = self; b = n[3]; c = n[4]
        //                  a = self; b = n[4]; c = n[5]
        //                  a = self; b = n[5]; c = n[0]
        if (numNeighbors == 6)
        {
            sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[0]].pos, nodeArray[node.neighbors[1]].pos);
            sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[1]].pos, nodeArray[node.neighbors[2]].pos);
            sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[2]].pos, nodeArray[node.neighbors[3]].pos);
            sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[3]].pos, nodeArray[node.neighbors[4]].pos);
            sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[4]].pos, nodeArray[node.neighbors[5]].pos);
            sumVector += CaclulateOneNormal(node.pos, nodeArray[node.neighbors[5]].pos, nodeArray[node.neighbors[0]].pos);
            sumVector = sumVector / 6;
            node.norm = sumVector;      //change the Node stuct
            nodeArray[p] = node;
            n[p] = sumVector;           //change the normal array
        }
    }


    Vector3 CaclulateOneNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 l, r;
        l = b - a;
        r = c - a;
        Vector3 norm, dir;
        dir = Vector3.Cross(l, r);
        norm = Vector3.Normalize(dir);
        return norm;
    }

    void UpdateNormalLines()
    {
        //Debug.Log("update normal lines called");
        for (int i = 0; i<nodeArray.Length; i++)
        {
            Vector3 startPos, endPos, V, scale;
            GameObject nLine = nodeArray[i].normalLine;
            startPos = nodeArray[i].pos;
            endPos = startPos + nodeArray[i].norm * 3;

            V = endPos - startPos;
            float vMagnitude = V.magnitude;
            float size = vMagnitude / 2;
            scale = nLine.transform.localScale;
            scale.y = size;

            nLine.transform.localScale = scale;
            nLine.transform.localRotation = Quaternion.FromToRotation(Vector3.up, V.normalized);
            nLine.transform.localPosition = (startPos + endPos) / 2;
        }
    }
}
