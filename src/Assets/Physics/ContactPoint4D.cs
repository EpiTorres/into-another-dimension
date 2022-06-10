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
 *-------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ContactPoint4D
{
    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Constructors

    public Vector4 p1; // Contact point (on the first object's surface)
    public Vector4 p2; // Contact point (on the second object's surface)
    public Vector4 normal; // Collision normal
    public Vector4 tangent; // Tangential velocity vector
    public float separation; // Distance between the two objects
    public Vector4 r1, r2; // Direction vectors from b1 and b2 to the position
    public float Pn = 0;   // Accumulated normal impulse
    public float Pt = 0;   // Accumulated tangent impulse
    public float massNormal; // Mass along the normal
    public float massTangent; // Mass along the tangent vector
    public float bias; // Bias factor for correcting impulses

    public ContactPoint4D(Vector4 position, Vector4 normal, float separation)
    {
        this.p1 = position;
        this.p2 = position;
        this.normal = normal;
        this.separation = separation;
    }

    public ContactPoint4D(Vector4 p1, Vector4 p2, Vector4 normal, float separation)
    {
        Vector4 midPoint = (p1 + p2) / 2;
        this.p1 = midPoint;
        this.p2 = midPoint;
        this.normal = normal;
        this.separation = separation;
    }
    ////////////////////////////////////////////////////////////////////
}