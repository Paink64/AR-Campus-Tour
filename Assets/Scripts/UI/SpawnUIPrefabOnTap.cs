using UnityEngine;

public class SpawnUIPrefabOnTap : MonoBehaviour
{
    public GameObject uiPrefab;
    public PointOfInterest poi;  

    private GameObject _spawned;

    public void SetPOI(PointOfInterest p)
    {
        poi = p;
    }

    private void OnMouseDown()
    {
        if (uiPrefab == null)
        {
            return;
        }

        if (poi == null)
        {
            return;
        }

        // Only spawn once
        if (_spawned != null)
        {
            _spawned.SetActive(true);
            PushData();
            return;
        }

        _spawned = Instantiate(uiPrefab);

        PushData();
    }

    private void PushData()
    {
        var view = _spawned.GetComponentInChildren<FullScreenPOIView>(true);
        if (view != null)
        {
            view.SetData(poi);
        }
    }
}
