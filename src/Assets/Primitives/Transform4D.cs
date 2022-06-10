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
public class Transform4D : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Event Method

    public float radiansXY = 0; // Starting angle rotation for the XY plane
    public float radiansXZ = 0; // Starting angle rotation for the XZ plane
    public float radiansXW = 0; // Starting angle rotation for the XW plane
    public float radiansYZ = 0; // Starting angle rotation for the YZ plane
    public float radiansYW = 0; // Starting angle rotation for the YW plane
    public float radiansZW = 0; // Starting angle rotation for the ZW plane

    public Vector4 position = Vector4.zero; // Position in world space
    public Vector4 scale = Vector4.one; // Scale of the object
    public Rotor4D rotor = new Rotor4D(); // Orientation

    public void Awake()
    {
        rotor = new Rotor4D(new Bivector4D(1, 0, 0, 0, 0, 0), radiansXY)
            * new Rotor4D(new Bivector4D(0, 1, 0, 0, 0, 0), radiansXZ)
            * new Rotor4D(new Bivector4D(0, 0, 1, 0, 0, 0), radiansXW)
            * new Rotor4D(new Bivector4D(0, 0, 0, 1, 0, 0), radiansYZ)
            * new Rotor4D(new Bivector4D(0, 0, 0, 0, 1, 0), radiansYW)
            * new Rotor4D(new Bivector4D(0, 0, 0, 0, 0, 1), radiansZW);
    }
    ////////////////////////////////////////////////////////////////////
}
