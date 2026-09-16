using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    public Camera targetCamera;

    [Header("Rotation")]
    public bool lockVertical = true;
    public bool allowVerticalTilt = true;
    [Range(0f, 89f)]
    public float maxVerticalAngle = 45f;

    // Runs once a Frame
    void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;
        // object position - camera position = direction from the camera to the object
        Vector3 dir = transform.position - targetCamera.transform.position;

        // Rotates towards camera, but no tilt up or down
        if (lockVertical && !allowVerticalTilt)
            dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        //Create a rotation that points in the dir direction saved as four-dimensional number system
        Quaternion lookRot = Quaternion.LookRotation(dir);

        // Apply vertical tilt
        if (allowVerticalTilt)
        {
            /*
            Converts the quaternion rotation into euler rotation easier to understand
            X = tilt up/down
            Y = turn left/right
            Z = roll sideways
            */
            Vector3 euler = lookRot.eulerAngles;

            float pitch = NormalizeAngle(euler.x);
            pitch = Mathf.Clamp(pitch, -maxVerticalAngle, maxVerticalAngle);

            lookRot = Quaternion.Euler(pitch, euler.y, 0f);
        }

        transform.rotation = lookRot;
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
