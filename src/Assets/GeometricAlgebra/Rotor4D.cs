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
 *         
 * This code was adapted from:
 *      - https://marctenbosch.com/quaternions/code.htm
 *-------------------------------------------------------------------*/
using UnityEngine;


public class Rotor4D
{
    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Constructors

    private float a = 1;    // Scalar
    private float XY = 0;   // Bivector coefficient for XY plane
    private float XZ = 0;   // Bivector coefficient for XZ plane
    private float XW = 0;   // Bivector coefficient for XW plane
    private float YZ = 0;   // Bivector coefficient for YZ plane
    private float YW = 0;   // Bivector coefficient for YW plane
    private float ZW = 0;   // Bivector coefficient for ZW plane
    private float XYZW = 0; // Pseudoscalar

    // Matrix Representation of the Rotor
    private Matrix4x4 rotationMatrix = Matrix4x4.identity;

    /*------------------------------------------------------------------
     * Creates a Rotor from the given coefficients
     *------------------------------------------------------------------*/
    public Rotor4D(float a, float XY, float XZ, float XW, float YZ, 
        float YW, float ZW, float XYZW)
    {
        this.a = a;

        this.XY = XY;
        this.XZ = XZ;
        this.XW = XW;
        this.YZ = YZ;
        this.YW = YW;
        this.ZW = ZW;

        this.XYZW = XYZW;

        this.rotationMatrix = this.CalculateMatrix();
    }

    /*------------------------------------------------------------------
     * Creates a Rotor from the given plane and angle in radians
     *------------------------------------------------------------------*/
    public Rotor4D(Bivector4D plane, float rad)
    {

        this.a = Mathf.Cos(rad / 2.0F);

        float sinAngle = Mathf.Sin(rad / 2.0F);
        this.XY = -sinAngle * plane.XY;
        this.XZ = -sinAngle * plane.XZ;
        this.XW = -sinAngle * plane.XW;
        this.YZ = -sinAngle * plane.YZ;
        this.YW = -sinAngle * plane.YW;
        this.ZW = -sinAngle * plane.ZW;

        this.rotationMatrix = this.CalculateMatrix();
    }

    public Rotor4D() : this(1, 0, 0, 0, 0, 0, 0, 0) { }
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Overloaded Operators

    /*------------------------------------------------------------------
     * Addition and subtraction operators
     *------------------------------------------------------------------*/
    public static Rotor4D operator +(Rotor4D p) => p;
    public static Rotor4D operator -(Rotor4D p) 
        => new Rotor4D(-p.a, -p.XY, -p.XZ, -p.XW, -p.YZ, -p.YW, -p.ZW, -p.XYZW);
    public static Rotor4D operator +(Rotor4D p, Rotor4D q)
        => new Rotor4D(p.a+q.a, p.XY+q.XY, p.XZ+q.XZ, p.XW+q.XW, p.YZ+q.YZ, p.YW+q.YW, p.ZW+q.ZW, p.XYZW+q.XYZW);
    public static Rotor4D operator -(Rotor4D p, Rotor4D q) => p + (-q);


