using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraControl : MonoBehaviour {

    public Transform LookAtPosition;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 v = LookAtPosition.localPosition - transform.localPosition; //forward
        Vector3 u = transform.up;                                           //up
        Vector3 w = Vector3.Cross(v, u);                                    //right
        Vector3 newCameraPos;

        //Alt left click tumbler
        if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKey(KeyCode.Mouse0))
            {
                float RotateDeltaX = Input.GetAxis("Mouse X");
                float RotateDeltaY = Input.GetAxis("Mouse Y");
                float turnSpeed = 2f;
                Quaternion qr = Quaternion.AngleAxis(turnSpeed * RotateDeltaY, w);
                Quaternion qu = Quaternion.AngleAxis(turnSpeed * RotateDeltaX, u);
                Debug.Log(qu);

                //Debug.Log(u);
                if (u.y >= 0.1)
                {
                    Matrix4x4 rr = Matrix4x4.TRS(Vector3.zero, qr, Vector3.one);
                    Matrix4x4 invPr = Matrix4x4.TRS(-LookAtPosition.localPosition, Quaternion.identity, Vector3.one);
                    rr = invPr.inverse * rr * invPr;
                    newCameraPos = rr.MultiplyPoint(transform.localPosition);
                    transform.localPosition = newCameraPos;
                }
                Matrix4x4 ru = Matrix4x4.TRS(Vector3.zero, qu, Vector3.one);
                Matrix4x4 invPu = Matrix4x4.TRS(-LookAtPosition.localPosition, Quaternion.identity, Vector3.one);
                ru = invPu.inverse * ru * invPu;
                newCameraPos = ru.MultiplyPoint(transform.localPosition);
                transform.localPosition = newCameraPos;
            }

            //Alt right click panning
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKey(KeyCode.Mouse1))
            {
                transform.localPosition += w * 0.05f * Input.GetAxis("Mouse X");
                LookAtPosition.localPosition += w * 0.05f * Input.GetAxis("Mouse X");
                transform.localPosition += -u * Input.GetAxis("Mouse Y");
                LookAtPosition.localPosition += -u * Input.GetAxis("Mouse Y");
            }

            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                transform.localPosition += v * Input.GetAxis("Mouse ScrollWheel");
            }
        }
        transform.LookAt(LookAtPosition.localPosition);
    }
}
