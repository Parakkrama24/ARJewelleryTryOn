using UnityEngine;

public static class HandCoordinateConverter
{
    public static Vector3 ToUnityWorld(Vector3 mediaPipeWorld, Camera camera)
    {
        Vector3 cameraSpace = new Vector3(
            mediaPipeWorld.x,
            -mediaPipeWorld.y,
            mediaPipeWorld.z
        );

        return camera.transform.TransformPoint(cameraSpace);
    }
}