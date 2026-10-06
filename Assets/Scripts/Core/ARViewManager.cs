//-----------------------------------------------------------------------
// <copyright file="ARViewManager.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    /// <summary>
    /// A manager component that helps with hosting and resolving Cloud Anchors.
    /// </summary>
    public class ARViewManager : MonoBehaviour
    {
        /// <summary>
        /// The main controller for Persistent Cloud Anchors sample.
        /// </summary>
        public PersistentCloudAnchorsController Controller;

        /// <summary>
        /// The 3D object that represents a Cloud Anchor.
        /// </summary>
        public GameObject CloudAnchorPrefab;

        /// <summary>
        /// Prefab used when placing an anchor for HOSTING.
        /// </summary>
        public GameObject HostingAnchorPrefab;

        /// <summary>
        /// The game object that includes <see cref="MapQualityIndicator"/> to visualize
        /// map quality result.
        /// </summary>
        public GameObject MapQualityIndicatorPrefab;

        /// <summary>
        /// The UI element that displays the instructions to guide hosting experience.
        /// </summary>
        public GameObject InstructionBar;

        /// <summary>
        /// The UI panel that allows the user to name the Cloud Anchor.
        /// </summary>
        public GameObject NamePanel;

        /// <summary>
        /// The UI element that displays warning message for invalid input name.
        /// </summary>
        public GameObject InputFieldWarning;

        /// <summary>
        /// The input field for naming Cloud Anchor.
        /// </summary>
        public InputField NameField;

        /// <summary>
        /// The instruction text in the top instruction bar.
        /// </summary>
        public Text InstructionText;

        /// <summary>
        /// Display the tracking helper text when the session in not tracking.
        /// </summary>
        public Text TrackingHelperText;

        /// <summary>
        /// The debug text in bottom snack bar.
        /// </summary>
        public Text DebugText;

        /// <summary>
        /// The button to save the typed name.
        /// </summary>
        public Button SaveButton;

        /// <summary>
        /// The button to save current cloud anchor id into clipboard.
        /// </summary>
        public Button ShareButton;

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.Initializing">.</see>
        /// </summary>
        private const string _initializingMessage = "Tracking is being initialized.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.Relocalizing">.</see>
        /// </summary>
        private const string _relocalizingMessage = "Tracking is resuming after an interruption.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientLight">.</see>
        /// </summary>
        private const string _insufficientLightMessage = "Too dark. Try moving to a well-lit area.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientLight">
        /// in Android S or above.</see>
        /// </summary>
        private const string _insufficientLightMessageAndroidS =
            "Too dark. Try moving to a well-lit area. " +
            "Also, make sure the Block Camera is set to off in system settings.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.InsufficientFeatures">.</see>
        /// </summary>
        private const string _insufficientFeatureMessage =
            "Can't find anything. Aim device at a surface with more texture or color.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.ExcessiveMotion">.</see>
        /// </summary>
        private const string _excessiveMotionMessage = "Moving too fast. Slow down.";

        /// <summary>
        /// Helper message for <see cref="NotTrackingReason.Unsupported">.</see>
        /// </summary>
        private const string _unsupportedMessage = "Tracking lost reason is not supported.";

        /// <summary>
        /// The time between enters AR View and ARCore session starts to host or resolve.
        /// </summary>
        private const float _startPrepareTime = 3.0f;

        /// <summary>
        /// Android 12 (S) SDK version.
        /// </summary>
        private const int _androidSSDKVesion = 31;

        /// <summary>
        /// Pixel Model keyword.
        /// </summary>
        private const string _pixelModel = "pixel";

        /// <summary>
        /// The timer to indicate whether the AR View has passed the start prepare time.
        /// </summary>
        private float _timeSinceStart;

        /// <summary>
        /// True if the app is in the process of returning to home page due to an invalid state,
        /// otherwise false.
        /// </summary>
        private bool _isReturning;

        /// <summary>
        /// The MapQualityIndicator that attaches to the placed object.
        /// </summary>
        private MapQualityIndicator _qualityIndicator = null;

        /// <summary>
        /// The history data that represents the current hosted Cloud Anchor.
        /// </summary>
        private CloudAnchorHistory _hostedCloudAnchor;

        /// <summary>
        /// An ARAnchor indicating the 3D object has been placed on a flat surface and
        /// is waiting for hosting.
        /// </summary>
        private ARAnchor _anchor = null;

        /// <summary>
        /// The promise for the async hosting operation, if any.
        /// </summary>
        private HostCloudAnchorPromise _hostPromise = null;

        /// <summary>
        /// The result of the hosting operation, if any.
        /// </summary>
        private HostCloudAnchorResult _hostResult = null;

        /// <summary>
        /// The coroutine for the hosting operation, if any.
        /// </summary>
        private IEnumerator _hostCoroutine = null;

        /// <summary>
        /// The promises for the async resolving operations, if any.
        /// </summary>
        private List<ResolveCloudAnchorPromise> _resolvePromises =
            new List<ResolveCloudAnchorPromise>();

        /// <summary>
        /// The results of the resolving operations, if any.
        /// </summary>
        private List<ResolveCloudAnchorResult> _resolveResults =
            new List<ResolveCloudAnchorResult>();

        /// <summary>
        /// The coroutines of the resolving operations, if any.
        /// </summary>
        private List<IEnumerator> _resolveCoroutines = new List<IEnumerator>();

        private Color _activeColor;
        private AndroidJavaClass _versionInfo;

        private GameObject _spawnedPoiVisual = null;
        private GameObject _lastResolvedAnchorGO = null;
        private AudioSource _audio;


        /// <summary>
        /// Get the camera pose for the current frame.
        /// </summary>
        /// <returns>The camera pose of the current frame.</returns>
        public Pose GetCameraPose()
        {
            return new Pose(Controller.MainCamera.transform.position,
                Controller.MainCamera.transform.rotation);
        }

        /// <summary>
        /// Callback handling the validation of the input field.
        /// </summary>
        /// <param name="inputString">The current value of the input field.</param>
        public void OnInputFieldValueChanged(string inputString)
        {
            // Cloud Anchor name should only contains: letters, numbers, hyphen(-), underscore(_).
            var regex = new Regex("^[a-zA-Z0-9-_]*$");
            InputFieldWarning.SetActive(!regex.IsMatch(inputString));
            SetSaveButtonActive(!InputFieldWarning.activeSelf && inputString.Length > 0);
        }

        /// <summary>
        /// Callback handling "Ok" button click event for input field.
        /// </summary>
        public void OnSaveButtonClicked()
        {
            _hostedCloudAnchor.Name = NameField.text;
            Controller.SaveCloudAnchorHistory(_hostedCloudAnchor);

            DebugText.text = string.Format("Saved Cloud Anchor:\n{0}.", _hostedCloudAnchor.Name);
            ShareButton.gameObject.SetActive(true);
            NamePanel.SetActive(false);
        }

        /// <summary>
        /// Callback handling "Share" button click event.
        /// </summary>
        public void OnShareButtonClicked()
        {
            GUIUtility.systemCopyBuffer = _hostedCloudAnchor.Id;
            DebugText.text = "Copied cloud id: " + _hostedCloudAnchor.Id;
        }

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            _activeColor = SaveButton.GetComponentInChildren<Text>().color;
            _versionInfo = new AndroidJavaClass("android.os.Build$VERSION");
        }

        /// <summary>
        /// The Unity OnEnable() method.
        /// </summary>
        public void OnEnable()
        {
            _timeSinceStart = 0.0f;
            _isReturning = false;
            _anchor = null;
            _qualityIndicator = null;
            _hostPromise = null;
            _hostResult = null;
            _hostCoroutine = null;
            _resolvePromises.Clear();
            _resolveResults.Clear();
            _resolveCoroutines.Clear();

            InstructionBar.SetActive(true);
            NamePanel.SetActive(false);
            InputFieldWarning.SetActive(false);
            ShareButton.gameObject.SetActive(false);
            UpdatePlaneVisibility(true);

            switch (Controller.Mode)
            {
                case PersistentCloudAnchorsController.ApplicationMode.Ready:
                    ReturnToHomePage("Invalid application mode, returning to home page...");
                    break;
                case PersistentCloudAnchorsController.ApplicationMode.Hosting:
                case PersistentCloudAnchorsController.ApplicationMode.Resolving:
                    InstructionText.text = "Detecting flat surface...";
                    DebugText.text = "ARCore is preparing for " + Controller.Mode;
                    break;
            }
        }

        /// <summary>
        /// The Unity OnDisable() method.
        /// </summary>
        public void OnDisable()
        {
            if (_qualityIndicator != null)
            {
                Destroy(_qualityIndicator.gameObject);
                _qualityIndicator = null;
            }

            if (_anchor != null)
            {
                Destroy(_anchor.gameObject);
                _anchor = null;
            }

            if (_hostCoroutine != null)
            {
                StopCoroutine(_hostCoroutine);
            }

            _hostCoroutine = null;

            if (_hostPromise != null)
            {
                _hostPromise.Cancel();
                _hostPromise = null;
            }

            _hostResult = null;

            foreach (var coroutine in _resolveCoroutines)
            {
                StopCoroutine(coroutine);
            }

            _resolveCoroutines.Clear();

            foreach (var promise in _resolvePromises)
            {
                promise.Cancel();
            }

            _resolvePromises.Clear();

            foreach (var result in _resolveResults)
            {
                if (result.Anchor != null)
                {
                    Destroy(result.Anchor.gameObject);
                }
            }

            _resolveResults.Clear();
            UpdatePlaneVisibility(false);
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            // Give ARCore some time to prepare for hosting or resolving.
            if (_timeSinceStart < _startPrepareTime)
            {
                _timeSinceStart += Time.deltaTime;
                if (_timeSinceStart >= _startPrepareTime)
                {
                    UpdateInitialInstruction();
                }

                return;
            }

            ARCoreLifecycleUpdate();
            if (_isReturning)
            {
                return;
            }

            if (_timeSinceStart >= _startPrepareTime)
            {
                DisplayTrackingHelperMessage();
            }

            if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Resolving)
            {
                ResolvingCloudAnchors();
            }
            else if (Controller.Mode == PersistentCloudAnchorsController.ApplicationMode.Hosting)
            {
                // Perform hit test and place an anchor on the hit test result.
                if (_anchor == null)
                {
                    // If the player has not touched the screen then the update is complete.
                    Touch touch;
                    if (Input.touchCount < 1 ||
                        (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
                    {
                        return;
                    }

                    // Ignore the touch if it's pointing on UI objects.
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        return;
                    }

                    // Perform hit test and place a pawn object.
                    PerformHitTest(touch.position);
                }

                HostingCloudAnchor();
            }
        }

        private void PerformHitTest(Vector2 touchPos)
        {
            List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
            Controller.RaycastManager.Raycast(
                touchPos, hitResults, TrackableType.PlaneWithinPolygon);

            var planeType = PlaneAlignment.HorizontalUp;
            if (hitResults.Count > 0)
            {
                ARPlane plane = Controller.PlaneManager.GetPlane(hitResults[0].trackableId);
                if (plane == null)
                {
                    Debug.LogWarningFormat("Failed to find the ARPlane with TrackableId {0}",
                        hitResults[0].trackableId);
                    return;
                }

                planeType = plane.alignment;
                var hitPose = hitResults[0].pose;
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    hitPose.rotation.eulerAngles =
                        new Vector3(0.0f, Controller.MainCamera.transform.eulerAngles.y, 0.0f);
                }

                _anchor = Controller.AnchorManager.AttachAnchor(plane, hitPose);
            }

            if (_anchor != null)
            {
                // ✅ Use Hosting prefab when placing for hosting
                var prefabToSpawn = HostingAnchorPrefab != null ? HostingAnchorPrefab : CloudAnchorPrefab;
                Instantiate(prefabToSpawn, _anchor.transform);

                // Attach map quality indicator to this anchor.
                var indicatorGO = Instantiate(MapQualityIndicatorPrefab, _anchor.transform);
                _qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
                _qualityIndicator.DrawIndicator(planeType, Controller.MainCamera);

                InstructionText.text = " To save this location, walk around the object to " +
                    "capture it from different angles";
                DebugText.text = "Waiting for sufficient mapping quaility...";

                UpdatePlaneVisibility(false);
            }
        }


        private void HostingCloudAnchor()
        {
            // There is no anchor for hosting.
            if (_anchor == null)
            {
                return;
            }

            // There is a pending or finished hosting task.
            if (_hostPromise != null || _hostResult != null)
            {
                return;
            }

            // Update map quality:
            int qualityState = 2;
            // Can pass in ANY valid camera pose to the mapping quality API.
            // Ideally, the pose should represent users’ expected perspectives.
            FeatureMapQuality quality =
                Controller.AnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
            DebugText.text = "Current mapping quality: " + quality;
            qualityState = (int)quality;
            _qualityIndicator.UpdateQualityState(qualityState);

            // Hosting instructions:
            var cameraDist = (_qualityIndicator.transform.position -
                Controller.MainCamera.transform.position).magnitude;
            if (cameraDist < _qualityIndicator.Radius * 1.5f)
            {
                InstructionText.text = "You are too close, move backward.";
                return;
            }
            else if (cameraDist > 10.0f)
            {
                InstructionText.text = "You are too far, come closer.";
                return;
            }
            else if (_qualityIndicator.ReachTopviewAngle)
            {
                InstructionText.text =
                    "You are looking from the top view, move around from all sides.";
                return;
            }
            else if (!_qualityIndicator.ReachQualityThreshold)
            {
                InstructionText.text = "Save the object here by capturing it from all sides.";
                return;
            }

            // Start hosting:
            InstructionText.text = "Processing...";
            DebugText.text = "Mapping quality has reached sufficient threshold, " +
                "creating Cloud Anchor.";
            DebugText.text = string.Format(
                "FeatureMapQuality has reached {0}, triggering CreateCloudAnchor.",
                Controller.AnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose()));

            // Creating a Cloud Anchor with lifetime = 1 day.
            // This is configurable up to 365 days when keyless authentication is used.
            var promise = Controller.AnchorManager.HostCloudAnchorAsync(_anchor, 365);
            if (promise.State == PromiseState.Done)
            {
                Debug.LogFormat("Failed to host a Cloud Anchor.");
                OnAnchorHostedFinished(false);
            }
            else
            {
                _hostPromise = promise;
                _hostCoroutine = HostAnchor();
                StartCoroutine(_hostCoroutine);
            }
        }

        private IEnumerator HostAnchor()
        {
            yield return _hostPromise;
            _hostResult = _hostPromise.Result;
            _hostPromise = null;

            if (_hostResult.CloudAnchorState == CloudAnchorState.Success)
            {
                int count = Controller.LoadCloudAnchorHistory().Collection.Count;
                _hostedCloudAnchor =
                    new CloudAnchorHistory("CloudAnchor" + count, _hostResult.CloudAnchorId);
                OnAnchorHostedFinished(true, _hostResult.CloudAnchorId);
            }
            else
            {
                OnAnchorHostedFinished(false, _hostResult.CloudAnchorState.ToString());
            }
        }

        // Modified from original to resolve one anchor at a time.
