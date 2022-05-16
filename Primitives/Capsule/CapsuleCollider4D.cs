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

[ExecuteInEditMode]
public class CapsuleCollider4D : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Event Method
    public Transform4D transform4D;
    public RigidBody4D rigidBody;

    public Vector4 center;
    public Rotor4D rotor;
    public Matrix4x4 rotation;

    public float radius;
    public float height;
    public Vector4 endPointA;
    public Vector4 endPointB;

    public void Start()
    {
        this.transform4D = this.GetComponent<Transform4D>();
        this.rigidBody = this.GetComponent<RigidBody4D>();
        this.rigidBody.SetCapsuleInertia(this.radius, this.height);
        this.UpdateCollider();
    }
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Public Helper Methods

    /*------------------------------------------------------------------
     * Updates the collider's variables using its transform
     *------------------------------------------------------------------*/
    public void UpdateCollider()
    {
        this.center = this.transform4D.position;
        this.rotor = this.transform4D.rotor;
        this.rotation = this.rotor.ToMatrix();
        this.CalculateEndPoints();
    }


    /*------------------------------------------------------------------
     * Returns the closest point on the capsule's underlying line 
     * segment to a given point
     *------------------------------------------------------------------*/
    public Vector4 ClosestPointOnLineSegment(Vector4 p)
    {
        Vector4 ab = endPointB - endPointA;
        Vector4 ap = p - endPointA;
        float t = Vector4.Dot(ap, ab) / ab.sqrMagnitude;
        return endPointA + Mathf.Min(Mathf.Max(t, 0), 1) * ab;
    }

    ////////////////////////////////////////////////////////////////////
    // Private Helper Methods

    /*------------------------------------------------------------------
     * Calculates the endpoints of the line segment spanning the capsule
     *------------------------------------------------------------------*/
    private void CalculateEndPoints()
    {
        Vector4 rotatedHeight = rotor.Rotate(new Vector4(0, height / 2.0F, 0, 0));
        endPointA = center + rotatedHeight;
        endPointB = center - rotatedHeight;
    }
    ////////////////////////////////////////////////////////////////////
}
