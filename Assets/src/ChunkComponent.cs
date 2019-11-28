using System;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

[Serializable]
public class ChunkComponent : MonoBehaviour
{
	public VoxelComponent voxelComponent;
	public Chunk chunk;

	public Coord3D position;

	public int renderUpdates = 0;
	public int collisionUpdates = 0;

	public MeshFilter meshFilter;
	public Mesh mesh;

	public int colliderIndex = 0;
	public bool pooledColliders = false;
	public List<BoxCollider> colliders;

	public void Start()
	{
		mesh = new Mesh();
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //  Allow up to 4b verts

		//float timeStart = Time.realtimeSinceStartup;

		pooledColliders = true;
		colliders = new List<BoxCollider>();

		//colliders = new BoxCollider[ (int) ((Constants.CHUNK_SIZE * Constants.CHUNK_SIZE * Constants.CHUNK_SIZE) * 0.5f) ];
		// for (int i = 0; i < colliders.Length; i++)
		// {
		// 	// GameObject obj = new GameObject();
		// 	// BoxCollider collider = obj.AddComponent<BoxCollider>();
		// 	// obj.transform.parent = this.transform;

		// 	BoxCollider collider = Instantiate(GameMaster.Instance.voxelColliderPrefab, this.transform).GetComponent<BoxCollider>();
		// 	colliders[i] = collider;
		// 	// collider.transform.parent = this.transform;
		// 	// collider.transform.localPosition = Vector3.zero;
		// 	// BoxCollider collider = gameObject.AddComponent<BoxCollider>();
		// 	// collider.gameObject.SetActive(false);

		// 	pooledColliders = true;
		// }

		//Debug.Log("collision pool time: " + (Time.realtimeSinceStartup - timeStart));
	}

	// public void Update()
	// {
	// 	if (pooledColliders == false)
	// 	{
	// 		if (colliderIndex < colliders.Length)
	// 		{
	// 			int added = 0;

	// 			for (int i = 0; i < GameMaster.Instance.getFPS(); i++)
	// 			{
	// 				if (i + colliderIndex >= colliders.Length)
	// 				{
	// 					break;
	// 				}

	// 				// BoxCollider collider = Instantiate(GameMaster.Instance.voxelColliderPrefab, Vector3.zero, Quaternion.identity).GetComponent<BoxCollider>();
	// 				// collider.transform.parent = this.transform;
	// 				// collider.transform.localPosition = Vector3.zero;
	// 				BoxCollider collider = gameObject.AddComponent<BoxCollider>();
	// 				collider.enabled = false;

	// 				colliders[i + colliderIndex] = collider;
	// 				added += 1;
	// 			}

	// 			colliderIndex += added;
	// 		}
	// 		else
	// 		{
	// 			//Debug.Log("elapsed time: " + (Time.time - poolStartTime));
	// 			pooledColliders = true;
	// 		}
	// 	}
	// }

	public void BuildChunk()
	{
		if (chunk == null)
		{
			Debug.Log("Null chunk!");
			// chunk = voxelComponent.voxelObject.GenerateChunk(position.x, position.y, position.z);
			// chunk.component = this;
			// chunk.PrepareLoad();
		}
		else
		{
			// if (chunk.getState() == ChunkState.Default)
			// {
			// 	chunk = voxelComponent.voxelObject.GenerateChunk(position.x, position.y, position.z);
			// 	chunk.component = this;
			// 	chunk.PrepareLoad();
			// }

			/*if (chunk.getState() == ChunkState.Unloaded)	//	Clear out this chunk if it was unloaded
			{
				chunk.Unload();
				chunk = null;
				meshFilter.mesh.Clear();
			}*/

			//	Potential lag point but for some reason chunks dont always have references to their voxel object
			chunk = voxelComponent.voxelObject.getChunk(position.x, position.y, position.z);

			if (chunk.getCollisionState() == CollisionState.Ready && pooledColliders == true)	//	Assign collision to this chunk if it is ready
			{
				chunk.BuildCollision();

				if (chunk.getCollisionState() == CollisionState.Built)
				{
					//BoxCollider[] colliders = gameObject.GetComponents<BoxCollider>();
					// foreach (BoxCollider collider in colliders)
					// {
					// 	collider.enabled = false;
					// 	//Destroy(collider.gameObject);
					// 	// collider.gameObject.SetActive(false);
					// }

					//	Expand the collider list if necessary
					while (colliders.Count < chunk.getCollisionData().centers.Length)
					{
						colliders.Add(null);
					}

					for (int i = 0; i < colliders.Count; i++)
					{
						BoxCollider collider = colliders[i];
						if (collider != null) { collider.enabled = false; }

						if (i < chunk.getCollisionData().centers.Length)
						{
							if (collider == null)
							{
								collider = Instantiate(GameMaster.Instance.voxelColliderPrefab, this.transform).GetComponent<BoxCollider>();
								colliders[i] = collider;
							}
							collider.enabled = true;
							collider.center = chunk.getCollisionData().centers[i] + (new Vector3( chunk.getX(), chunk.getY(), chunk.getZ() ) * Constants.CHUNK_SIZE) + voxelComponent.pivotPoint;
							collider.size = chunk.getCollisionData().sizes[i];
						}
					}

					// for (int i = 0; i < chunk.getCollisionData().centers.Length; i++)
					// {
					// 	//BoxCollider collider = gameObject.AddComponent<BoxCollider>();
					// 	BoxCollider collider;

					// 	if (i >= colliders.Count)
					// 	{
					// 		collider = Instantiate(GameMaster.Instance.voxelColliderPrefab, this.transform).GetComponent<BoxCollider>();
					// 		colliders.Add(collider);
					// 	}

					// 	collider = colliders[i];

					// 	collider.enabled = true;
					// 	collider.center = chunk.getCollisionData().centers[i] + (new Vector3( chunk.getX(), chunk.getY(), chunk.getZ() ) * Constants.CHUNK_SIZE) + voxelComponent.pivotPoint;
					// 	collider.size = chunk.getCollisionData().sizes[i];
					// }

					collisionUpdates++;
				}

				chunk.ClearCollisionBuffer();
			}
			else if (chunk.getCollisionState() == CollisionState.Waiting)
			{
				if (voxelComponent != null)
				{
					chunk.setCollisionState(CollisionState.Queued);
					ChunkCollisionBuilder.instance.Queue(position);
				}
			}

			if (chunk.getRenderState() == RenderState.Ready)	//	Render the chunk if it is ready
			{
				chunk.Render();

				if (chunk.getRenderState() == RenderState.Rendered)
				{
					mesh.Clear();
					mesh.vertices = chunk.getRenderData().vertices;
					mesh.triangles = chunk.getRenderData().triangles;
					mesh.normals = chunk.getRenderData().normals;
					mesh.uv = chunk.getRenderData().uvs;
					mesh.colors = chunk.getRenderData().colors;
					mesh.RecalculateNormals();

					renderUpdates++;
				}

				chunk.ClearRenderBuffer();
			}
			else if (chunk.getRenderState() == RenderState.Waiting)
			{
				if (voxelComponent != null)
				{
					ChunkRenderer.instance.Queue(position);
					chunk.setRenderState(RenderState.Queued);
				}
			}
		}
	}
}
