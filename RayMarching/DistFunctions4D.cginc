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
 *		- https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
 *-------------------------------------------------------------------*/


//------------------------------------------------------------------------
// Helper Functions ------------------------------------------------------
float dot2(float2 v) { return dot(v, v); }
float dot2(float3 v) { return dot(v, v); }
float ndot(float2 a, float2 b) { return a.x * b.x - a.y * b.y; }

float2x2 Rot(float a) {
    float s = sin(a);
    float c = cos(a);
    return float2x2(c, -s, s, c);
}

//------------------------------------------------------------------------
// SDF Operators ------------------------------------------------------

// Union
float4 opUnion(float4 d1, float4 d2)
{
	return (d1.w < d2.w) ? d1 : d2;
}

// Smooth Union
// Color of the resulting pixel is an interpolation between the two colors
float4 opSmoothUnion(float4 d1, float4 d2, float k)
{
	float h = clamp(0.5 + 0.5 * (d2.w - d1.w) / k, 0.0, 1.0);
	float dist = lerp(d2.w, d1.w, h) - k * h * (1.0 - h);
	float3 color = lerp(d2.rgb, d1.rgb, h);
	return float4(color, dist);
}

// Subtraction
// Color of the resulting pixel is is the color of d1
float4 opSubtraction(float4 d1, float4 d2)
{
    return float4(d1.rgb, max(d1.w, -d2.w));
}

// Smooth Subtraction
// Color of the resulting pixel is the color of d1
float4 opSmoothSubtraction(float4 d1, float4 d2, float k)
{
    float h = clamp(0.5 - 0.5 * (d1.w + d2.w) / k, 0.0, 1.0);
    float dist = lerp(d1.w, -d2.w, h) + k * h * (1.0 - h);
    return float4(d1.rgb, dist);
}

// Intersection
// Color of the resulting pixel is the color of d1
float4 opIntersection(float4 d1, float4 d2)
{
    return float4(d1.rgb, max(d1.w, d2.w));
}

// Smooth Intersection
// Color of the resulting pixel is the color of d1
float4 opSmoothIntersection(float4 d1, float4 d2, float k)
{
	float h = clamp(0.5 - 0.5 * (d1.w - d2.w) / k, 0.0, 1.0);
	float dist = lerp(d1.w, d2.w, h) + k * h * (1.0 - h);
    return float4(d1.rgb, dist);
}


//------------------------------------------------------------------------
// Planes ----------------------------------------------------------------
// Regular Plane
float sdPlane4D(float4 p, float4 n, float offset)
{
	// n must be normalized
    return dot(p, n) - offset;
}

//------------------------------------------------------------------------
// Spheres ---------------------------------------------------------------

float sdSphere4D(float4 p, float s)
{
    return length(p) - s;
}

//------------------------------------------------------------------------
// Boxes -----------------------------------------------------------------

// Regular Box
// b: size of box x/y/z/w
float sdBox4D(float4 p, float4 b)
{
    float4 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, max(q.z, q.w))), 0.0);
}

// Rounded Box
// b: size of box x/y/z/w
float sdRndBox4D(float4 p, float4 b, float r)
{
    return sdBox4D(p, b) - r;
}


// Box Frame
// b: size of box x/y/z/w
float sdBoxFrame4D(float4 p, float4 b, float e)
{
	p = abs(p) - b;
	float4 q = abs(p + e) - e;
    return min(min(min(
		length(max(float4(p.x, q.y, q.z, q.w), 0.0)) + min(max(p.x, max(q.y, max(q.z, q.w))), 0.0),
		length(max(float4(q.x, p.y, q.z, q.w), 0.0)) + min(max(q.x, max(p.y, max(q.z, q.w))), 0.0)),
		length(max(float4(q.x, q.y, p.z, q.w), 0.0)) + min(max(q.x, max(q.y, max(p.z, q.w))), 0.0)),
		length(max(float4(q.x, q.y, q.z, p.w), 0.0)) + min(max(q.x, max(q.y, max(q.z, p.w))), 0.0));
}


//------------------------------------------------------------------------
// Capsules -------------------------------------------------------------- 

// Capsule
float sdCapsule4D(float4 p, float4 a, float4 b, float r)
{
	float3 pa = p - a, ba = b - a;
	float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
	return length(pa - ba * h) - r;
}

// Vertical Capsule
float sdVrtCapsule4D(float4 p, float h, float r)
{
	p.y -= clamp(p.y, 0.0, h);
	return length(p) - r;
}

// Vertical Capsule (Fixed)
float sdVrtFCapsule4D(float4 p, float h, float r)
{
	p.y += h / 2.0f;
	p.y -= clamp(p.y, 0.0, h);
	
	return length(p) - r;
}


// Vertical Cylinder
float sdVrtCylinder4D(float4 p, float h, float r)
{
	float2 d = abs(float2(length(p.xzw), p.y)) - float2(r, h);
	return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}