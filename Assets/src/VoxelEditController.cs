using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Swordfish;

public class VoxelEditController : MonoBehaviour
{
    private Mesh displayModel;
    private VoxelHitData hitData;

    public Material displayMaterial;

    public AudioClip placeSound;

    public int currentPaletteIndex = 2;
    public Voxel currentVoxel = Voxel.METAL_PANEL;

    public int rotationIndex = 0;
    public Direction rotationDirection = Direction.NORTH;
    public Direction rotationOrientation = Direction.NORTH;

    public Image paletteImage;

    public void Start()
    {
        Texture2D texture = GameMaster.Instance.getCachedImage(currentVoxel.ToString()).texture;
        paletteImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        displayModel = new Mesh();
        UpdateDisplayModel();
    }

    // public void Update()
    // {
    //     RaycastHit hit;
    //     if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 8))
    //     {
    //         hitData = new VoxelHitData(hit);

    //         if (hitData.isValid())
    //         {
    //             FloatingOrigin.Shift(hitData.rayInfo.transform.position);
    //         }
    //     }
    // }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) == true)
        {
            currentPaletteIndex = Util.WrapInt(currentPaletteIndex += 1, 2, Enum.GetValues(typeof(Voxel)).Length - 1);
            currentVoxel = (Voxel)currentPaletteIndex;
            Texture2D texture = GameMaster.Instance.getCachedImage(currentVoxel.ToString()).texture;
            paletteImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        if (Input.GetKeyDown(KeyCode.F2) == true)
        {
            if (hitData != null && hitData.isValid())
            {
                Util.SaveVoxelObject(hitData.voxelObject, "test");
            }
        }

        if (Input.GetKeyDown(KeyCode.F3) == true)
        {
            if (hitData != null && hitData.isValid())
            {
                Util.LoadVoxelObject(hitData.voxelObject, "test");
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            rotationIndex = Util.WrapInt(rotationIndex += 1, 0, 3);
            rotationDirection = (Direction)rotationIndex;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            rotationIndex = Util.WrapInt(rotationIndex -= 1, 0, 3);
            rotationDirection = (Direction)rotationIndex;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 8))
        {
            hitData = new VoxelHitData(hit);

            if (hitData.isValid())
            {
                rotationOrientation = hitData.getFace().getOpposite();

                if (Input.GetMouseButtonDown(1) == true)
                {
                    if (hitData.getAtFace().getType() == Voxel.VOID)
                    {
                        Block thisBlock = hitData.voxelObject.setBlockAt(hitData.atFace, currentVoxel);

                        if (thisBlock.getChunk() != null)
                        {
                            AudioSource.PlayClipAtPoint(placeSound, hit.point);

                            if (thisBlock.getBlockData() is Rotatable)
                            {
                                Rotatable data = (Rotatable)thisBlock.getBlockData();
                                data.setDirection(rotationDirection);
                                thisBlock.setBlockData(data);
                            }

                            if (thisBlock.getBlockData() is Orientated)
                            {
                                Orientated data = (Orientated)thisBlock.getBlockData();
                                data.setOrientation(rotationOrientation);
                                thisBlock.setBlockData(data);
                            }

                            if ( hitData.getFace() == Direction.BELOW )
                            {
                                if (thisBlock.getBlockData() is Flippable)
                                {
                                    Flippable data = (Flippable)thisBlock.getBlockData();
                                    data.setFlipped(true);
                                    thisBlock.setBlockData(data);
                                }
                            }
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(0) == true)
                {
                    Block clickedBlock = hitData.getAt();
                    if (clickedBlock.getType() != Voxel.VOID && clickedBlock.getType() != Voxel.SHIP_CORE)
                    {
                        Block thisBlock = hitData.voxelObject.setBlockAt(hitData.atHit, Voxel.VOID);

                        if (thisBlock.getChunk() != null)
                        {
                            AudioSource.PlayClipAtPoint(placeSound, hit.point);
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(2) == true)
                {
                    Block clickedBlock = hitData.getAt();
                    if (clickedBlock.getType() != Voxel.VOID && clickedBlock.getType() != Voxel.SHIP_CORE)
                    {
                        currentVoxel = clickedBlock.getType();
                        currentPaletteIndex = (int)currentVoxel;
                    }
                }

                UpdateDisplayModel();
                Graphics.DrawMesh(displayModel, hitData.rayInfo.transform.rotation * (hitData.localFacePosition + hitData.component.pivotPoint) + hitData.rayInfo.transform.position, hitData.rayInfo.transform.rotation, displayMaterial, 0);
            }
        }
    }

    public void UpdateDisplayModel()
    {
        List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<Color> colors = new List<Color>();

        Block thisBlock = currentVoxel.toBlock();

        if (thisBlock.getBlockData() is Rotatable)
        {
            Rotatable data = (Rotatable)thisBlock.getBlockData();
            data.setDirection(rotationDirection);
            thisBlock.setBlockData(data);
        }

        if (thisBlock.getBlockData() is Orientated && hitData != null)
        {
            Orientated data = (Orientated)thisBlock.getBlockData();
            data.setOrientation(rotationOrientation);
            thisBlock.setBlockData(data);
        }

        if ( hitData != null && hitData.getFace() == Direction.BELOW )
        {
            if (thisBlock.getBlockData() is Flippable)
            {
                Flippable data = (Flippable)thisBlock.getBlockData();
                data.setFlipped(true);
                thisBlock.setBlockData(data);
            }
        }

        switch (thisBlock.getModelType())
        {
            case ModelType.CUBE:
                ModelBuilder.Cube.Top(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Cube.Bottom(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Cube.North(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Cube.East(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Cube.South(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Cube.West(vertices, triangles, normals, uvs, colors, thisBlock);
                break;

            case ModelType.SLOPE:
                ModelBuilder.Slope.Face(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Slope.Bottom(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Slope.North(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Slope.East(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Slope.West(vertices, triangles, normals, uvs, colors, thisBlock);
                break;

            case ModelType.CUSTOM:
                ModelBuilder.Custom.Build(vertices, triangles, normals, uvs, colors, thisBlock.getModelData(), thisBlock);
                break;

            case ModelType.CUSTOM_CUBE:
                ModelBuilder.Cube.Top(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Cube.Bottom(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Cube.North(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Cube.South(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Cube.East(vertices, triangles, normals, uvs, colors, thisBlock);
                ModelBuilder.Cube.West(vertices, triangles, normals, uvs, colors, thisBlock);
                break;

            case ModelType.CROSS_SECTION_SMALL:
                ModelBuilder.CrossSection.Small.Build(vertices, triangles, normals, uvs, colors, thisBlock);
                break;

            case ModelType.CUBE_HALF:
                Vector3 scale = new Vector3(1.0f, 1.0f, 0.5f);
                ModelBuilder.Cube.Top(vertices, triangles, normals, uvs, colors, thisBlock, scale);
                ModelBuilder.Cube.Bottom(vertices, triangles, normals, uvs, colors, thisBlock, scale);
                ModelBuilder.Cube.North(vertices, triangles, normals, uvs, colors, thisBlock, scale);
                ModelBuilder.Cube.South(vertices, triangles, normals, uvs, colors, thisBlock, scale);
                ModelBuilder.Cube.East(vertices, triangles, normals, uvs, colors, thisBlock, scale);
                ModelBuilder.Cube.West(vertices, triangles, normals, uvs, colors, thisBlock, scale);
                break;
        }

        displayModel.Clear();

        displayModel.vertices = vertices.ToArray();
        displayModel.triangles = triangles.ToArray();
        displayModel.normals = normals.ToArray();
        displayModel.uv = uvs.ToArray();
        displayModel.colors = colors.ToArray();
    }
}