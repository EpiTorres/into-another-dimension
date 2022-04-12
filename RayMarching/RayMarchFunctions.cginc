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


//////////////////////////////////////////////////////////////////////
// Helper Functions for Generating the SDF

/*------------------------------------------------------------------
 * Returns the color and distance of the ith plane from the given
 * the given position vector.
 *------------------------------------------------------------------*/
float4 generatePlane(float4 p, int i)
{
    return float4(_pCol[i].rgb, sdPlane4D(p, _pNorm[i], _pOff[i]));
}


/*------------------------------------------------------------------
 * Returns the color and distance of the ith box from the given
 * the given position vector.
 *------------------------------------------------------------------*/
float4 generateBox(float4 p, int i)
{
    p = p - _bPos[i]; // Translates the box
    p = mul(p, _bRot[i]); // Rotates the box
    return float4(_bCol[i].rgb, sdBox4D(p, _bScale[i]));
}


/*------------------------------------------------------------------
 * Returns the color and distance of the ith sphere from the given
 * the given position vector.
 *------------------------------------------------------------------*/
float4 generateSphere(float4 p, int i)
{
    p = p - _sPos[i]; // Translates the sphere
    p = mul(p, _sRot[i]); // Rotates the sphere
    return float4(_sCol[i].rgb, sdSphere4D(p, _sRad[i]));
}


/*------------------------------------------------------------------
 * Returns the color and distance of the ith capsule from the given
 * the given position vector.
 *------------------------------------------------------------------*/
float4 generateCapsule(float4 p, int i)
{
    p = p - _cPos[i]; // Translates the capsule
    p = mul(p, _cRot[i]); // Rotates the capsule
    p.y += _cHt[i] / 2.0F;
    return float4(_cCol[i].rgb, sdVrtCapsule4D(p, _cHt[i], _cRad[i]));
}


/*------------------------------------------------------------------
 * Returns the color and distance of the ith capsule from the given
 * the given position vector.
 *------------------------------------------------------------------*/
float4 distanceField(float4 p)
{
    // Generates the first object in the space
    float4 sdf = generatePlane(p, 0);
    
    // Generates the planes
    for (int i = 1; i < _pCount; i++)
    {
        sdf = opUnion(sdf, generatePlane(p, i));
    }
    
    // Generates the boxes
    for (int j = 0; j < _bCount; j++)
    {
        sdf = opUnion(sdf, generateBox(p, j));
    }
    
    // Generates the spheres
    for (int l = 0; l < _sCount; l++) 
    {
        sdf = opUnion(sdf, generateSphere(p, l));
    }

    // Generates the spheres
    for (int m = 0; m < _cCount; m++)
    {
        sdf = opUnion(sdf, generateCapsule(p, m));
    }
    
    return sdf;
}
//////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////
// Helper Functions for Performing Ray Marching

/*------------------------------------------------------------------
 * Calculates the normal of the SDF at the given point. This code
 * was adapted from:
 * https://www.iquilezles.org/www/articles/normalsSDF/normalsSDF.htm
 * https://www.youtube.com/playlist?list=PL3POsQzaCw53iK_EhOYR39h1J9Lvg-m-g
 *------------------------------------------------------------------*/
float4 calcNormal(float4 p)
{
    const float eps = 0.0001;
    const float2 h = float2(eps, 0.0);

    // Derivative with respect to the x-axis
    float dX = distanceField(p + h.xyyy).w - distanceField(p - h.xyyy).w;
    // Derivative with respect to the y-axis
    float dY = distanceField(p + h.yxyy).w - distanceField(p - h.yxyy).w;
    // Derivative with respect to the z-axis
    float dZ = distanceField(p + h.yyxy).w - distanceField(p - h.yyxy).w;
    // Derivative with respect to the w-axis
    float dW = distanceField(p + h.yyyx).w - distanceField(p - h.yyyx).w;

    return normalize(float4(dX, dY, dZ, dW));
}
       

/*------------------------------------------------------------------
 * Calculates the soft shadows. This code was adapted from:
 * https://www.iquilezles.org/www/articles/rmshadows/rmshadows.htm
 * https://www.youtube.com/playlist?list=PL3POsQzaCw53iK_EhOYR39h1J9Lvg-m-g
 *------------------------------------------------------------------*/
float softShadows(float4 ro, float4 rd, float4 n)
{
    float res = 1.0;
    float ph = 1e20;
    for (float t = _shadeMinDist; t < _shadeMaxDist;)
    {
        float h = distanceField(ro + rd * t).w;
        if (h < _prec)
        {
            return 0.0;
        }

        float y = h * h / (2.0 * ph);
        float d = sqrt(h * h - y * y);

        res = min(res, _shadeSmooth * d / max(0.0, t - y));
        ph = h;
        t += h;
    }
    return res * clamp(dot(n, rd), 0, 1);
}

            
/*------------------------------------------------------------------
 * Calculates the lighting and shading of the given position. This 
 * code was adapted from:
 * https://www.youtube.com/playlist?list=PL3POsQzaCw53iK_EhOYR39h1J9Lvg-m-g
 *------------------------------------------------------------------*/
float3 shading(float4 p, float4 n, fixed3 c)
{     
    // Diffuse color
    float3 dCol = c.rgb * _colInt;
    // Lambertian lighting model
    float3 l = _lightCol * dot(-_lightDir, n);
    l = l * 0.5 + 0.5; // Scales the lighting so that it is between [0.5, 1]
    // Soft shadows
    float s = softShadows(p, -_lightDir, n);
    s = s * 0.5 + 0.5; // Scales the soft shadows so that they are between [0.5, 1]

    return dCol * (l * _lightInt) * (pow(s, _shadeInt));
}
            

/*------------------------------------------------------------------
 * Uses the sphere tracing approach to implement the ray marching
 * algorithm. Returns true if an object was hit or false otherwise.
 * This code was adapted from:
 * https://www.youtube.com/playlist?list=PL3POsQzaCw53iK_EhOYR39h1J9Lvg-m-g
 *------------------------------------------------------------------*/
bool rayMarching(float4 ro, float4 rd, float depth, float maxT, int maxI, inout float4 p, inout fixed3 c)
{
    bool hit = false;
    float t = 0.0f; // Distance traveled along the ray direction

    for (int i = 0; i < maxI; i++)
    {
        // Draw environment
        if (t >= maxT || t >= depth) break;

        p = ro + rd * t;
        
        // Check for hit in distance field
        float4 d = distanceField(p);
        float h = d.w;
        if (h < _prec)
        {
            c = d.rgb;
            hit = true;
            break;
        }

        t += h;
    }

    return hit;
}
//////////////////////////////////////////////////////////////////////