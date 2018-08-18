using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothInit : MonoBehaviour 
{
	public bool useUv = false;
	public bool useGradientForUv = false;
	public bool useRed = true;
	public float maxDistance = 0.15f;
	[Range(0,1)] public float uvThreshold = 0.25f;
	public Animator animator;
	public bool debugObjs = true;
	public bool endOnFirstFail = true;
	public bool oneTestPerVert = false;
	public bool checkViaMinDistance = true;
	public float minDistForMatch = 0.001f;
	public Vector3 compareRotation;
	public bool useInverseTransformIfNotAligned;
	ClothSphereColliderPair[] colliderSpheres;
	CapsuleCollider[] colliderCapsules;

	MeshRenderer[] debugCubes;
	MeshRenderer[] debugSpheres;

	SkinnedMeshRenderer smr;
	Mesh mesh;
	Cloth cloth;
	Transform rootBone;
	ClothSkinningCoefficient[] newConstraints;
	float damping;
	Vector3 animPos;

	void Start () 
	{
		Initialize();
		Prepare();
		if(debugObjs) DebugShapes();
		ZeroOut();

		for(int c = 0; c < cloth.vertices.Length; ++c)
		{
			int matchingIndex = c;
			Vector3 clothVert = cloth.vertices[c];
			Vector3 meshVert = mesh.vertices[c];

//			if(TransformClothVert(clothVert) != TransformMeshVert(meshVert))
			if(!CompareVerts(clothVert, meshVert))
			{
				if(oneTestPerVert) continue;
				for(int m = 0; m < mesh.vertices.Length; ++m)
				{
					var mIndex = m;
					mIndex = (m + c) % mesh.vertices.Length;
					meshVert = mesh.vertices[mIndex];
//					if(TransformClothVert(clothVert) == TransformMeshVert(meshVert))
					if(CompareVerts(clothVert, meshVert))
					{
						matchingIndex = mIndex;
						break;
					}
				}
				if(matchingIndex == c)
				{
					Debug.Log("Couldn't find a match");
					if(endOnFirstFail)
					{
						Debug.Log("FAILED! After " + c + " vertices.");
						Repair();
						return;
					}
					continue;
				}
			}

			ApplyClothVert(c, matchingIndex);
		}

		Repair();

		Debug.Log("UPDATED CLOTH CONSTRAINTS on " + gameObject.name + ". Copy Cloth settings to GameObject out of play mode and turn this script off.");
	}

	void Initialize()
	{
		smr = GetComponent<SkinnedMeshRenderer>();
		mesh = smr.sharedMesh;
		cloth = GetComponent<Cloth>();
	}

	void Prepare()
	{
		rootBone = smr.rootBone;
		smr.rootBone = null;

		newConstraints = cloth.coefficients;

		if(animator != null)
		{
			animator.enabled = false;
			animPos = animator.transform.position;
			animator.transform.position = Vector3.zero;
		} else {
			animPos = transform.position;
			transform.position = Vector3.zero;
		}

		//		Vector3 origScale = transform.lossyScale;
		//		transform.lossyScale = 1;

		// Really oughtta put the scale safeguard back in.

		// Get rid of colliders
		colliderSpheres = cloth.sphereColliders;
		cloth.sphereColliders = new ClothSphereColliderPair[]{};
		colliderCapsules = cloth.capsuleColliders;
		cloth.capsuleColliders = new CapsuleCollider[]{};

		damping = cloth.damping;
		cloth.damping = 1;

//		cloth.enabled = false;
	}

	void Repair()
	{
		cloth.coefficients = newConstraints;

		smr.rootBone = rootBone;

		if(animator != null)
		{
			animator.enabled = true;
			animator.transform.position = animPos;
		} else {
			transform.position = animPos;
		}

		cloth.capsuleColliders = colliderCapsules;
		cloth.sphereColliders = colliderSpheres;

		cloth.damping = damping;

//		cloth.enabled = true;

		//		transform.lossyScale = origScale;
	}

	void DebugShapes()
	{
		Debug.Log("Cubes are cloth vertices, spheres are mesh vertices");
		const float objScale = 0.01f;

		debugCubes = new MeshRenderer[cloth.vertices.Length];
		debugSpheres = new MeshRenderer[mesh.vertices.Length];

		for(int c = 0 ; c < cloth.vertices.Length; ++c)
		{
			Vector3 clothVert = cloth.vertices[c];
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.transform.localScale *= objScale;
			cube.transform.localPosition = TransformClothVert(clothVert);
			debugCubes[c] = cube.GetComponent<MeshRenderer>();
		}
		for(int m = 0 ; m < mesh.vertices.Length; ++m)
		{
			Vector3 meshVert = mesh.vertices[m];
			GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphere.transform.localScale *= objScale;
			sphere.transform.localPosition = TransformMeshVert(meshVert);
			debugSpheres[m] = sphere.GetComponent<MeshRenderer>();
		}
	}

	void ZeroOut()
	{
		for(int c = 0; c < cloth.vertices.Length; ++c)
		{
			newConstraints[c].maxDistance = 0;
		}
		cloth.coefficients = newConstraints;
	}

	void ApplyClothVert(int c, int m)
	{
		if(debugObjs)
		{
			debugCubes[c].material = null;
			debugSpheres[m].material = null;
		}

		float finalValue = maxDistance;
		if(useUv)
		{
			if(useGradientForUv)
			{
				var x = mesh.uv[m].x;
				var perc = Mathf.Clamp01(x - uvThreshold) / (1 - uvThreshold);
				//					Debug.Log(x + " - " + perc);
				finalValue *= perc;
			}
			else
			{
				if(mesh.uv[m].x < uvThreshold)
					finalValue *= 0;
			}
		}
		if(useRed)
		{
			finalValue *= mesh.colors[m].r;
		}
		newConstraints[c].maxDistance = finalValue;
	}

	Vector3 TransformMeshVert(Vector3 vert)
	{
		vert =  Quaternion.Euler(compareRotation) * vert;
		if(useInverseTransformIfNotAligned)
			vert = rootBone.InverseTransformPoint(vert);
		return vert;
	}

	Vector3 TransformClothVert(Vector3 vert)
	{
		// vert =  Quaternion.Euler(compareRotation) * vert;
		// vert = cloth.transform.parent.InverseTransformPoint(vert);
		// vert = rootBone.TransformPoint(vert);
		return vert;
	}

	bool CompareVerts(Vector3 cloth, Vector3 mesh)
	{
		float sqrMaxDist = Mathf.Pow(minDistForMatch, 2);
		if(checkViaMinDistance)
		{
			return Vector3.SqrMagnitude(TransformClothVert(cloth) - TransformMeshVert(mesh)) < sqrMaxDist;
		} else {
			return TransformClothVert(cloth) == TransformMeshVert(mesh);
		}
		return false;
	}
}