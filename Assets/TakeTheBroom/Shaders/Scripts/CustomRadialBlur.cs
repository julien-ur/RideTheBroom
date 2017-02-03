﻿using UnityEngine;
using System.Collections;

namespace UnityStandardAssets.ImageEffects
{
[ExecuteInEditMode]
public class CustomRadialBlur : MonoBehaviour
{
    public Shader rbShader;
   
    public float blurStrength = 2.2f;
    public float blurWidth = 1.0f;
    public float clearRadius = 0.1f;
 
    private Material rbMaterial = null;
    private bool isOpenGL;
 
    private Material GetMaterial()
    {
        if (rbMaterial == null)
        {
            rbMaterial = new Material(rbShader);
            rbMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        return rbMaterial;
    }
 
    void Start()
    {
        if (rbShader == null)
        {
            Debug.LogError("shader missing!", this);
        }
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        isOpenGL = SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL");
    }
 
    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        //If we run in OpenGL mode, our UV coords are
        //not in 0-1 range, because of the texRECT sampler
        float ImageWidth = 1;
        float ImageHeight = 1;
        if (false && isOpenGL)
        {
            ImageWidth = source.width;
            ImageHeight = source.height;
        }
 
        GetMaterial().SetFloat("_BlurStrength", blurStrength);
        GetMaterial().SetFloat("_BlurWidth", blurWidth);
        GetMaterial().SetFloat("_iHeight",ImageWidth);
        GetMaterial().SetFloat("_iWidth", ImageHeight);
        GetMaterial().SetFloat("_ClearRadius", clearRadius);
        ImageEffects.BlitWithMaterial(GetMaterial(), source, dest);
    }
}
}