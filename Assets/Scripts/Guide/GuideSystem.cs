using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class GuideSystem : MonoBehaviour
{
    [Header("GPS Settings")]

    [Tooltip("Desired accuracy")]
    public float AccuracyInMeters = 5.0f; 

    [Tooltip("Minimum distance to trigger an update")]
    public float updateDistanceInMeters = 2.0f; 

    public Text InstructionText;
    public RectTransform imageToRotate;
    public float rotationSpeed = 5f;

    public bool IsServicesInitialized { get; private set; } = false;

    private ARViewManager arData;

    private void Start()
    {
        arData = GetComponent<ARViewManager>();

        StartCoroutine(InitializeLocationAndCompass());
    }

    private IEnumerator InitializeLocationAndCompass()
    {
        // Permissions for Android
        #if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitForSeconds(2.0f); 
        }
        #endif

        // Check if device location settings enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("GPS hardware or location settings are disabled on this device.");
            yield break;
        }

        Input.location.Start(AccuracyInMeters, updateDistanceInMeters);

        int maxWaitSeconds = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWaitSeconds > 0)
        {
            yield return new WaitForSeconds(1.0f);
            maxWaitSeconds--;
        }

        if (maxWaitSeconds <= 0 || Input.location.status == LocationServiceStatus.Failed)
        {
            yield break;
        }

        Input.compass.enabled = true;
        IsServicesInitialized = true;
    }

    private void Update()
    {
        if (!IsServicesInitialized || Input.location.status != LocationServiceStatus.Running) return;

        float currentLatitude = Input.location.lastData.latitude;
        float currentLongitude = Input.location.lastData.longitude;
        float currentHeading = Input.compass.trueHeading;

        float destinationBearing = CalculateBearing(currentLatitude, currentLongitude, (float)arData.poiLatitude, (float)arData.poiLongitude);
        float turnAngle = destinationBearing - currentHeading;
        if (turnAngle > 180) turnAngle -= 360;
        if (turnAngle < -180) turnAngle += 360;

        string turnDirection = turnAngle >= 0 ? $"Turn Right {turnAngle:F0}°" : $"Turn Left {Mathf.Abs(turnAngle):F0}°";
        if (Mathf.Abs(turnAngle) < 5f) turnDirection = "Target directly ahead!";

        if (imageToRotate != null)
        {
            imageToRotate.Rotate(Vector3.forward * Input.compass.trueHeading);
        }
        if (imageToRotate != null)
        {
            float targetUIRotation = destinationBearing - currentHeading;
            Quaternion targetQuaternion = Quaternion.Euler(0, 0, -targetUIRotation);
            imageToRotate.localRotation = Quaternion.Slerp(imageToRotate.localRotation, targetQuaternion, Time.deltaTime * rotationSpeed);
        }
        // Test output
        InstructionText.text = $"Lat: {currentLatitude:F6} | Lon: {currentLongitude:F6} | Heading: {currentHeading:F1}° \nNext POI: {arData.poiTitle} \nLat: {arData.poiLatitude:F6} | Lon: {arData.poiLongitude:F6} Bearing: {destinationBearing:F0}°\n<b>{turnDirection}</b>";
    }

    private float CalculateBearing(float latitude1, float longitude1, float latitude2, float longitude2)
    {
        // Convert degrees to radians
        float rLatitude1 = latitude1 * Mathf.Deg2Rad;
        float rLongitude1 = longitude1 * Mathf.Deg2Rad;
        float rLatitude2 = latitude2 * Mathf.Deg2Rad;
        float rLongitude2 = longitude2 * Mathf.Deg2Rad;

        float diffLongitude = rLongitude2 - rLongitude1;

        float y = Mathf.Sin(diffLongitude) * Mathf.Cos(rLatitude2);
        float x = Mathf.Cos(rLatitude1) * Mathf.Sin(rLatitude2) - Mathf.Sin(rLatitude1) * Mathf.Cos(rLatitude2) * Mathf.Cos(diffLongitude);

        float bearingRad = Mathf.Atan2(y, x);
        float twoPi = 2 * Mathf.PI;
        bearingRad = ((bearingRad % twoPi) + twoPi) % twoPi;
        float bearingDeg = bearingRad * Mathf.Rad2Deg;

        return bearingDeg;
    }

}