    /*------------------------------------------------------------------
     * Geometric multiplication of two Rotors
     *------------------------------------------------------------------*/
    public static Rotor4D operator *(Rotor4D p, Rotor4D q)
    {
        float a = p.a*q.a - p.XY*q.XY - p.XZ*q.XZ - p.XW*q.XW - p.YZ*q.YZ - p.YW*q.YW - p.ZW*q.ZW + p.XYZW*q.XYZW;
        float XY = p.a*q.XY + p.XY*q.a - p.XZ*q.YZ - p.XW*q.YW + p.YZ*q.XZ + p.YW*q.XW - p.ZW*q.XYZW - p.XYZW*q.ZW;
        float XZ = p.a*q.XZ + p.XY*q.YZ + p.XZ*q.a - p.XW*q.ZW - p.YZ*q.XY + p.YW * q.XYZW + p.ZW*q.XW  + p.XYZW*q.YW;
        float XW = p.a*q.XW + p.XY*q.YW + p.XZ*q.ZW + p.XW*q.a - p.YW*q.XY - p.YZ * q.XYZW - p.ZW*q.XZ  - p.XYZW*q.YZ;
        float YZ = p.a*q.YZ - p.XY*q.XZ + p.XZ*q.XY - p.XW * q.XYZW + p.YZ*q.a - p.YW*q.ZW + p.ZW*q.YW - p.XYZW*q.XW;
        float YW = p.a*q.YW - p.XY*q.XW + p.XZ * q.XYZW + p.XW*q.XY + p.YZ*q.ZW + p.YW*q.a - p.ZW*q.YZ + p.XYZW*q.XZ;
        float ZW = p.a*q.ZW - p.XY * q.XYZW - p.XZ*q.XW + p.XW*q.XZ - p.YZ*q.YW + p.YW*q.YZ + p.ZW*q.a - p.XYZW*q.XY;
        float XYZW = p.a*q.XYZW + p.XY*q.ZW - p.XZ*q.YW + p.XW*q.YZ + p.YZ*q.XW - p.YW*q.XZ + p.ZW*q.XY + p.XYZW*q.a;

        return new Rotor4D(a, XY, XZ, XW, YZ, YW, ZW, XYZW);

    }

    /*------------------------------------------------------------------
     * Geometric multiplication between a bivector and a rotor
     *------------------------------------------------------------------*/
    public static Rotor4D operator *(Bivector4D bv, Rotor4D r)
    {
        float a = -bv.XY * r.XY - bv.XZ * r.XZ - bv.XW * r.XW - bv.YZ * r.YZ - bv.YW * r.YW - bv.ZW * r.ZW;
        float XY = bv.XY * r.a - bv.XZ * r.YZ - bv.XW * r.YW + bv.YZ * r.XZ + bv.YW * r.XW - bv.ZW * r.XYZW;
        float XZ = bv.XY * r.YZ + bv.XZ * r.a - bv.XW * r.ZW - bv.YZ * r.XY + bv.YW * r.XYZW + bv.ZW * r.XW;
        float XW = bv.XY * r.YW + bv.XZ * r.ZW + bv.XW * r.a - bv.YZ * r.XYZW - bv.YW * r.XY - bv.ZW * r.XZ;
        float YZ = -bv.XY * r.XZ + bv.XZ * r.XY - bv.XW * r.XYZW + bv.YZ * r.a - bv.YW * r.ZW + bv.ZW * r.YW;
        float YW = -bv.XY * r.XW + bv.XZ * r.XYZW + bv.XW * r.XY + bv.YZ * r.ZW + bv.YW * r.a - bv.ZW * r.YZ;
        float ZW = -bv.XY * r.XYZW - bv.XZ * r.XW + bv.XW * r.XZ - bv.YZ * r.YW + bv.YW * r.YZ + bv.ZW * r.a;
        float XYZW = bv.XY * r.ZW - bv.XZ * r.YW + bv.XW * r.YZ + bv.YZ * r.XW - bv.YW * r.XZ + bv.ZW * r.XY;

        return new Rotor4D(a, XY, XZ, XW, YZ, YW, ZW, XYZW);
    }

    /*------------------------------------------------------------------
     * Scalar multiplication of a float and a rotor
     *------------------------------------------------------------------*/
    public static Rotor4D operator *(float s, Rotor4D p)
        => new Rotor4D(s * p.a, s * p.XY, s * p.XZ, s * p.XW, s * p.YZ, s * p.YW, s * p.ZW, s * p.XYZW);
    public static Rotor4D operator *(Rotor4D p, float s) => s * p;
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Private Helper Methods

