using System;
using System.Collections;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using CurioClerk.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CurioClerk.Tests.PlayMode
{
    public sealed class ShiftPresentationViewPlayModeTests
    {
        private GameObject _host;

        [TearDown]
        public void TearDown()
        {
            if (_host != null)
            {
                UnityEngine.Object.DestroyImmediate(_host);
            }
        }

        [UnityTest]
        public IEnumerator DocketProgress_LabelsEmptyAndCompletedStamps()
        {
            var view = CreateDocketView(out var labels, out var surfaces);
            var docket = new DocketState();
            docket.TryStamp(Destination.Repair);

            view.Refresh(docket, 0, 4, "EMPTY", "COMPLETE");
            yield return null;

            Assert.That(labels[0].text, Is.EqualTo("COMPLETE"));
            Assert.That(labels[1].text, Is.EqualTo("EMPTY"));
            Assert.That(labels[2].text, Is.EqualTo("EMPTY"));
            Assert.That(surfaces[0].color, Is.Not.EqualTo(surfaces[1].color));
        }

        [UnityTest]
        public IEnumerator DocketProgress_KeepsFinalCounterAtRequiredTotal()
        {
            var view = CreateDocketView(out var labels, out _);

            view.Refresh(null, 4, 4, "EMPTY", "COMPLETE");
            yield return null;

            Assert.That(_host.transform.Find("Counter").GetComponent<TMP_Text>().text, Is.EqualTo("4 / 4"));
            Assert.That(labels[0].text, Is.EqualTo("EMPTY"));
            Assert.That(labels[1].text, Is.EqualTo("EMPTY"));
            Assert.That(labels[2].text, Is.EqualTo("EMPTY"));
        }

        [UnityTest]
        public IEnumerator FeedbackAnimator_CorrectInvokesCallbackAndRestoresArtwork()
        {
            var animator = CreateFeedbackAnimator(
                out _,
                out var artwork,
                out _,
                out _);
            var restPosition = artwork.anchoredPosition;
            var completed = false;

            animator.PlayCorrect(() => completed = true);
            yield return new WaitForSecondsRealtime(0.7f);

            Assert.That(completed, Is.True);
            AssertPositionAtRest(artwork, restPosition);
            Assert.That(artwork.localScale, Is.EqualTo(Vector3.one));
        }

        [UnityTest]
        public IEnumerator FeedbackAnimator_CorrectFilesArtworkTowardDestinationAndRevealsSeal()
        {
            var animator = CreateFeedbackAnimator(
                out _,
                out var artwork,
                out _,
                out _);
            var destination = CreateRect("Destination", new Vector2(220f, -150f));
            var seal = CreateChildRect(artwork, "FarewellSeal");
            var sealGroup = seal.gameObject.AddComponent<CanvasGroup>();
            sealGroup.alpha = 0f;
            animator.ConfigureFarewell(seal, sealGroup);
            var restWorldPosition = artwork.position;
            var completed = false;

            animator.PlayCorrect(destination, () => completed = true);
            yield return new WaitForSecondsRealtime(0.16f);

            Assert.That(sealGroup.alpha, Is.GreaterThan(0.2f),
                "The destination seal must appear before the curio leaves the desk.");

            yield return new WaitForSecondsRealtime(0.28f);
            Assert.That(Vector3.Distance(artwork.position, destination.position),
                Is.LessThan(Vector3.Distance(restWorldPosition, destination.position)),
                "The curio artwork must travel toward the selected filing desk.");

            yield return new WaitForSecondsRealtime(0.35f);
            Assert.That(completed, Is.True);
            Assert.That(sealGroup.alpha, Is.Zero.Within(0.01f));
            Assert.That(Vector3.Distance(seal.localScale, Vector3.one), Is.LessThan(0.01f));
            Assert.That(Quaternion.Angle(seal.localRotation, Quaternion.identity), Is.LessThan(0.01f));
            Assert.That(artwork.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f).Within(0.01f));
            Assert.That(Vector3.Distance(artwork.position, restWorldPosition), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator FeedbackAnimator_HoldInvokesCallbackAndRestoresArtwork()
        {
            var animator = CreateFeedbackAnimator(
                out _,
                out var artwork,
                out _,
                out var heldPreview);
            var restPosition = artwork.anchoredPosition;
            var completed = false;

            animator.PlayHold(() => completed = true);
            yield return new WaitForSecondsRealtime(0.18f);

            Assert.That(Vector3.Distance(heldPreview.localScale, Vector3.one), Is.GreaterThan(0.01f),
                "The Hold slot must react when a curio is placed there.");

            yield return new WaitForSecondsRealtime(0.42f);

            Assert.That(completed, Is.True);
            AssertPositionAtRest(artwork, restPosition);
            Assert.That(artwork.localScale, Is.EqualTo(Vector3.one));
            Assert.That(heldPreview.localScale, Is.EqualTo(Vector3.one));
        }

        [UnityTest]
        public IEnumerator FeedbackAnimator_DisableThenEnableCompletesPendingFilingOnce()
        {
            var animator = CreateFeedbackAnimator(out _, out _, out _, out _);
            var destination = CreateRect("Destination", new Vector2(220f, -150f));
            var completionCount = 0;

            animator.PlayCorrect(destination, () => completionCount++);
            yield return null;
            _host.SetActive(false);
            yield return null;
            Assert.That(completionCount, Is.Zero,
                "Disabling must not finish a transition while its screen is inactive.");

            _host.SetActive(true);
            yield return null;
            Assert.That(completionCount, Is.EqualTo(1));

            _host.SetActive(false);
            _host.SetActive(true);
            yield return null;
            Assert.That(completionCount, Is.EqualTo(1),
                "A resumed filing callback must run exactly once.");
        }

        [UnityTest]
        public IEnumerator FeedbackAnimator_WrongMovesCardAndReturnsItToRest()
        {
            var animator = CreateFeedbackAnimator(
                out var card,
                out _,
                out _,
                out _);
            var restPosition = card.anchoredPosition;

            animator.PlayWrong();
            yield return null;

            Assert.That(card.anchoredPosition, Is.Not.EqualTo(restPosition),
                "Wrong feedback must shake the curio card, not only the feedback strip.");

            yield return new WaitForSecondsRealtime(0.4f);
            AssertPositionAtRest(card, restPosition);
            Assert.That(card.localScale, Is.EqualTo(Vector3.one));
        }

        [UnityTest]
        public IEnumerator FeedbackAnimator_DisableRestoresEveryConfiguredTransform()
        {
            var animator = CreateFeedbackAnimator(
                out var card,
                out var artwork,
                out var feedback,
                out var heldPreview);
            var cardRest = card.anchoredPosition;
            var artworkRest = artwork.anchoredPosition;
            var feedbackRest = feedback.anchoredPosition;
            var heldRest = heldPreview.anchoredPosition;

            animator.PlayHold(() => { });
            yield return null;
            animator.gameObject.SetActive(false);

            AssertTransformAtRest(card, cardRest);
            AssertTransformAtRest(artwork, artworkRest);
            AssertTransformAtRest(feedback, feedbackRest);
            AssertTransformAtRest(heldPreview, heldRest);
        }

        [UnityTest]
        public IEnumerator FeedbackAnimator_DisableThenEnableResumesIdleArtworkMotion()
        {
            var animator = CreateFeedbackAnimator(
                out _,
                out var artwork,
                out _,
                out _);
            var restPosition = artwork.anchoredPosition;

            animator.SetIdleEnabled(true);
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(Vector2.Distance(artwork.anchoredPosition, restPosition), Is.GreaterThan(0.1f));

            _host.SetActive(false);
            AssertPositionAtRest(artwork, restPosition);

            _host.SetActive(true);
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(Vector2.Distance(artwork.anchoredPosition, restPosition), Is.GreaterThan(0.1f),
                "Idle artwork motion must resume after its screen is re-enabled.");
        }

        [UnityTest]
        public IEnumerator DocketProgress_CompleteInvokesCallbackAndRestoresStampSurfaces()
        {
            var view = CreateDocketView(out _, out var surfaces);
            var completed = false;
            var restScales = new[]
            {
                new Vector3(0.91f, 1.03f, 1f),
                new Vector3(1.07f, 0.94f, 1f),
                new Vector3(0.98f, 1.05f, 1f)
            };
            var restColors = new[]
            {
                new Color(0.42f, 0.18f, 0.22f, 0.76f),
                new Color(0.21f, 0.47f, 0.29f, 0.82f),
                new Color(0.63f, 0.39f, 0.11f, 0.88f)
            };
            for (var index = 0; index < surfaces.Length; index++)
            {
                surfaces[index].rectTransform.localScale = restScales[index];
                surfaces[index].color = restColors[index];
            }

            view.PlayComplete(() => completed = true);
            yield return new WaitForSecondsRealtime(0.1f);
            view.PlayComplete(() => completed = true);
            yield return new WaitForSecondsRealtime(0.8f);

            Assert.That(completed, Is.True);
            for (var index = 0; index < surfaces.Length; index++)
            {
                Assert.That(Vector3.Distance(surfaces[index].rectTransform.localScale, restScales[index]),
                    Is.LessThan(0.001f));
                Assert.That(Vector4.Distance(surfaces[index].color, restColors[index]),
                    Is.LessThan(0.001f));
            }
        }

        [UnityTest]
        public IEnumerator DocketProgress_CompletionRevealsConnectedCenterSigilThenRestoresIt()
        {
            var view = CreateDocketView(out _, out _);
            var sigil = CreateImage("CompletionSigil");
            sigil.enabled = false;
            sigil.color = new Color(0.94f, 0.70f, 0.31f, 0f);
            var restColor = sigil.color;
            var restScale = new Vector3(0.86f, 0.92f, 1f);
            sigil.rectTransform.localScale = restScale;
            view.ConfigureCompletionSigil(sigil);
            var completed = false;

            view.PlayComplete(() => completed = true);
            yield return new WaitForSecondsRealtime(0.24f);

            Assert.That(sigil.enabled, Is.True);
            Assert.That(sigil.color.a, Is.GreaterThan(0.2f));
            Assert.That(Vector3.Distance(sigil.rectTransform.localScale, restScale), Is.GreaterThan(0.02f),
                "A completed docket must connect its three stamps through a central reveal.");

            yield return new WaitForSecondsRealtime(0.55f);

            Assert.That(completed, Is.True);
            Assert.That(sigil.enabled, Is.False);
            Assert.That(Vector4.Distance(sigil.color, restColor), Is.LessThan(0.001f));
            Assert.That(Vector3.Distance(sigil.rectTransform.localScale, restScale), Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator DocketProgress_DisableThenEnableCompletesPendingPulseOnce()
        {
            var view = CreateDocketView(out _, out _);
            var completionCount = 0;

            view.PlayComplete(() => completionCount++);
            yield return null;
            _host.SetActive(false);
            yield return null;
            Assert.That(completionCount, Is.Zero);

            _host.SetActive(true);
            yield return null;
            Assert.That(completionCount, Is.EqualTo(1));

            _host.SetActive(false);
            _host.SetActive(true);
            yield return null;
            Assert.That(completionCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ResultLedgerAnimator_RevealsFourRowsInSequence()
        {
            var animator = CreateResultLedgerAnimator(out var rows);

            animator.Play();

            for (var index = 0; index < rows.Length; index++)
            {
                Assert.That(rows[index].alpha, Is.Zero);
            }

            yield return new WaitForSecondsRealtime(0.12f);
            Assert.That(rows[0].alpha, Is.GreaterThan(rows[3].alpha + 0.1f),
                "Completed docket rows must reveal in ledger order, not all at once.");

            yield return new WaitForSecondsRealtime(0.7f);
            for (var index = 0; index < rows.Length; index++)
            {
                Assert.That(rows[index].alpha, Is.EqualTo(1f).Within(0.01f));
                Assert.That(Vector3.Distance(rows[index].transform.localScale, Vector3.one),
                    Is.LessThan(0.01f));
            }
        }

        [UnityTest]
        public IEnumerator ResultLedgerAnimator_DisableRestoresEveryRow()
        {
            var animator = CreateResultLedgerAnimator(out var rows);
            animator.Play();
            yield return new WaitForSecondsRealtime(0.12f);

            _host.SetActive(false);

            for (var index = 0; index < rows.Length; index++)
            {
                Assert.That(rows[index].alpha, Is.EqualTo(1f).Within(0.01f));
                Assert.That(Vector3.Distance(rows[index].transform.localScale, Vector3.one),
                    Is.LessThan(0.01f));
            }
        }

        [Test]
        public void ResultLedgerAnimator_RejectsInvalidRowCollections()
        {
            _host = new GameObject("ResultLedgerAnimatorValidationHost", typeof(RectTransform));
            var animator = _host.AddComponent<ResultLedgerAnimator>();

            Assert.Throws<ArgumentException>(() => animator.Configure(null));
            Assert.Throws<ArgumentException>(() => animator.Configure(new CanvasGroup[3]));
            Assert.Throws<ArgumentException>(() => animator.Configure(new CanvasGroup[4]));
        }

        [UnityTest]
        public IEnumerator ResultLedgerAnimator_RepeatedPlayRestartsAndCompletesCleanly()
        {
            var animator = CreateResultLedgerAnimator(out var rows);
            animator.Play();
            yield return new WaitForSecondsRealtime(0.12f);

            animator.Play();
            for (var index = 0; index < rows.Length; index++)
            {
                Assert.That(rows[index].alpha, Is.Zero,
                    "Restarting an in-flight reveal must reset every ledger row.");
            }

            yield return new WaitForSecondsRealtime(0.82f);
            for (var index = 0; index < rows.Length; index++)
            {
                Assert.That(rows[index].alpha, Is.EqualTo(1f).Within(0.01f));
                Assert.That(Vector3.Distance(rows[index].transform.localScale, Vector3.one),
                    Is.LessThan(0.01f));
            }
        }

        private DocketProgressView CreateDocketView(out TMP_Text[] labels, out Image[] surfaces)
        {
            _host = new GameObject("DocketProgressViewTestHost", typeof(RectTransform));
            var counter = CreateText("Counter");
            labels = new[]
            {
                CreateText("RepairStatus"),
                CreateText("StorageStatus"),
                CreateText("VaultStatus")
            };
            surfaces = new[]
            {
                CreateImage("RepairSurface"),
                CreateImage("StorageSurface"),
                CreateImage("VaultSurface")
            };

            var view = _host.AddComponent<DocketProgressView>();
            view.Configure(
                counter,
                surfaces,
                labels,
                new Color(0.15f, 0.12f, 0.14f, 1f),
                new[]
                {
                    new Color(0.71f, 0.43f, 0.47f, 1f),
                    new Color(0.44f, 0.54f, 0.42f, 1f),
                    new Color(0.88f, 0.64f, 0.29f, 1f)
                });
            return view;
        }

        private ShiftFeedbackAnimator CreateFeedbackAnimator(
            out RectTransform card,
            out RectTransform artwork,
            out RectTransform feedback,
            out RectTransform heldPreview)
        {
            _host = new GameObject("ShiftFeedbackAnimatorTestHost", typeof(RectTransform));
            card = CreateRect("Card", new Vector2(12f, 18f));
            artwork = CreateRect("Artwork", new Vector2(5f, 9f));
            artwork.SetParent(card, false);
            artwork.anchoredPosition = new Vector2(5f, 9f);
            artwork.gameObject.AddComponent<CanvasGroup>();
            feedback = CreateRect("Feedback", new Vector2(3f, 7f));
            heldPreview = CreateRect("HeldPreview", new Vector2(80f, 30f));
            var animator = _host.AddComponent<ShiftFeedbackAnimator>();
            animator.Configure(card, artwork, feedback, heldPreview);
            return animator;
        }

        private ResultLedgerAnimator CreateResultLedgerAnimator(out CanvasGroup[] rows)
        {
            _host = new GameObject("ResultLedgerAnimatorTestHost", typeof(RectTransform));
            rows = new CanvasGroup[4];
            for (var index = 0; index < rows.Length; index++)
            {
                var row = new GameObject(
                    "ResultRow" + index,
                    typeof(RectTransform),
                    typeof(CanvasGroup));
                row.transform.SetParent(_host.transform, false);
                rows[index] = row.GetComponent<CanvasGroup>();
            }

            var animator = _host.AddComponent<ResultLedgerAnimator>();
            animator.Configure(rows);
            return animator;
        }

        private RectTransform CreateRect(string name, Vector2 anchoredPosition)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(_host.transform, false);
            var rect = child.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            return rect;
        }

        private static RectTransform CreateChildRect(RectTransform parent, string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }

        private static void AssertTransformAtRest(RectTransform transform, Vector2 restPosition)
        {
            AssertPositionAtRest(transform, restPosition);
            Assert.That(transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(transform.localRotation, Is.EqualTo(Quaternion.identity));
        }

        private static void AssertPositionAtRest(RectTransform transform, Vector2 restPosition)
        {
            Assert.That(Vector2.Distance(transform.anchoredPosition, restPosition), Is.LessThan(0.01f));
        }

        private TMP_Text CreateText(string name)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            child.transform.SetParent(_host.transform, false);
            return child.GetComponent<TMP_Text>();
        }

        private Image CreateImage(string name)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(Image));
            child.transform.SetParent(_host.transform, false);
            return child.GetComponent<Image>();
        }
    }
}
