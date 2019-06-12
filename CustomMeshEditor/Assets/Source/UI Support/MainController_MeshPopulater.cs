using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public partial class MainController : MonoBehaviour
{   

    //For all meshes
    public GameObject mMeshObj;                  //Object with mesh to morph
    public GameObject mNormalLine;
    Mesh mMesh;                                                 //Mesh to morph
    GameObject mSphere;                                         //Clone "Adjustable nodes"
    public Node[] nodeArray;                        //Array of all Adjustable nodes
    float totalSize = 40;                    //Scale of Mesh object
    float rowIncrement;                         //Used to measure displace between Nodes
    float colIncrement;
    int rowResolution;                                //Indicates the mesh density 3x3, 4x4 ect
    int colResolution;                                
    Vector3[] v;                             //Vector array for mesh creation
    Vector3[] n;                             //Normal array for mesh creation
    int[] t;                                 //Triangle array for mesh creation

    //For cylinder Mesh
    float radius = 20;                       //radius of cylinder
    float height = 40;                       //Height of cylinder
    //float heightIncrement;
    float circumference;
    //float columnIncrement;                   //Used for cylinder "crunch"

    //UV stuff
    public static Vector2 Offset = Vector2.zero;
    public static Vector2 Scale = Vector2.one;
    public static float Rotation = 0f;
    Vector2[] uv;            //Vector array for texture scaling


    //To keep track of normal / and vec number
    public struct Node
    {
        public GameObject adjNode;
        public GameObject normalLine;
        public int vectorNum;                //Slighty redunant, but keeps tracks of position within vector array, and node array
        public Vector3 pos;
        public Vector3 norm;
        public int[] neighbors;              //All neighbor nodes for normal cacluculations
    }

    void Start()
    {
        mMeshResolutionRowSlider.SetSliderListener(CallSetRow);
        mMeshResolutionColSlider.SetSliderListener(CallSetCol);
        mCylinderRotationSlider.SetSliderListener(CylinderRotationUpdater);
        mMesh = mMeshObj.GetComponent<MeshFilter>().mesh;
        mMesh.Clear();
        rowResolution = 10;
        colResolution = 10;
        FlatMeshSetResolution(10, 10);                 //initial size, can be changed 
        circumference = 270;

        //Add listener for when the value of the Dropdown changes, to take action
        mMeshDropdown.onValueChanged.AddListener(delegate {
            ChangeMesh(mMeshDropdown);
        });
    }

    void CallSetRow(float rs)
    {
        if (mMeshDropdown.value == 0)
            FlatMeshSetResolution(rs, colResolution);

        if (mMeshDropdown.value == 1)
            CylinderMeshSetResolution(rs, colResolution);
    }

    void CallSetCol(float cs)
    {
        if (mMeshDropdown.value == 0)
            FlatMeshSetResolution(rowResolution, cs);

        if (mMeshDropdown.value == 1)
            CylinderMeshSetResolution(rowResolution, cs);
    }

    void ChangeMesh(Dropdown d)
    {
        if (d.value == 0)
        {
            FlatMeshSetResolution(rowResolution, colResolution);
            MainCamera.transform.localPosition = new Vector3(20, 22, -2);
            MainCamera.transform.eulerAngles = new Vector3(57, 0, 0);
            mCameraLookAt.transform.localPosition = new Vector3(20, 3, 10);

            mMeshResolutionColSlider.InitSliderRange(2, 20, colResolution);
            mMeshResolutionRowSlider.InitSliderRange(2, 20, rowResolution);
        }
        if (d.value == 1)
        {
            CylinderMeshSetResolution(rowResolution, colResolution);
            MainCamera.transform.localPosition = new Vector3(45, 27, 0);
            MainCamera.transform.eulerAngles = new Vector3(14, -78, 0);
            mCameraLookAt.transform.localPosition = new Vector3(-4, 14, 0);

            if (colResolution < 4)
                colResolution = 4;
            if (rowResolution < 4)
                rowResolution = 4;
            mMeshResolutionColSlider.InitSliderRange(4, 20, colResolution);
            mMeshResolutionRowSlider.InitSliderRange(4, 20, rowResolution);

        }
    }

    void CreateMesh()
    {
        mMesh.vertices = v;
        mMesh.triangles = t;
        mMesh.normals = n;
    }

    void TestNeighbor()
    {
        //for (int i = 0; i < nodeArray.Length; i++)
        //{
        //    Node n = new Node();
        //    n = nodeArray[i];
        //    if (n.neighbors == null)
        //    {
        //        Debug.Log("neighbor array is null");
        //    }
        //    //Debug.Log(n.neighbors.Length);
        //    Debug.Log("Node number: " + i);
        //    for (int j = 0; j < n.neighbors.Length; j++)
        //    {
        //        Debug.Log("Neighbors: " + n.neighbors[j]);
        //    }
        //}
        for (int i = 0; i < nodeArray.Length; i++)
        {
            Debug.Log("Node number: " + i);
            Debug.Log("neigh Size: " + nodeArray[i].neighbors.Length);
        }
    }

    void PopulateNeighborArray(Node n, int rowRes, int colRes, int c)
    {
        int BL = 0;
        int BR = rowRes - 1;
        int TL = rowRes * (colRes - 1);
        int TR = TL + (rowRes - 1);
        //Determines which case to be filled
        //Corners
        if (c == BL || c == BR || c == TL || c == TR)
        {
            PopulateCornerCases(n, rowRes, colRes, c);
            return;
        }
        //Bottom edge
        if (c > 0 && c < rowRes - 1)
        {
            PopulateEdgeCases(n, rowRes, colRes, c, 1);
            return;
        }
        //Top Edge
        if (c > TL && c < TL + (rowRes - 1)) 
        {
            PopulateEdgeCases(n, rowRes, colRes, c, 4);
            return;
        }
        //left column or right column
        for (int i = 1; i < colRes - 1; i++)
        {
            if (c == (rowRes * i))
            {
                PopulateEdgeCases(n, rowRes, colRes, c, 2);
                return;
            }
            if (c == (rowRes * i) + (rowRes - 1))
            {
                PopulateEdgeCases(n, rowRes, colRes, c, 3);
                return;
            }
        }
        //If reached this point w/o returning, its a center node
        PopulateCenterCases(n, rowRes, colRes, c);
    }

    void PopulateCornerCases(Node n, int rowRes, int colRes, int c)
    {
        //Debug.Log("CornerCase called");

        //Bottom Left
        if (c == 0)
        {
            n.neighbors = new int[2];
            n.neighbors[1] = 1;
            n.neighbors[0] = rowRes;
        }

        //Bottom Right
        if (c == rowRes - 1)
        {
            n.neighbors = new int[3];
            n.neighbors[0] = c - 1;
            n.neighbors[1] = 2 * rowRes - 2;
            n.neighbors[2] = 2 * rowRes - 1;
        }

        //Top Left
        if (c == rowRes * (colRes - 1))
        {
            n.neighbors = new int[3];
            n.neighbors[0] = c + 1;
            n.neighbors[1] = c - rowRes + 1;
            n.neighbors[2] = c - rowRes;
        }

        //Top Right
        if (c == (rowRes * (colRes - 1)) + (rowRes - 1))
        {
            n.neighbors = new int[2];
            n.neighbors[1] = c - 1;
            n.neighbors[0] = c - rowRes;
        }
        nodeArray[c] = n;
    }

    void PopulateCenterCases(Node n, int rowRes, int colRes, int c)
    {
        //Debug.Log("CenterCase called");
        n.neighbors = new int[6];
        n.neighbors[5] = c - 1;
        n.neighbors[4] = c - rowRes;
        n.neighbors[3] = c - rowRes + 1;
        n.neighbors[2] = c + 1;
        n.neighbors[1] = c + rowRes;
        n.neighbors[0] = c + rowRes - 1;
        nodeArray[c] = n;
    }

    void PopulateEdgeCases(Node n, int rowRes, int colRes, int c, int side)
    {
        //Debug.Log("EdgeCase called");
        n.neighbors = new int[4];
        //side 1 = bottom
        if (side == 1)
        {
            n.neighbors[3] = c + 1;
            n.neighbors[2] = c + rowRes;
            n.neighbors[1] = c + rowRes - 1;
            n.neighbors[0] = c - 1;

        }
        //side 2 = left edge
        if (side == 2)
        {
            n.neighbors[3] = c - rowRes;
            n.neighbors[2] = c - rowRes + 1;
            n.neighbors[1] = c + 1;
            n.neighbors[0] = c + rowRes;
        }
        //side 3 = right edge
        if (side == 3)
        {
            n.neighbors[3] = c + rowRes;
            n.neighbors[2] = c + rowRes - 1;
            n.neighbors[1] = c - 1;
            n.neighbors[0] = c - rowRes;
        }
        //side 4 = TOP
        if (side == 4)
        {
            n.neighbors[3] = c - 1;
            n.neighbors[2] = c - rowRes;
            n.neighbors[1] = c - rowRes + 1;
            n.neighbors[0] = c + 1;
        }
        nodeArray[c] = n;
    }

    void PopulateVectorArray(Vector3 pos, int c)
    {
        v[c] = pos;
    }

    void PopulateTriangleArray()
    {
        int tSize = ((rowResolution - 1) * (colResolution - 1) * 6);
        t = new int[tSize];               //Triangles
        int arrayCounter = 0;
        int counter = 0;
        int colNum = 1;
        while (arrayCounter != t.Length)
        {

            t[arrayCounter] = counter; arrayCounter++;
            t[arrayCounter] = counter + rowResolution; arrayCounter++;
            t[arrayCounter] = counter + 1; arrayCounter++;

            t[arrayCounter] = counter + 1; arrayCounter++;
            t[arrayCounter] = counter + rowResolution; arrayCounter++;
            t[arrayCounter] = counter + rowResolution + 1; arrayCounter++;

            if (counter == rowResolution * colNum - 2)
            {
                counter += 2;
                colNum++;
            }
            else
                counter++;
        }
    }

    void PopulateNormalArray()
    {
        n = new Vector3[rowResolution * colResolution];
        for (int i = 0; i < rowResolution * colResolution; i++)
        {
            n[i] = new Vector3(0, 1, 0);
        }
    }

    void DestructScalerObjects()
    {
        //Destroys previous adjustables
        foreach (GameObject mObj in GameObject.FindGameObjectsWithTag("mAdjustable"))
        {
            Destroy(mObj);
        }
        foreach (GameObject mObj in GameObject.FindGameObjectsWithTag("mNormalLine"))
        {
            Destroy(mObj);
        }
        foreach (GameObject mObj in GameObject.FindGameObjectsWithTag("mNonAdjustable"))
        {
            Destroy(mObj);
        }
    }

    void UpdateAllNormals()
    {
        for (int i = 0; i < nodeArray.Length; i++)
        {
            UpdateNormal(i);
        }
    }

    void UpdateNormal(int p)
    {
        Node n = nodeArray[p];
        //Debug.Log("num of neighbors = " + n.neighbors.Length);
        //Recalc normals of all neighbor nodes
        for (int i = 0; i < n.neighbors.Length; i++)
        {
            int loc = n.neighbors[i];
            CalculateAvgNormal(loc);
            UpdateNormalLines();
        }
    }

    void CalculateAvgNormal(int p)
    {
        //Debug.Log("P: " + p);
       // Debug.Log("Nodearray size: " + nodeArray.Length);
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
            //Debug.Log("NodeArray Size: " + nodeArray.Length);
            //Debug.Log("Neigh length " + node.neighbors.Length);
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
        for (int i = 0; i < nodeArray.Length; i++)
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

    //Called every update
    void ScaleTextureToFitMesh()
    {
        //Initial scale the texture to fit the mesh
        uv = new Vector2[rowResolution * colResolution];
        //uv = mMesh.uv;
        Quaternion rotate = Quaternion.Euler(0, 0, 0); //Initial rotation for cylinder
        int counter = 0;
        for (int i = 0; i < colResolution; i++)
        {
            for (int j = 0; j < rowResolution; j++)
            {
                if (mMeshDropdown.value == 0) //Flat option
                    rotate = Quaternion.Euler(0, 0, 0); //Initial rotation for flat
                uv[counter] = rotate * new Vector2(j* rowIncrement, i* colIncrement); //Initialize the texture to fit the mesh
                counter++;
            }
        }

        //Applying the transformations
        rotate = Quaternion.Euler(0, 0, Rotation); //Get the rotation angle
        for (int i = 0; i < rowResolution * colResolution; i++)
        {
            //Apply the Scale
            uv[i].x *= Scale.x * .025f;
            uv[i].y *= Scale.y * .025f;

            //Translate by offset
            uv[i].x += Offset.x * .025f;
            uv[i].y += Offset.y * .025f;

            //Apply the rotation
            uv[i] = rotate * uv[i];
        }
        //Debug.Log(Offset);
        //Set the mesh uvs to the updated uvs
        mMesh.uv = uv;
        mMesh.uv2 = uv;
    }
}