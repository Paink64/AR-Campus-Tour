using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FullScreenPOIUI : MonoBehaviour
{
    public static FullScreenPOIUI Instance { get; private set; }

    [Header("Root")]
    public GameObject panelRoot;

    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public POISlideshow slideshow;
    public Button closeButton;

    void Awake()
    {
        Instance = this;

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Open(PointOfInterest poi)
    {
        if (poi == null) 
            return;

        if (titleText != null) 
            titleText.text = poi.title ?? "";
        if (bodyText != null) 
            bodyText.text = poi.description ?? "";

        if (slideshow != null)
            slideshow.SetSlides(poi.gallery, 0);

        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}
