/* ------------------------------------------------------------------
 * Author: Epifanio Torres
 * Date: 4/11/2022
 *
 * Copyright (c) 2022 Epifanio Torres
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any
 * damages arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any
 * purpose, including commercial applications, and to alter it and
 * redistribute it freely, subject to the following restrictions:
 *      1. The origin of this software must not be misrepresented;
 *         you must not claim that you wrote the original software.
 *         If you use this software in a product, an acknowledgment
 *         in the product documentation would be appreciated but is
 *         not required.
 *      2. Altered source versions must be plainly marked as such, and
 *         must not be misrepresented as being the original software.
 *      3. This notice may not be removed or altered from any source
 *         distribution.
 * 
 * This code was adapted from:
 *      - https://www.youtube.com/c/PeerPlay/featured
 *-------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class RayMarchCamera : SceneViewFilter
{
    [SerializeField]
    private Shader _shader;

    public Material _rayMarchMaterial
    {
        get
        {
            if (!_rayMarchMat && _shader)
            {
                _rayMarchMat = new Material(_shader);
                _rayMarchMat.hideFlags = HideFlags.HideAndDontSave;
            }
            return _rayMarchMat;
        }
    }
    private Material _rayMarchMat;

    [Header("Ray Marcher")]
    public float _maxDistance;
    [Range(1, 300)]
    public int _maxIterations;
    [Range(0.1f, 0.001f)]
    public float _precision;
    public float _currentWPos;

    [Header("Lighting, Shading, & Color")]
    public GameObject _directionalLight;
    public float _shadowMinDistance;
    public float _shadowMaxDistance;
    public float _shadowIntensity;
    public float _shadowSmoothness;
    [Range(0, 3)]
    public int _reflectionCount;
    [Range(0, 1)]
    public float _reflectionIntensity;
    [Range(0, 4)]
    public float _colorIntensity;

    [Header("4D Distance Field")]
    public GameObject _primitives;

    private Plane4D[] _planes;
    private int _planeCount = 0;

    private Box4D[] _boxes;
    private int _boxCount = 0;


    private Sphere4D[] _spheres;
    private int _sphereCount = 0;

    private Capsule4D[] _capsules;
    private int _capsuleCount = 0;

    private Controller controller;

    public Camera _camera
    {
        get
        {
            if (!_cam)
            {
                _cam = GetComponent<Camera>();
            }
            return _cam;
        }
    }
    private Camera _cam;

    public void Start()
    {
        _planes = _primitives.GetComponentsInChildren<Plane4D>();
        _planeCount = _planes.Length;

        _boxes = _primitives.GetComponentsInChildren<Box4D>();
        _boxCount = _boxes.Length;

        _spheres = _primitives.GetComponentsInChildren<Sphere4D>();
        _sphereCount = _spheres.Length;

        _capsules = _primitives.GetComponentsInChildren<Capsule4D>();
        _capsuleCount = _capsules.Length;

        controller = this.GetComponent<Controller>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!_rayMarchMaterial)
        {
            Graphics.Blit(source, destination);
            return;
        }


        ///////////////////////////////////////////////////////////////////
        // Camera & Ray Marching

        _rayMarchMaterial.SetMatrix("_CamFrustum", CamFrustum(_camera));
        if (controller && controller.transform4D)
        {
            _rayMarchMaterial.SetInt("_UseCamToWorld", 0);
            _rayMarchMaterial.SetMatrix("_CamRot", controller.transform4D.rotor.ToMatrix());
            _rayMarchMaterial.SetVector("_CamPos", controller.transform4D.position);
        }
        else
        {
            _rayMarchMaterial.SetInt("_UseCamToWorld", 1);
            _rayMarchMaterial.SetMatrix("_CamToWorld", _camera.cameraToWorldMatrix);
            _rayMarchMaterial.SetVector("_CamPos", new Vector4(_cam.transform.position.x, _cam.transform.position.y, _cam.transform.position.z, _currentWPos));
        }

        // Ray Marcher Variables
        _rayMarchMaterial.SetFloat("_maxDist", _maxDistance);
        _rayMarchMaterial.SetInt("_maxIter", _maxIterations);
        _rayMarchMaterial.SetFloat("_prec", _precision);

        ///////////////////////////////////////////////////////////////////
        // 4D Primitives

        // HyperPlanes
        if (_planeCount > 0)
        {
            Vector4[] planeNorm = new Vector4[_planeCount];
            float[] planeOff = new float[_planeCount];
            Color[] planeCol = new Color[_planeCount];

            for (int i = 0; i < _planeCount; i++)
            {
                planeNorm[i] = _planes[i].normal;
                planeOff[i] = _planes[i].offset;
                planeCol[i] = _planes[i].color;
            }

            _rayMarchMaterial.SetInt("_pCount", _planeCount);
            _rayMarchMaterial.SetVectorArray("_pNorm", planeNorm);
            _rayMarchMaterial.SetFloatArray("_pOff", planeOff);
            _rayMarchMaterial.SetColorArray("_pCol", planeCol);
        }

        // HyperCubes
        if (_boxCount > 0)
        {
            Vector4[] boxPos = new Vector4[_boxCount];
            Matrix4x4[] boxRot = new Matrix4x4[_boxCount];
            Vector4[] boxScale = new Vector4[_boxCount];
            Color[] boxCol = new Color[_boxCount];

            for (int i = 0; i < _boxCount; i++)
            {
                boxPos[i] = _boxes[i].transform4D.position;
                boxRot[i] = _boxes[i].transform4D.rotor.ToMatrix();
                boxScale[i] = _boxes[i].transform4D.scale;
                boxCol[i] = _boxes[i].color;
            }

            _rayMarchMaterial.SetInt("_bCount", _boxCount);
            _rayMarchMaterial.SetVectorArray("_bPos", boxPos);
            _rayMarchMaterial.SetMatrixArray("_bRot", boxRot);
            _rayMarchMaterial.SetVectorArray("_bScale", boxScale);
            _rayMarchMaterial.SetColorArray("_bCol", boxCol);
        }


        // HyperSpheres
        if (_sphereCount > 0)
        {
            Vector4[] spherePos = new Vector4[_sphereCount];
            Matrix4x4[] sphereRot = new Matrix4x4[_sphereCount];
            float[] sphereRad = new float[_sphereCount];
            Color[] sphereCol = new Color[_sphereCount];

            for (int i = 0; i < _sphereCount; i++)
            {
                spherePos[i] = _spheres[i].transform4D.position;
                sphereRot[i] = _spheres[i].transform4D.rotor.ToMatrix();
                sphereRad[i] = _spheres[i].radius;
                sphereCol[i] = _spheres[i].color;
            }

            _rayMarchMaterial.SetInt("_sCount", _sphereCount);
            _rayMarchMaterial.SetVectorArray("_sPos", spherePos);
            _rayMarchMaterial.SetMatrixArray("_sRot", sphereRot);
            _rayMarchMaterial.SetFloatArray("_sRad", sphereRad);
            _rayMarchMaterial.SetColorArray("_sCol", sphereCol);
        }


        // Capsules
        if (_capsuleCount > 0)
        {
            Vector4[] capsulePos = new Vector4[_capsuleCount];
            Matrix4x4[] capsuleRot = new Matrix4x4[_capsuleCount];
            float[] capsuleRad = new float[_capsuleCount];
            float[] capsuleHt = new float[_capsuleCount];
            Color[] capsuleCol = new Color[_capsuleCount];

            for (int i = 0; i < _capsuleCount; i++)
            {
                capsulePos[i] = _capsules[i].transform4D.position;
                capsuleRot[i] = _capsules[i].transform4D.rotor.ToMatrix();
                capsuleRad[i] = _capsules[i].radius;
                capsuleHt[i] = _capsules[i].height;
                capsuleCol[i] = _capsules[i].color;
            }

            _rayMarchMaterial.SetInt("_cCount", _capsuleCount);
            _rayMarchMaterial.SetVectorArray("_cPos", capsulePos);
            _rayMarchMaterial.SetMatrixArray("_cRot", capsuleRot);
            _rayMarchMaterial.SetFloatArray("_cRad", capsuleRad);
            _rayMarchMaterial.SetFloatArray("_cHt", capsuleHt);
            _rayMarchMaterial.SetColorArray("_cCol", capsuleCol);
        }


        ///////////////////////////////////////////////////////////////////
        // Lighting & Shading

        // For Colors
        _rayMarchMaterial.SetFloat("_colInt", _colorIntensity);

        // For Lighting
        Vector4 _lightDir = _directionalLight ? _directionalLight.transform.forward : Vector3.down;
        _rayMarchMaterial.SetVector("_lightDir", _lightDir);
        _rayMarchMaterial.SetColor("_lightCol", _directionalLight ? _directionalLight.GetComponent<Light>().color : Color.white);
        _rayMarchMaterial.SetFloat("_lightInt", _directionalLight ? _directionalLight.GetComponent<Light>().intensity : 1f);

        // For Shadows
        _rayMarchMaterial.SetFloat("_shadeMinDist", _shadowMinDistance);
        _rayMarchMaterial.SetFloat("_shadeMaxDist", _shadowMaxDistance);
        _rayMarchMaterial.SetFloat("_shadeInt", _shadowIntensity);
        _rayMarchMaterial.SetFloat("_shadeSmooth", _shadowSmoothness);

        _rayMarchMaterial.SetInt("_refCount", _reflectionCount);
        _rayMarchMaterial.SetFloat("_refInt", _reflectionIntensity);


        ///////////////////////////////////////////////////////////////////
        // Shader Set Up

        RenderTexture.active = destination;
        _rayMarchMaterial.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        _rayMarchMaterial.SetPass(0);
        GL.Begin(GL.QUADS);

        // Bottom Left
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);

        // Bottom Right
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);

        // Top Right
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);

        // Top Left
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();

    }


    /*------------------------------------------------------------------
     * Returns the Camera's View Frustum
     *------------------------------------------------------------------*/
    private Matrix4x4 CamFrustum(Camera cam)
    {
        Matrix4x4 frustum = Matrix4x4.identity;
        float fov = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        Vector3 forward = new Vector4(0, 0, 1);
        Vector3 up = new Vector4(0, fov, 0);
        Vector3 right = new Vector4(fov * cam.aspect, 0, 0);

        Vector3 topLeft = (-forward - right + up);
        Vector3 topRight = (-forward + right + up);
        Vector3 botLeft = (-forward - right - up);
        Vector3 botRight = (-forward + right - up);


        frustum.SetRow(0, topLeft);
        frustum.SetRow(1, topRight);
        frustum.SetRow(2, botRight);
        frustum.SetRow(3, botLeft);
        return frustum;
    }
}
