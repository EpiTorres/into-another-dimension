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

public class SutherlandHodgman : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////
    // Public Methods

    /*------------------------------------------------------------------
     * Returns an array of contact points corresponding to the contact
     * manifold between the given box and plane colliders
     *------------------------------------------------------------------*/
    public static ContactPoint4D[] GetContactManifold(BoxCollider4D a, PlaneCollider4D b,
    Vector4 normal, float separation)
    {
        // Gets the cell of the box collider that is incident to the plane's normal
        Cell incident = GetIncidentCell(a.Cells, b.normal);
        List<Vertex[]> vertsList = incident.GetCellFacesAsVertices();

        // TODO you can remove the duplicates and then do remove exterior vertices
        // Then in remove exterior vertices you can simply use the distance from
        // each vertex as its corresponding separation and initialize things that way

        // Removes any vertices that are outside of the plane
        Vector4 planeNormal = b.normal;
        Vector4 pointOnPlane = b.pointOnPlane + b.normal * 0.1F; // Moves the plane up a little
        List<Vertex> contactManifold = RemoveExteriorVertices(vertsList, planeNormal, pointOnPlane);
        ContactPoint4D[] uniquePoints = GetUniquePoints(contactManifold, normal, separation);
        return uniquePoints;
    }


    /*------------------------------------------------------------------
     * Returns an array of contact points corresponding to the contact
     * manifold between the two given box colliders
     *------------------------------------------------------------------*/
    public static ContactPoint4D[] GetContactManifold(BoxCollider4D a, BoxCollider4D b,
    Vector4 normal, float separation)
    {
        Cell referenceCell, incidentCell;
        Cell[] neighbors = GetReferenceAndIncidentCells(a.Cells, b.Cells, normal,
            out referenceCell, out incidentCell);
        return ClipCell(referenceCell, incidentCell, neighbors, normal, separation);
    }

    ////////////////////////////////////////////////////////////////////
    // Private Helper Methods

    /*------------------------------------------------------------------
     * Given a list of vertices, removes all vertices that are outside
     * of the plane defined by the given plane normal and plane point.
     * Returns the a list of the remaining vertices.
     *------------------------------------------------------------------*/
    private static List<Vertex> RemoveExteriorVertices(List<Vertex[]> vertsList, Vector4 planeNormal, Vector4 pointOnPlane)
    {
        List<Vertex> contactManifold = new List<Vertex>();
        // Clips all of the faces using the current hyperplane
        for (int i = 0; i < vertsList.Count; i++)
        {
            Vertex[] verts = vertsList[i];
            for (int j = 0; j < verts.Length; j++)
            {
                Vector4 p = verts[j].point;
                float d = MathUtil.PointDistFromHyperPlane(planeNormal, pointOnPlane, p);
                if (d <= Mathf.Epsilon)
                {
                    contactManifold.Add(verts[j]);
                }
            }
        }
        return contactManifold;
    }

    /*------------------------------------------------------------------
     * Returns an array of contact points that corresponds to the unique
     * vertices in the given list
     *------------------------------------------------------------------*/
    private static ContactPoint4D[] GetUniquePoints(List<Vertex> verts, Vector4 normal, float separation)
    {
        Dictionary<int, ContactPoint4D> uniquePoints = new Dictionary<int, ContactPoint4D>();

        for (int i = 0; i < verts.Count; i++)
        {
            // Adds the current point if it isn't already in the dictionary
            int vertID = verts[i].id;
            if (!uniquePoints.ContainsKey(vertID))
            {
                // TODO actually implement the separation thing
                ContactPoint4D contact = new ContactPoint4D(verts[i].point, normal, separation);
                uniquePoints.Add(vertID, contact);
            }
        }
        ContactPoint4D[] output = new ContactPoint4D[uniquePoints.Count];
        uniquePoints.Values.CopyTo(output, 0);
        return output;
    }

    /*------------------------------------------------------------------
     * Returns the Cell from the given array that is most anti-parallel
     * to the given normal
     *------------------------------------------------------------------*/
    private static Cell GetIncidentCell(Cell[] cells, Vector4 normal)
    {
        normal = normal.normalized; // ensures that the normal is normalized

        if (cells.Length <= 0) return null;

        // Gets the most parallel and anti-parallel cells in aCells
        float aMin = float.MaxValue;
        Cell incident = cells[0];
        for (int i = 0; i < cells.Length; i++)
        {
            // should I use the squaring approach?
            Cell cell = cells[i];
            Vector4 cNorm = cell.normal;
            float cosTheta = Vector4.Dot(cNorm, normal) / cNorm.magnitude;
            //print("cosNorm" + cNorm);
            //print("cosPoint" + cells[i].faces[0].verts[0].point);
            //print("cos" + cosTheta);
            if (cosTheta < (aMin + Mathf.Epsilon))
            {
                incident = cell;
                aMin = cosTheta;
            }
        }

        return incident;
    }

    /*------------------------------------------------------------------
     * Sets the value of referenceCell to be the cell (in either aCells
     * or bCells) that is most parallel to the given normal. If 
     * referenceCell corresponds to a cell from aCells, sets the value 
     * of incidentCell to be the most anti-parallel cell in bCells;
     * otherwise, if referenceCell corresponds to a Cell from bCells,
     * sets the value of incident cell to be the most anti-parallel cell
     * in aCells. Returns an array of cells corresponding to the 
     * neighboring cells of the incident cell.                            
     *------------------------------------------------------------------*/
    public static Cell[] GetReferenceAndIncidentCells(Cell[] aCells, Cell[] bCells, Vector4 normal,
        out Cell referenceCell, out Cell incidentCell)
    {
        normal = normal.normalized; // Ensures that the given normal is normalized

        // Gets the most parallel and anti-parallel cells in aCells
        float aMax = float.MinValue;
        float aMin = float.MaxValue;
        Cell aReference = aCells[0];
        Cell aIncident = aCells[0];
        for (int i = 0; i < aCells.Length; i++)
        {
            Cell aCell = aCells[i];
            Vector4 aNorm = aCell.normal;
            float aCosTheta = Vector4.Dot(aNorm, normal) / aNorm.magnitude;
            if (aCosTheta > 0 && aCosTheta > (aMax - Mathf.Epsilon))
            {
                aReference = aCell;
                aMax = aCosTheta;

            }
            if (aCosTheta < 0 && aCosTheta < (aMin + Mathf.Epsilon))
            {
                aIncident = aCell;
                aMin = aCosTheta;
            }
        }


        // Gets the most parallel and anti-parallel cells in bCells
        float bMax = float.MinValue;
        float bMin = float.MaxValue;
        Cell bReference = bCells[0];
        Cell bIncident = bCells[0];
        for (int i = 0; i < bCells.Length; i++)
        {
            Cell bCell = bCells[i];
            Vector4 bNorm = bCell.normal;
            float bCosTheta = Vector4.Dot(bNorm, normal) / bNorm.magnitude;
            if (bCosTheta > 0 && bCosTheta > (bMax - Mathf.Epsilon))
            {
                bReference = bCell;
                bMax = bCosTheta;

            }
            if (bCosTheta < 0 && bCosTheta < (bMin + Mathf.Epsilon))
            {
                bIncident = bCell;
                bMin = bCosTheta;
            }
        }

        if (aMax > bMax)
        {
            referenceCell = aReference;
            incidentCell = bIncident;
            return Cell.GetNeighbors(aCells, aReference.neighborIDs);
        }
        else
        {
            referenceCell = bReference;
            incidentCell = aIncident;
            return Cell.GetNeighbors(bCells, bReference.neighborIDs);
        };
    }

    /*------------------------------------------------------------------
     * Given the normal and a point of a hyperplane, clips the given
     * face (represented as a array of vertices) and returns an array
     * of vertices corresponding to the clipped face.                            
     *------------------------------------------------------------------*/
    private static Vertex[] ClipFace(Vector4 planeNormal, Vector4 pointOnPlane, Vertex[] verts)
    {
        List<Vertex> output = new List<Vertex>();
        for (int i = 0; i < verts.Length; i++)
        {
            int j = (i + 1) % verts.Length;

            Vector4 p1 = verts[i].point;
            Vector4 p2 = verts[j].point;

            float d1 = MathUtil.PointDistFromHyperPlane(planeNormal, pointOnPlane, p1);
            float d2 = MathUtil.PointDistFromHyperPlane(planeNormal, pointOnPlane, p2);

            // Both points are on/inside the clipping plane
            if ((Mathf.Approximately(d1, 0) && Mathf.Approximately(d2, 0))
                || (d1 < Mathf.Epsilon && d2 < Mathf.Epsilon))
            {
                output.Add(verts[i]);
            }
            // There is an intersection
            else if ((d1 > Mathf.Epsilon && d2 < Mathf.Epsilon)
                || (d1 < Mathf.Epsilon && d2 > Mathf.Epsilon))
            {
                // Calculates the intersection point
                // https://math.stackexchange.com/questions/83990/line-and-plane-intersection-in-3d
                Vector4 dir = p2 - p1;
                float t = (Vector4.Dot(planeNormal, pointOnPlane) - Vector4.Dot(planeNormal, p1)) / Vector4.Dot(planeNormal, dir);
                Vector4 intersectionPoint = p1 + t * dir;

                // If the first point of the pair is inside the clipping hyperplane...
                if (d1 < Mathf.Epsilon)
                {
                    output.Add(verts[i]);
                    output.Add(new Vertex(verts[j].id, intersectionPoint));
                }
                // Otherwise, the second point is inside the clipping plane
                else output.Add(new Vertex(verts[i].id, intersectionPoint));
            }
        }

        return output.ToArray();
    }

    /*------------------------------------------------------------------
     * Given a reference cell, incident cell, and the array of the 
     * incident cell's neighboring cells, performs a generalized version
     * of the Sutherland-Hodgman algorithm to clip the incident cell.
     *------------------------------------------------------------------*/
    private static ContactPoint4D[] ClipCell(Cell referenceCell, Cell incidentCell,  
        Cell[] neighborCells, Vector4 normal, float separation)
    {
        // List<Polygon> polygons = incidentCell.GetFacesAsPolygons();
        List<Vertex[]> vertsList = incidentCell.GetCellFacesAsVertices();
        List<Vertex[]> tempList = new List<Vertex[]>();
        Vector4 planeNormal;
        Vector4 pointOnPlane;

        // TODO the 
        for (int i = 0; i < neighborCells.Length; i++)
        {
            Cell current = neighborCells[i];
            planeNormal = current.normal;
            pointOnPlane = current.faces[0].verts[0].point;

            // Clips all of the faces using the current hyperplane
            for (int j = 0; j < vertsList.Count; j++)
            {
                Vertex[] verts = vertsList[j];
                Vertex[] outVerts = ClipFace(planeNormal, pointOnPlane, verts);
                if (outVerts.Length > 0) tempList.Add(outVerts);
            }
            vertsList = tempList;
            tempList = new List<Vertex[]>();
        }
        // Performs the final clipping step that removes any vertices that are
        // outside of the current cell
        planeNormal = referenceCell.normal;
        pointOnPlane = referenceCell.faces[0].verts[0].point + referenceCell.normal * Mathf.Epsilon;
        List<Vertex> contactManifold = RemoveExteriorVertices(vertsList, planeNormal, pointOnPlane);
        return GetUniquePoints(contactManifold, normal, separation);
    }
    ////////////////////////////////////////////////////////////////////
}
