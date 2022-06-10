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

public class CapsuleBoxCol
{
    /*------------------------------------------------------------------
     * If the capsule and box colliders are intersecting, returns an 
     * array with one ContactPoint4D. Otherwise, returns an empty 
     * ContactPoint4D array.
     *------------------------------------------------------------------*/
    public static ContactPoint4D[] CheckCollision(CapsuleCollider4D c, BoxCollider4D b)
    {
        // Gets the closest point on the capsule's line segment to the box's center
        // This point is treated as the center of a sphere for the collision detection
        Vector4 sphereCenter = c.ClosestPointOnLineSegment(b.center);

        // Finds the closest point on the box to the sphere's center
        Vector4 closestPoint = b.ClosestPointOnSurface(sphereCenter);

        Vector4 distVec = sphereCenter - closestPoint;
        float separation = distVec.magnitude - c.radius;
        Vector4 normal = distVec.normalized;

        ContactPoint4D[] contactPoints = new ContactPoint4D[0];
        if (separation < Mathf.Epsilon)
        {
            // The contact point with respect to the sphere's surface
            Vector4 p1 = sphereCenter - normal * c.radius;
            // The contact point with respect to the box's surface
            Vector4 p2 = closestPoint;
            contactPoints = new ContactPoint4D[]
            {
                new ContactPoint4D(p1, p2, normal, separation)
            };
        }

        return contactPoints;
    }
}
