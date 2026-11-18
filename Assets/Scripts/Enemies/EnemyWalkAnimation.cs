using System.Collections;
using UnityEngine;

namespace UngrownCastle.Enemies
{
    public enum AnimationStyle
    {
        BipedWalk,   // Zombie, Kobold, Ork, Troll, Cannibal, Tauren
        SlimeBounce, // Slime
        FlyingFlap   // Harpy
    }

    public class EnemyWalkAnimation : MonoBehaviour
    {
        [Header("Animation Style")]
        [SerializeField] private AnimationStyle style = AnimationStyle.BipedWalk;

        [Header("Transforms To Animate")]
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform weaponTransform;
        [SerializeField] private Transform leftWingTransform;
        [SerializeField] private Transform rightWingTransform;
        [SerializeField] private Transform tailTransform;

        [Header("Walk Settings")]
        [SerializeField] private float walkFrequency = 8f;
        [SerializeField] private float bobHeight = 0.08f;
        [SerializeField] private float swayAngle = 6f;
        [SerializeField] private float weaponSwingAngle = 18f;

        [Header("Slime Bounce Settings")]
        [SerializeField] private float bounceFrequency = 6f;
        [SerializeField] private float squashX = 1.25f;
        [SerializeField] private float stretchY = 1.3f;

        [Header("Flying Settings")]
        [SerializeField] private float flapFrequency = 14f;
        [SerializeField] private float wingFlapAngle = 35f;
        [SerializeField] private float flyBobHeight = 0.12f;

        private Vector3 initialBodyPos;
        private Vector3 initialHeadPos;
        private Quaternion initialWeaponRot;
        private Quaternion initialLeftWingRot;
        private Quaternion initialRightWingRot;
        private Quaternion initialTailRot;
        private Vector3 initialScale;

        private bool isAttacking = false;
        private bool isDead = false;
        private float walkTimer = 0f;
        private Coroutine attackSwingCoroutine;

        private void Awake()
        {
            if (bodyTransform == null) bodyTransform = transform;
            initialBodyPos = bodyTransform.localPosition;
            initialScale = bodyTransform.localScale;

            if (headTransform != null) initialHeadPos = headTransform.localPosition;
            if (weaponTransform != null) initialWeaponRot = weaponTransform.localRotation;
            if (leftWingTransform != null) initialLeftWingRot = leftWingTransform.localRotation;
            if (rightWingTransform != null) initialRightWingRot = rightWingTransform.localRotation;
            if (tailTransform != null) initialTailRot = tailTransform.localRotation;
        }

        private void Update()
        {
            if (isDead) return;

            walkTimer += Time.deltaTime;

            if (!isAttacking)
            {
                switch (style)
                {
                    case AnimationStyle.BipedWalk:
                        AnimateBipedWalk();
                        break;
                    case AnimationStyle.SlimeBounce:
                        AnimateSlimeBounce();
                        break;
                    case AnimationStyle.FlyingFlap:
                        AnimateFlyingFlap();
                        break;
                }
            }
        }

        private void AnimateBipedWalk()
        {
            // Bouncing up and down as feet take steps
            float bob = Mathf.Abs(Mathf.Sin(walkTimer * walkFrequency)) * bobHeight;
            bodyTransform.localPosition = initialBodyPos + new Vector3(0, bob, 0);

            // Torso / body swaying forward and back
            float sway = Mathf.Sin(walkTimer * walkFrequency * 0.5f) * swayAngle;
            bodyTransform.localRotation = Quaternion.Euler(0, 0, sway);

            // Head nodding gently
            if (headTransform != null)
            {
                float headBob = Mathf.Sin(walkTimer * walkFrequency + 0.5f) * (bobHeight * 0.5f);
                headTransform.localPosition = initialHeadPos + new Vector3(0, headBob, 0);
            }

            // Weapon swinging back and forth with walk cycle
            if (weaponTransform != null && attackSwingCoroutine == null)
            {
                float weaponSwing = Mathf.Sin(walkTimer * walkFrequency) * weaponSwingAngle;
                weaponTransform.localRotation = initialWeaponRot * Quaternion.Euler(0, 0, weaponSwing);
            }

            // Tail swaying (Tauren)
            if (tailTransform != null)
            {
                float tailSwing = Mathf.Sin(walkTimer * walkFrequency * 0.7f) * 15f;
                tailTransform.localRotation = initialTailRot * Quaternion.Euler(0, 0, tailSwing);
            }
        }

