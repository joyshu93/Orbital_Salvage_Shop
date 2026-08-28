using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurioClerk.Presentation
{
    public sealed class ResultLedgerAnimator : MonoBehaviour
    {
        private const int RequiredRowCount = 4;
        private const float RowDelay = 0.10f;
        private const float RevealDuration = 0.18f;
        private static readonly Vector3 HiddenScale = new Vector3(0.96f, 0.96f, 1f);

        private CanvasGroup[] _rows = Array.Empty<CanvasGroup>();
        private Coroutine _routine;

        public void Configure(IReadOnlyList<CanvasGroup> rows)
        {
            if (rows == null || rows.Count != RequiredRowCount)
            {
                throw new ArgumentException("Exactly four result ledger rows are required.", nameof(rows));
            }

            _rows = new CanvasGroup[RequiredRowCount];
            for (var index = 0; index < _rows.Length; index++)
            {
                _rows[index] = rows[index] != null
                    ? rows[index]
                    : throw new ArgumentException("Result ledger rows cannot contain null.", nameof(rows));
            }
        }

        public void Play()
        {
            if (_rows.Length != RequiredRowCount)
            {
                throw new InvalidOperationException("Configure the result ledger animator before playing it.");
            }

            StopActiveRoutine();
            ResetRowsForReveal();
            _routine = StartCoroutine(RevealRows());
        }

        private IEnumerator RevealRows()
        {
            var elapsed = 0f;
            var totalDuration = RowDelay * (_rows.Length - 1) + RevealDuration;
            yield return null;
            while (elapsed < totalDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                for (var index = 0; index < _rows.Length; index++)
                {
                    var progress = Mathf.Clamp01((elapsed - RowDelay * index) / RevealDuration);
                    var eased = 1f - Mathf.Pow(1f - progress, 3f);
                    _rows[index].alpha = eased;
                    _rows[index].transform.localScale = Vector3.LerpUnclamped(HiddenScale, Vector3.one, eased);
                }

                yield return null;
            }

            RestoreRows();
            _routine = null;
        }

        private void OnDisable()
        {
            StopActiveRoutine();
            RestoreRows();
        }

        private void StopActiveRoutine()
        {
            if (_routine == null)
            {
                return;
            }

            StopCoroutine(_routine);
            _routine = null;
        }

        private void ResetRowsForReveal()
        {
            for (var index = 0; index < _rows.Length; index++)
            {
                _rows[index].alpha = 0f;
                _rows[index].transform.localScale = HiddenScale;
            }
        }

        private void RestoreRows()
        {
            for (var index = 0; index < _rows.Length; index++)
            {
                if (_rows[index] == null)
                {
                    continue;
                }

                _rows[index].alpha = 1f;
                _rows[index].transform.localScale = Vector3.one;
            }
        }
    }
}
