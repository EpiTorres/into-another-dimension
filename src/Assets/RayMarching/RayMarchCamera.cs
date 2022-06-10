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
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class RayMarchCamera : SceneViewFilter
{
    [SerializeField]
    private Shader shader;

    public Material rayMarchMaterial
    {
        get
        {
            if (!rayMarchMat && shader)
            {
                rayMarchMat = new Material(shader);
                rayMarchMat.hideFlags = HideFlags.HideAndDontSave;
            }
            return rayMarchMat;
        }
    }
    private Material rayMarchMat;

    [Header("Ray Marcher")]
    public float maxDistance = 200;
    [Range(1, 300)]
    public int maxIterations = 200;
    [Range(0.1f, 0.001f)]
    public float precision = 0.001F;
    public float currentWPos = 0F;

    [Header("Lighting, Shading, & Color")]
    public GameObject directionalLight;
    public float shadowMinDistance = 0.5F;
    public float shadowMaxDistance = 15F;
    public float shadowIntensity = 0.5F;
    public float shadowSmoothness = 60F;
    [Range(0, 3)]
    public int reflectionCount = 0;
    [Range(0, 1)]
    public float reflectionIntensity = 0.5F;
    [Range(0, 4)]
    public float colorIntensity = 1F;

    [Header("4D Distance Field")]
    public GameObject primitives;

    private Plane4D[] planes;
    private int planeCount = 0;

    private Box4D[] boxes;
    private int boxCount = 0;


    private Sphere4D[] spheres;
    private int sphereCount = 0;

    private Capsule4D[] capsules;
    private int capsuleCount = 0;

    private Controller controller;

    public Camera _camera
    {
        get
        {
            if (!cam)
            {
                cam = GetComponent<Camera>();
            }
            return cam;
        }
    }
    private Camera cam;

    public void Start()
    {
        planes = primitives.GetComponentsInChildren<Plane4D>();
        planeCount = planes.Length;

        boxes = primitives.GetComponentsInChildren<Box4D>();
        boxCount = boxes.Length;

        spheres = primitives.GetComponentsInChildren<Sphere4D>();
        sphereCount = spheres.Length;

        capsules = primitives.GetComponentsInChildren<Capsule4D>();
        capsuleCount = capsules.Length;

        controller = this.GetComponent<Controller>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!rayMarchMaterial)
        {
            Graphics.Blit(source, destination);
            return;
        }


        ///////////////////////////////////////////////////////////////////
        // Camera & Ray Marching

        rayMarchMaterial.SetMatrix("CamFrustum", CamFrustum(_camera));
        if (controller && controller.transform4D)
        {
            rayMarchMaterial.SetInt("UseCamToWorld", 0);
            rayMarchMaterial.SetMatrix("CamRot", controller.transform4D.rotor.ToMatrix());
            rayMarchMaterial.SetVector("CamPos", controller.transform4D.position);
        }
        else
        {
            rayMarchMaterial.SetInt("UseCamToWorld", 1);
            rayMarchMaterial.SetMatrix("CamToWorld", _camera.cameraToWorldMatrix);
            rayMarchMaterial.SetVector("CamPos", new Vector4(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z, currentWPos));
        }

        // Ray Marcher Variables
        rayMarchMaterial.SetFloat("maxDist", maxDistance);
        rayMarchMaterial.SetInt("maxIter", maxIterations);
        rayMarchMaterial.SetFloat("prec", precision);

        ///////////////////////////////////////////////////////////////////
        // 4D Primitives

        // HyperPlanes
        if (planeCount > 0)
        {
            Vector4[] planeNorm = new Vector4[planeCount];
            float[] planeOff = new float[planeCount];
            Color[] planeCol = new Color[planeCount];

            for (int i = 0; i < planeCount; i++)
            {
                planeNorm[i] = planes[i].normal;
                planeOff[i] = planes[i].offset;
                planeCol[i] = planes[i].color;
            }

            rayMarchMaterial.SetInt("pCount", planeCount);
            rayMarchMaterial.SetVectorArray("pNorm", planeNorm);
            rayMarchMaterial.SetFloatArray("pOff", planeOff);
            rayMarchMaterial.SetColorArray("pCol", planeCol);
        }

        // HyperCubes
        if (boxCount > 0)
        {
            Vector4[] boxPos = new Vector4[boxCount];
            Matrix4x4[] boxRot = new Matrix4x4[boxCount];
            Vector4[] boxScale = new Vector4[boxCount];
            Color[] boxCol = new Color[boxCount];

            for (int i = 0; i < boxCount; i++)
            {
                boxPos[i] = boxes[i].transform4D.position;
                boxRot[i] = boxes[i].transform4D.rotor.ToMatrix();
                boxScale[i] = boxes[i].transform4D.scale;
                boxCol[i] = boxes[i].color;
            }

            rayMarchMaterial.SetInt("bCount", boxCount);
            rayMarchMaterial.SetVectorArray("bPos", boxPos);
            rayMarchMaterial.SetMatrixArray("bRot", boxRot);
            rayMarchMaterial.SetVectorArray("bScale", boxScale);
            rayMarchMaterial.SetColorArray("bCol", boxCol);
        }


        // HyperSpheres
        if (sphereCount > 0)
        {
            Vector4[] spherePos = new Vector4[sphereCount];
            Matrix4x4[] sphereRot = new Matrix4x4[sphereCount];
            float[] sphereRad = new float[sphereCount];
            Color[] sphereCol = new Color[sphereCount];

            for (int i = 0; i < sphereCount; i++)
            {
                spherePos[i] = spheres[i].transform4D.position;
                sphereRot[i] = spheres[i].transform4D.rotor.ToMatrix();
                sphereRad[i] = spheres[i].radius;
                sphereCol[i] = spheres[i].color;
            }

            rayMarchMaterial.SetInt("sCount", sphereCount);
            rayMarchMaterial.SetVectorArray("sPos", spherePos);
            rayMarchMaterial.SetMatrixArray("sRot", sphereRot);
            rayMarchMaterial.SetFloatArray("sRad", sphereRad);
            rayMarchMaterial.SetColorArray("sCol", sphereCol);
        }


        // Capsules
        if (capsuleCount > 0)
        {
            Vector4[] capsulePos = new Vector4[capsuleCount];
            Matrix4x4[] capsuleRot = new Matrix4x4[capsuleCount];
            float[] capsuleRad = new float[capsuleCount];
            float[] capsuleHt = new float[capsuleCount];
            Color[] capsuleCol = new Color[capsuleCount];

            for (int i = 0; i < capsuleCount; i++)
            {
                capsulePos[i] = capsules[i].transform4D.position;
                capsuleRot[i] = capsules[i].transform4D.rotor.ToMatrix();
                capsuleRad[i] = capsules[i].radius;
                capsuleHt[i] = capsules[i].height;
                capsuleCol[i] = capsules[i].color;
            }

            rayMarchMaterial.SetInt("cCount", capsuleCount);
            rayMarchMaterial.SetVectorArray("cPos", capsulePos);
            rayMarchMaterial.SetMatrixArray("cRot", capsuleRot);
            rayMarchMaterial.SetFloatArray("cRad", capsuleRad);
            rayMarchMaterial.SetFloatArray("cHt", capsuleHt);
            rayMarchMaterial.SetColorArray("cCol", capsuleCol);
        }


        ///////////////////////////////////////////////////////////////////
        // Lighting & Shading

        // For Colors
        rayMarchMaterial.SetFloat("colInt", colorIntensity);

        // For Lighting
        Vector4 lightDir = directionalLight ? directionalLight.transform.forward : Vector3.down;
        rayMarchMaterial.SetVector("lightDir", lightDir);
        rayMarchMaterial.SetColor("lightCol", directionalLight ? directionalLight.GetComponent<Light>().color : Color.white);
        rayMarchMaterial.SetFloat("lightInt", directionalLight ? directionalLight.GetComponent<Light>().intensity : 1f);

        // For Shadows
        rayMarchMaterial.SetFloat("shadeMinDist", shadowMinDistance);
        rayMarchMaterial.SetFloat("shadeMaxDist", shadowMaxDistance);
        rayMarchMaterial.SetFloat("shadeInt", shadowIntensity);
        rayMarchMaterial.SetFloat("shadeSmooth", shadowSmoothness);

        rayMarchMaterial.SetInt("refCount", reflectionCount);
        rayMarchMaterial.SetFloat("refInt", reflectionIntensity);


        ///////////////////////////////////////////////////////////////////
        // Shader Set Up

        RenderTexture.active = destination;
        rayMarchMaterial.SetTexture("MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        rayMarchMaterial.SetPass(0);
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