        private void AnimateSlimeBounce()
        {
            // Bouncy squash & stretch hop
            float cycle = Mathf.Repeat(walkTimer * bounceFrequency, Mathf.PI);
            float hop = Mathf.Sin(cycle);

            // Vertical position
            bodyTransform.localPosition = initialBodyPos + new Vector3(0, hop * bobHeight * 2f, 0);

            // Scale squash and stretch
            if (hop > 0.4f)
            {
                // In air: stretched tall and thin
                float factor = (hop - 0.4f) / 0.6f;
                float sx = Mathf.Lerp(1f, 1f / stretchY, factor);
                float sy = Mathf.Lerp(1f, stretchY, factor);
                bodyTransform.localScale = new Vector3(initialScale.x * sx, initialScale.y * sy, initialScale.z);
            }
            else
            {
                // On ground: squashed wide and flat
                float factor = 1f - (hop / 0.4f);
                float sx = Mathf.Lerp(1f, squashX, factor);
                float sy = Mathf.Lerp(1f, 1f / squashX, factor);
                bodyTransform.localScale = new Vector3(initialScale.x * sx, initialScale.y * sy, initialScale.z);
            }
        }

        private void AnimateFlyingFlap()
        {
            // Hovering sine wave
            float hover = Mathf.Sin(walkTimer * 3f) * flyBobHeight;
            bodyTransform.localPosition = initialBodyPos + new Vector3(0, hover, 0);

            // Flapping wings
            float flap = Mathf.Sin(walkTimer * flapFrequency);

            if (leftWingTransform != null)
            {
                leftWingTransform.localRotation = initialLeftWingRot * Quaternion.Euler(0, 0, flap * wingFlapAngle);
            }

            if (rightWingTransform != null)
            {
                rightWingTransform.localRotation = initialRightWingRot * Quaternion.Euler(0, 0, -flap * wingFlapAngle);
            }
        }

        public void SetAttacking(bool attacking)
        {
            isAttacking = attacking;
            if (!attacking)
            {
                // Reset positions when moving again
                bodyTransform.localPosition = initialBodyPos;
                bodyTransform.localRotation = Quaternion.identity;
                bodyTransform.localScale = initialScale;
            }
        }

        public void TriggerAttackSwing()
        {
            if (attackSwingCoroutine != null)
            {
                StopCoroutine(attackSwingCoroutine);
            }
            attackSwingCoroutine = StartCoroutine(AttackSwingRoutine());
        }

        private IEnumerator AttackSwingRoutine()
        {
            float duration = 0.25f;
            float elapsed = 0f;

            // Wind up (lean back)
            while (elapsed < duration * 0.4f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.4f);
                bodyTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, -12f, t));
                if (weaponTransform != null)
                {
                    weaponTransform.localRotation = initialWeaponRot * Quaternion.Euler(0, 0, Mathf.Lerp(0, -35f, t));
                }
                yield return null;
            }

            // Swing forward (lunge towards castle)
            elapsed = 0f;
            while (elapsed < duration * 0.6f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.6f);
                bodyTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-12f, 20f, t));
                if (weaponTransform != null)
                {
                    weaponTransform.localRotation = initialWeaponRot * Quaternion.Euler(0, 0, Mathf.Lerp(-35f, 50f, t));
                }
                yield return null;
            }

            // Return to rest
            elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.15f;
                bodyTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(20f, 0, t));
                if (weaponTransform != null)
                {
                    weaponTransform.localRotation = Quaternion.Lerp(weaponTransform.localRotation, initialWeaponRot, t);
                }
                yield return null;
            }

            attackSwingCoroutine = null;
        }

        public void OnDeath()
        {
            isDead = true;
            if (attackSwingCoroutine != null)
            {
                StopCoroutine(attackSwingCoroutine);
            }

            // Tumble back on death
            bodyTransform.localRotation = Quaternion.Euler(0, 0, -75f);
            bodyTransform.localPosition = initialBodyPos + new Vector3(0, -0.1f, 0);
        }
    }
}
