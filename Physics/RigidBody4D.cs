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

public class RigidBody4D : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Event Methods

    // Transform of the rigid body's object
    public Transform4D transform4D;
    // Boolean that determines if the body can move linearly
    public bool fixedLinear = true;
    // Boolean that determines if the body can rotate
    public bool fixedAngular = true;
    // Mass of the rigid body
    [Range(1, 10)]
    public float mass = 1;
    // Coefficition of friction of the rigid body
    [Range(0, 1)]
    public float frictionStrength = 0.5F;
    // Vector representing the body's velocity
    public Vector4 velocity = Vector4.zero;
    // Bivector representing the body's angular velocity
    public Bivector4D angularVelocity = new Bivector4D();
    // Position of the body's center of mass
    public Vector4 centerOfMass = Vector4.zero;
    // Moment of inertia about the center of mass
    public InertiaTensor4D inertia = new InertiaTensor4D();
    // Inverse moment of inertia about the center of mass
    public InertiaTensor4D invInertia = new InertiaTensor4D();
    

    public void Start()
    {
        this.transform4D = this.GetComponent<Transform4D>();
    }
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Methods for Setting the Rigid Body's Inertia

    /*------------------------------------------------------------------
     * Sets the moment of inertia of this rigid body (and updates
     * its corresponding instanc variables), assuming that the body
     * corresponds to a sphere
     *------------------------------------------------------------------*/
    public void SetSphereInertia(float radius)
    {
        this.inertia = InertiaTensor4D.GetSphereInertia(radius, mass);
        this.invInertia = this.inertia.Inverse();
    }


    /*------------------------------------------------------------------
     * Sets the moment of inertia of this rigid body (and updates
     * its corresponding instanc variables), assuming that the body
     * corresponds to a capsule
     *------------------------------------------------------------------*/
    public void SetCapsuleInertia(float radius, float height)
    {
        this.inertia = InertiaTensor4D.GetCapsuleInertia(radius, height, mass);
        this.invInertia = this.inertia.Inverse();
    }


    /*------------------------------------------------------------------
     * Sets the moment of inertia of this rigid body (and updates
     * its corresponding instanc variables), assuming that the body
     * corresponds to a box
     *------------------------------------------------------------------*/
    public void SetBoxInertia()
    {
        Vector4 scale = transform4D.scale;
        this.inertia = InertiaTensor4D.GetBoxInertia(scale, mass);
        this.invInertia = this.inertia.Inverse();
    }


    ////////////////////////////////////////////////////////////////////
    // Methods for Updating the Rigid Body

    /*------------------------------------------------------------------
     * Adds the given vector to the body's velocity (if the body can
     * move linearly)
     *------------------------------------------------------------------*/
    public void AddVelocity(Vector4 newVelocity)
    {
        if (!this.fixedLinear) this.velocity += newVelocity;
    }


    /*------------------------------------------------------------------
     * Adds the given bivector to the body's angular velocity (if the 
     * body can rotate)
     *------------------------------------------------------------------*/
    public void AddAngularVelocity(Bivector4D angularVelocity)
    {
        if (!this.fixedAngular) this.angularVelocity += angularVelocity;
    }


    /*------------------------------------------------------------------
     * Given the time step, updates the position and rotation of this
     * body and its corresponding transform
     *------------------------------------------------------------------*/
    public void UpdateTransform(float t)
    {
        if (!this.fixedLinear)
        {
            transform4D.position += this.velocity * t;
            this.centerOfMass = transform4D.position;
        }
        if (!this.fixedAngular)
        {
            transform4D.rotor -= (0.5F * this.angularVelocity) * transform4D.rotor * t;
            transform4D.rotor.Normalize();
        }
    }
    ////////////////////////////////////////////////////////////////////
}
