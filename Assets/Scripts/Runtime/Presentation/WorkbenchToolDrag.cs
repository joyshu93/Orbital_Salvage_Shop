using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed class WorkbenchToolDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private string _toolId;
        private RectTransform[] _targets;
        private string[] _targetIds;
        private Action<string, string> _apply;
        private Action<string> _select;
        private RectTransform _ghost;
        private RectTransform _canvas;
        private int? _pointerId;

        public void Configure(string toolId, RectTransform[] targets, string[] targetIds,
            Action<string, string> apply, Action<string> select)
        {
            _toolId = toolId;
            _targets = targets;
            _targetIds = targetIds;
            _apply = apply;
            _select = select;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || _pointerId.HasValue) return;
            _pointerId = eventData.pointerId;
            eventData.eligibleForClick = false;
            _select?.Invoke(_toolId);
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            _canvas = (RectTransform)canvas.transform;
            var ghost = new GameObject("WorkbenchDraggedTool", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            _ghost = ghost.GetComponent<RectTransform>();
            _ghost.SetParent(_canvas, false);
            _ghost.sizeDelta = new Vector2(220, 100);
            ghost.GetComponent<Image>().color = new Color(0.88f, 0.64f, 0.29f, 0.92f);
            ghost.GetComponent<Image>().raycastTarget = false;
            ghost.GetComponent<CanvasGroup>().blocksRaycasts = false;
            var source = GetComponentInChildren<TMP_Text>();
            if (source != null)
            {
                var label = Instantiate(source, _ghost, false);
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = Vector2.zero;
                label.rectTransform.offsetMax = Vector2.zero;
                label.raycastTarget = false;
            }
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_pointerId != eventData.pointerId || _ghost == null) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas, eventData.position,
                    eventData.pressEventCamera, out var point))
                _ghost.anchoredPosition = point;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_pointerId != eventData.pointerId) return;
            eventData.eligibleForClick = false;
            string targetId = null;
            var nearest = float.MaxValue;
            if (_targets != null)
            {
                for (var index = 0; index < _targets.Length; index++)
                {
                    var target = _targets[index];
                    if (target == null || !target.gameObject.activeInHierarchy ||
                        !RectTransformUtility.RectangleContainsScreenPoint(target, eventData.position, eventData.pressEventCamera)) continue;
                    var center = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, target.TransformPoint(target.rect.center));
                    var distance = (eventData.position - center).sqrMagnitude;
                    if (distance >= nearest) continue;
                    nearest = distance;
                    targetId = _targetIds[index];
                }
            }
            CancelDrag();
            if (targetId != null) _apply?.Invoke(_toolId, targetId);
        }

        private void OnDisable() => CancelDrag();
        private void OnDestroy() => CancelDrag();

        private void CancelDrag()
        {
            _pointerId = null;
            if (_ghost == null) return;
            _ghost.gameObject.SetActive(false);
            Destroy(_ghost.gameObject);
            _ghost = null;
        }
    }
}
