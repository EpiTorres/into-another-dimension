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

public class CapsulePlaneCol
{
    /*------------------------------------------------------------------
     * If the the capsule is intersecting the plane collider, 
     * returns a ContactPoint4D array with the contact points. Otherwise, 
     * returns an empty ContactPoint4D array.
     *------------------------------------------------------------------*/
    public static ContactPoint4D[] CheckCollision(CapsuleCollider4D c, PlaneCollider4D p)
    {
        // Calculates the distance of the endpoints from the plane
        float s0 = MathUtil.PointDistFromHyperPlane(p.normal, p.pointOnPlane, c.endPointA) - c.radius;
        float s1 = MathUtil.PointDistFromHyperPlane(p.normal, p.pointOnPlane, c.endPointB) - c.radius;

        ContactPoint4D[] contactPoints = new ContactPoint4D[0];

        // If both of the line segment's endpoints are below the plane, puts both endpoints
        // in the ContactPoint4D array. Otherwise, puts only the endpoint below the plane
        // in the array.

        if (s0 < Mathf.Epsilon && s1 < Mathf.Epsilon)
        {
            Vector4 p1 = c.endPointA - p.normal * c.radius;
            Vector4 p2 = c.endPointB - p.normal * c.radius;
            contactPoints = new ContactPoint4D[] {
                    new ContactPoint4D(p1, p.normal, s0),
                    new ContactPoint4D(p2, p.normal, s1),
            };
        }
        else if (s0 < Mathf.Epsilon)
        {
            Vector4 p1 = c.endPointA - p.normal * c.radius;
            contactPoints = new ContactPoint4D[] { new ContactPoint4D(p1, p.normal, s0) };
        }
        else if (s1 < Mathf.Epsilon)
        {
            Vector4 p1 = c.endPointB - p.normal * c.radius;
            contactPoints = new ContactPoint4D[] { new ContactPoint4D(p1, p.normal, s1) };
        }
        return contactPoints;
    }
}
