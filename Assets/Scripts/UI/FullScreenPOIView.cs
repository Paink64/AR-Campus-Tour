using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FullScreenPOIView : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public Image galleryImage;

    [Header("Gallery Settings")]
    public float secondsPerImage = 3f;
    public bool autoplay = true;

    private Sprite[] _gallery;
    private int _currentIndex;
    private float _timer;

    public void SetData(PointOfInterest poi)
    {
        if (poi == null)
        {
            return;
        }

        // Text
        if (titleText != null)
            titleText.text = poi.title ?? "";

        if (bodyText != null)
            bodyText.text = poi.description ?? "";

        // Store gallery
        _gallery = poi.gallery;
        _currentIndex = 0;
        _timer = 0f;

        UpdateImage();

    }

    private void Update()
    {
        if (!autoplay) return;
        if (_gallery == null || _gallery.Length <= 1) return;
        if (galleryImage == null || !galleryImage.enabled) return;

        _timer += Time.deltaTime;

        if (_timer >= secondsPerImage)
        {
            _timer = 0f;
            NextImage();
        }
    }

    private void NextImage()
    {
        if (_gallery == null || _gallery.Length == 0) return;

        int attempts = 0;

        do
        {
            _currentIndex = (_currentIndex + 1) % _gallery.Length;
            attempts++;
        }
        while (_gallery[_currentIndex] == null && attempts < _gallery.Length);

        UpdateImage();
    }

    private void UpdateImage()
    {
        if (galleryImage == null)
            return;

        if (_gallery == null || _gallery.Length == 0)
        {
            galleryImage.enabled = false;
            return;
        }

        // Find first valid image
        int attempts = 0;
        while (_gallery[_currentIndex] == null && attempts < _gallery.Length)
        {
            _currentIndex = (_currentIndex + 1) % _gallery.Length;
            attempts++;
        }

        if (_gallery[_currentIndex] != null)
        {
            galleryImage.sprite = _gallery[_currentIndex];
            galleryImage.preserveAspect = true;
            galleryImage.enabled = true;
        }
        else
        {
            galleryImage.enabled = false;
        }
    }
}
