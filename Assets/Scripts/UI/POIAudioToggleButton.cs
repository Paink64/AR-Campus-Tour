using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class POIAudioToggleButton : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("UI")]
    public Button playButton;
    public Button skipForwardButton;
    public Button skipBackwardButton;
    public TextMeshProUGUI buttonLabel;

    [Header("Settings")]
    public float skipSeconds = 5f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (playButton != null)
            playButton.onClick.AddListener(TogglePlayPause);

        if (skipForwardButton != null)
            skipForwardButton.onClick.AddListener(SkipForward);

        if (skipBackwardButton != null)
            skipBackwardButton.onClick.AddListener(SkipBackward);

        UpdateLabel();
    }

    void Start()
    {
        audioSource.Play();
    }

    public void SetPOI(PointOfInterest poi)
    {
        if (audioSource == null) return;

        audioSource.Stop();
        audioSource.clip = (poi != null) ? poi.audioGuide : null;
        audioSource.time = 0f;

        bool hasClip = audioSource.clip != null;

        audioSource.clip = poi.audioGuide;
        audioSource.Play();

        if (playButton != null)
            playButton.interactable = hasClip;

        if (skipForwardButton != null)
            skipForwardButton.interactable = hasClip;

        if (skipBackwardButton != null)
            skipBackwardButton.interactable = hasClip;

        UpdateLabel();
    }

    public void TogglePlayPause()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        if (audioSource.isPlaying)
            audioSource.Pause();
        else
            audioSource.Play();

        UpdateLabel();
    }

    public void SkipForward()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        audioSource.time = Mathf.Min(
            audioSource.time + skipSeconds,
            audioSource.clip.length
        );
    }

    public void SkipBackward()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        audioSource.time = Mathf.Max(
            audioSource.time - skipSeconds,
            0f
        );
    }

    private void UpdateLabel()
    {
        if (buttonLabel == null) return;

        if (audioSource == null || audioSource.clip == null)
            buttonLabel.text = "No Audio";
        else
            buttonLabel.text = audioSource.isPlaying ? "Pause" : "Play";
    }
}
