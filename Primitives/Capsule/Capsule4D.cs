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
public class Capsule4D : MonoBehaviour
{

    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Event Method
    public Transform4D transform4D;
    public CapsuleCollider4D capsuleCollider;
    public RigidBody4D rigidBody;

    public Color color;
    public float radius;
    public float height;

    public void Start()
    {
        transform4D = this.GetComponent<Transform4D>();
        capsuleCollider = this.GetComponent<CapsuleCollider4D>();
        rigidBody = this.GetComponent<RigidBody4D>();

        capsuleCollider.radius = this.radius;
        capsuleCollider.height = this.height;
    }
    ////////////////////////////////////////////////////////////////////
}
