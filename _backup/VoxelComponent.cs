using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish;

public class VoxelComponent : MonoBehaviour
{
	public VoxelObject voxelObject;
	public Vector3 pivotPoint;

    public bool reinitialize = false;

	public ulong tickRate = 25;
	public ulong ticks = 0;
	public ulong tickTime = 0;

	public int sizeX = 4;
	public int sizeZ = 4;
    public int sizeY = 4;
	public GameObject chunkPrefab;

	public bool canTick = false;

	public GameObject[,,] chunkObjects;
	public ChunkComponent[,,] chunkComponents;

	public Queue<ChunkComponent> buildingChunks = new Queue<ChunkComponent>();

	public VoxelObjectType type = VoxelObjectType.GENERIC;

	private bool loaded = false;
	private bool initialized = false;

	public void setName(string _name)
	{
		gameObject.name = _name;
		voxelObject.setName(_name);
	}

	public VoxelComponent ChangeType(VoxelObjectType _type)
	{
		return Initialize(_type);
	}

	public VoxelComponent Initialize(VoxelObjectType _type = VoxelObjectType.GENERIC)
	{
		foreach (Transform child in transform)
		{
			Destroy(child.gameObject);
		}

		type = _type;
		switch (type)
		{
			case VoxelObjectType.GENERIC:
				voxelObject = new VoxelObject(sizeX, sizeY, sizeZ, this);
				break;

			case VoxelObjectType.ASTEROID:
				voxelObject = new Asteroid(sizeX, sizeY, sizeZ, this);
				break;

			case VoxelObjectType.PLANETOID:
				voxelObject = new Planetoid(sizeX, sizeY, sizeZ, this);
				break;
		}

		voxelObject.setName(gameObject.name);

		chunkObjects = new GameObject[sizeX, sizeY, sizeZ];
		chunkComponents = new ChunkComponent[sizeX, sizeY, sizeZ];

		pivotPoint = new Vector3(sizeX * Constants.CHUNK_SIZE, sizeY * Constants.CHUNK_SIZE, sizeZ * Constants.CHUNK_SIZE) * 0.5f;
        pivotPoint += (Vector3.one * 0.5f);
		pivotPoint *= -1;

		//StartCoroutine("CreateChunkComponents");

		for (int x = 0; x < sizeX; x++)
		{
			for (int z = 0; z < sizeZ; z++)
			{
				for (int y = sizeY - 1; y >= 0; y--)
				{
					// Vector3 position = new Vector3(x * Constants.CHUNK_SIZE, y * Constants.CHUNK_SIZE, z * Constants.CHUNK_SIZE);
                    // position += pivotPoint;

					// GameObject temp = (GameObject)Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
					// ChunkComponent chunkComponent = temp.GetComponent<ChunkComponent>();
					// chunkComponent.position = new Coord3D(x, y, z, voxelObject);
					// chunkComponent.voxelComponent = this;

					// temp.name = ("Chunk:" + x + "-" + y + "-" + z);
					// temp.transform.parent = this.transform;
					// temp.transform.localPosition = position;
					// chunkObjects[x, y, z] = temp;
					ChunkComponent chunkComponent = this.gameObject.AddComponent<ChunkComponent>();
					chunkComponent.position = new Coord3D(x, y, z, voxelObject);
					chunkComponent.voxelComponent = this;
					chunkComponents[x, y, z] = chunkComponent;

					ChunkLoader.instance.Queue(chunkComponent);
				}
			}
		}

		reinitialize = false;
		initialized = true;
		return this;
	}

	// public IEnumerator CreateChunkComponents()
	// {
	// 	for (int x = 0; x < sizeX; x++)
	// 	{
	// 		for (int z = 0; z < sizeZ; z++)
	// 		{
	// 			for (int y = sizeY - 1; y >= 0; y--)
	// 			{
	// 				// Vector3 position = new Vector3(x * Constants.CHUNK_SIZE, y * Constants.CHUNK_SIZE, z * Constants.CHUNK_SIZE);
    //                 // position += pivotPoint;

	// 				// GameObject temp = (GameObject)Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
	// 				// ChunkComponent chunkComponent = temp.GetComponent<ChunkComponent>();
	// 				// chunkComponent.position = new Coord3D(x, y, z, voxelObject);
	// 				// chunkComponent.voxelComponent = this;

	// 				// temp.name = ("Chunk:" + x + "-" + y + "-" + z);
	// 				// temp.transform.parent = this.transform;
	// 				// temp.transform.localPosition = position;
	// 				// chunkObjects[x, y, z] = temp;
	// 				ChunkComponent chunkComponent = this.gameObject.AddComponent<ChunkComponent>();
	// 				chunkComponent.position = new Coord3D(x, y, z, voxelObject);
	// 				chunkComponent.voxelComponent = this;
	// 				chunkComponents[x, y, z] = chunkComponent;

	// 				ChunkLoader.instance.Queue(chunkComponent);
	// 				yield return null;
	// 			}
	// 		}
	// 	}
	// }

