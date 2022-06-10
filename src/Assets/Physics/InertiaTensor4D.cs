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

public class InertiaTensor4D
{
    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Constructors

    // Diagonal entries of the inertia tensor
    public float m00, m11, m22, m33, m44, m55 = 0f;

    /*------------------------------------------------------------------
     * Creates a diagonal inertia tensor with the given diagonal entries
     *------------------------------------------------------------------*/
    public InertiaTensor4D(float m00, float m11, float m22, 
        float m33, float m44, float m55)
    {
        this.m00 = m00;
        this.m11 = m11;
        this.m22 = m22;
        this.m33 = m33;
        this.m44 = m44;
        this.m55 = m55;
    }

    /*------------------------------------------------------------------
     * Creates a diagonal inertia tensor whose diagonal entries are 1
     *------------------------------------------------------------------*/
    public InertiaTensor4D() : this(1, 1, 1, 1, 1, 1) { }
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Mathematical Operators

    /*------------------------------------------------------------------
     * Matrix multiplication
     *------------------------------------------------------------------*/
    public static Bivector4D operator *(InertiaTensor4D i, Bivector4D bv)
    {
        float XY = i.m00 * bv.XY;
        float XZ = i.m11 * bv.XZ;
        float XW = i.m22 * bv.XW;
        float YZ = i.m33 * bv.YZ;
        float YW = i.m44 * bv.YW;
        float ZW = i.m55 * bv.ZW;

        return new Bivector4D(XY, XZ, XW, YZ, YW, ZW);
    }
    ////////////////////////////////////////////////////////////////////
    

    ////////////////////////////////////////////////////////////////////
    // Public Methods

    /*------------------------------------------------------------------
     * Returns the inverse of this tensor. Please note that all zero
     * components will remain zero (to prevent division by 0)
     *------------------------------------------------------------------*/
    public InertiaTensor4D Inverse()
    {
        float m00 = Mathf.Approximately(this.m00, 0) ? 0 : (1 / this.m00);
        float m11 = Mathf.Approximately(this.m11, 0) ? 0 : (1 / this.m11);
        float m22 = Mathf.Approximately(this.m22, 0) ? 0 : (1 / this.m22);
        float m33 = Mathf.Approximately(this.m33, 0) ? 0 : (1 / this.m33);
        float m44 = Mathf.Approximately(this.m44, 0) ? 0 : (1 / this.m44);
        float m55 = Mathf.Approximately(this.m55, 0) ? 0 : (1 / this.m55);

        return new InertiaTensor4D(m00, m11, m22, m33, m44, m55);
    }


    /*------------------------------------------------------------------
     * Calculates the inertia tensor of a hypersphere with the given 
     * radius and mass
     *------------------------------------------------------------------*/
    public static InertiaTensor4D GetSphereInertia(float radius, float mass)
    {
        float moi = (1 / 3F) * mass * radius * radius;
        return new InertiaTensor4D(moi, moi, moi, moi, moi, moi);
    }


    /*------------------------------------------------------------------
     * Calculates the inertia tensor of a hypercapsule with the given 
     * radius, height, and mass
     *------------------------------------------------------------------*/
    public static InertiaTensor4D GetCapsuleInertia(float radius, float height, float mass)
    {
        // Calculates a ratio for determining the mass of the spherinder
        // and hemispheres from a total mass value
        float ratio = (3F * Mathf.PI * radius * radius) / (8F * height);

        // Calculates the parallel and perpendicular moments of inertia
        // for one hemisphere
        float massOfHemisphere = mass / (2F * (1F + (1F / ratio)));
        // distance from the center of mass of the hemisphere
        float d = (16F * radius) / (15F * Mathf.PI);
        float moiPerpendicular = (1F / 3F) * radius * radius * massOfHemisphere;
        float moiParallel = massOfHemisphere * ((1F / 3F) * radius * radius
            + (height * height) / 4F
            + (height * d));

        // Multiplies the parallel and perpendicular moments of inertia
        // by two to account for the two hemispheres
        moiParallel *= 2F;
        moiPerpendicular *= 2F;

        // Calculates the moment of inertia of the spherinder
        float massOfSpherinder = mass / (1F + ratio);

        // Adds the moment of inertia of the spherinder to the
        // to the parallel and perpendicular moments of inertia
        moiPerpendicular += (2F / 5F) * massOfSpherinder * radius * radius;
        moiParallel += (1F / 3F) * massOfSpherinder * ((2F / 5F) * radius * radius + height * height / 4);


        return new InertiaTensor4D(moiParallel, moiPerpendicular, moiPerpendicular,
            moiParallel, moiParallel, moiPerpendicular);
    }


    /*------------------------------------------------------------------
     * Calculates the inertia tensor of a hyperbox with the given 
     * scale and mass
     *------------------------------------------------------------------*/
    public static InertiaTensor4D GetBoxInertia(Vector4 scale, float mass)
    {
        Vector4 sSqrd = Vector4.Scale(scale, scale);
        return new InertiaTensor4D(
            (1 / 12F) * mass * (sSqrd.z + sSqrd.w),
            (1 / 12F) * mass * (sSqrd.y + sSqrd.w),
            (1 / 12F) * mass * (sSqrd.y + sSqrd.z),
            (1 / 12F) * mass * (sSqrd.x + sSqrd.w),
            (1 / 12F) * mass * (sSqrd.x + sSqrd.z),
            (1 / 12F) * mass * (sSqrd.x + sSqrd.y)
        );
    }
    ////////////////////////////////////////////////////////////////////
}