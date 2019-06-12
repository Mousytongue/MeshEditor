using System; // for assert
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // for GUI elements: Button, Toggle

public partial class MainController : MonoBehaviour
{

    // reference to all UI elements in the Canvas
    public Camera MainCamera = null;
    public TheWorld TheWorld = null;
    public GameObject mCameraLookAt;
    public SliderWithEcho mMeshResolutionRowSlider;           //Slider to change density on flat mesh
    public SliderWithEcho mMeshResolutionColSlider;       //Slider to change density on cylinder mesh
    public SliderWithEcho mCylinderRotationSlider;             //Slider to change cylinder rotation
    public Dropdown mMeshDropdown;

    // Update is called once per frame
    void Update()
    {
        ProcessMouseEvents();
        ScaleTextureToFitMesh();
    }
}