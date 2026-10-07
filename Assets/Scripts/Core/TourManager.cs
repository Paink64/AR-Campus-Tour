using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors;

public class TourManager : MonoBehaviour
{
    [Header("References")]
    public PersistentCloudAnchorsController Controller;
    public TMP_Dropdown TourDropdown;

    [Header("Tours")]
    public List<Tour> Tours = new List<Tour>();

    private Tour _activeTour;
    private int _poiIndex = -1;

    public ARViewManager ArView;

    private void OnEnable()
    {
        BuildTourDropdown();

        TourDropdown.onValueChanged.RemoveListener(OnTourSelected);
        TourDropdown.onValueChanged.AddListener(OnTourSelected);

        if (Tours != null && Tours.Count > 0)
            OnTourSelected(TourDropdown.value);
    }

    private void OnDisable()
    {
        if (TourDropdown != null)
            TourDropdown.onValueChanged.RemoveListener(OnTourSelected);
    }

    private void BuildTourDropdown()
    {
        if (TourDropdown == null)
        {
            return;
        }

        TourDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();

        foreach (var t in Tours)
            options.Add(new TMP_Dropdown.OptionData(t ? t.tourName : "Please select a tour"));

        TourDropdown.AddOptions(options);
    }

    private void OnTourSelected(int index)
    {
        if (Tours == null)
        {
            return;
        }

        if (index < 0 || index >= Tours.Count)
        {
            return;
        }

        if (Tours[index] == null)
        {
            return;
        }

        _activeTour = Tours[index];
        _poiIndex = -1;
    }

    public PointOfInterest CurrentPOI
    {
        get
        {
            if (_activeTour == null) return null;
            if (_activeTour.pois == null) return null;
            if (_poiIndex < 0 || _poiIndex >= _activeTour.pois.Count) return null;
            return _activeTour.pois[_poiIndex];
        }
    }

    
    // Finds the POI in the active tour that matches the given cloud anchor id.
    public PointOfInterest GetPOIByCloudId(string cloudId)
    {
        if (_activeTour == null || _activeTour.pois == null) return null;
        if (string.IsNullOrEmpty(cloudId)) return null;

        cloudId = cloudId.Trim();

        foreach (var poi in _activeTour.pois)
        {
            if (poi == null) continue;

            var id = poi.cloudAnchorId?.Trim();
            if (string.IsNullOrEmpty(id)) continue;

            if (id == cloudId)
                return poi;
        }

        return null;
    }

    public void OnNextPressed()
    {
        if (_activeTour == null)
        {
            return;
        }

        ResolveNextPOI();
    }

    public void OnPreviousPressed()
    {
        if (_activeTour == null)
        {
            return;
        }

        ResolvePreviousPOI();
    }

    // Called by ARViewManager when a cloud anchor successfully resolves.
    // Updates the current POI index to the resolved POI.
    public void OnPOIResolved(string resolvedCloudId)
    {
        if (_activeTour == null || _activeTour.pois == null)
            return;

        if (string.IsNullOrEmpty(resolvedCloudId))
            return;

        resolvedCloudId = resolvedCloudId.Trim();

        for (int i = 0; i < _activeTour.pois.Count; i++)
        {
            var poi = _activeTour.pois[i];
            if (poi == null) continue;

            var id = poi.cloudAnchorId?.Trim();
            if (string.IsNullOrEmpty(id)) continue;

            if (id == resolvedCloudId)
            {
                _poiIndex = i;
                return;
            }
        }
    }

    public void ResolveNextPOI()
    {
        if (_activeTour == null || _activeTour.pois == null)
            return;

        for (int i = _poiIndex + 1; i < _activeTour.pois.Count; i++)
        {
            var poi = _activeTour.pois[i];
            if (poi == null) continue;

            var id = poi.cloudAnchorId?.Trim();
            if (string.IsNullOrEmpty(id)) continue;

            _poiIndex = i;
            BeginResolveSingle(id);
            return;
        }
    }

    // Resolves the previous POI with a valid cloudAnchorId
    public void ResolvePreviousPOI()
    {
        if (_activeTour == null || _activeTour.pois == null)
            return;

        // If we haven't started yet so _poiIndex == -1) there's no "previous"
        if (_poiIndex <= 0)
        {
            return;
        }

        for (int i = _poiIndex - 1; i >= 0; i--)
        {
            var poi = _activeTour.pois[i];
            if (poi == null) continue;

            var id = poi.cloudAnchorId?.Trim();
            if (string.IsNullOrEmpty(id)) continue;

            _poiIndex = i;
            BeginResolveSingle(id);
            return;
        }
    }

    public void ResolveCurrentPOI()
    {
        var poi = CurrentPOI;
        if (poi == null)
        {
            return;
        }

        var id = poi.cloudAnchorId?.Trim();
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        BeginResolveSingle(id);
    }

    private void BeginResolveSingle(string cloudAnchorId)
    {
        if (Controller == null)
        {
            return;
        }

        Controller.Mode = PersistentCloudAnchorsController.ApplicationMode.Resolving;

        Controller.ResolvingSet.Clear();
        Controller.ResolvingSet.Add(cloudAnchorId);
    }
}
