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

public class BoxBoxCol : MonoBehaviour
{

    /*------------------------------------------------------------------
     * Returns an array that consists of all possible normals 
     * corresponding to potential separating hyperplanes
     *------------------------------------------------------------------*/
    private static Vector4[] GetSeparatingAxes(BoxCollider4D b1, BoxCollider4D b2)
    {
        Vector4[] axes = new Vector4[]
        {
            b1.X.normalized,
            b1.Y.normalized,
            b1.Z.normalized,
            b1.W.normalized,

            (b1.X ^ new Bivector4D(b2.X, b2.Y)).normalized,
            (b1.X ^ new Bivector4D(b2.X, b2.Z)).normalized,
            (b1.X ^ new Bivector4D(b2.X, b2.W)).normalized,
            (b1.X ^ new Bivector4D(b2.Y, b2.Z)).normalized,
            (b1.X ^ new Bivector4D(b2.Y, b2.W)).normalized,
            (b1.X ^ new Bivector4D(b2.Z, b2.W)).normalized,

            (b1.Y ^ new Bivector4D(b2.X, b2.Y)).normalized,
            (b1.Y ^ new Bivector4D(b2.X, b2.Z)).normalized,
            (b1.Y ^ new Bivector4D(b2.X, b2.W)).normalized,
            (b1.Y ^ new Bivector4D(b2.Y, b2.Z)).normalized,
            (b1.Y ^ new Bivector4D(b2.Y, b2.W)).normalized,
            (b1.Y ^ new Bivector4D(b2.Z, b2.W)).normalized,

            (b1.Z ^ new Bivector4D(b2.X, b2.Y)).normalized,
            (b1.Z ^ new Bivector4D(b2.X, b2.Z)).normalized,
            (b1.Z ^ new Bivector4D(b2.X, b2.W)).normalized,
            (b1.Z ^ new Bivector4D(b2.Y, b2.Z)).normalized,
            (b1.Z ^ new Bivector4D(b2.Y, b2.W)).normalized,
            (b1.Z ^ new Bivector4D(b2.Z, b2.W)).normalized,

            (b1.W ^ new Bivector4D(b2.X, b2.Y)).normalized,
            (b1.W ^ new Bivector4D(b2.X, b2.Z)).normalized,
            (b1.W ^ new Bivector4D(b2.X, b2.W)).normalized,
            (b1.W ^ new Bivector4D(b2.Y, b2.Z)).normalized,
            (b1.W ^ new Bivector4D(b2.Y, b2.W)).normalized,
            (b1.W ^ new Bivector4D(b2.Z, b2.W)).normalized,

            b2.X,
            b2.Y,
            b2.Z,
            b2.W,

            (b2.X ^ new Bivector4D(b1.X, b1.Y)).normalized,
            (b2.X ^ new Bivector4D(b1.X, b1.Z)).normalized,
            (b2.X ^ new Bivector4D(b1.X, b1.W)).normalized,
            (b2.X ^ new Bivector4D(b1.Y, b1.Z)).normalized,
            (b2.X ^ new Bivector4D(b1.Y, b1.W)).normalized,
            (b2.X ^ new Bivector4D(b1.Z, b1.W)).normalized,

            (b2.Y ^ new Bivector4D(b1.X, b1.Y)).normalized,
            (b2.Y ^ new Bivector4D(b1.X, b1.Z)).normalized,
            (b2.Y ^ new Bivector4D(b1.X, b1.W)).normalized,
            (b2.Y ^ new Bivector4D(b1.Y, b1.Z)).normalized,
            (b2.Y ^ new Bivector4D(b1.Y, b1.W)).normalized,
            (b2.Y ^ new Bivector4D(b1.Z, b1.W)).normalized,

            (b2.Z ^ new Bivector4D(b1.X, b1.Y)).normalized,
            (b2.Z ^ new Bivector4D(b1.X, b1.Z)).normalized,
            (b2.Z ^ new Bivector4D(b1.X, b1.W)).normalized,
            (b2.Z ^ new Bivector4D(b1.Y, b1.Z)).normalized,
            (b2.Z ^ new Bivector4D(b1.Y, b1.W)).normalized,
            (b2.Z ^ new Bivector4D(b1.Z, b1.W)).normalized,

            (b2.W ^ new Bivector4D(b1.X, b1.Y)).normalized,
            (b2.W ^ new Bivector4D(b1.X, b1.Z)).normalized,
            (b2.W ^ new Bivector4D(b1.X, b1.W)).normalized,
            (b2.W ^ new Bivector4D(b1.Y, b1.Z)).normalized,
            (b2.W ^ new Bivector4D(b1.Y, b1.W)).normalized,
            (b2.W ^ new Bivector4D(b1.Z, b1.W)).normalized
        };

        return axes;
    }


    /*------------------------------------------------------------------
     * If the two box colliders are intersecting, returns a 
     * ContactPoint4D array with the contact points. Otherwise, returns
     * an empty ContactPoint4D array.
     *------------------------------------------------------------------*/
    public static ContactPoint4D[] CheckCollision(BoxCollider4D b1, BoxCollider4D b2)
    {
        // Gets the potential separating axes
        Vector4[] axes = GetSeparatingAxes(b1, b2);

        bool isSeparated = false;
        Vector4 minVec = axes[0];
        float leastPen = float.MaxValue;

        // Loops through all vertices in b1 and b2 in order to project them onto
        // each potential separating axis to determine if there is a separating axis
        for (int i = 0; i < axes.Length; i++)
        {
            if (!(axes[i] == Vector4.zero))
            {
                float aMin = float.MaxValue;
                float aMax = float.MinValue;
                float bMin = float.MaxValue;
                float bMax = float.MinValue;

                for (int j = 0; j < b1.Vertices.Length; j++)
                {
                    float aDist = Vector4.Dot(b1.Vertices[j].point, axes[i]);
                    aMin = aDist < aMin ? aDist : aMin;
                    aMax = aDist > aMax ? aDist : aMax;

                    float bDist = Vector4.Dot(b2.Vertices[j].point, axes[i]);
                    bMin = bDist < bMin ? bDist : bMin;
                    bMax = bDist > bMax ? bDist : bMax;
                }

                var longSpan = Mathf.Max(aMax, bMax) - Mathf.Min(aMin, bMin);
                var sumSpan = aMax - aMin + bMax - bMin;

                if (longSpan >= sumSpan)
                {
                    isSeparated = true;
                    break;
                }
                else if (leastPen > (sumSpan - longSpan))
                {
                    minVec = axes[i];
                    leastPen = sumSpan - longSpan;
                }
            }
        }

        // If there is no separating axis, returns the contact manifold corresponding
        // to the least separating axis
        ContactPoint4D[] contactPoints = new ContactPoint4D[0];
        if (!isSeparated) 
        {
            // Corrects the direction of the minimum translation vector
            if (Vector4.Dot(b1.center - b2.center, minVec) < 0) minVec *= -1;
            contactPoints = SutherlandHodgman.GetContactManifold(b1, b2, minVec, -leastPen);
        }
        return contactPoints;

    }
}
