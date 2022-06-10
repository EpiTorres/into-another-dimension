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

public class Bivector4D
{
    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Constructors

    public float XY = 0; // Coefficient for the XY plane
    public float XZ = 0; // Coefficient for the XZ plane
    public float XW = 0; // Coefficient for the XW plane
    public float YZ = 0; // Coefficient for the YZ plane
    public float YW = 0; // Coefficient for the YW plane
    public float ZW = 0; // Coefficient for the ZW plane

    /*------------------------------------------------------------------
     * Creates a Bivector from the given coefficients
     *------------------------------------------------------------------*/
    public Bivector4D(float XY, float XZ, float XW, float YZ, float YW, float ZW)
    {
        this.XY = XY;
        this.XZ = XZ;
        this.XW = XW;
        this.YZ = YZ;
        this.YW = YW;
        this.ZW = ZW;
    }

    /*------------------------------------------------------------------
     * Creates a Bivector by performing the wedge product on two vectors
     * 
     * Code modified from https://marctenbosch.com/quaternions/code.htm
     *------------------------------------------------------------------*/
    public Bivector4D(Vector4 u, Vector4 v)
    {
        this.XY = u.x * v.y - u.y * v.x;
        this.XZ = u.x * v.z - u.z * v.x;
        this.XW = u.x * v.w - u.w * v.x;
        this.YZ = u.y * v.z - u.z * v.y;
        this.YW = u.y * v.w - u.w * v.y;
        this.ZW = u.z * v.w - u.w * v.z;
    }

    public Bivector4D() : this(0, 0, 0, 0, 0, 0) { }
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Mathematical Operators

    /*------------------------------------------------------------------
     * Addition and subtraction operations
     *------------------------------------------------------------------*/
    public static Bivector4D operator +(Bivector4D p) => p;
    public static Bivector4D operator -(Bivector4D p)
        => new Bivector4D(-p.XY, -p.XZ, -p.XW, -p.YZ, -p.YW, -p.ZW);

    public static Bivector4D operator +(Bivector4D p, Bivector4D q)
        => new Bivector4D(p.XY + q.XY, p.XZ + q.XZ, p.XW + q.XW, p.YZ + q.YZ, p.YW + q.YW, p.ZW + q.ZW);
    public static Bivector4D operator -(Bivector4D p, Bivector4D q) => p + (-q);

    /*------------------------------------------------------------------
     * Wedge product between a bivector and a vector
     *------------------------------------------------------------------*/
    public static Vector4 operator ^(Vector4 v, Bivector4D bv)
    {
        float X = v.y * bv.ZW - v.z * bv.YW + v.w * bv.YZ;
        float Y = -v.x * bv.ZW + v.z * bv.XW - v.w * bv.XZ;
        float Z = v.x * bv.YW - v.y * bv.XW + v.w * bv.XY;
        float W = -v.x * bv.YZ + v.y * bv.XZ - v.z * bv.XY;

        return new Vector4(X, Y, Z, W);
    }
    public static Vector4 operator ^(Bivector4D bv, Vector4 v) => v ^ bv;

    /*------------------------------------------------------------------
     * Scalar multiplication of a float and a Bivector
     *------------------------------------------------------------------*/
    public static Bivector4D operator *(float s, Bivector4D p)
        => new Bivector4D(s * p.XY, s * p.XZ, s * p.XW, s * p.YZ, s * p.YW, s * p.ZW);
    public static Bivector4D operator *(Bivector4D bv, float s) => s * bv;

    /*------------------------------------------------------------------
     * Geometric product between a vector and a bivector
     *------------------------------------------------------------------*/
    public static Vector4 operator *(Vector4 v, Bivector4D bv)
    {
        float X = v.w * bv.YZ - v.z * bv.YW + v.y * bv.ZW - v.y * bv.XY - v.z * bv.XZ - v.w * bv.XW;
        float Y = -bv.XZ * v.w + bv.XW * v.z - bv.ZW * v.x + v.x * bv.XY - v.z * bv.YZ - v.w * bv.YW;
        float Z = bv.XY * v.w - bv.XW * v.y + bv.YW * v.x + v.x * bv.XZ + v.y * bv.YZ - v.w * bv.ZW;
        float W = -bv.XY * v.z + bv.XZ * v.y - bv.YZ * v.x + v.x * bv.XW + v.y * bv.YW + v.z * bv.ZW;

        return new Vector4(X, Y, Z, W);
    }
    public static Vector4 operator *(Bivector4D bv, Vector4 v)
    {
        float X = bv.YZ * v.w - bv.YW * v.z + bv.ZW * v.y + bv.XY * v.y + bv.XZ * v.z + bv.XW * v.w;
        float Y = -bv.XZ * v.w + bv.XW * v.z - bv.ZW * v.x - bv.XY * v.x + bv.YZ * v.z + bv.YW * v.w;
        float Z = bv.XY * v.w - bv.XW * v.y + bv.YW * v.x - bv.XZ * v.x - bv.YZ * v.y + bv.ZW * v.w;
        float W = -bv.XY * v.z + bv.XZ * v.y - bv.YZ * v.x - bv.XW * v.x - bv.YW * v.y - bv.ZW * v.z;

        return new Vector4(X, Y, Z, W);
    }

