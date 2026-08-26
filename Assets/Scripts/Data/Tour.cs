using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tour", menuName = "CampusTour/Tour")]
public class Tour : ScriptableObject
{
    public string tourName;
    [TextArea] public string tourDescription;

    public List<PointOfInterest> pois = new List<PointOfInterest>();
}
