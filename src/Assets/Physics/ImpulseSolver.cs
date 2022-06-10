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
public class ImpulseSolver : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Event Methods

    // Number of sequential impulse solver iterations
    public int iterations = 15;
    // Size of the time step
    public float dt = 0.015F; 
    // Acceleration due to gravity
    public Vector4 gravity = new Vector4(0, -10, 0, 0);
    [Range(0, 0.5F)]
    // Allowed penetration of colliding objects
    public float allowedPenetration = 0.1F;
    [Range(0, 0.5F)]
    // Bias factor that corrects penetrating objects based on depth
    public float biasFactor = 0.2F;
    // Boolean that determines if friction will be calculated
    public bool enableFriction = true;


    // Array of the sphere colliders in the scene
    private SphereCollider4D[] spheres;
    // Number of sphere colliders in the scene
    private int sphereCount = 0;
    // Array of the capsule colliders in the scene
    private CapsuleCollider4D[] capsules;
    // Number of capsule colliders in the scene
    private int capsuleCount = 0;
    // Array of the box colliders in the scene
    private BoxCollider4D[] boxes;
    // Number of box colliders in the scene
    private int boxCount = 0;
    // Array of the plane colliders in the scene
    private PlaneCollider4D[] planes;
    // Number of plane colliders in the scene
    private int planeCount = 0;
    // Array of collision solvers for collision resolution
    private CollisionArbiter[] solvers;
    // Number of collision solvers
    private int numOfSolvers = 0;

    /*------------------------------------------------------------------
     * Sets up the ImpulseSolver to act as a singleton
     *------------------------------------------------------------------*/
    public static ImpulseSolver instance { get; private set; }
    public void Awake()
    {
        // Ensures that there is only one instance of the ImpulseSolver
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    /*------------------------------------------------------------------
     * Gets all of the primitives that are children of this script's
     * object. Updates the value of the numOfSolvers value and the 
     * associated solvers array based on the number of child objects
     * in the scene
     *------------------------------------------------------------------*/
    public void Start()
    {
        spheres = this.GetComponentsInChildren<SphereCollider4D>();
        sphereCount = spheres.Length;

        capsules = this.GetComponentsInChildren<CapsuleCollider4D>();
        capsuleCount = capsules.Length;

        boxes = this.GetComponentsInChildren<BoxCollider4D>();
        boxCount = boxes.Length;

        planes = this.GetComponentsInChildren<PlaneCollider4D>();
        planeCount = planes.Length;

        // Calculates the number of solvers needed for pairwise
        // collision detection
        if (sphereCount > 1)
        {
            numOfSolvers += (sphereCount * (sphereCount - 1)) / 2;
        }
        numOfSolvers += sphereCount * capsuleCount;
        numOfSolvers += sphereCount * boxCount;
        numOfSolvers += sphereCount * planeCount;

        if (capsuleCount > 1)
        {
            numOfSolvers += (capsuleCount * (capsuleCount - 1)) / 2;
        }
        numOfSolvers += capsuleCount * boxCount;
        numOfSolvers += capsuleCount * planeCount;

        if (boxCount > 1)
        {
            numOfSolvers += (boxCount * (boxCount - 1)) / 2;
        }
        numOfSolvers += boxCount * planeCount;

        // Initializes a new array with the appropriate number of solvers
        solvers = new CollisionArbiter[numOfSolvers];
    }

    /*------------------------------------------------------------------
     * FixedUpdate is called 50 times per second
     *------------------------------------------------------------------*/
    void FixedUpdate()
    {
        float invDT = dt > 0 ? (1F / dt) : 0;

        // The broad phase collision detection
        BroadPhase();

        // Integrates the forces (only gravity at the moment)
        IntegrateForces();

        // Performs the presolve step of the iterations
        for (int i = 0; i < numOfSolvers; i++)
        {
            if (solvers[i] != null) solvers[i].PreSolve(invDT);
        }

        // Repeatedly iterates over the solvers to apply sequential impulses
        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < numOfSolvers; j++) if (solvers[j] != null) solvers[j].Solve();
        }

        // Integrates the velocities
        IntegrateVelocities();
    }
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Private Helper Methods

    /*------------------------------------------------------------------
     * Loops through each pair of objects in order to check for 
     * collisions. If a collision has happened, the corresponding
     * collision solver in the array will be set; otherwise, the 
     * corresponding array element will be set to null
     *------------------------------------------------------------------*/
    private void BroadPhase()
    {
        int solverIdx = 0;

        for (int i = 0; i < sphereCount; i++)
        {
            SphereCollider4D a = spheres[i];
            for (int j = i+1; j < sphereCount; j++)
            {
                SphereCollider4D b = spheres[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }

            
            for (int j = 0; j < capsuleCount; j++)
            {
                CapsuleCollider4D b = capsules[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }
        
            for (int j = 0; j < boxCount; j++)
            {
                BoxCollider4D b = boxes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }
            

            for (int j = 0; j < planeCount; j++)
            {
                PlaneCollider4D b = planes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }
        }


        for (int i = 0; i < capsuleCount; i++)
        {
            CapsuleCollider4D a = capsules[i];
            for (int j = i + 1; j < capsuleCount; j++)
            {
                CapsuleCollider4D b = capsules[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }

            for (int j = 0; j < boxCount; j++)
            {
                BoxCollider4D b = boxes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }

            for (int j = 0; j < planeCount; j++)
            {
                PlaneCollider4D b = planes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }
        }


        for (int i = 0; i < boxCount; i++)
        {
            BoxCollider4D a = boxes[i];
            for (int j = i+1; j < boxCount; j++)
            {
                BoxCollider4D b = boxes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }
            

            for (int j = 0; j < planeCount; j++)
            {
                PlaneCollider4D b = planes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }
        }
    }

    /*------------------------------------------------------------------
     * Integrates the forces acting on each rigid body. At the moment,
     * this only includes the force of gravity
     *------------------------------------------------------------------*/
    private void IntegrateForces()
    {
        for (int i = 0; i < sphereCount; i++)
        {
            spheres[i].rigidBody.AddVelocity(gravity * dt);
        }

        for (int j = 0; j < capsuleCount; j++)
        {
            capsules[j].rigidBody.AddVelocity(gravity * dt);
        }
        
        for (int i = 0; i < boxCount; i++)
        {
            boxes[i].rigidBody.AddVelocity(gravity * dt);
        }
    }

    /*------------------------------------------------------------------
     * Integrates the velocities of the rigid bodies and updates their 
     * corresponding transforms and colliders
     *------------------------------------------------------------------*/
    private void IntegrateVelocities()
    {
        for (int i = 0; i < sphereCount; i++)
        {
            spheres[i].rigidBody.UpdateTransform(dt);
            spheres[i].UpdateCollider();
        }

        for (int i = 0; i < capsuleCount; i++)
        {
            capsules[i].rigidBody.UpdateTransform(dt);
            capsules[i].UpdateCollider();
        }
        
        for (int i = 0; i < boxCount; i++)
        {
            boxes[i].rigidBody.UpdateTransform(dt);
            boxes[i].UpdateCollider();
        }
    }
    ////////////////////////////////////////////////////////////////////
}
