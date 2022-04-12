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
 *		- https://github.com/thisstillwill/Lucid
 *-------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Controller : MonoBehaviour
{
	public float moveSpeed = 1f;

	Vector3 mouseLook;
	Vector3 smoothV;
	public float sensitivity = 5.0f;
	public float smoothing = 2.0f;

	public float yAngle;
	public float xAngle;

	public Matrix4x4 rotMatrix = Matrix4x4.identity;
	public Transform4D transform4D;

	public void Awake()
	{
		transform4D = this.GetComponent<Transform4D>();

		// lock cursor
		Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
		Time.timeScale = 1f;
	}

	private void FixedUpdate()
	{
		Vector4 forward = transform4D.rotor.ToMatrix() * (new Vector4(0, 0, -1, 0) * Input.GetAxis("Vertical"));
		Vector4 slep = transform4D.rotor.ToMatrix() * (new Vector4(0, 0, 0, -1) * Input.GetAxis("Horizontal"));
		transform4D.position += (forward + slep) * moveSpeed * Time.deltaTime;
	}

	private void Update()
	{
		if (Cursor.lockState == CursorLockMode.None)
        {
			if (Input.GetMouseButton(0))
            {
				Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
		}

		var mouseDelta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse ScrollWheel"));
		mouseDelta = Vector3.Scale(mouseDelta, new Vector3(sensitivity * smoothing, sensitivity * smoothing, sensitivity * smoothing * 5));
		smoothV.x = Mathf.Lerp(smoothV.x, mouseDelta.x, 1f / smoothing);
		smoothV.y = Mathf.Lerp(smoothV.y, mouseDelta.y, 1f / smoothing);
		smoothV.z = Mathf.Lerp(smoothV.z, mouseDelta.z, 1f / smoothing);
		mouseLook += smoothV / 2;

		Rotor4D rotor = new Rotor4D(new Bivector4D(0, 1, 0, 0, 0, 0), mouseLook.x) 
			* new Rotor4D(new Bivector4D(0, 0, 0, 1, 0, 0), mouseLook.y) 
			* new Rotor4D(new Bivector4D(0, 0, 0, 0, 0, 1), -mouseLook.z);
		transform4D.rotor = rotor.Normalized();
	}
}