using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CurioClerk.Presentation
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class ArtifactDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _rectTransform;
        private RectTransform[] _dropTargets = Array.Empty<RectTransform>();
        private Action<int> _dropped;
        private Vector2 _restingPosition;
        private Vector3 _restingScale;
        private float _canvasScale = 1f;

        public void Configure(RectTransform[] dropTargets, Action<int> dropped)
        {
            _dropTargets = dropTargets ?? Array.Empty<RectTransform>();
            _dropped = dropped;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _rectTransform = GetComponent<RectTransform>();
            _restingPosition = _rectTransform.anchoredPosition;
            _restingScale = _rectTransform.localScale;
            var canvas = GetComponentInParent<Canvas>();
            _canvasScale = canvas == null ? 1f : Mathf.Max(0.01f, canvas.scaleFactor);
            _rectTransform.localScale = _restingScale * 1.03f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_rectTransform == null)
            {
                return;
            }

            _rectTransform.anchoredPosition += eventData.delta / _canvasScale;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var targetIndex = FindTarget(eventData.position, eventData.pressEventCamera);
            ResetVisual();
            if (targetIndex >= 0)
            {
                _dropped?.Invoke(targetIndex);
            }
        }

        private int FindTarget(Vector2 screenPosition, Camera eventCamera)
        {
            for (var index = 0; index < _dropTargets.Length; index++)
            {
                var target = _dropTargets[index];
                if (target != null && RectTransformUtility.RectangleContainsScreenPoint(target, screenPosition, eventCamera))
                {
                    return index;
                }
            }

            return -1;
        }

        private void ResetVisual()
        {
            if (_rectTransform == null)
            {
                return;
            }

            _rectTransform.anchoredPosition = _restingPosition;
            _rectTransform.localScale = _restingScale;
        }
    }
}
