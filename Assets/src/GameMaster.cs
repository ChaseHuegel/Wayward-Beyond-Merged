﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Swordfish;
using Swordfish.items;

public class GameMaster : MonoBehaviour
{
	public bool debug = false;

	public bool reinitialize = false;

	public VoxelMaster voxelMaster;
	public ulong tickRate = 25;
	public ulong ticks = 0;
	public ulong tickTime = 0;

	public int loadQueue = 0;
	public int renderQueue = 0;
	public int collisionQueue = 0;

	public Material[] voxelMaterials;
	public Material[] thumbnailMaterials;
	public Material[] selectionMaterials;
	public Mesh[] models;
	public Texture2D[] images;

	public CachedModel[] cachedModels;
	public CachedImage[] cachedImages;
	public CachedImage[] itemTextures;
	public CachedImage[] voxelTextures;
	public CachedImage[] voxelThumbnails;

	private int frameCount = 0;
 	private float dt = 0.0f;
	private float fps = 0.0f;
	private int updateRate = 4;

	private float renderRate = 0.0f;
	private float collisionRate = 0.0f;
	private float loadRate = 0.0f;

	public int rotations = 0;

	public PlayerMotor player;

	public GameObject voxelObjectPrefab;
	public GameObject voxelColliderPrefab;
	public GameObject droppedItemPrefab;

	public AudioSource audioPlayer;
	public AudioClip placeSound;
	public AudioClip pickupSound;

	public bool followingShip = false;
	public Transform playerCamera = null;
	public Transform shipCamera = null;
	public Transform shipObject = null;

