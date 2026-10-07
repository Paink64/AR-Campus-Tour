namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// Resolve menu manager that lets users pick a Tour (list of POIs),
    /// then starts resolving the first POI's Cloud Anchor ID.
    /// </summary>
    public class ResolveMenuManager : MonoBehaviour
    {
        [Header("ARCore Sample Controller")]
        public PersistentCloudAnchorsController Controller;

        [Header("Tour UI")]
        public TMP_Dropdown TourDropdown;

        [Tooltip("Assign your Tour assets here.")]
        public List<Tour> Tours = new List<Tour>();

        [Header("UI")]
        public Button ResolveButton;
        public Text tourDescription;


        private Color _activeColor;
        private Tour _selectedTour;

        public void Awake()
        {
            _activeColor = ResolveButton.GetComponent<Image>().color;
        }

        public void OnEnable()
        {
            SetButtonActive(ResolveButton, false);

            BuildTourDropdown();

            // Make sure we don't have more than one selected
            TourDropdown.onValueChanged.RemoveListener(OnTourDropdownChanged);
            TourDropdown.onValueChanged.AddListener(OnTourDropdownChanged);

            // Initialize selection
            if (Tours != null && Tours.Count > 0)
            {
                TourDropdown.value = 0;
                OnTourDropdownChanged(0);
            }
            else
            {
                _selectedTour = null;
                SetButtonActive(ResolveButton, false);
            }
        }

        public void OnDisable()
        {
            TourDropdown.onValueChanged.RemoveListener(OnTourDropdownChanged);
            TourDropdown.ClearOptions();
            _selectedTour = null;
        }

        private void BuildTourDropdown()
        {
            TourDropdown.ClearOptions();

            var options = new List<TMP_Dropdown.OptionData>();
            if (Tours != null)
            {
                foreach (var tour in Tours)
                {
                    options.Add(new TMP_Dropdown.OptionData(tour ? tour.tourName : "Please select a tour"));
                }
            }

            TourDropdown.AddOptions(options);
        }

        private void OnTourDropdownChanged(int index)
        {
            if (Tours == null || index < 0 || index >= Tours.Count || Tours[index] == null)
            {
                _selectedTour = null;
                tourDescription.text = "Please select a tour";
                SetButtonActive(ResolveButton, false);
                return;
            }

            _selectedTour = Tours[index];
            tourDescription.text = _selectedTour.tourDescription;
            // Enable resolve only if the tour has at least one POI with a valid cloudAnchorId
            bool hasAnyResolvable = TryGetFirstResolvableAnchorId(_selectedTour, out _);
            SetButtonActive(ResolveButton, hasAnyResolvable);
        }

        /// <summary>
        /// Hook this up to the Resolve button's OnClick in the Inspector.
        /// Starts resolving ONLY the first POI anchor in the selected tour.
        /// </summary>
        public void OnResolveButtonClicked()
        {
            if (_selectedTour == null)
            {
                return;
            }

            if (!TryGetFirstResolvableAnchorId(_selectedTour, out string firstAnchorId))
            {
                SetButtonActive(ResolveButton, false);
                return;
            }

            // Resolve ONLY one at a time: start with the first POI in the tour that has an anchor id
            Controller.ResolvingSet.Clear();
            Controller.ResolvingSet.Add(firstAnchorId);

            // Enter resolving mode and go to AR view
            Controller.Mode = PersistentCloudAnchorsController.ApplicationMode.Resolving;
            Controller.SwitchToPrivacyPrompt();
        }

        private bool TryGetFirstResolvableAnchorId(Tour tour, out string anchorId)
        {
            anchorId = null;
            if (tour == null || tour.pois == null) return false;

            foreach (var poi in tour.pois)
            {
                if (poi == null) continue;
                var id = poi.cloudAnchorId != null ? poi.cloudAnchorId.Trim() : "";
                if (!string.IsNullOrEmpty(id))
                {
                    anchorId = id;
                    return true;
                }
            }

            return false;
        }

        private void SetButtonActive(Button button, bool active)
        {
            button.GetComponent<Image>().color = active ? _activeColor : Color.grey;
            button.enabled = active;
        }
    }
}
