using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MainController : MonoBehaviour
{

    Vector3 X = new Vector3(1, 0, 0);
    Vector3 Y = new Vector3(0, 1, 0);
    Vector3 Z = new Vector3(0, 0, 1);
    GameObject mAdj;
    GameObject mArm;
    bool bDown = false;

    void ProcessMouseEvents()
    {

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            //Shows nodes on Ctrl down
            ShowAdjNodes();

            //Left Click can Select the node.
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity);
                if (hit)
                {
                    SelectObjectAt(hitInfo.transform.gameObject, hitInfo.point);
                }
            }
        }
        //When ctrl is releases, and mSelected is null, hides nodes
        if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
        {
            if (mSelectedObj == null)
                HideAdjNodes();
        }


        //Left hold can drag an arm
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity);
            if (hit && hitInfo.transform.gameObject == mSelectedArm && mSelectedObj != null)
            {
                //Debug.Log("bDown = true");
                bDown = true;
                mArm = hitInfo.transform.gameObject;
                mAdj = hitInfo.transform.parent.gameObject;
            }
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            //Debug.Log("dDown = false");
            bDown = false;
        }

        if (bDown == true)
        {
            if (Input.GetMouseButton(0))
            {
                //Debug.Log("Drag mouse being called");
                {
                    //Adjust along X
                    if (mArm.transform.name == "xArm")
                    {
                        mAdj.transform.localPosition += X * Input.GetAxis("Mouse X");
                        if (mMeshDropdown.value == 0)
                            UpdateFlatMeshNode(mSelectedArm);
                        else
                            UpdateCylinderMeshNode(mSelectedArm, Input.GetAxis("Mouse X"));
                    }

                    //Adjust along Y
                    if (mArm.transform.name == "yArm")
                    {
                        mAdj.transform.localPosition += Y * Input.GetAxis("Mouse Y");
                        if (mMeshDropdown.value == 0)
                            UpdateFlatMeshNode(mSelectedArm);
                        else
                            UpdateCylinderMeshNode(mSelectedArm, Input.GetAxis("Mouse Y"));
                    }

                    //Adjust Along Z
                    if (mArm.transform.name == "zArm")
                    {
                        mAdj.transform.localPosition += Z * Input.GetAxis("Mouse Y");
                        if (mMeshDropdown.value == 0)
                            UpdateFlatMeshNode(mSelectedArm);
                        else
                            UpdateCylinderMeshNode(mSelectedArm, Input.GetAxis("Mouse X"));
                    }
                }
            }

        }


        
    }
}



