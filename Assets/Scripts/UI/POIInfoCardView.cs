using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class POIInfoCardView : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public Image galleryImage;
    public AudioSource audioSource;

    [Header("Slideshow")]
    public float secondsPerSlide = 2.5f;
    public bool autoplay = true;

    private Sprite[] _slides;
    private int _index;
    private float _timer;

    public void SetData(PointOfInterest poi)
    {
        if (poi == null)
        {
            return;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = poi.audioGuide;
            audioSource.time = 0f;
        }

        // --- Text ---
        if (titleText != null){
            titleText.text = poi.title ?? "";
            Debug.LogWarning("Data title");
            Debug.LogWarning(poi.title);
            Debug.LogWarning(titleText.text);
        }

        if (bodyText != null)
            bodyText.text = poi.description ?? "";

        // --- Audio ---
        if (audioSource != null)
            audioSource.clip = poi.audioGuide;

        // --- Gallery ---
        SetGallery(poi.gallery);

        Debug.Log($"[POIInfoCardView] SetData OK: '{poi.title}', gallery count = {(poi.gallery == null ? 0 : poi.gallery.Length)}");
    }

    private void SetGallery(Sprite[] gallery)
    {
        _slides = gallery;
        _index = 0;
        _timer = 0f;

        if (galleryImage == null)
            return;

        // No images → hide gallery
        if (_slides == null || _slides.Length == 0)
        {
            galleryImage.enabled = false;
            return;
        }

        // Find first non-null image
        for (int i = 0; i < _slides.Length; i++)
        {
            if (_slides[i] != null)
            {
                _index = i;
                ShowSlide(_index);
                return;
            }
        }

        // All images were null
        galleryImage.enabled = false;
    }

    void Update()
    {
        if (!autoplay) return;
        if (galleryImage == null || !galleryImage.enabled) return;
        if (_slides == null || _slides.Length <= 1) return;

        _timer += Time.deltaTime;
        if (_timer < secondsPerSlide) return;

        _timer = 0f;
        AdvanceSlide();
    }

    private void AdvanceSlide()
    {
        if (_slides == null || _slides.Length == 0) return;

        int attempts = 0;

        do
        {
            _index = (_index + 1) % _slides.Length;
            attempts++;
        }
        while (_slides[_index] == null && attempts < _slides.Length);

        if (_slides[_index] != null)
            ShowSlide(_index);
    }

    private void ShowSlide(int index)
    {
        if (galleryImage == null || _slides == null) return;

        Sprite s = _slides[index];
        if (s == null)
        {
            galleryImage.enabled = false;
            return;
        }

        galleryImage.sprite = s;
        galleryImage.preserveAspect = true;
        galleryImage.enabled = true;
    }
}