    /*------------------------------------------------------------------
     * Calculates the matrix representation of this rotor
     *------------------------------------------------------------------*/
    private Matrix4x4 CalculateMatrix()
    {
        // First column
        Vector4 v0 = new Vector4(
            this.a * this.a - this.XY * this.XY - this.XZ * this.XZ - this.XW * this.XW + this.YZ * this.YZ + this.YW * this.YW + this.ZW * this.ZW - this.XYZW * this.XYZW,
            -this.a * this.XY - this.XY * this.a - this.XZ * this.YZ - this.XW * this.YW - this.YZ * this.XZ - this.YW * this.XW - this.ZW * this.XYZW - this.XYZW * this.ZW,
            -this.a * this.XZ + this.XY * this.YZ - this.XZ * this.a - this.XW * this.ZW + this.YZ * this.XY + this.YW * this.XYZW - this.ZW * this.XW + this.XYZW * this.YW,
            -this.a * this.XW + this.XY * this.YW + this.XZ * this.ZW - this.XW * this.a - this.YZ * this.XYZW + this.YW * this.XY + this.ZW * this.XZ - this.XYZW * this.YZ
        );

        // Second column
        Vector4 v1 = new Vector4(
            this.XY * this.a + this.a * this.XY - this.YZ * this.XZ - this.YW * this.XW - this.XZ * this.YZ - this.XW * this.YW + this.XYZW * this.ZW + this.ZW * this.XYZW,
            -this.XY * this.XY + this.a * this.a - this.YZ * this.YZ - this.YW * this.YW + this.XZ * this.XZ + this.XW * this.XW - this.XYZW * this.XYZW + this.ZW * this.ZW,
            -this.XY * this.XZ - this.a * this.YZ - this.YZ * this.a - this.YW * this.ZW - this.XZ * this.XY - this.XW * this.XYZW - this.XYZW * this.XW - this.ZW * this.YW,
            -this.XY * this.XW - this.a * this.YW + this.YZ * this.ZW - this.YW * this.a + this.XZ * this.XYZW - this.XW * this.XY + this.XYZW * this.XZ + this.ZW * this.YZ
        );


        // Third column
        Vector4 v2 = new Vector4(
            this.XZ * this.a + this.YZ * this.XY + this.a * this.XZ - this.ZW * this.XW + this.XY * this.YZ - this.XYZW * this.YW - this.XW * this.ZW - this.YW * this.XYZW,
            -this.XZ * this.XY + this.YZ * this.a + this.a * this.YZ - this.ZW * this.YW - this.XY * this.XZ + this.XYZW * this.XW + this.XW * this.XYZW - this.YW * this.ZW,
            -this.XZ * this.XZ - this.YZ * this.YZ + this.a * this.a - this.ZW * this.ZW + this.XY * this.XY - this.XYZW * this.XYZW + this.XW * this.XW + this.YW * this.YW,
            -this.XZ * this.XW - this.YZ * this.YW - this.a * this.ZW - this.ZW * this.a - this.XY * this.XYZW - this.XYZW * this.XY - this.XW * this.XZ - this.YW * this.YZ
        );

        // Fourth column
        Vector4 v3 = new Vector4(
            this.XW * this.a + this.YW * this.XY + this.ZW * this.XZ + this.a * this.XW + this.XYZW * this.YZ + this.XY * this.YW + this.XZ * this.ZW + this.YZ * this.XYZW,
            -this.XW * this.XY + this.YW * this.a + this.ZW * this.YZ + this.a * this.YW - this.XYZW * this.XZ - this.XY * this.XW - this.XZ * this.XYZW + this.YZ * this.ZW,
            -this.XW * this.XZ - this.YW * this.YZ + this.ZW * this.a + this.a * this.ZW + this.XYZW * this.XY + this.XY * this.XYZW - this.XZ * this.XW - this.YZ * this.YW,
            -this.XW * this.XW - this.YW * this.YW - this.ZW * this.ZW + this.a * this.a - this.XYZW * this.XYZW + this.XY * this.XY + this.XZ * this.XZ + this.YZ * this.YZ
        );

        return new Matrix4x4(v0, v1, v2, v3);
    }
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Utility Helper Methods

    /*------------------------------------------------------------------
     * Returns the reversed version of this rotor
     *------------------------------------------------------------------*/
    public Rotor4D Reverse()
    {
        return new Rotor4D(this.a, -this.XY, -this.XZ, -this.XW, -this.YZ, -this.YW, -this.ZW, this.XYZW);
    }