    /*------------------------------------------------------------------
     * Geometric product between two bivectors
     *------------------------------------------------------------------*/
    public static Bivector4D operator *(Bivector4D bu, Bivector4D bv)
    {
        // Coefficient for XY plane
        float XY =  - bu.XZ * bv.YZ - bu.XW * bv.YW + bu.YZ * bv.XZ + bu.YW * bv.XW;
        // Coefficient for XZ plane
        float XZ = bu.XY * bv.YZ - bu.XW * bv.ZW - bu.YZ * bv.XY + bu.ZW * bv.XW;
        // Coefficient for XW plane
        float XW = bu.XY * bv.YW + bu.XZ * bv.ZW - bu.YW * bv.XY - bu.ZW * bv.XZ;
        // Coefficient for YZ plane
        float YZ = - bu.XY * bv.XZ + bu.XZ * bv.XY - bu.YW * bv.ZW + bu.ZW * bv.YW;
        // Coefficient for YW plane
        float YW = - bu.XY * bv.XW + bu.XW * bv.XY + bu.YZ * bv.ZW - bu.ZW * bv.YZ;
        // Coefficient for ZW plane
        float ZW = - bu.XZ * bv.XW + bu.XW * bv.XZ - bu.YZ * bv.YW + bu.YW * bv.YZ;

        return new Bivector4D(XY, XZ, XW, YZ, YW, ZW);
    }

    /*------------------------------------------------------------------
     * Returns a vector representing the left contraction of the given
     * bivector and vector
     *------------------------------------------------------------------*/
    public Vector4 LeftContraction(Vector4 v)
    {
        float X = -v.y * this.XY - v.z * this.XZ - v.w * this.XW;
        float Y = v.x * this.XY - v.z * this.YZ - v.w * this.YW;
        float Z = v.x * this.XZ + v.y * this.YZ - v.w * this.ZW;
        float W = v.x * this.XW + v.y * this.YW + v.z * this.ZW;
        return new Vector4(X, Y, Z, W);
    }
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Public Methods

    /*------------------------------------------------------------------
     * Returns the squared length of this bivector
     *------------------------------------------------------------------*/
    private float LengthSqrd()
    {
        return this.XY * this.XY + this.XZ * this.XZ + this.XW * this.XW + this.YZ * this.YZ
            + this.YW * this.YW + this.ZW * this.ZW;
    }

    /*------------------------------------------------------------------
     * Returns the length of this Bivector
     *------------------------------------------------------------------*/
    private float Length()
    {
        return Mathf.Sqrt(LengthSqrd());
    }

    /*------------------------------------------------------------------
     * Normalizes this bivector
     *------------------------------------------------------------------*/
    public void Normalize()
    {
        float l = Length();
        if (l > Mathf.Epsilon)
        {
            this.XY /= l; this.XZ /= l; this.XW /= l;
            this.YZ /= l; this.YW /= l; this.ZW /= l;
        }
    }

    /*------------------------------------------------------------------
     * Returns a new vector corresponding to the normalized version of 
     * this rotor
     *------------------------------------------------------------------*/
    public Bivector4D Normalized()
    {
        float l = Length();
        if (l > Mathf.Epsilon) return (1F / l) * this;
        else return this;
    }

    /*------------------------------------------------------------------
     * Returns the result of the component-wise multiplication of the
     * given bivectors
     *------------------------------------------------------------------*/
    public Bivector4D ComponentMultiply(Bivector4D bv)
    {
        float XY = this.XY * bv.XY;
        float XZ = this.XZ * bv.XZ;
        float XW = this.XW * bv.XW;
        float YZ = this.YZ * bv.YZ;
        float YW = this.YW * bv.YW;
        float ZW = this.ZW * bv.ZW;

        return new Bivector4D(XY, XZ, XW, YZ, YW, ZW);
    }


    /*------------------------------------------------------------------
     * Returns a string representation of this bivector
     *------------------------------------------------------------------*/
    public override string ToString()
    {
        return string.Format("({0})XY + ({1})XZ + ({2})XW + ({3})YZ + ({4})YW + ({5})ZW", 
            this.XY, this.XZ, this.XW, this.YZ, this.YW, this.ZW);
    }
    ////////////////////////////////////////////////////////////////////
}
