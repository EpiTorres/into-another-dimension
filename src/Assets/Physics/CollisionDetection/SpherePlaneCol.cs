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

public class SpherePlaneCol
{
    /*------------------------------------------------------------------
     * If the the sphere collider is intersecting the plane collider, 
     * returns an array with one ContactPoint4D. Otherwise, returns an 
     * empty ContactPoint4D array.
     *------------------------------------------------------------------*/
    public static ContactPoint4D[] CheckCollision(SphereCollider4D s, PlaneCollider4D p)
    {
        float D = Vector4.Dot(p.normal, p.pointOnPlane);
        float dist = (Vector4.Dot(s.center, p.normal) - D) / p.normal.sqrMagnitude;
        float separation = dist - s.radius;

        ContactPoint4D[] contactPoints = new ContactPoint4D[0];
        if (separation < Mathf.Epsilon)
        {
            contactPoints = new ContactPoint4D[] { new ContactPoint4D(s.center - p.normal * s.radius, p.normal, separation) };
        }
        return contactPoints;

    }
}
