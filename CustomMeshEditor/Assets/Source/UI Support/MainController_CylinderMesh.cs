using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MainController : MonoBehaviour {

    

	void CylinderMeshSetResolution(float rowRes, float colRes)
    {
        if (mMeshDropdown.value == 0)
            return;
       
        DestructScalerObjects();
        mMesh.Clear();

        rowResolution = (int)rowRes;
        colResolution = (int)colRes;
        colIncrement = height / (colResolution - 1);
        rowIncrement = circumference / (rowResolution - 1);
        v = new Vector3[rowResolution * colResolution];
        nodeArray = new Node[rowResolution * colResolution];
        uv = new Vector2[rowResolution * colResolution];

        int counter = 0;
        for (int i = 0; i < colResolution; i++)
        {
             for (int j = 0; j < rowResolution; j++){
                //Create Node here
                mSphere = Resources.Load("PrefabObjects/AdjNode") as GameObject;
                GameObject newSphere = GameObject.Instantiate(mSphere);
                if (j == 0)
                {
                    newSphere.tag = "mAdjustable";
                    newSphere.GetComponent<MeshRenderer>().material.color = new Color(255, 255, 255);
                }
                else
                {
                    newSphere.tag = "mNonAdjustable";
                }
                float rad = DegreeToRadian(j * rowIncrement);
                Vector3 pos = new Vector3(radius * Mathf.Cos(rad), i * colIncrement, radius * Mathf.Sin(rad));
                newSphere.transform.localPosition = pos;
                newSphere.name = counter.ToString();   //May be redundant

                //Normal Line
                mNormalLine = Resources.Load("PrefabObjects/NormalLine") as GameObject;
                GameObject newLine = GameObject.Instantiate(mNormalLine);

                //Set node struct
                Node n = new Node();
                n.adjNode = newSphere;
                n.pos = pos;
                n.vectorNum = counter;
                n.norm = new Vector3(0, 1, 0);  // not applicable for cylinder, recalculate normal after mesh creation
                n.normalLine = newLine;
                nodeArray[counter] = n;

                //populate the arrays
                PopulateNeighborArray(n, rowResolution, colResolution, counter);
                PopulateVectorArray(pos, counter);
                counter++;                
            }
        }
        PopulateNormalArray();
        UpdateAllNormals();
        PopulateTriangleArray();
        CreateMesh();
        UpdateNormalLines();
        UpdateNodeOrientation();
        HideAll();
        //TestNeighbor();
    }

    void UpdateNodeOrientation()
    {
        for (int i = 0; i < nodeArray.Length; i++)
        {
            Vector3 nRot;
            nodeArray[i].adjNode.transform.forward = nodeArray[i].norm;
            nRot = nodeArray[i].adjNode.transform.localEulerAngles;
            nRot.y -= 90;
            nodeArray[i].adjNode.transform.localEulerAngles = nRot;
        }
    }
    void CylinderRotationUpdater(float f)
    {
        circumference = (int)f;
        CylinderMeshSetResolution(rowResolution, colResolution);
    }

    void UpdateCylinderMeshNode(GameObject obj, float d)
    {
        GameObject mAdjObj = obj.transform.parent.gameObject;
        int numInArray = int.Parse(mAdjObj.transform.name);
        Vector3 zVec, yVec, xVec = new Vector3(0, 0, 0);

        //Determine Row
        int row = (numInArray / rowResolution);
                   
        //Cycle all within that row and change
        for (int i = row*rowResolution; i < (row*rowResolution) + rowResolution; i++)
        {
            Vector3 nPos = nodeArray[i].pos;
            GameObject n = nodeArray[i].adjNode;
            if (obj.name == "zArm")
            {               
                Transform c = n.gameObject.transform.Find("zArm");
                zVec = c.transform.up;
                nPos += zVec * d;
            }
            if (obj.name == "yArm")
            {
                Transform c = n.gameObject.transform.Find("yArm");
                yVec = c.transform.up;
                nPos += yVec * d;
            }
            if (obj.name == "xArm")
            {
                Transform c = n.gameObject.transform.Find("xArm");
                xVec = c.transform.up;
                nPos -= xVec * d;
            }

            v[i] = nPos;
            nodeArray[i].pos = nPos;
            nodeArray[i].adjNode.transform.localPosition = nPos;
            UpdateNormal(i);
        }
        CreateMesh();
    }

    float DegreeToRadian(float d)
    {
        float r;
        r = d * (Mathf.PI / 180);
        return r;
    }
}
