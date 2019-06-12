using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MainController : MonoBehaviour {

    void CallFlatSetRow(float rs)
    {
        FlatMeshSetResolution(rs, colResolution);
    }

    void CallFlatSetCol(float cs)
    {
        FlatMeshSetResolution(rowResolution, cs);
    }

    void FlatMeshSetResolution(float rs, float cs)
    {
        if (mMeshDropdown.value == 1)
            return;

        
        DestructScalerObjects();
        mMesh.Clear();

        rowResolution = (int)rs;                                              // s = value recieved from slider (whole ints only)
        colResolution = (int)cs;
        rowIncrement = totalSize / (rowResolution - 1);                         //Scaling increment
        colIncrement = totalSize / (colResolution - 1);
        v = new Vector3[rowResolution * colResolution];                               //Vectors    
        nodeArray = new Node[rowResolution * colResolution];
        uv = new Vector2[rowResolution * colResolution];

        //Algorithm to handle density changes, populates all arrays.
        int counter = 0;
        for (int i = 0; i < colResolution; i++)
        {
            for (int j = 0; j < rowResolution; j++)
            {
                //Create Scaleable nodes here
                mSphere = Resources.Load("PrefabObjects/AdjNode") as GameObject;
                GameObject newSphere = GameObject.Instantiate(mSphere);
                newSphere.tag = "mAdjustable";
                Vector3 pos = new Vector3(j * rowIncrement, 0, i * colIncrement);
                newSphere.transform.localPosition = pos;
                newSphere.name = counter.ToString();  //May be redundant

                //Create Normal Line here
                mNormalLine = Resources.Load("PrefabObjects/NormalLine") as GameObject;
                GameObject newLine = GameObject.Instantiate(mNormalLine);


                //Sets the values within the node stuct
                Node n = new Node();
                n.adjNode = newSphere;
                n.pos = pos;
                n.vectorNum = counter;
                n.norm = new Vector3(0, 1, 0);
                n.normalLine = newLine;
                nodeArray[counter] = n;

                //Populates the arrays
                PopulateNeighborArray(n, rowResolution, colResolution, counter);
                PopulateVectorArray(pos, counter);
                counter++;
            }
        }
        //Populate the final arrays + creates mesh
        PopulateNormalArray();
        PopulateTriangleArray();

        CreateMesh();
        UpdateNormalLines();
        HideAll();                      //turns the nodes invisable by default
        //TestNeighbor();               //For testing purposes
    }

    void UpdateFlatMeshNode(GameObject obj)
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


}
