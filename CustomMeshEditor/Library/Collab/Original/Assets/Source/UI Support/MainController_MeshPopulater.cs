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
    float increment;                         //Used to measure displace between Nodes
    int size;                                //Indicates the mesh density 3x3, 4x4 ect
    Vector3[] v;                             //Vector array for mesh creation
    Vector3[] n;                             //Normal array for mesh creation
    int[] t;                                 //Triangle array for mesh creation

    //For cylinder Mesh
    float radius = 20;                       //radius of cylinder
    float height = 40;                       //Height of cylinder
    float heightIncrement;
    float circumference;
    float columnIncrement;                   //Used for cylinder "crunch"

    //UV stuff
    public Vector2 Offset = Vector2.zero;
    public Vector2 Scale = Vector2.one;
    //Vector2[] mInitUV = null; // initial values
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
        mFlatMeshResolutionSlider.SetSliderListener(FlatMeshSetResolution);
        mCylinderMeshResolutionSlider.SetSliderListener(CylinderMeshSetResolution);
        mCylinderRotationSlider.SetSliderListener(CylinderRotationUpdater);
        mMesh = mMeshObj.GetComponent<MeshFilter>().mesh;
        mMesh.Clear();
        FlatMeshSetResolution(10);                 //initial size, can be changed 
        //CylinderMeshSetResolution(10);
        circumference = 270;

        //Add listener for when the value of the Dropdown changes, to take action
        mMeshDropdown.onValueChanged.AddListener(delegate {
            ChangeMesh(mMeshDropdown);
        });

        mAdj.GetComponent<Image>().material.color = new Color(Random.value, Random.value, Random.value);
    }

    void ChangeMesh(Dropdown d)
    {
        if (d.value == 0)
        {
            FlatMeshSetResolution(size);
            MainCamera.transform.localPosition = new Vector3(20, 20, -41);
            MainCamera.transform.eulerAngles = new Vector3(52, -1, 0);
            mCameraLookAt.transform.localPosition = new Vector3(20, 0, -23);
        }
        if (d.value == 1)
        {
            CylinderMeshSetResolution(size);
            MainCamera.transform.localPosition = new Vector3(45, 27, -13);
            MainCamera.transform.eulerAngles = new Vector3(14, -78, 0);
            mCameraLookAt.transform.localPosition = new Vector3(-4, 14, -3);
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
        for (int i = 0; i < nodeArray.Length; i++)
        {
            Node n = new Node();
            n = nodeArray[i];
            if (n.neighbors == null)
            {
                Debug.Log("neighbor array is null");
            }
            //Debug.Log(n.neighbors.Length);
            Debug.Log("Node number: " + i);
            for (int j = 0; j < n.neighbors.Length; j++)
            {
                Debug.Log("Neighbors: " + n.neighbors[j]);
            }
        }
    }

    void PopulateNeighborArray(Node n, int s, int c)
    {
        //Debug.Log("PopulateNA called" + c);
        //Determines which case to be filled
        //Corners
        if (c == 0 || c == s - 1 || c == (s - 1) * s || c == (s * s) - 1)
        {
            PopulateCornerCases(n, s, c);
            return;
        }
        //top row
        if (c > 0 && c < s - 1)
        {
            PopulateEdgeCases(n, s, c, 1);
            return;
        }
        //bot row
        if (c > s * (s - 1) && c < (s * s) - 1)
        {
            PopulateEdgeCases(n, s, c, 4);
            return;
        }
        //left column or right column
        for (int i = 1; i <= s - 2; i++)
        {
            if (c == (s * i))
            {
                PopulateEdgeCases(n, s, c, 2);
                return;
            }
            if (c == (s * i) + (size - 1))
            {
                PopulateEdgeCases(n, s, c, 3);
                return;
            }
        }
        //If reached this point w/o returning, its a center node
        PopulateCenterCases(n, s, c);
    }

    void PopulateCornerCases(Node n, int s, int c)
    {
        //Debug.Log("CornerCase called");
        if (c == 0)
        {
            n.neighbors = new int[3];
            n.neighbors[2] = 1;
            n.neighbors[1] = s + 1;
            n.neighbors[0] = s;
        }
        if (c == s - 1)
        {
            n.neighbors = new int[2];
            n.neighbors[0] = s - 2;
            n.neighbors[1] = 2 * size - 1;
        }
        if (c == (s - 1) * s)
        {
            n.neighbors = new int[2];
            n.neighbors[0] = ((s - 1) * s) + 1;
            n.neighbors[1] = (s - 2) * s;
        }
        if (c == (s * s) - 1)
        {
            n.neighbors = new int[3];
            n.neighbors[2] = (s * s) - 2;
            n.neighbors[1] = (s * s) - (size + 2);
            n.neighbors[0] = (s * s) - (size + 1);
        }
        nodeArray[c] = n;
    }

    void PopulateCenterCases(Node n, int s, int c)
    {
        //Debug.Log("CenterCase called");
        n.neighbors = new int[6];
        n.neighbors[5] = c - 1;
        n.neighbors[4] = c - s - 1;
        n.neighbors[3] = c - s;
        n.neighbors[2] = c + 1;
        n.neighbors[1] = c + s + 1;
        n.neighbors[0] = c + s;
        nodeArray[c] = n;
    }

    void PopulateEdgeCases(Node n, int s, int c, int side)
    {
        //Debug.Log("EdgeCase called");
        n.neighbors = new int[4];
        //side 1 = top
        if (side == 1)
        {
            n.neighbors[0] = c - 1;
            n.neighbors[1] = c + s;
            n.neighbors[2] = c + s + 1;
            n.neighbors[3] = c + 1;

        }
        //side 2 = left edge
        if (side == 2)
        {
            n.neighbors[3] = c - s;
            n.neighbors[2] = c + 1;
            n.neighbors[1] = c + s + 1;
            n.neighbors[0] = c + s;
        }
        //side 3 = right edge
        if (side == 3)
        {
            n.neighbors[3] = c + s;
            n.neighbors[2] = c - 1;
            n.neighbors[1] = c - s - 1;
            n.neighbors[0] = c - s;
        }
        //side 4 = bot
        if (side == 4)
        {
            n.neighbors[3] = c - 1;
            n.neighbors[2] = c - s - 1;
            n.neighbors[1] = c - s;
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
        int tSize = ((size - 1) * (size - 1) * 6);
        t = new int[tSize];               //Triangles
        int arrayCounter = 0;
        int counter = 0;
        int row = 1;
        while (arrayCounter != t.Length)
        {

            t[arrayCounter] = counter; arrayCounter++;
            t[arrayCounter] = counter + size; arrayCounter++;
            t[arrayCounter] = counter + size + 1; arrayCounter++;

            t[arrayCounter] = counter; arrayCounter++;
            t[arrayCounter] = counter + size + 1; arrayCounter++;
            t[arrayCounter] = counter + 1; arrayCounter++;

            if (counter == size * row - 2)
            {
                counter += 2;
                row++;
            }
            else
                counter++;
        }
    }

    void PopulateNormalArray()
    {
        n = new Vector3[size * size];
        for (int i = 0; i < size * size; i++)
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
        uv = mMesh.uv;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                uv[i + j * size] = new Vector2(j / (float)(size - 1), i / (float)(size - 1));
            }
        }
        mMesh.uv = uv;
        mMesh.uv2 = uv;
    }
}