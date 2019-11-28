using System;
using System.Collections;
using System.Collections.Generic;

namespace Swordfish
{
	[Serializable]
	public class VoxelMaster
	{
		public Dictionary<Guid, VoxelObject> voxelObjects = new Dictionary<Guid, VoxelObject>();

		public void Tick()
		{
			foreach (KeyValuePair<Guid, VoxelObject> entry in voxelObjects)
            {
                VoxelObject thisVoxelObject = entry.Value;

				if (thisVoxelObject != null && thisVoxelObject.isStatic == false)
				{
					thisVoxelObject.Tick();
				}

				if (thisVoxelObject.component != null)
				{
					thisVoxelObject.component.Tick();
				}
			}
		}

		public void Update()
		{
			foreach (KeyValuePair<Guid, VoxelObject> entry in voxelObjects)
            {
                VoxelObject thisVoxelObject = entry.Value;

				if (thisVoxelObject != null)
				{
					thisVoxelObject.Update();
				}

				if (thisVoxelObject.component != null)
				{
					thisVoxelObject.component.TryBuildChunk();
				}
			}
		}

		public void Render()
		{
			foreach (KeyValuePair<Guid, VoxelObject> entry in voxelObjects)
            {
                VoxelObject thisVoxelObject = entry.Value;

				if (thisVoxelObject != null && thisVoxelObject.loaded == true)
				{
					thisVoxelObject.Render();
				}
			}
		}
	}
}