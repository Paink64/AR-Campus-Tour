using UnityEngine;

[CreateAssetMenu(fileName = "POI_", menuName = "CampusTour/Point Of Interest")]
public class PointOfInterest : ScriptableObject
{
    [Header("Display")]
    public string title;

    [TextArea(3, 5)]
    public string description;

    [Header("Gallery")]
    [Tooltip("Images shown in the POI slideshow")]
    public Sprite[] gallery;

    [Tooltip("Optional audio narration for this POI")]
    public AudioClip audioGuide;

    [Header("Location (needed for arrow navigation)")]
    public double latitude;
    public double longitude;

    [Header("Cloud Anchor")]
    [Tooltip("Set this after hosting Anchor")]
    public string cloudAnchorId;
}
