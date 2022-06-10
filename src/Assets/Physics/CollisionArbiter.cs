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

public class CollisionArbiter
{
    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Constructors

    // Number of contact points
    public int numOfContacts = 0;
    // Array of contact points
    public ContactPoint4D[] contactPoints;
    // Rigid body of the first object
    public RigidBody4D b1;
    // Rigid body of the second object
    public RigidBody4D b2;
    // Coefficient of friction
    public float friction = 0.5F;

    /*------------------------------------------------------------------
     * Constructor for an arbiter between a sphere and a capsule
     *------------------------------------------------------------------*/
    public CollisionArbiter(SphereCollider4D a, SphereCollider4D b)
    {
        contactPoints = SphereSphereCol.CheckCollision(a, b);
        numOfContacts = contactPoints.Length;

        b1 = a.rigidBody;
        b2 = b.rigidBody;
        friction = Mathf.Max(b1.frictionStrength, b2.frictionStrength);
    }

    /*------------------------------------------------------------------
     * Constructor for an arbiter between a sphere and a capsule
     *------------------------------------------------------------------*/
    public CollisionArbiter(SphereCollider4D a, CapsuleCollider4D b)
    {
        contactPoints = SphereCapsuleCol.CheckCollision(a, b);
        numOfContacts = contactPoints.Length;

        b1 = a.rigidBody;
        b2 = b.rigidBody;
        friction = Mathf.Max(b1.frictionStrength, b2.frictionStrength);
    }

    /*------------------------------------------------------------------
     * Creates a Rotor from the given coefficients
     *------------------------------------------------------------------*/
    public CollisionArbiter(SphereCollider4D a, BoxCollider4D b)
    {
        contactPoints = SphereBoxCol.CheckCollision(a, b);
        numOfContacts = contactPoints.Length;

        b1 = a.rigidBody;
        b2 = b.rigidBody;
        friction = Mathf.Max(b1.frictionStrength, b2.frictionStrength);
    }

    /*------------------------------------------------------------------
     * Creates a Rotor from the given coefficients
     *------------------------------------------------------------------*/
    public CollisionArbiter(SphereCollider4D a, PlaneCollider4D b)
    {
        contactPoints = SpherePlaneCol.CheckCollision(a, b);
        numOfContacts = contactPoints.Length;

        b1 = a.rigidBody;
        b2 = b.rigidBody;
        friction = Mathf.Max(b1.frictionStrength, b2.frictionStrength);
    }

    /*------------------------------------------------------------------
     * Creates a Rotor from the given coefficients
     *------------------------------------------------------------------*/
    public CollisionArbiter(CapsuleCollider4D a, CapsuleCollider4D b)
    {
        contactPoints = CapsuleCapsuleCol.CheckCollision(a, b);
        numOfContacts = contactPoints.Length;

        b1 = a.rigidBody;
        b2 = b.rigidBody;
        friction = Mathf.Max(b1.frictionStrength, b2.frictionStrength);
    }

    /*------------------------------------------------------------------
     * Creates a Rotor from the given coefficients
     *------------------------------------------------------------------*/
    public CollisionArbiter(CapsuleCollider4D a, BoxCollider4D b)
    {
        contactPoints = CapsuleBoxCol.CheckCollision(a, b);
        numOfContacts = contactPoints.Length;

        b1 = a.rigidBody;
        b2 = b.rigidBody;
        friction = Mathf.Max(b1.frictionStrength, b2.frictionStrength);
    }

    /*------------------------------------------------------------------
     * Creates a Rotor from the given coefficients
     *------------------------------------------------------------------*/
    public CollisionArbiter(CapsuleCollider4D a, PlaneCollider4D b)
    {
        contactPoints = CapsulePlaneCol.CheckCollision(a, b);
        numOfContacts = contactPoints.Length;

        b1 = a.rigidBody;
        b2 = b.rigidBody;
        friction = Mathf.Max(b1.frictionStrength, b2.frictionStrength);
    }

    /*------------------------------------------------------------------
     * Creates a Rotor from the given coefficients
     *------------------------------------------------------------------*/
    public CollisionArbiter(BoxCollider4D a, BoxCollider4D b)
    {
        contactPoints = BoxBoxCol.CheckCollision(a, b);
        numOfContacts = contactPoints.Length;

        b1 = a.rigidBody;
        b2 = b.rigidBody;
        friction = Mathf.Max(b1.frictionStrength, b2.frictionStrength);
    }

    /*------------------------------------------------------------------
     * Creates a Rotor from the given coefficients
     *------------------------------------------------------------------*/
    public CollisionArbiter(BoxCollider4D a, PlaneCollider4D b)
    {
        contactPoints = BoxPlaneCol.CheckCollision(a, b);
        numOfContacts = contactPoints.Length;

        b1 = a.rigidBody;
        b2 = b.rigidBody;
        friction = Mathf.Max(b1.frictionStrength, b2.frictionStrength);
    }
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Methods for Solving Collisions

