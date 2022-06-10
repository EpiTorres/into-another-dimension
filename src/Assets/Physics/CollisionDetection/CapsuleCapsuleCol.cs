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

public class CapsuleCapsuleCol
{
    /*------------------------------------------------------------------
     * If the two capsule colliders are intersecting, returns an array 
     * with one ContactPoint4D. Otherwise, returns an empty 
     * ContactPoint4D array.
     *------------------------------------------------------------------*/
    public static ContactPoint4D[] CheckCollision(CapsuleCollider4D c1, CapsuleCollider4D c2)
    {
        // Calculates the squared distances of the between the capsule's endpoints
        float d0 = (c2.endPointA - c1.endPointA).magnitude;
        float d1 = (c2.endPointB - c1.endPointA).magnitude;
        float d2 = (c2.endPointA - c1.endPointB).magnitude;
        float d3 = (c2.endPointB - c1.endPointB).magnitude;

        // Selects the closest endpoint from capsule 1 to capsule 2
        Vector4 c1Center = (d2 < d0 || d2 < d1 || d3 < d0 || d3 < d1)
            ? c1.endPointB : c1.endPointA;

        // Calculates the point on capsule 2 that's closest to the selected endpoint
        Vector4 c2Center = c2.ClosestPointOnLineSegment(c1Center);

        // Calculates the point on capsule 1 that's closest to the point on capsule 2
        c1Center = c1.ClosestPointOnLineSegment(c2Center);

        // Calculates the collision information and returns the contact points
        Vector4 dir = c1Center - c2Center;
        float separation = dir.magnitude - (c1.radius + c2.radius);
        Vector4 normal = dir.normalized;

        ContactPoint4D[] contactPoints = new ContactPoint4D[0];
        if (separation < Mathf.Epsilon)
        {
            Vector4 p1 = c1Center - normal * c1.radius;
            Vector4 p2 = c2Center + normal * c2.radius;
            contactPoints = new ContactPoint4D[] { new ContactPoint4D(p1, p2, normal, separation) };
        }
        return contactPoints;
    }
}
