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

public class Cell
{
    ////////////////////////////////////////////////////////////////////
    // Instance Variables & Constructor

    public int id; // The unique ID number for this cell
    public Face[] faces; // Array of this cell's faces
    public Vector4 normal; // The cell's corresponding normal
    public int[] neighborIDs; // Array of ID's of this cell's neighbors

    public Cell(int id, Vector4 normal, Face[] faces, int[] neighborIDs)
    {
        this.id = id;
        this.normal = normal;
        this.faces = faces;
        this.neighborIDs = neighborIDs;
    }
    ////////////////////////////////////////////////////////////////////


    ////////////////////////////////////////////////////////////////////
    // Public Helper Methods

    /*------------------------------------------------------------------
     * Returns an array of cells (from the given array) whose IDs are
     * in the neighbor IDs array
     *------------------------------------------------------------------*/
    public static Cell[] GetNeighbors(Cell[] cells, int[] neighborIDs)
    {
        Cell[] neighbors = new Cell[neighborIDs.Length];
        for (int i = 0; i < neighborIDs.Length; i++)
        {
            neighbors[i] = cells[neighborIDs[i]];
        }

        return neighbors;
    }

    /*------------------------------------------------------------------
     * Returns a list of arrays of vertices, where each array of vertices
     * corresponds to a face of this cell
     *------------------------------------------------------------------*/
    public List<Vertex[]> GetCellFacesAsVertices()
    {
        List<Vertex[]> vList = new List<Vertex[]>();
        for (int i = 0; i < faces.Length; i++)
        {
            vList.Add(faces[i].verts);
        }

        return vList;
    }
    ////////////////////////////////////////////////////////////////////
}
