using System;
using UnityEngine;
using Highfly.Core;
using Highfly.Combat;

namespace Highfly.World
{
    /// <summary>
    /// Swaps retargetable complete Quaternius humanoid outfits while preserving
    /// a single stable physics root, FURY input and Guinevere combat component.
    /// Every appearance must have a valid Humanoid avatar and a common controller.
    /// </summary>
    public sealed class HighflyHunterAppearance : MonoBehaviour
    {
        [SerializeField] private GameObject[] variants;
        [SerializeField] private Animator[] variantAnimators;
        [SerializeField] private HighflyThirdPersonMotor motor;
        [SerializeField] private HighflyGuinevereCombat combat;
        [SerializeField] private HighflyCombatController legacyCombat;
        [SerializeField] private int initialVariant;

        private int _current = -1;
        public event Action<int> AppearanceChanged;
        public int CurrentIndex => _current;
        public int VariantCount => variants != null ? variants.Length : 0;
        public string CurrentAppearance => _current >= 0 && _current < VariantCount &&
            variants[_current] != null ? variants[_current].name : "KayKit";

        private void Start()
        {
            if (VariantCount > 0) SetAppearance(Mathf.Clamp(initialVariant, 0, VariantCount - 1));
        }

        public void NextLook()
        {
            if (VariantCount == 0) return;
            SetAppearance((_current + 1) % VariantCount);
        }

        public bool SetAppearance(int index)
        {
            if (variants == null || variantAnimators == null ||
                index < 0 || index >= variants.Length ||
                index >= variantAnimators.Length) return false;

            Animator selected = variantAnimators[index];
            if (selected == null || selected.avatar == null ||
                !selected.avatar.isValid || !selected.avatar.isHuman ||
                selected.runtimeAnimatorController == null)
                return false;

            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i] != null) variants[i].SetActive(i == index);
            }
            _current = index;
            if (motor != null) motor.BindVisual(selected);
            if (combat != null) combat.BindAnimator(selected);
            if (legacyCombat != null) legacyCombat.BindAnimator(selected);
            AppearanceChanged?.Invoke(index);
            return true;
        }
    }
}
