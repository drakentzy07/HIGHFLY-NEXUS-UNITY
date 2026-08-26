using UnityEngine;
using UnityEngine.UI;
using Highfly.Core;

namespace Highfly.World
{
    public sealed class HighflyInteractionController : MonoBehaviour
    {
        [SerializeField] private float interactionRange = 3.1f;
        [SerializeField] private float refreshInterval = 0.12f;
        [SerializeField] private Text promptText;
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private Button interactButton;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Text dialogueTitle;
        [SerializeField] private Text dialogueBody;
        [SerializeField] private Button closeDialogueButton;

        private HighflyInteractable _current;
        private float _nextRefresh;

        public bool DialogueOpen => dialoguePanel != null && dialoguePanel.activeSelf;

        private void Start()
        {
            if (interactButton != null)
                interactButton.onClick.AddListener(InteractCurrent);
            if (closeDialogueButton != null)
                closeDialogueButton.onClick.AddListener(CloseDialogue);

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            SetPromptVisible(false);
        }

        private void OnDestroy()
        {
            if (interactButton != null)
                interactButton.onClick.RemoveListener(InteractCurrent);
            if (closeDialogueButton != null)
                closeDialogueButton.onClick.RemoveListener(CloseDialogue);
        }

        private void Update()
        {
            if (DialogueOpen)
            {
                SetPromptVisible(false);
                return;
            }

            if (Time.unscaledTime < _nextRefresh)
                return;

            _nextRefresh = Time.unscaledTime + refreshInterval;
            RefreshCurrent();
        }

        public void InteractCurrent()
        {
            if (DialogueOpen)
            {
                CloseDialogue();
                return;
            }

            if (_current == null)
                RefreshCurrent();

            if (_current != null)
                _current.Interact(this);
        }

        public void ShowDialogue(string title, string body)
        {
            if (dialoguePanel == null)
                return;

            if (dialogueTitle != null)
                dialogueTitle.text = title;
            if (dialogueBody != null)
                dialogueBody.text = body;

            dialoguePanel.SetActive(true);
            SetPromptVisible(false);
        }

        public void CloseDialogue()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            RefreshCurrent();
        }

        public void Teleport(Vector3 destination, Vector3 facing, bool restoreResources)
        {
            CharacterController characterController = GetComponent<CharacterController>();
            bool wasEnabled = characterController != null && characterController.enabled;

            if (wasEnabled)
                characterController.enabled = false;

            transform.position = destination;

            if (facing.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(facing.normalized, Vector3.up);

            if (wasEnabled)
                characterController.enabled = true;

            Highfly.Combat.HighflyWorldSafety safety = GetComponent<Highfly.Combat.HighflyWorldSafety>();
            if (safety != null)
                safety.SetRespawnPosition(destination);

            if (restoreResources)
            {
                HighflyPlayerResources resources = GetComponent<HighflyPlayerResources>();
                if (resources != null)
                    resources.RestoreAll();
            }

            _current = null;
            SetPromptVisible(false);
        }

        public void RestoreAll()
        {
            HighflyPlayerResources resources = GetComponent<HighflyPlayerResources>();
            if (resources != null)
                resources.RestoreAll();
        }

        private void RefreshCurrent()
        {
            HighflyInteractable best = null;
            float bestScore = float.PositiveInfinity;

            Collider[] hits = Physics.OverlapSphere(
                transform.position + Vector3.up * 0.9f,
                interactionRange,
                ~0,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                if (hit == null)
                    continue;

                HighflyInteractable interactable = hit.GetComponentInParent<HighflyInteractable>();
                if (interactable == null || !interactable.isActiveAndEnabled)
                    continue;

                Vector3 delta = interactable.InteractionPoint - transform.position;
                delta.y = 0f;
                float distance = delta.magnitude;

                if (distance > interactionRange + 0.5f)
                    continue;

                float anglePenalty = 0f;
                if (distance > 0.05f)
                {
                    float dot = Vector3.Dot(transform.forward, delta.normalized);
                    anglePenalty = Mathf.Lerp(1.5f, 0f, Mathf.InverseLerp(-0.3f, 0.9f, dot));
                }

                float score = distance + anglePenalty;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = interactable;
                }
            }

            _current = best;
            if (_current == null)
            {
                SetPromptVisible(false);
                return;
            }

            if (promptText != null)
                promptText.text = _current.Prompt + "  •  " + _current.DisplayName;
            SetPromptVisible(true);
        }

        private void SetPromptVisible(bool visible)
        {
            if (promptPanel != null)
                promptPanel.SetActive(visible);
            if (interactButton != null)
                interactButton.gameObject.SetActive(visible);
        }
    }
}
