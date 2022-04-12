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
    private SphereCollider4D[] _spheres;
    // Number of sphere colliders in the scene
    private int _sphereCount = 0;
    // Array of the capsule colliders in the scene
    private CapsuleCollider4D[] _capsules;
    // Number of capsule colliders in the scene
    private int _capsuleCount = 0;
    // Array of the box colliders in the scene
    private BoxCollider4D[] _boxes;
    // Number of box colliders in the scene
    private int _boxCount = 0;
    // Array of the plane colliders in the scene
    private PlaneCollider4D[] _planes;
    // Number of plane colliders in the scene
    private int _planeCount = 0;
    // Array of collision solvers for collision resolution
    private CollisionArbiter[] _solvers;
    // Number of collision solvers
    private int _numOfSolvers = 0;

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
     * object. Updates the value of the _numOfSolvers value and the 
     * associated _solvers array based on the number of child objects
     * in the scene
     *------------------------------------------------------------------*/
    public void Start()
    {
        _spheres = this.GetComponentsInChildren<SphereCollider4D>();
        _sphereCount = _spheres.Length;

        _capsules = this.GetComponentsInChildren<CapsuleCollider4D>();
        _capsuleCount = _capsules.Length;

        _boxes = this.GetComponentsInChildren<BoxCollider4D>();
        _boxCount = _boxes.Length;

        _planes = this.GetComponentsInChildren<PlaneCollider4D>();
        _planeCount = _planes.Length;

        // Calculates the number of solvers needed for pairwise
        // collision detection
        if (_sphereCount > 1)
        {
            _numOfSolvers += (_sphereCount * (_sphereCount - 1)) / 2;
        }
        _numOfSolvers += _sphereCount * _capsuleCount;
        _numOfSolvers += _sphereCount * _boxCount;
        _numOfSolvers += _sphereCount * _planeCount;

        if (_capsuleCount > 1)
        {
            _numOfSolvers += (_capsuleCount * (_capsuleCount - 1)) / 2;
        }
        _numOfSolvers += _capsuleCount * _boxCount;
        _numOfSolvers += _capsuleCount * _planeCount;

        if (_boxCount > 1)
        {
            _numOfSolvers += (_boxCount * (_boxCount - 1)) / 2;
        }
        _numOfSolvers += _boxCount * _planeCount;

        // Initializes a new array with the appropriate number of solvers
        _solvers = new CollisionArbiter[_numOfSolvers];
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
        for (int i = 0; i < _numOfSolvers; i++)
        {
            if (_solvers[i] != null) _solvers[i].PreSolve(invDT);
        }

        // Repeatedly iterates over the solvers to apply sequential impulses
        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < _numOfSolvers; j++) if (_solvers[j] != null) _solvers[j].Solve();
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

        for (int i = 0; i < _sphereCount; i++)
        {
            SphereCollider4D a = _spheres[i];
            for (int j = i+1; j < _sphereCount; j++)
            {
                SphereCollider4D b = _spheres[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                _solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }

            
            for (int j = 0; j < _capsuleCount; j++)
            {
                CapsuleCollider4D b = _capsules[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                _solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }
        
            for (int j = 0; j < _boxCount; j++)
            {
                BoxCollider4D b = _boxes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                _solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }
            

            for (int j = 0; j < _planeCount; j++)
            {
                PlaneCollider4D b = _planes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                _solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }
        }


        for (int i = 0; i < _capsuleCount; i++)
        {
            CapsuleCollider4D a = _capsules[i];
            for (int j = i + 1; j < _capsuleCount; j++)
            {
                CapsuleCollider4D b = _capsules[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                _solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }

            for (int j = 0; j < _boxCount; j++)
            {
                BoxCollider4D b = _boxes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                _solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }

            for (int j = 0; j < _planeCount; j++)
            {
                PlaneCollider4D b = _planes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                _solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }
        }


        for (int i = 0; i < _boxCount; i++)
        {
            BoxCollider4D a = _boxes[i];
            for (int j = i+1; j < _boxCount; j++)
            {
                BoxCollider4D b = _boxes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                _solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
                solverIdx++;
            }
            

            for (int j = 0; j < _planeCount; j++)
            {
                PlaneCollider4D b = _planes[j];
                CollisionArbiter newSolver = new CollisionArbiter(a, b);
                _solvers[solverIdx] = newSolver.numOfContacts > 0 ? newSolver : null;
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
        for (int i = 0; i < _sphereCount; i++)
        {
            _spheres[i].rigidBody.AddVelocity(gravity * dt);
        }

        for (int j = 0; j < _capsuleCount; j++)
        {
            _capsules[j].rigidBody.AddVelocity(gravity * dt);
        }
        
        for (int i = 0; i < _boxCount; i++)
        {
            _boxes[i].rigidBody.AddVelocity(gravity * dt);
        }
    }

    /*------------------------------------------------------------------
     * Integrates the velocities of the rigid bodies and updates their 
     * corresponding transforms and colliders
     *------------------------------------------------------------------*/
    private void IntegrateVelocities()
    {
        for (int i = 0; i < _sphereCount; i++)
        {
            _spheres[i].rigidBody.UpdateTransform(dt);
            _spheres[i].UpdateCollider();
        }

        for (int i = 0; i < _capsuleCount; i++)
        {
            _capsules[i].rigidBody.UpdateTransform(dt);
            _capsules[i].UpdateCollider();
        }
        
        for (int i = 0; i < _boxCount; i++)
        {
            _boxes[i].rigidBody.UpdateTransform(dt);
            _boxes[i].UpdateCollider();
        }
    }
    ////////////////////////////////////////////////////////////////////
}
