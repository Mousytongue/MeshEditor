using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MainController : MonoBehaviour {
    GameObject mSelectedObj = null;
    GameObject mSelectedArm = null;
    Color retainedNodeColor;
    Color retainedArmColor;
    Color selectedNodeColor = new Color(200, 200, 0);
    Color selectedArmColor = new Color(100, 0, 100);


    public void SelectObjectAt(GameObject obj, Vector3 p)
    {
        if (obj.tag == "mAdjustable")
        {
            UnselectCurrent();
            mSelectedObj = obj;
            SetSelectedColor();
            ShowAdjArms();
        }
        if (obj.tag == "mAdjustableArm")
        {
            if (mSelectedObj == null)
                return;
            UnselectCurrentArm();
            mSelectedArm = obj;
            SetSelectedColor();
        }
    }
    void UnselectCurrent()
    {
        if (mSelectedObj != null)
        {
            if (mSelectedArm != null)
                mSelectedArm.GetComponent<MeshRenderer>().material.color = retainedArmColor;

            mSelectedObj.GetComponent<MeshRenderer>().material.color = retainedNodeColor;           
            HideAdjArms();
            mSelectedObj = null;
            mSelectedArm = null;
        }
    }
    void UnselectCurrentArm()
    {
        if (mSelectedArm != null)
        {
            mSelectedArm.GetComponent<MeshRenderer>().material.color = retainedArmColor;
            mSelectedArm = null;
        }
    }
    void SetSelectedColor()
    {
        //Selection was a base node
        if (mSelectedArm == null)
        {
            retainedNodeColor = mSelectedObj.GetComponent<MeshRenderer>().material.color;
            mSelectedObj.GetComponent<MeshRenderer>().material.color = selectedNodeColor;
        }
        //Selection was an arm
        else
        {
            retainedArmColor = mSelectedArm.GetComponent<MeshRenderer>().material.color;
            mSelectedArm.GetComponent<MeshRenderer>().material.color = selectedArmColor;
        }
    }

    //show and hide functions for Ctrl presses
    void ShowAdjNodes()
    {
        //Debug.Log("ShowAdjNodes called");
        foreach (GameObject mObj in GameObject.FindGameObjectsWithTag("mAdjustable"))
        {
            MeshRenderer mr = mObj.GetComponent<MeshRenderer>();
            mr.enabled = true;
        }
        foreach (GameObject mObj in GameObject.FindGameObjectsWithTag("mNormalLine"))
        {
            MeshRenderer mr = mObj.GetComponent<MeshRenderer>();
            mr.enabled = true;
        }
    }
    void ShowAdjArms()
    {
        //Debug.Log("ShowAdjNodeFull called");
        foreach (Transform child in mSelectedObj.transform)
        {
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            mr.enabled = true;
        }
    }
    void HideAdjNodes()
    {
        //Debug.Log("Hide nodes called");
        foreach (GameObject mObj in GameObject.FindGameObjectsWithTag("mAdjustable"))
        {
            MeshRenderer mr = mObj.GetComponent<MeshRenderer>();
            mr.enabled = false;

        }
    }
    void HideAdjArms()
    {
        foreach (Transform child in mSelectedObj.transform)
        {
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            mr.enabled = false;
        }
    }
    void HideAll()
    {
        foreach (GameObject mObj in GameObject.FindGameObjectsWithTag("mAdjustable"))
        {
            MeshRenderer mr = mObj.GetComponent<MeshRenderer>();
            mr.enabled = false;
            foreach (Transform child in mObj.transform)
            {
                mr = child.GetComponent<MeshRenderer>();
                mr.enabled = false;
            }
        }
        foreach (GameObject mObj in GameObject.FindGameObjectsWithTag("mNormalLine"))
        {
            MeshRenderer mr = mObj.GetComponent<MeshRenderer>();
            mr.enabled = false;
        }
    }
}