    /*------------------------------------------------------------------
     * Precalculates the important info for the contact resolution
     *------------------------------------------------------------------*/
    public void PreSolve(float invDT)
    {
        float allowedPenetration = ImpulseSolver.instance.allowedPenetration;
        float biasFactor = ImpulseSolver.instance.biasFactor;

        for (int i = 0; i < numOfContacts; i++)
        {
            ContactPoint4D contact = contactPoints[i];
            Vector4 normal = contact.normal;
            contact.r1 = contact.p1 - b1.centerOfMass;
            contact.r2 = contact.p2 - b2.centerOfMass;

            // Calculates the velocity vector that is tangent to the collision normal
            Vector4 dv = b1.velocity + b1.angularVelocity.LeftContraction(contact.r1);
            Vector4 tangent = (dv - Vector4.Dot(dv, normal) * normal).normalized;
            contact.tangent = tangent;

            float massNormal = 0;
            float massTangent = 0;

            // Handles the linear components for the mass calculations
            if (!b1.fixedLinear)
            {
                float invMass = 1F / b1.mass;
                massNormal += invMass;
                massTangent += invMass;
            }
            if (!b2.fixedLinear)
            {
                float invMass = 1F / b2.mass;
                massNormal += invMass; // todo replace with a precomputed invMass value
                massTangent += invMass;
            }

            // Handles the angular components for the mass calculations
            if (!b1.fixedAngular)
            {
                Bivector4D nrInert = b1.invInertia * new Bivector4D(contact.r1, normal);
                Vector4 nLeftContraction = nrInert.LeftContraction(contact.r1);
                massNormal += Vector4.Dot(nLeftContraction, normal);


                Bivector4D trInert = b1.invInertia * new Bivector4D(contact.r1, contact.tangent);
                Vector4 tLeftContraction = trInert.LeftContraction(contact.r1);
                massTangent += Vector4.Dot(tLeftContraction, contact.tangent);
            }
            if (!b2.fixedAngular)
            {
                Bivector4D nrInert = b2.invInertia * new Bivector4D(contact.r2, normal);
                Vector4 nLeftContraction = nrInert.LeftContraction(contact.r2);
                massNormal += Vector4.Dot(nLeftContraction, normal);

                Bivector4D trInert = b2.invInertia * new Bivector4D(contact.r2, contact.tangent);
                Vector4 tLeftContraction = trInert.LeftContraction(contact.r2);
                massTangent += Vector4.Dot(tLeftContraction, contact.tangent);
            }

            // Updates the contact point's mass along the normal and along the tangent
            contact.massNormal = 1F / massNormal;
            contact.massTangent = 1F / massTangent;

            // Updates the contact point's bias value
            contact.bias = -biasFactor * invDT * Mathf.Min(0F, contact.separation + allowedPenetration);
        }
    }

    /*------------------------------------------------------------------
     * Solves the current collision between the two colliding objects
     * by applying equal and opposite impulses to their rigid bodies
     *------------------------------------------------------------------*/
    public void Solve()
    {
        for (int i = 0; i < contactPoints.Length; i++)
        {
            ContactPoint4D contact = contactPoints[i];
            Vector4 normal = contact.normal;

            //--------------------------------------------------------
            // Normal Impulse Calculations

            // Relative velocity at contact after the impulses have been updated
            Vector4 dv = b1.velocity + b1.angularVelocity.LeftContraction(contact.r1) 
                - b2.velocity - b2.angularVelocity.LeftContraction(contact.r2);

            // Compute the normal impulse
            float vN = Vector4.Dot(dv, normal);
            float dPn = contact.massNormal * (-vN + contact.bias);
            float Pn0 = contact.Pn;
            contact.Pn = Mathf.Max(Pn0 + dPn, 0);
            dPn = contact.Pn - Pn0;

            // Applies the contact impulse to the rigid bodies
            Vector4 Pn = dPn * normal;
            ApplyImpulse(Pn, contact.r1, contact.r2);

            //--------------------------------------------------------
            // Frictional Impulse Calculations

            if (ImpulseSolver.instance.enableFriction)
            {
                float vt = -Vector4.Dot(dv, contact.tangent);
                float dPt = contact.massTangent * vt;

                // Computes the friction impulse
                float maxPt = this.friction * contact.Pn;

                // Clamps the friction
                float oldTangentImpulse = contact.Pt;
                contact.Pt = Mathf.Clamp(oldTangentImpulse + dPt, -maxPt, maxPt);
                dPt = contact.Pt - oldTangentImpulse;

                // Applies the contact impulse to the rigid bodies
                Vector4 Pt = dPt * contact.tangent;
                ApplyImpulse(Pt, contact.r1, contact.r2);
            }
        }
    }

    ////////////////////////////////////////////////////////////////////
    // Private Helper Methods

    /*------------------------------------------------------------------
     * Applies the given impulse to the CollisionArbiter's rigid bodies
     *------------------------------------------------------------------*/
    private void ApplyImpulse(Vector4 impulse, Vector4 r1, Vector4 r2)
    {
        b1.AddVelocity((1F / b1.mass) * impulse);
        b1.AddAngularVelocity(b1.invInertia * new Bivector4D(r1, impulse));
        b2.AddVelocity(-(1F / b2.mass) * impulse);
        b2.AddAngularVelocity(-(b2.invInertia * new Bivector4D(r2, impulse)));
    }
    ////////////////////////////////////////////////////////////////////
}
