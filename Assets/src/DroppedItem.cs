using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swordfish.items;

namespace Swordfish
{
    public class DroppedItem : Interactable
    {
		public Billboard billboard = null;
		public MeshRenderer renderer = null;
		public Mesh mesh = null;

		public TextMesh displayName = null;
		public ItemType presetItem = ItemType.VOID;
		public Voxel presetVoxel = Voxel.VOID;
		public int presetCount = 1;

		[SerializeField]
		protected Item item = null;

		public void setItem(Item _item)
		{
			item = _item;

			UpdateDisplay();
		}

		public Item getItem()
		{
			return item;
		}

		public void Start()
		{
			if (presetItem != ItemType.VOID)
			{
				if (presetItem == ItemType.BLOCKITEM)
				{
					item = presetVoxel.toItem();
					item.setAmount(presetCount);
				}
				else
				{
					item = presetItem.toItem();
					item.setAmount(presetCount);
				}

				UpdateDisplay();
			}
		}

		public void LateUpdate()
		{
			if (item.getType() == ItemType.BLOCKITEM && mesh != null)
			{
				Vector3 position = new Vector3(transform.position.x, transform.position.y + (Mathf.Sin(Time.time * 3) * 0.1f), transform.position.z);
				Quaternion rotation = Quaternion.Euler(0, Mathf.Sin(Time.time * 0.5f) * 180, 0);
				Vector3 scale = Vector3.one * 0.5f;
				Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);

				if (item.getAmount() > 1)
				{
					int drawCount = 2 + (int)( item.getAmount() * 0.05f );

					for (int i = 0; i < drawCount; i++)
					{
						UnityEngine.Random.seed = i;

						Vector3 position2 = new Vector3( position.x + ((UnityEngine.Random.value - 0.5f) * 0.5f), position.y + ((UnityEngine.Random.value - 0.5f) * 0.5f), position.z + ((UnityEngine.Random.value - 0.5f) * 0.5f) );

						matrix = Matrix4x4.TRS(position2, rotation, scale * UnityEngine.Random.value );
						Graphics.DrawMesh(mesh, matrix, GameMaster.Instance.voxelMaterials[0], 0);
					}

					//Graphics.DrawMeshInstanced(mesh, 0, GameMaster.Instance.voxelMaterials[0], matrices);
				}
				else
				{
					Graphics.DrawMesh(mesh, matrix, GameMaster.Instance.voxelMaterials[0], 0);
				}
			}

			if (Vector3.Distance(transform.position, GameMaster.Instance.player.transform.position) <= 2.0f)
			{
				Interact(GameMaster.Instance.player.gameObject);
			}

			if (item == null || item.isValid() == false)
			{
				Destroy(this.gameObject);
			}
		}

		public void UpdateDisplay()
		{
			if (item.getType() == ItemType.BLOCKITEM)
			{
				renderer.enabled = false;

				mesh = ModelBuilder.GetVoxelMesh( ((BLOCKITEM)item).getVoxel() );
			}
			else
			{
				renderer.enabled = true;

				CachedImage cachedImage = item.getImageData();

				Texture2D texture = null;
				if (cachedImage != null) { texture = cachedImage.texture; }

				renderer.material.SetTexture("_BaseColorMap", texture);
			}

			displayName.text = item.getName();
		}

        public override void Hover(GameObject _interactor = null)
        {
			UpdateDisplay();
        }

        public override void Interact(GameObject _interactor = null)
        {
			Entity entity = _interactor.GetComponent<Entity>();

			if (entity != null && item != null)
			{
				Inventory inventory = entity.getInventory();
				item = inventory.Add(item);

				if (item == null || item.isValid() == false)
				{
					GameMaster.Instance.PlaySound(GameMaster.Instance.pickupSound, transform.position, 2.0f);
					Destroy(this.gameObject);
				}
			}
        }
    }
}