using System;
using UnityEngine;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed class WorkbenchAtmosphere : MonoBehaviour
    {
        private const float ReactionDuration = .55f;
        private RectTransform _artifact;
        private Image _frost;
        private Image _warmth;
        private RectTransform _rainRoot;
        private Image[] _drops;
        private Vector2 _restPosition;
        private Vector3 _restScale;
        private Quaternion _restRotation;
        private Color _frostColor;
        private Color _warmthColor;
        private float _progress;
        private float _reactionTime = ReactionDuration;
        private float _fadeTime = ReactionDuration;
        private float _frostFrom;
        private float _warmthFrom;
        private float _rainStrength;
        private float _rainClock;
        private string _effect;
        private bool _complete;

        public void Configure(RectTransform artifact, Image frost, Image warmth, bool rain)
        {
            if (artifact == null) throw new ArgumentNullException(nameof(artifact));
            Settle();
            RemoveRain();
            _artifact = artifact;
            _frost = frost;
            _warmth = warmth;
            _restPosition = artifact.anchoredPosition;
            _restScale = artifact.localScale;
            _restRotation = artifact.localRotation;
            _frostColor = frost != null ? frost.color : Color.clear;
            _warmthColor = warmth != null ? warmth.color : Color.clear;
            _progress = 0;
            _complete = false;
            _rainClock = 0;
            _rainStrength = rain ? 1 : 0;
            var graphic = artifact.GetComponent<Graphic>();
            if (graphic != null) graphic.raycastTarget = false;
            if (frost != null) frost.raycastTarget = false;
            if (warmth != null) warmth.raycastTarget = false;
            if (rain) CreateRain();
            Settle();
            if (_rainRoot != null) _rainRoot.gameObject.SetActive(isActiveAndEnabled);
        }

        public void Apply(string effect, float progress, bool complete)
        {
            if (_artifact == null) return;
            _frostFrom = _frost != null ? _frost.color.a : 0;
            _warmthFrom = _warmth != null ? _warmth.color.a : 0;
            _progress = complete ? 1 : Mathf.Clamp01(progress);
            _complete = complete;
            _fadeTime = 0;
            if (!string.IsNullOrEmpty(effect))
            {
                RestorePose();
                _effect = effect;
                _reactionTime = 0;
            }
            if (!isActiveAndEnabled) Settle();
        }

        private void Update()
        {
            if (_artifact == null) return;
            var delta = Time.unscaledDeltaTime;
            _reactionTime = Mathf.Min(ReactionDuration, _reactionTime + delta);
            _fadeTime = Mathf.Min(ReactionDuration, _fadeTime + delta);
            var phase = _reactionTime / ReactionDuration;
            var pulse = Mathf.Sin(phase * Mathf.PI);
            RestorePose();
            if (_effect == "turn")
                _artifact.localRotation = _restRotation * Quaternion.Euler(0, 0, pulse * -12);
            else if (_effect == "repair")
                _artifact.anchoredPosition = _restPosition + new Vector2(
                    Mathf.Sin(phase * Mathf.PI * 4) * (1 - phase) * 8, pulse * 4);
            if (_effect == "reveal") _artifact.localScale = _restScale * (1 + pulse * .035f);
            SetColors(Mathf.SmoothStep(0, 1, _fadeTime / ReactionDuration), pulse);
            if (_reactionTime >= ReactionDuration) _effect = null;
            _rainClock += delta;
            _rainStrength = Mathf.MoveTowards(_rainStrength, _complete ? 0 : 1 - _progress, delta * 1.8f);
            UpdateRain();
        }

        private void SetColors(float blend, float pulse)
        {
            if (_frost != null)
            {
                var color = _frostColor;
                color.a = Mathf.Lerp(_frostFrom, _frostColor.a * (1 - _progress), blend);
                _frost.color = color;
            }
            if (_warmth != null)
            {
                var glow = _effect == "reveal" || _effect == "warmth" ? pulse : 0;
                var color = Color.Lerp(_warmthColor, Color.white, glow * .7f);
                color.a = Mathf.Lerp(_warmthFrom, Mathf.Clamp01(_warmthColor.a + .09f * _progress), blend) + glow * .16f;
                _warmth.color = color;
            }
        }

        private void CreateRain()
        {
            _rainRoot = new GameObject("WorkbenchRain", typeof(RectTransform)).GetComponent<RectTransform>();
            _rainRoot.SetParent(_artifact, false);
            _rainRoot.anchorMin = Vector2.zero;
            _rainRoot.anchorMax = Vector2.one;
            _rainRoot.offsetMin = _rainRoot.offsetMax = Vector2.zero;
            _drops = new Image[7];
            for (var index = 0; index < _drops.Length; index++)
            {
                var drop = new GameObject("RainDrop", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                drop.rectTransform.SetParent(_rainRoot, false);
                drop.rectTransform.sizeDelta = new Vector2(3, 14 + index % 3 * 3);
                drop.rectTransform.localRotation = Quaternion.Euler(0, 0, -8);
                drop.raycastTarget = false;
                _drops[index] = drop;
            }
            UpdateRain();
        }

        private void UpdateRain()
        {
            if (_drops == null) return;
            foreach (var drop in _drops)
            {
                if (drop == null) continue;
                var index = drop.transform.GetSiblingIndex();
                var fall = Mathf.Repeat(_rainClock * (.43f + index * .025f) + index * .31f, 1);
                var point = new Vector2(.26f + index * .08f, Mathf.Lerp(.64f, .16f, fall));
                drop.rectTransform.anchorMin = drop.rectTransform.anchorMax = point;
                drop.rectTransform.anchoredPosition = Vector2.zero;
                drop.color = new Color(.65f, .83f, 1, Mathf.Sin(fall * Mathf.PI) * .5f * _rainStrength);
            }
        }

        private void RestorePose()
        {
            if (_artifact == null) return;
            _artifact.anchoredPosition = _restPosition;
            _artifact.localScale = _restScale;
            _artifact.localRotation = _restRotation;
        }

        private void Settle()
        {
            _effect = null;
            _reactionTime = _fadeTime = ReactionDuration;
            RestorePose();
            SetColors(1, 0);
            _rainStrength = _complete ? 0 : 1 - _progress;
            UpdateRain();
        }

        private void RemoveRain()
        {
            if (_rainRoot != null)
            {
                _rainRoot.gameObject.SetActive(false);
                _rainRoot.SetParent(null, false);
                Destroy(_rainRoot.gameObject);
            }
            _rainRoot = null;
            _drops = null;
        }

        private void OnEnable()
        {
            if (_rainRoot != null) _rainRoot.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            Settle();
            if (_rainRoot != null) _rainRoot.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Settle();
            RemoveRain();
        }
    }
}