	public void Update()
	{
		if (loaded == false && voxelObject.loaded == true)
		{
			loaded = true;
			Util.LoadVoxelObject(voxelObject, voxelObject.getName());
		}

		if (reinitialize == true)
		{
			reinitialize = false;
			Initialize(type);
		}
		else if (initialized == true)
		{
			if (buildingChunks.Count > 0)
			{
				// ChunkComponent thisComponent = buildingChunks.Dequeue();
				// if (thisComponent != null) { thisComponent.BuildChunk(); }

				for (int i = 0; i < 4; i++)
				{
					if (buildingChunks.Count > 0)
					{
						ChunkComponent thisComponent = buildingChunks.Dequeue();
						if (thisComponent != null) { thisComponent.BuildChunk(); }
					}
				}
			}

			for (int x = 0; x < sizeX; x++)
			{
				for (int z = 0; z < sizeZ; z++)
				{
					for (int y = 0; y < sizeY; y++)
					{
						ChunkComponent thisComponent = chunkComponents[x, y, z];

						if (thisComponent == null) { Debug.Log("Null component!"); return; }
						if (thisComponent.chunk == null) { Debug.Log("Null chunk!"); return; }

						if (thisComponent.chunk != null)
						{
							if (thisComponent.chunk.getCollisionState() != CollisionState.Built || thisComponent.chunk.getRenderState() != RenderState.Rendered)
							{
								buildingChunks.Enqueue(thisComponent);//thisComponent.BuildChunk();
							}
						}
					}
				}
			}
		}
	}

	//	Render chunks
	public void LateUpdate()
	{
		for (int n = 0; n < GameMaster.Instance.voxelMaterials.Length; n++)
		{
			for (int x = 0; x < sizeX; x++)
			{
				for (int z = 0; z < sizeZ; z++)
				{
					for (int y = 0; y < sizeY; y++)
					{
						ChunkComponent thisComponent = chunkComponents[x, y, z];

						if (thisComponent.mesh != null)
						{
							Vector3 position = (thisComponent.position.toVector3() * Constants.CHUNK_SIZE) + pivotPoint;
							position = transform.rotation * position;
							position += transform.position;
							Graphics.DrawMesh(thisComponent.mesh, Matrix4x4.TRS(position, transform.rotation, transform.lossyScale), GameMaster.Instance.voxelMaterials[n], 0);
						}
					}
				}
			}
		}
	}

	public void FixedUpdate()
	{
		if (initialized == true)
		{
			tickTime++;

			if (tickTime % tickRate == 0 && tickTime != 0)
			{
				if (canTick == true)
				{
					voxelObject.Tick();
				}

				ticks++;
				tickTime = 0;
			}
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (Application.platform != RuntimePlatform.WindowsEditor) { return; }

		Matrix4x4 rotationMatrix;

		if (Application.isPlaying == true && initialized == true)
		{
			for (int x = 0; x < sizeX; x++)
			{
				for (int z = 0; z < sizeZ; z++)
				{
					for (int y = 0; y < sizeY; y++)
					{
						ChunkComponent thisComponent = chunkComponents[x, y, z];
						Chunk thisChunk = thisComponent.chunk;

						if (thisChunk != null)
						{
							if (thisChunk.isVoid() == false && thisChunk.isSolid() == false)
							{
								rotationMatrix = Matrix4x4.TRS(transform.TransformPoint(thisComponent.transform.localPosition), transform.rotation, transform.lossyScale);
								Gizmos.matrix = rotationMatrix;
								Gizmos.color = Color.green;
								Gizmos.DrawWireCube((Vector3.one * (Constants.CHUNK_SIZE * 0.5f)), new Vector3(Constants.CHUNK_SIZE, Constants.CHUNK_SIZE, Constants.CHUNK_SIZE));
							}
							else if (thisChunk.isSolid() == true)
							{
								rotationMatrix = Matrix4x4.TRS(transform.TransformPoint(thisComponent.transform.localPosition), transform.rotation, transform.lossyScale);
								Gizmos.matrix = rotationMatrix;
								Gizmos.color = Color.blue;
								Gizmos.DrawWireCube((Vector3.one * (Constants.CHUNK_SIZE * 0.5f)), new Vector3(Constants.CHUNK_SIZE, Constants.CHUNK_SIZE, Constants.CHUNK_SIZE));
							}
						}
					}
				}
			}
		}
		else
		{
			pivotPoint = new Vector3(sizeX * Constants.CHUNK_SIZE, sizeY * Constants.CHUNK_SIZE, sizeZ * Constants.CHUNK_SIZE) * 0.5f;
			pivotPoint += (Vector3.one * 0.5f);
			pivotPoint *= -1;

			for (int x = 0; x < sizeX; x++)
			{
				for (int z = 0; z < sizeZ; z++)
				{
					for (int y = 0; y < sizeY; y++)
					{
						Vector3 chunkPos = new Vector3(x * Constants.CHUNK_SIZE, y * Constants.CHUNK_SIZE, z * Constants.CHUNK_SIZE);
                    	chunkPos += pivotPoint;
						rotationMatrix = Matrix4x4.TRS(transform.TransformPoint(chunkPos), transform.rotation, transform.lossyScale);
						Gizmos.matrix = rotationMatrix;
						Gizmos.color = Color.red;
						Gizmos.DrawWireCube((Vector3.one * (Constants.CHUNK_SIZE * 0.5f)), new Vector3(Constants.CHUNK_SIZE, Constants.CHUNK_SIZE, Constants.CHUNK_SIZE));
					}
				}
			}
		}

        Gizmos.color = Color.yellow;
		rotationMatrix = Matrix4x4.TRS(transform.TransformPoint(Vector3.zero), transform.rotation, transform.lossyScale);
		Gizmos.matrix = rotationMatrix;
		Gizmos.DrawWireCube(-(Vector3.one * 0.5f),
            new Vector3(Constants.CHUNK_SIZE * sizeX, Constants.CHUNK_SIZE * sizeY, Constants.CHUNK_SIZE * sizeZ)
            );
	}
}