	//  Keep this object alive
    private static GameMaster _instance;
    public static GameMaster Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("GameMaster").GetComponent<GameMaster>();
            }

            return _instance;
        }
    }

	public void Start()
	{
		GameMaster.Instance.Initialize();
	}

	public void OnGUI()
	{
		if (debug == true)
		{
			GUILayout.Label("FPS: " + (int)fps);
			GUILayout.Label("Render Thread: " + ChunkRenderer.instance.State() + " / " + renderQueue + " / TPS: " + renderRate);
			GUILayout.Label("Collision Thread: " + ChunkCollisionBuilder.instance.State() + " / " + collisionQueue + " / TPS: " + collisionRate);
			GUILayout.Label("Load Thread: " + ChunkLoader.instance.State() + " / " + loadQueue + " / TPS: " + loadRate);
		}
	}

	public void Initialize()
	{
		voxelMaster = new VoxelMaster();

		cachedModels = new CachedModel[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			cachedModels[i] = new CachedModel(models[i].vertices, models[i].normals, models[i].triangles, models[i].uv, models[i].colors, models[i].name);
		}

		//	Setup material textures
		voxelMaterials[0].SetTexture("_albedo", ResourceManager.GetImage("atlas.metal_01").image.texture);
		voxelMaterials[0].SetTexture("_emission", ResourceManager.GetImage("atlas.metal_01_e").image.texture);
		voxelMaterials[0].SetTexture("_metallic", ResourceManager.GetImage("atlas.metal_01_m").image.texture);

		thumbnailMaterials[0].SetTexture("_UnlitColorMap", ResourceManager.GetImage("atlas.metal_01").image.texture);
		selectionMaterials[0].SetTexture("_UnlitColorMap", ResourceManager.GetImage("atlas.metal_01").image.texture);

		CreateVoxelThumbnails();

		ChunkRenderer.instance.Abort();
		ChunkRenderer.instance.ClearQueue();

		ChunkCollisionBuilder.instance.Abort();
		ChunkCollisionBuilder.instance.ClearQueue();

		ChunkLoader.instance.Abort();
		ChunkLoader.instance.ClearQueue();

		//GC.Collect();

		ChunkRenderer.instance.setState(RenderState.Ready);
		ChunkRenderer.instance.Start();

		ChunkCollisionBuilder.instance.setState(CollisionState.Ready);
		ChunkCollisionBuilder.instance.Start();

		ChunkLoader.instance.setState(ChunkState.Ready);
		ChunkLoader.instance.Start();
	}

	public void OnApplicationQuit()
	{
		ChunkRenderer.instance.Abort();
		ChunkCollisionBuilder.instance.Abort();
		ChunkLoader.instance.Abort();
	}

	public void FixedUpdate()
	{
		tickTime++;

		if (tickTime % tickRate == 0 && tickTime != 0)
		{
			voxelMaster.Tick();

			ticks++;
			tickTime = 0;
		}
	}

	public void LateUpdate()
	{
		voxelMaster.Render();
	}

	public void Update()
	{
		if (Time.frameCount % 60 == 0)
		{
			GC.Collect();
		}

		voxelMaster.Update();

		if (InputManager.Get("Camera View") == true)
		{
			followingShip = !followingShip;

			playerCamera.position = shipObject.position;

			shipCamera.gameObject.SetActive( followingShip );
			playerCamera.gameObject.SetActive( !followingShip );

			shipObject.GetComponent<GentleBob>().isEnabled = followingShip;
			shipObject.GetComponent<Rigidbody>().isKinematic = !followingShip;
		}

		if (Input.GetKeyDown(KeyCode.Escape) == true)
		{
			Application.Quit();
		}

		if (Input.GetKeyDown(KeyCode.F1) == true)
		{
			SpawnVoxelObject(Position.fromVector3(player.transform.position));
		}

		if (Input.GetKeyDown(KeyCode.F2) == true)
		{
			SpawnAsteroid(Position.fromVector3(player.transform.position));
		}

		if (Input.GetKeyDown(KeyCode.F3) == true)
		{
			debug = !debug;
		}

		if (reinitialize == true)
		{
			reinitialize = false;
			Initialize();
		}

		renderQueue = ChunkRenderer.instance.QueueSize();
		collisionQueue = ChunkCollisionBuilder.instance.QueueSize();
		loadQueue = ChunkLoader.instance.QueueSize();

		frameCount++;
		dt += Time.deltaTime;
		if (dt > 1.0f/updateRate)
		{
			fps = frameCount / dt;
			frameCount = 0;

			if (ChunkRenderer.instance.State() != RenderState.Waiting)
			{
				renderRate = ChunkRenderer.instance.updates / dt;
			}

			if (ChunkCollisionBuilder.instance.State() != CollisionState.Waiting)
			{
				collisionRate = ChunkCollisionBuilder.instance.updates / dt;
			}

			if (ChunkLoader.instance.State() != ChunkState.Waiting)
			{
				loadRate = ChunkLoader.instance.updates / dt;
			}

			ChunkRenderer.instance.updates = 0;
			ChunkCollisionBuilder.instance.updates = 0;
			ChunkLoader.instance.updates = 0;

			dt -= 1.0f/updateRate;
		}
	}

	public int getFPS()
	{
		return (int)fps;
	}

	public CachedModel getCachedModel(string _name)
	{
		for (int i = 0; i < cachedModels.Length; i++)
		{
			if (cachedModels[i].name == _name)
			{
				return cachedModels[i];
			}
		}

		return null;
	}

	public CachedImage getCachedImage(string _name)
	{
		for (int i = 0; i < voxelThumbnails.Length; i++)
		{
			if (voxelThumbnails[i].name == _name)
			{
				return voxelThumbnails[i];
			}
		}

		return null;
	}

	public void CreateVoxelThumbnails()
	{
		Voxel[] voxels = (Voxel[])Enum.GetValues(typeof(Voxel));
		voxelThumbnails = new CachedImage[voxels.Length];
		for(int i = 0; i < voxels.Length; i++)
		{
			Voxel voxel = voxels[i];
			Block thisBlock = voxel.toBlock();

			int width = 512, height = 512;
			Texture2D result = new Texture2D(width, height);
			Camera renderCamera = new GameObject().AddComponent<Camera>();
			renderCamera.transform.position = new Vector3(-1, 1, -1);
			renderCamera.transform.LookAt(Vector3.zero);
			renderCamera.orthographic = true;
			renderCamera.orthographicSize = 1.0f;
			renderCamera.clearFlags = CameraClearFlags.SolidColor;
			renderCamera.backgroundColor = Color.green;
			// renderCamera.allowMSAA = false;

			RenderTexture temp = RenderTexture.active;
			RenderTexture renderTex = RenderTexture.GetTemporary( width, height, 32 );
			RenderTexture.active = renderTex;
			renderCamera.targetTexture = renderTex;
			// GL.Clear(true, true, Color.green);

			Mesh mesh = ModelBuilder.GetVoxelMesh(voxel);

			Position offset = thisBlock.getViewOffset();
			Graphics.DrawMesh(mesh, Matrix4x4.TRS(new Vector3(offset.x, offset.y, offset.z), Quaternion.identity, Vector3.one), thumbnailMaterials[0], 0, renderCamera);

			renderCamera.Render();
			renderCamera.targetTexture = null;

			result = new Texture2D( width, height, TextureFormat.RGBA32, false );
			result.ReadPixels( new Rect( 0, 0, width, height ), 0, 0, false );
			result.Apply( false, false );

			RenderTexture.active = temp;
			RenderTexture.ReleaseTemporary( renderTex );

			Color clearColor = new Color(0, 0, 0, 0);
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < width; y++)
				{
					if (result.GetPixel(x, y).g >= 0.75f && result.GetPixel(x, y).r <= 0.4f && result.GetPixel(x, y).b <= 0.4f)
					{
						clearColor = result.GetPixel(x, y);
						clearColor.a = 0.0f;
						result.SetPixel(x, y, clearColor);
					}
				}
			}
			result.Apply( false, false );
			result.filterMode = FilterMode.Point;

			voxelThumbnails[i] = new CachedImage(result, voxel.ToString());
			Destroy(renderCamera.gameObject);
		}
	}

	public void PlaySound(AudioClip _clip, float _volume = 1.0f)
	{
		audioPlayer.PlayOneShot(_clip, _volume);
	}

	public void PlaySound(AudioClip _clip, Vector3 _position, float _volume = 1.0f)
	{
		AudioSource.PlayClipAtPoint(_clip, _position, _volume);
	}

	public Item DropItemNaturally(Position _position, Voxel _voxel, int _count = 1)
	{
		return ((BLOCKITEM)DropItemNaturally(_position, _voxel.toItem(), _count)).setVoxel(_voxel);
	}

	public Item DropItemNaturally(Position _position, ItemType _type, int _count = 1)
	{
		return DropItemNaturally(_position, _type.toItem(), _count);
	}

	public Item DropItemNaturally(Position _position, Item _item, int _count = 1)
	{
		Vector3 offset = new Vector3( UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f ) * 0.5f;
		GameObject temp = (GameObject)Instantiate(droppedItemPrefab, _position.toVector3() + offset, Quaternion.identity);
		DroppedItem dropped = temp.GetComponent<DroppedItem>();
		Item item = _item.copy();
		item.setAmount(_count);
		dropped.setItem(item);
		return item;
	}

	public VoxelObject SpawnVoxelObject(Position _position)
	{
		VoxelComponent component = Instantiate(voxelObjectPrefab, _position.toVector3(), Quaternion.identity).GetComponent<VoxelComponent>();
		component.setName("voxelObject" + voxelMaster.voxelObjects.Count);
		component.Initialize(VoxelObjectType.GENERIC);
		return component.voxelObject;
	}

	public VoxelObject SpawnAsteroid(Position _position)
	{
		VoxelComponent component = Instantiate(voxelObjectPrefab, _position.toVector3(), Quaternion.identity).GetComponent<VoxelComponent>();
		component.setName("asteroid" + voxelMaster.voxelObjects.Count);
		component.Initialize(VoxelObjectType.ASTEROID);
		return component.voxelObject;
	}
}