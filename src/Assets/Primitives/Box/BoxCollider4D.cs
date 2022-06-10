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
public class BoxCollider4D : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Event Method
    public Transform4D transform4D;
    public RigidBody4D rigidBody;

    public Vector4 max;
    public Vector4 min;
    public Vector4 center;
    public Rotor4D rotor;
    public Matrix4x4 rotation;

    public Vector4 X;
    public Vector4 Y;
    public Vector4 Z;
    public Vector4 W;

    public Vertex[] Vertices;
    public Face[] Faces;
    public Cell[] Cells;


    public void Start()
    {
        this.transform4D = this.GetComponent<Transform4D>();
        this.rigidBody = this.GetComponent<RigidBody4D>();
        this.rigidBody.SetBoxInertia();
        this.UpdateCollider();
    }
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Public Helper Method

    /*------------------------------------------------------------------
     * Updates the collider's variables using its transform
     *------------------------------------------------------------------*/
    public void UpdateCollider()
    {
        this.center = this.transform4D.position;
        this.rotor = this.transform4D.rotor;
        this.rotation = this.rotor.ToMatrix();
        this.X = rotation.GetColumn(0);
        this.Y = rotation.GetColumn(1);
        this.Z = rotation.GetColumn(2);
        this.W = rotation.GetColumn(3);

        this.max = this.transform4D.scale;
        this.min = -max;

        this.CalculateCells();
    }


    /*------------------------------------------------------------------
     * For a given point outside of this box collider, calculates and
     * returns the point closest to the box collider
     *------------------------------------------------------------------*/
    public Vector4 ClosestPointOnSurface(Vector4 point)
    {
        // Converts the given point into the box's local coordinates
        Vector4 localPoint = rotor.Reverse().Rotate(point - center);

        // Finds the closest point on the box to the sphere's center
        // in the box's local coordinates
        Vector4 closestPoint = new Vector4();
        closestPoint.x = Mathf.Clamp(localPoint.x, min.x, max.x);
        closestPoint.y = Mathf.Clamp(localPoint.y, min.y, max.y);
        closestPoint.z = Mathf.Clamp(localPoint.z, min.z, max.z);
        closestPoint.w = Mathf.Clamp(localPoint.w, min.w, max.w);

        // Returns the closest point in world space
        return rotor.Rotate(closestPoint) + center;
    }



    ////////////////////////////////////////////////////////////////////
    // Private Helper Methods

    /*------------------------------------------------------------------
     * Uses the position and rotation to calculate its corners
     *------------------------------------------------------------------*/
    private void CalculateVerts()
    {
        Vertices = new Vertex[]
        {
            new Vertex(0, center + rotation * min),
            new Vertex(1, center + (rotation * new Vector4(max.x, min.y, min.z, min.w))),
            new Vertex(2, center + (rotation * new Vector4(min.x, max.y, min.z, min.w))),
            new Vertex(3, center + (rotation * new Vector4(min.x, min.y, max.z, min.w))), 
            new Vertex(4, center + (rotation * new Vector4(min.x, min.y, min.z, max.w))),
            new Vertex(5, center + (rotation * new Vector4(max.x, max.y, min.z, min.w))),
            new Vertex(6, center + (rotation * new Vector4(max.x, min.y, max.z, min.w))),
            new Vertex(7, center + (rotation * new Vector4(max.x, min.y, min.z, max.w))),
            new Vertex(8, center + (rotation * new Vector4(min.x, max.y, max.z, min.w))),
            new Vertex(9, center + (rotation * new Vector4(min.x, max.y, min.z, max.w))),
            new Vertex(10, center + (rotation * new Vector4(min.x, min.y, max.z, max.w))),
            new Vertex(11, center + (rotation * new Vector4(max.x, max.y, max.z, min.w))),
            new Vertex(12, center + (rotation * new Vector4(max.x, max.y, min.z, max.w))),
            new Vertex(13, center + (rotation * new Vector4(max.x, min.y, max.z, max.w))),
            new Vertex(14, center + (rotation * new Vector4(min.x, max.y, max.z, max.w))),
            new Vertex(15, center + rotation * max)
        };
    }


    /*------------------------------------------------------------------
     * Uses the array of vertices to create an array of faces
     *------------------------------------------------------------------*/
    private void CalculateFaces()
    {
        this.CalculateVerts();
        Faces = new Face[]
        {
            new Face(0, new Vertex[] { Vertices[0], Vertices[1], Vertices[5], Vertices[2] }),
            new Face(1, new Vertex[] { Vertices[0], Vertices[1], Vertices[6], Vertices[3] }),
            new Face(2, new Vertex[] { Vertices[0], Vertices[1], Vertices[7], Vertices[4] }),
            new Face(3, new Vertex[] { Vertices[0], Vertices[2], Vertices[8], Vertices[3] }),
            new Face(4, new Vertex[] { Vertices[0], Vertices[2], Vertices[9], Vertices[4] }),
            new Face(5, new Vertex[] { Vertices[0], Vertices[3], Vertices[10], Vertices[4] }),

            new Face(6, new Vertex[] { Vertices[1], Vertices[5], Vertices[11], Vertices[6] }),
            new Face(7, new Vertex[] { Vertices[1], Vertices[5], Vertices[12], Vertices[7] }),
            new Face(8, new Vertex[] { Vertices[1], Vertices[6], Vertices[13], Vertices[7] }),

            new Face(9, new Vertex[] { Vertices[2], Vertices[5], Vertices[11], Vertices[8] }),
            new Face(10, new Vertex[] { Vertices[2], Vertices[5], Vertices[12], Vertices[9] }),
            new Face(11, new Vertex[] { Vertices[2], Vertices[8], Vertices[14], Vertices[9] }),

            new Face(12, new Vertex[] { Vertices[3], Vertices[6], Vertices[11], Vertices[8] }),
            new Face(13, new Vertex[] { Vertices[3], Vertices[6], Vertices[13], Vertices[10] }),
            new Face(14, new Vertex[] { Vertices[3], Vertices[8], Vertices[14], Vertices[10] }),

            new Face(15, new Vertex[] { Vertices[4], Vertices[7], Vertices[12], Vertices[9] }),
            new Face(16, new Vertex[] { Vertices[4], Vertices[7], Vertices[13], Vertices[10] }),
            new Face(17, new Vertex[] { Vertices[4], Vertices[9], Vertices[14], Vertices[10] }),

            new Face(18, new Vertex[] { Vertices[5], Vertices[11], Vertices[15], Vertices[12] }),
            new Face(19, new Vertex[] { Vertices[6], Vertices[11], Vertices[15], Vertices[13] }),
            new Face(20, new Vertex[] { Vertices[7], Vertices[12], Vertices[15], Vertices[13] }),
            new Face(21, new Vertex[] { Vertices[8], Vertices[11], Vertices[15], Vertices[14] }),
            new Face(22, new Vertex[] { Vertices[9], Vertices[12], Vertices[15], Vertices[14] }),
            new Face(23, new Vertex[] { Vertices[10], Vertices[13], Vertices[15], Vertices[14] })
        };
    }


    /*------------------------------------------------------------------
     * Uses the array of faces to create an array of 3-cells
     *------------------------------------------------------------------*/
    private void CalculateCells()
    {
        this.CalculateFaces();
        // The normals of the cells point outwards
        Cells = new Cell[]
        {
            new Cell(0, -W,
                new Face[] { Faces[0], Faces[1], Faces[3], Faces[6], Faces[9], Faces[12] },
                new int[] { 2, 3, 4, 5, 6, 7 }),
            new Cell(1, W,
                new Face[] { Faces[15], Faces[16], Faces[17], Faces[20], Faces[22], Faces[23] },
                new int[] { 2, 3, 4, 5, 6, 7 }) ,

            new Cell(2, -Z,
                new Face[] { Faces[0], Faces[2], Faces[4], Faces[7], Faces[10], Faces[15] },
                new int[] { 0, 1, 4, 5, 6, 7  }),
            new Cell(3, Z,
                new Face[] { Faces[12], Faces[13], Faces[14], Faces[19], Faces[21], Faces[23] },
                new int[] { 0, 2, 4, 5, 6, 7 }),

            new Cell(4, -Y,
                new Face[] { Faces[1], Faces[2], Faces[5], Faces[8], Faces[13], Faces[16] },
                new int[] { 0, 1, 2, 3, 6, 7 }),
            new Cell(5, Y,
                new Face[] { Faces[9], Faces[10], Faces[11], Faces[18], Faces[21], Faces[22] },
                new int[] { 0, 1, 2, 3, 6, 7 }),

            new Cell(6, -X,
                new Face[] { Faces[3], Faces[4], Faces[5], Faces[11], Faces[14], Faces[17] },
                new int[] { 0, 1, 2, 3, 4, 5 }),
            new Cell(7, X,
                new Face[] { Faces[6], Faces[7], Faces[8], Faces[18], Faces[19], Faces[20] },
                new int[] { 0, 1, 2, 3, 4, 5, })
        };
    }
    ////////////////////////////////////////////////////////////////////   
}