    /*------------------------------------------------------------------
     * Returns the squared length of this rotor
     *------------------------------------------------------------------*/
    public float LengthSqrd()
    {
        return this.a * this.a + this.XY * this.XY + this.XZ * this.XZ
            + this.XW * this.XW + this.YZ * this.YZ + this.YW * this.YW
            + this.ZW * this.ZW + this.XYZW * this.XYZW;
    }


    /*------------------------------------------------------------------
     * Returns the length of this rotor
     *------------------------------------------------------------------*/
    public float Length()
    {
        return Mathf.Sqrt(LengthSqrd());
    }


    /*------------------------------------------------------------------
     * Normalizes this rotor
     *------------------------------------------------------------------*/
    public void Normalize()
    {
        float l = Length();
        if (l > Mathf.Epsilon)
        {
            this.a /= l; this.XY /= l; this.XZ /= l; this.XW /= l;
            this.YZ /= l; this.YW /= l; this.ZW /= l; this.XYZW /= l;
        }
    }

    /*------------------------------------------------------------------
     * Returns a new vector corresponding to the normalized version of 
     * this rotor
     *------------------------------------------------------------------*/
    public Rotor4D Normalized()
    {
        float l = Length();
        if (l > Mathf.Epsilon) return (1F / l) * this;
        else return this;
    }

    /*------------------------------------------------------------------
     * Unoptimized rotation of a given vector with this rotor.
     * 
     *------------------------------------------------------------------*/
    public Vector4 Rotate(Vector4 v)
    {
        // q = R v (4D Case)
        float qX = this.a * v.x + this.XY * v.y + this.XZ * v.z + this.XW * v.w;
        float qY = (-this.XY * v.x) + this.a * v.y + this.YZ * v.z + this.YW * v.w;
        float qZ = (-this.XZ * v.x) + (-this.YZ * v.y) + this.a * v.z + this.ZW * v.w;
        float qW = (-this.XW * v.x) + (-this.YW * v.y) + (-this.ZW * v.z) + this.a * v.w;

        // Trivectors
        float qXYZ = this.YZ * v.x + (-this.XZ * v.y) + this.XY * v.z + this.XYZW * v.w;
        float qXYW = this.YW * v.x + (-this.XW * v.y) + (-this.XYZW * v.z) + this.XY * v.w;
        float qXZW = this.ZW * v.x + this.XYZW * v.y + (-this.XW * v.z) + this.XZ * v.w;
        float qYZW = (-this.XYZW * v.x) + this.ZW * v.y + (-this.YW * v.z) + this.YZ * v.w;

        // r = q R*
        Vector4 r;
        r.x = qX * this.a - (-qY * this.XY) - (-qZ * this.XZ) - (-qW * this.XW) - (-qXYZ * this.YZ) - (-qXYW * this.YW) - (-qXZW * this.ZW) + qYZW * this.XYZW;
        r.y = qY * this.a - qX * this.XY - qXYZ * this.XZ - qXYW * this.XW - (-qZ * this.YZ) - (-qW * this.YW) - (-qYZW * this.ZW) + (-qXZW * this.XYZW);
        r.z = qZ * this.a - (-qXYZ * this.XY) - qX * this.XZ - qXZW * this.XW - qY * this.YZ - qYZW * this.YW - (-qW * this.ZW) + qXYW * this.XYZW;
        r.w = qW * this.a - (-qXYW * this.XY) - (-qXZW * this.XZ) - qX * this.XW - (-qYZW * this.YZ) - qY * this.YW - qZ * this.ZW + (-qXYZ * this.XYZW);

        return r;
    }


    /*------------------------------------------------------------------
     * Returns the matrix representation of the rotor
     *------------------------------------------------------------------*/
    public Matrix4x4 ToMatrix()
    {
        return this.rotationMatrix;
    }

    /*------------------------------------------------------------------
     * Returns a string representation of this rotor
     *------------------------------------------------------------------*/
    public override string ToString()
    {
        return string.Format("{0} + ({1})XY + ({2})XZ + ({3})XW + ({4})YZ + ({5})YW + ({6})ZW + ({7})XYZW",
            this.a, this.XY, this.XZ, this.XW, this.YZ, this.YW, this.ZW, this.XYZW);
    }
    ////////////////////////////////////////////////////////////////////
}