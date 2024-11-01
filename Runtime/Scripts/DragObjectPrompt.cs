using UnityEngine;
using UnityEngine.UI;

namespace ezygamers.ObjectMovementPrompt
{
    public class DragObjectPrompt : MonoBehaviour
    {
        [Header("Prompt Settings")]
        [SerializeField] private bool enableAutoPrompt = true;
        
        [Header("References")]
        [SerializeField] private Image handPromptImage;
        
        [SerializeField] private float idleTimeThreshold = 5f;      // Time before showing prompt
        [SerializeField] private float animationDuration = 1.5f;    // Duration of single animation
        [SerializeField] private float verticalDistance = 1000f;     // Distance to move down
        [SerializeField] private float curveOffset = 50f;          // How much the curve bends to the right

        private RectTransform handRectTransform;
        private Vector2 startPosition;
        private Vector2 endPosition;
        private bool isAnimating;
        private int? currentTweenId;
        private LTDescr movementTween;
        
        private void Awake()
        {
            if (!ValidateComponents()) return;
            SetupPromptPositions();
            InitializePrompt();
        }

        private void Start()
        {
            if (enableAutoPrompt)
            {
                ScheduleAnimation();
            }
        }

        private void OnDestroy()
        {
            CancelCurrentAnimation();
        }

        private bool ValidateComponents()
        {
            if (handPromptImage == null)
            {
                Debug.LogError("Hand Prompt Image not assigned to DragHandPrompt script!");
                enabled = false;
                return false;
            }
            
            handRectTransform = handPromptImage.rectTransform;
            return true;
        }

        private void SetupPromptPositions()
        {
            startPosition = handRectTransform.anchoredPosition;
            endPosition = new Vector2(
                startPosition.x + curveOffset,
                startPosition.y - verticalDistance
            );
        }

        private void InitializePrompt()
        {
            handPromptImage.enabled = false;
            isAnimating = false;
        }

        private void ScheduleAnimation()
        {
            if (currentTweenId.HasValue)
            {
                LeanTween.cancel(currentTweenId.Value);
            }

            currentTweenId = LeanTween.delayedCall(idleTimeThreshold, () =>
            {
                if (enableAutoPrompt && !isAnimating)
                {
                    StartAnimation();
                }
            }).id;
        }

        public void CallObjectMovement()
        {
            if (isAnimating)
            {
                StopAnimation();
            }
        }

        public void StartPromptAnimation()
        {
            if (!enableAutoPrompt) return;
            StartAnimation();
        }

        public void StopPromptAnimation()
        {
            StopAnimation();
        }

        public void SetPromptEnabled(bool enabled)
        {
            enableAutoPrompt = enabled;
            if (!enabled)
            {
                StopAnimation();
            }
            else
            {
                ScheduleAnimation();
            }
        }

        private void StartAnimation()
        {
            //if (isAnimating) return;
            
            isAnimating = true;
            handPromptImage.enabled = true;
            
            // Reset position and make fully visible
            handRectTransform.anchoredPosition = startPosition;
            handPromptImage.color = new Color(1f, 1f, 1f, 1f);

            // Create the movement sequence
            movementTween = LeanTween.value(gameObject, 0f, 1f, animationDuration)
                .setOnUpdate((float t) =>
                {
                    // Calculate curved path using quadratic Bezier curve
                    Vector2 controlPoint = new Vector2(
                        startPosition.x + curveOffset,
                        startPosition.y - verticalDistance * 0.5f
                    );
                    
                    Vector2 newPosition = Vector2.Lerp(
                        Vector2.Lerp(startPosition, controlPoint, t),
                        Vector2.Lerp(controlPoint, endPosition, t),
                        t
                    );
                    
                    handRectTransform.anchoredPosition = newPosition;
                })
                .setOnComplete(() =>
                {
                    // Fade out when reaching end position
                    LeanTween.alpha(handRectTransform, 0f, 0.3f)
                        .setOnComplete(() =>
                        {
                            if (enableAutoPrompt)
                            {
                                // Reset position and alpha
                                handRectTransform.anchoredPosition = startPosition;
                                handPromptImage.color = new Color(1f, 1f, 1f, 1f);
                                
                                // Restart the movement sequence
                                StartAnimation();
                            }
                        });
                })
                .setEase(LeanTweenType.easeInOutSine);
        }

        private void StopAnimation()
        {
            if (!isAnimating) return;
            
            CancelCurrentAnimation();
            handPromptImage.enabled = false;
            isAnimating = false;
        }

        private void CancelCurrentAnimation()
        {
            LeanTween.cancel(gameObject);
            if (currentTweenId.HasValue)
            {
                LeanTween.cancel(currentTweenId.Value);
                currentTweenId = null;
            }
            if (movementTween != null)
            {
                LeanTween.cancel(movementTween.id);
            }
        }
    }
}