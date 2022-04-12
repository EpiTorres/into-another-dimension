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
 * This code was adapted from :
 *      - https ://www.youtube.com/playlist?list=PL3POsQzaCw53iK_EhOYR39h1J9Lvg-m-g
 *      - https ://gist.github.com/OMeyer973/61acc5e1ade54ccc35dacfa7c4b07b31
 *-------------------------------------------------------------------*/

Shader "Test/RayMarchShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            /*------------------------------------------------------------------
             * Camera
             *------------------------------------------------------------------*/
            uniform sampler2D _CameraDepthTexture;
            uniform float4x4 _CamFrustum, _CamRot;
            uniform float4 _CamPos;
            // These variables are used if the camera's Transform4D is undefined
            uniform float4x4 _CamToWorld;
            uniform int _UseCamToWorld;

            /*------------------------------------------------------------------
             * Ray Marching
             *------------------------------------------------------------------*/
            uniform float _maxDist;         // Max dist of rendered object 
            uniform int _maxIter;           // Max iterations of ray marching
            uniform float _prec;            // Precision of ray marching

            /*------------------------------------------------------------------
             * Lighting, Shadows, Colors, and Reflections
             *------------------------------------------------------------------*/

            uniform float4 _lightDir;       // Direction of the light
            uniform float3 _lightCol;       // Color of the light
            uniform float _lightInt;        // Intensity of the light

            uniform float _shadeInt;        // Intensity of the shadows
            uniform float _shadeMaxDist;    // Max dist for shadow generation 
            uniform float _shadeMinDist;    // Min dist for shadow generation
            uniform float _shadeSmooth;     // Smoothness of soft shadows

            uniform float _colInt;        // Intensity of colors

            uniform int _refCount;          // Number of reflection bounces
            uniform float _refInt;          // Intensity of reflections

            /*------------------------------------------------------------------
             * SDF Objects
             *------------------------------------------------------------------*/

            uniform int _pCount;        // Number of planes
            uniform float4 _pNorm[5];   // Normals of the planes
            uniform float _pOff[5];     // Planes' offsets along normals
            uniform fixed4 _pCol[5];    // Colors of the planes

            uniform int _bCount;            // Number of boxes
            uniform float4 _bPos[10];       // Boxes' positions
            uniform float4x4 _bRot[10];     // Boxes' rotation matrices
            uniform float4 _bScale[10];     // Boxes' scale vectors
            uniform fixed4 _bCol[10];       // Boxes' colors

            uniform int _sCount;            // Number of spheres
            uniform float4 _sPos[10];       // Spheres' positions
            uniform float4x4 _sRot[10];     // Spheres' rotation matrices
            uniform float _sRad[10];
            uniform fixed4 _sCol[10];

            // Spheres
            uniform int _cCount;            // Number of capsules
            uniform float4 _cPos[10];       // Capsules' positions
            uniform float4x4 _cRot[10];     // Capsules' rotation matrices
            uniform float _cHt[10];         // Capsules' height
            uniform float _cRad[10];        // Capsules' radii
            uniform fixed4 _cCol[10];       // Capsules' colors
            
            /*------------------------------------------------------------------
             * The Shader
             *------------------------------------------------------------------*/

             // Includes these files after the global variables so that the global
             // variables can be used in the functions
             #include "DistFunctions4D.cginc"
             #include "RayMarchFunctions.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 ray : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                half index = v.vertex.z;
                v.vertex.z = 0;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.ray = _CamFrustum[(int)index].xyzw;
                o.ray /= abs(o.ray.z);

                if (_UseCamToWorld) o.ray = mul(_CamToWorld, o.ray);
                else o.ray = mul(_CamRot, o.ray);

                return o;
            }

            /*------------------------------------------------------------------
             * Performs the ray marching algorithm to set the appropriate color
             * for the current pixel. 
             *------------------------------------------------------------------*/
            fixed4 frag(v2f i) : SV_Target
            {
                float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
                depth *= length(i.ray);
                fixed3 mainCol = tex2D(_MainTex, i.uv);

                // Sets the ray's origin to be the camera's position in world space
                float4 ro = _CamPos;
                // Rotates the ray's direction based on the camera's rotation matrix
                float4 rd = normalize(i.ray);
                
                fixed4 res = fixed4(0, 0, 0, 0);
                float4 p; // The intersection point
                fixed3 c; // The color of the object at the intersection point

                bool hit = rayMarching(ro, rd, depth, _maxDist, _maxIter, p, c);
                if (hit)
                {
                    
                    float4 n = calcNormal(p);
                    float3 s = shading(p, n, c);
                    res = fixed4(s, 1);
                    
                    // Iteratively applies the ray marching algorithm from
                    // the hit point to generate reflections
                    uint mipLevel = 2;
                    float invMipLevel = .5f;

                    for (int i = 0; i < _refCount; i++) { 
                        rd = normalize(reflect(rd, n));
                        ro = p + rd * .01;
                        hit = rayMarching(ro, rd, 
                            _maxDist * invMipLevel, _maxDist * invMipLevel, 
                            _maxIter / mipLevel, p, c);

                        if (hit)
                        {
                            n = calcNormal(p);
                            s = shading(p, n, c);
                            res += fixed4(s * _refInt, 0) * invMipLevel;
                        }
                        else break;
                        mipLevel *= 2;
                        invMipLevel *= .5;
                    }
                }

                // Calculates the final color of this pixel and returns it
                fixed4 returnCol = fixed4(
                    mainCol * (1.0 - res.w) + res.xyz * res.w,
                    1.0
                );
                return returnCol;
            }
            ENDCG
        }
    }
}
