using System;
using UnityEngine;

namespace Swordfish
{
    public class VoxelHitData
    {
        public RaycastHit rayInfo;

        public Vector3 transformPosition;

        public Vector3 worldPosition;
        public Vector3 localPosition;

        public Vector3 worldFacePosition;
        public Vector3 localFacePosition;

        public Vector3 worldNormal;
        public Vector3 localNormal;

        public Coord3D atHit;
        public Coord3D atFace;

        public VoxelObject voxelObject;
        public VoxelComponent component;

        public VoxelHitData(RaycastHit _hit)
        {
            rayInfo = _hit;

            component = _hit.transform.GetComponent<VoxelComponent>();

            if (component == null) { return; }

            voxelObject = component.voxelObject;

            worldNormal = _hit.normal;
            worldNormal.Normalize();
            localNormal = _hit.transform.InverseTransformDirection(_hit.normal);
            localNormal.Normalize();

            _hit.point += (_hit.normal * 0.1f);   //  Pad the ray to penetrate into the voxel's space

            transformPosition = _hit.transform.position + ((BoxCollider)_hit.collider).center;
            transformPosition -= (Vector3.one * 0.5f); //  Reverse block centering

            //localPosition = _hit.transform.InverseTransformPoint(_hit.point); //  USE CLICK SPACE INSTEAD OF COLLIDER SPACE
            localPosition = _hit.collider.transform.localPosition + ((BoxCollider)_hit.collider).center;
            localPosition -= component.pivotPoint;    //  Offset by the pivot
            localPosition -= (Vector3.one * 0.5f); //  Reverse block centering
            //localPosition += localNormal; //  The hit voxel is offset by the normal of the face we hit  //  USE CLICK SPACE INSTEAD OF COLLIDER SPACE
            localPosition = new Vector3(
                (float)Math.Round(localPosition.x, 0, MidpointRounding.ToEven),
                (float)Math.Round(localPosition.y, 0, MidpointRounding.ToEven),
                (float)Math.Round(localPosition.z, 0, MidpointRounding.ToEven) );
            localFacePosition = (localPosition + localNormal);

            worldPosition = _hit.transform.InverseTransformDirection(_hit.point);
            worldPosition -= worldNormal; //  The hit voxel is offset by the normal of the face we hit
            worldPosition = new Vector3(
                (float)Math.Round(worldPosition.x, 0, MidpointRounding.ToEven),
                (float)Math.Round(worldPosition.y, 0, MidpointRounding.ToEven),
                (float)Math.Round(worldPosition.z, 0, MidpointRounding.ToEven) );
            worldFacePosition = (worldPosition + worldNormal);

            atHit   = Coord3D.fromVector3(localPosition); //  The coord of the voxel that was hit
            atFace  = Coord3D.fromVector3(localFacePosition);  //  The coord of the voxel at the face that was hit
        }

        public Direction getFace()
        {
            if (localNormal == Vector3.forward)
                return Direction.NORTH;
            if (localNormal == -Vector3.forward)
                return Direction.SOUTH;
            if (localNormal == -Vector3.right)
                return Direction.WEST;
            if (localNormal == Vector3.right)
                return Direction.EAST;
            if (localNormal == Vector3.up)
                return Direction.ABOVE;
           if (localNormal == -Vector3.up)
                return Direction.BELOW;

            return Direction.NORTH;
        }

        public Block getAtFace()
        {
            if (voxelObject == null) { return Voxel.VOID.toBlock(); }

            return voxelObject.getBlockAt(atFace);
        }

        public Block getAt()
        {
            if (voxelObject == null) { return Voxel.VOID.toBlock(); }

            return voxelObject.getBlockAt(atHit);
        }

        public bool isValid()
        {
            return ( voxelObject != null && component != null );
        }
    }
}
