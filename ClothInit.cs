using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothInit : MonoBehaviour 
{
	public bool useUv = true;
	public bool useGradientForUv = true;
	public bool useRed = true;
	public float maxDistance = float.MaxValue;
	[Range(0,1)] public float uvThreshold = 0.25f;
	public Animator animator;
	public bool debugObjs = false;
	ClothSphereColliderPair[] colliderSpheres;
	CapsuleCollider[] colliderCapsules;

	void Start () 
	{
		SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
		Mesh mesh = smr.sharedMesh;
		Cloth cloth = GetComponent<Cloth>();
		Transform rootBone = smr.rootBone;
		ClothSkinningCoefficient[] newConstraints = cloth.coefficients;
		smr.rootBone = null;

		if(animator != null)
			animator.enabled = false;

//		Vector3 origScale = transform.lossyScale;
//		transform.lossyScale = 1;

		// Really oughtta put the scale safeguard back in.

		// Get rid of colliders
		colliderSpheres = cloth.sphereColliders;
		cloth.sphereColliders = new ClothSphereColliderPair[]{};
		colliderCapsules = cloth.capsuleColliders;
		cloth.capsuleColliders = new CapsuleCollider[]{};

		if(debugObjs)
		{
			Debug.Log("Cubes are cloth vertices, spheres are mesh vertices");
			const float objScale = 0.01f;
			foreach(Vector3 clothVert in cloth.vertices)
			{
				GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				cube.transform.localScale *= objScale;
				cube.transform.localPosition = clothVert;
			}
			foreach(Vector3 meshVert in mesh.vertices)
			{
				GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				sphere.transform.localScale *= objScale;
				sphere.transform.localPosition = meshVert;
			}
		}

		for(int c = 0; c < cloth.vertices.Length; ++c)
		{
			int matchingIndex = c;

			Vector3 clothVert = cloth.vertices[c];

			Vector3 meshVert = mesh.vertices[c];

			if(clothVert != meshVert)
			{
				for(int m = 0; m < mesh.vertices.Length; ++m)
				{
					var mIndex = m;
					mIndex = (m + c) % mesh.vertices.Length;
					meshVert = mesh.vertices[mIndex];
					if(clothVert == meshVert)
					{
						matchingIndex = mIndex;
						break;
					}
				}
				if(matchingIndex == c)
				{
					Debug.Log("Couldn't find a match");
					continue;
				}
			}

			float finalValue = maxDistance;
			if(useUv)
			{
				if(useGradientForUv)
				{
					var x = mesh.uv[matchingIndex].x;
					var perc = Mathf.Clamp01(x - uvThreshold) / (1 - uvThreshold);
//					Debug.Log(x + " - " + perc);
					finalValue *= perc;
				}
				else
				{
					if(mesh.uv[matchingIndex].x < uvThreshold)
						finalValue *= 0;
				}
			}
			if(useRed)
			{
				finalValue *= mesh.colors[matchingIndex].r;
			}
			newConstraints[c].maxDistance = finalValue;
		}

		cloth.coefficients = newConstraints;

		smr.rootBone = rootBone;

		if(animator != null)
			animator.enabled = true;

		cloth.capsuleColliders = colliderCapsules;
		cloth.sphereColliders = colliderSpheres;

//		transform.lossyScale = origScale;

		Debug.Log("UPDATED CLOTH CONSTRAINTS on " + gameObject.name + ". Copy Cloth settings to GameObject out of play mode and turn this script off.");
	}
}