private void ResolvingCloudAnchors()
{
    // No Cloud Anchor for resolving.
    if (Controller.ResolvingSet.Count == 0)
    {
        return;
    }

    // There are pending or finished resolving tasks.
    if (_resolvePromises.Count > 0 || _resolveResults.Count > 0)
    {
        return;
    }

    // ARCore session is not ready for resolving.
    if (ARSession.state != ARSessionState.SessionTracking)
    {
        return;
    }

    // ✅ Resolve exactly ONE anchor id (first one in the set).
    string cloudId = null;
    foreach (var id in Controller.ResolvingSet)
    {
        cloudId = id;
        break;
    }

    if (string.IsNullOrEmpty(cloudId))
    {
        Controller.ResolvingSet.Clear();
        return;
    }

    Debug.LogFormat("Attempting to resolve 1 Cloud Anchor: {0}", cloudId);

    var promise = Controller.AnchorManager.ResolveCloudAnchorAsync(cloudId);
    if (promise.State == PromiseState.Done)
    {
        Debug.LogFormat("Failed to resolve Cloud Anchor " + cloudId);
        OnAnchorResolvedFinished(false, cloudId);
    }
    else
    {
        _resolvePromises.Add(promise);
        var coroutine = ResolveAnchor(cloudId, promise);
        StartCoroutine(coroutine);
    }

    // Clear so we don't re-resolve every frame.
    Controller.ResolvingSet.Clear();
}


        private IEnumerator ResolveAnchor(string cloudId, ResolveCloudAnchorPromise promise)
        {
            yield return promise;
            var result = promise.Result;
            _resolvePromises.Remove(promise);
            _resolveResults.Add(result);

            if (result.CloudAnchorState == CloudAnchorState.Success)
            {
                ClearPreviousResolvedPoi();

                _lastResolvedAnchorGO = (result.Anchor != null) ? result.Anchor.gameObject : null;

                if (_lastResolvedAnchorGO != null)
                {
                    _spawnedPoiVisual = Instantiate(CloudAnchorPrefab);
                    _spawnedPoiVisual.transform.SetParent(_lastResolvedAnchorGO.transform, false);
                    _spawnedPoiVisual.transform.localPosition = Vector3.zero;
                    _spawnedPoiVisual.transform.localRotation = Quaternion.identity;
                    _spawnedPoiVisual.transform.localScale = Vector3.one;

                    // ✅ Set the card content from the currently active POI
                    var tour = FindFirstObjectByType<TourManager>();
                    
                    Debug.LogWarning("tour");
                    Debug.LogWarning(tour);
                    if (tour != null)
                    {
                        var poi = tour.GetPOIByCloudId(cloudId);
                        Debug.LogWarning("cloudId");
                        Debug.LogWarning(cloudId);
                        Debug.LogWarning("poi");
                        Debug.LogWarning(poi);
                        var view = _spawnedPoiVisual.GetComponentInChildren<POIInfoCardView>(true);
                        if (view != null)
                        {
                            Debug.LogWarning("Data is SET");
                            view.SetData(poi);
                        }
                        Debug.LogWarning("Passed DataSet");
                        var tapSpawner = _spawnedPoiVisual.GetComponentInChildren<SpawnUIPrefabOnTap>(true);
                        if (tapSpawner != null)
                        {
                            tapSpawner.SetPOI(poi);
                        }
                        _audio = _spawnedPoiVisual.GetComponentInChildren<AudioSource>();

                    }
                    else
                    {
                        Debug.LogWarning("TourManager not found in scene.");
                    }

                }
                else
                {
                    Debug.LogWarning("Resolve succeeded but result.Anchor was null (no anchor transform to parent to).");
                }

                // ✅ Call after setup so we don't accidentally advance POI before we read it
                OnAnchorResolvedFinished(true, cloudId);
            }
            else
            {
                OnAnchorResolvedFinished(false, cloudId, result.CloudAnchorState.ToString());
            }



            // Allow future resolves while staying in ARView
            _resolveResults.Clear();
        }

        public void PlayAudio()
        {
            if (_audio != null && !_audio.isPlaying)
            {
                _audio.Play();
            }
            else if (_audio != null && _audio.isPlaying)
            {
                _audio.Pause();
            }
        }

        public void SkipAudioAheadFiveSeconds()
        {
            if (_audio != null && _audio.isPlaying)
            {
                float newTime = _audio.time + 5f;
                _audio.time = Mathf.Min(newTime, _audio.clip.length);
            }
        }

        public void SkipAudioBackFiveSeconds()
        {
            if (_audio != null && _audio.isPlaying)
            {
                float newTime = _audio.time - 5f;
                _audio.time = Mathf.Max(newTime, 0f);
            }
        }

        private void ClearPreviousResolvedPoi()
        {

            var oldAudio = _spawnedPoiVisual != null ? _spawnedPoiVisual.GetComponentInChildren<AudioSource>(true) : null;
            if (oldAudio != null) 
                oldAudio.Stop();
                
            if (_spawnedPoiVisual != null)
            {
                Destroy(_spawnedPoiVisual);
                _spawnedPoiVisual = null;
            }

            if (_lastResolvedAnchorGO != null)
            {
                Destroy(_lastResolvedAnchorGO);
                _lastResolvedAnchorGO = null;
            }

        }



        private void OnAnchorHostedFinished(bool success, string response = null)
        {
            if (success)
            {
                InstructionText.text = "Finish!";
                Invoke("DoHideInstructionBar", 1.5f);
                DebugText.text =
                    string.Format("Succeed to host the Cloud Anchor: {0}.", response);

                // Display name panel and hide instruction bar.
                NameField.text = _hostedCloudAnchor.Name;
                NamePanel.SetActive(true);
                SetSaveButtonActive(true);
            }
            else
            {
                InstructionText.text = "Host failed.";
                DebugText.text = "Failed to host a Cloud Anchor" + (response == null ? "." :
                    "with error " + response + ".");
            }
        }

        private void OnAnchorResolvedFinished(bool success, string cloudId, string response = null)
        {
            if (success)
            {
                InstructionText.text = "Resolve success!";
                DebugText.text =
                    string.Format("Succeed to resolve the Cloud Anchor: {0}.", cloudId);

                // ✅ Modern Unity API
                var tour = FindFirstObjectByType<TourManager>();
                if (tour != null)
                {
                    tour.OnPOIResolved(cloudId);
                }
            }
            else
            {
                InstructionText.text = "Resolve failed.";
                DebugText.text = "Failed to resolve Cloud Anchor: " + cloudId +
                    (response == null ? "." : "with error " + response + ".");
            }
        }


        private void UpdateInitialInstruction()
        {
            switch (Controller.Mode)
            {
                case PersistentCloudAnchorsController.ApplicationMode.Hosting:
                    // Initial instruction for hosting flow:
                    InstructionText.text = "Tap to place an object.";
                    DebugText.text = "Tap a vertical or horizontal plane...";
                    return;
                case PersistentCloudAnchorsController.ApplicationMode.Resolving:
                    // Initial instruction for resolving flow:
                    InstructionText.text =
                        "Look at the location you expect to see the AR experience appear.";
                    DebugText.text = string.Format("Attempting to resolve {0} anchors...",
                        Controller.ResolvingSet.Count);
                    return;
                default:
                    return;
            }
        }

        private void UpdatePlaneVisibility(bool visible)
        {
            foreach (var plane in Controller.PlaneManager.trackables)
            {
                plane.gameObject.SetActive(visible);
            }
        }

        private void ARCoreLifecycleUpdate()
        {
            // Only allow the screen to sleep when not tracking.
            var sleepTimeout = SleepTimeout.NeverSleep;
            if (ARSession.state != ARSessionState.SessionTracking)
            {
                sleepTimeout = SleepTimeout.SystemSetting;
            }

            Screen.sleepTimeout = sleepTimeout;

            if (_isReturning)
            {
                return;
            }

            // Return to home page if ARSession is in error status.
            if (ARSession.state != ARSessionState.Ready &&
                ARSession.state != ARSessionState.SessionInitializing &&
                ARSession.state != ARSessionState.SessionTracking)
            {
                ReturnToHomePage(string.Format(
                    "ARCore encountered an error state {0}. Please start the app again.",
                    ARSession.state));
            }
        }

        private void DisplayTrackingHelperMessage()
        {
            if (_isReturning || ARSession.notTrackingReason == NotTrackingReason.None)
            {
                TrackingHelperText.gameObject.SetActive(false);
            }
            else
            {
                TrackingHelperText.gameObject.SetActive(true);
                switch (ARSession.notTrackingReason)
                {
                    case NotTrackingReason.Initializing:
                        TrackingHelperText.text = _initializingMessage;
                        return;
                    case NotTrackingReason.Relocalizing:
                        TrackingHelperText.text = _relocalizingMessage;
                        return;
                    case NotTrackingReason.InsufficientLight:
                        if (_versionInfo.GetStatic<int>("SDK_INT") < _androidSSDKVesion)
                        {
                            TrackingHelperText.text = _insufficientLightMessage;
                        }
                        else
                        {
                            TrackingHelperText.text = _insufficientLightMessageAndroidS;
                        }

                        return;
                    case NotTrackingReason.InsufficientFeatures:
                        TrackingHelperText.text = _insufficientFeatureMessage;
                        return;
                    case NotTrackingReason.ExcessiveMotion:
                        TrackingHelperText.text = _excessiveMotionMessage;
                        return;
                    case NotTrackingReason.Unsupported:
                        TrackingHelperText.text = _unsupportedMessage;
                        return;
                    default:
                        TrackingHelperText.text =
                            string.Format("Not tracking reason: {0}", ARSession.notTrackingReason);
                        return;
                }
            }
        }

        private void ReturnToHomePage(string reason)
        {
            Debug.Log("Returning home for reason: " + reason);
            if (_isReturning)
            {
                return;
            }

            _isReturning = true;
            DebugText.text = reason;
            Invoke("DoReturnToHomePage", 3.0f);
        }

        private void DoReturnToHomePage()
        {
            Controller.SwitchToHomePage();
        }

        private void DoHideInstructionBar()
        {
            InstructionBar.SetActive(false);
        }

        private void SetSaveButtonActive(bool active)
        {
            SaveButton.enabled = active;
            SaveButton.GetComponentInChildren<Text>().color = active ? _activeColor : Color.gray;
        }
    }
}