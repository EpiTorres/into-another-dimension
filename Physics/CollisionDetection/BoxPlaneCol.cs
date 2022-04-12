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

public class BoxPlaneCol : MonoBehaviour
{
    /*------------------------------------------------------------------
     * If the the box collider is intersecting the plane collider, 
     * returns a ContactPoint4D array with the contact points. Otherwise, 
     * returns an empty ContactPoint4D array.
     *------------------------------------------------------------------*/
    public static ContactPoint4D[] CheckCollision(BoxCollider4D a, PlaneCollider4D b)
    {
        // Uses the plane normal as the axis for the Separating Axis Theorem
        Vector4 axis = b.normal;
        float planeOffset = Vector4.Dot(b.normal, b.pointOnPlane);

        // Loops through all vertices to project them onto the axis
        float aMin = float.MaxValue;
        float aMax = float.MinValue;
        for (int j = 0; j < a.Vertices.Length; j++)
        {
            float aDist = Vector4.Dot(a.Vertices[j].point, axis);
            aMin = aDist < aMin ? aDist : aMin;
            aMax = aDist > aMax ? aDist : aMax;
        }

        ContactPoint4D[] contactPoints = new ContactPoint4D[0];
        if (aMin < planeOffset && planeOffset < aMax) 
            contactPoints = SutherlandHodgman.GetContactManifold(a, b, axis, aMin - planeOffset);   

        return contactPoints;
    }
}
