using UnityEngine;

namespace Fulliron.Alchemy
{
    public class WorldBillboard : MonoBehaviour
    {
        [SerializeField] bool faceCamera = true;
        [SerializeField] bool flipFacing;

        void LateUpdate()
        {
            if (!faceCamera)
                return;

            var camera = Camera.main;
            if (camera == null)
                return;

            transform.rotation = camera.transform.rotation;
            if (flipFacing)
                transform.rotation *= Quaternion.Euler(0f, 180f, 0f);
        }
    }
}
