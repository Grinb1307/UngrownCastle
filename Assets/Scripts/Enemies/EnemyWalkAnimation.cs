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
        [SerializeField] private Transform leftLegTransform;
        [SerializeField] private Transform rightLegTransform;
        [SerializeField] private Transform leftArmTransform;
        [SerializeField] private Transform rightArmTransform;
        [SerializeField] private Transform weaponTransform;
        [SerializeField] private Transform leftWingTransform;
        [SerializeField] private Transform rightWingTransform;
        [SerializeField] private Transform tailTransform;

        [Header("Walk Settings")]
        [SerializeField] private float walkFrequency = 9f;
        [SerializeField] private float bobHeight = 0.05f;
        [SerializeField] private float swayAngle = 5f;
        [SerializeField] private float legSwingAngle = 32f;
        [SerializeField] private float armSwingAngle = 28f;
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
        private Quaternion initialLeftLegRot;
        private Quaternion initialRightLegRot;
        private Quaternion initialLeftArmRot;
        private Quaternion initialRightArmRot;
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
            if (leftLegTransform != null) initialLeftLegRot = leftLegTransform.localRotation;
            if (rightLegTransform != null) initialRightLegRot = rightLegTransform.localRotation;
            if (leftArmTransform != null) initialLeftArmRot = leftArmTransform.localRotation;
            if (rightArmTransform != null) initialRightArmRot = rightArmTransform.localRotation;
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
            float walkCycle = walkTimer * walkFrequency;

            // 1. Legs scissor swing
            float legSwing = Mathf.Sin(walkCycle) * legSwingAngle;
            if (leftLegTransform != null)
            {
                leftLegTransform.localRotation = initialLeftLegRot * Quaternion.Euler(0, 0, -legSwing);
            }
            if (rightLegTransform != null)
            {
                rightLegTransform.localRotation = initialRightLegRot * Quaternion.Euler(0, 0, legSwing);
            }

            // 2. Arms swing in opposition to legs
            float armSwing = Mathf.Sin(walkCycle) * armSwingAngle;
            if (leftArmTransform != null)
            {
                leftArmTransform.localRotation = initialLeftArmRot * Quaternion.Euler(0, 0, armSwing);
            }
            if (rightArmTransform != null)
            {
                rightArmTransform.localRotation = initialRightArmRot * Quaternion.Euler(0, 0, -armSwing);
            }

            // 3. Body bobs up & down twice per full stride
            float bob = Mathf.Abs(Mathf.Sin(walkCycle)) * bobHeight;
            bodyTransform.localPosition = initialBodyPos + new Vector3(0, bob, 0);

            // 4. Torso subtle sway
            float sway = Mathf.Sin(walkCycle * 0.5f) * swayAngle;
            bodyTransform.localRotation = Quaternion.Euler(0, 0, sway);

            // 5. Head nodding gently
            if (headTransform != null)
            {
                float headBob = Mathf.Sin(walkCycle + 0.5f) * (bobHeight * 0.5f);
                headTransform.localPosition = initialHeadPos + new Vector3(0, headBob, 0);
            }

            // 6. Weapon swing if separate from rightArm
            if (weaponTransform != null && attackSwingCoroutine == null && rightArmTransform == null)
            {
                float weaponSwing = Mathf.Sin(walkCycle) * weaponSwingAngle;
                weaponTransform.localRotation = initialWeaponRot * Quaternion.Euler(0, 0, weaponSwing);
            }

            // 7. Tail swaying (Tauren)
            if (tailTransform != null)
            {
                float tailSwing = Mathf.Sin(walkCycle * 0.7f) * 15f;
                tailTransform.localRotation = initialTailRot * Quaternion.Euler(0, 0, tailSwing);
            }
        }

        private void AnimateSlimeBounce()
        {
            // Bouncy squash & stretch hop
            float cycle = Mathf.Repeat(walkTimer * bounceFrequency, Mathf.PI);
            float hop = Mathf.Sin(cycle);

            bodyTransform.localPosition = initialBodyPos + new Vector3(0, hop * bobHeight * 2.5f, 0);

            if (hop > 0.4f)
            {
                float factor = (hop - 0.4f) / 0.6f;
                float sx = Mathf.Lerp(1f, 1f / stretchY, factor);
                float sy = Mathf.Lerp(1f, stretchY, factor);
                bodyTransform.localScale = new Vector3(initialScale.x * sx, initialScale.y * sy, initialScale.z);
            }
            else
            {
                float factor = 1f - (hop / 0.4f);
                float sx = Mathf.Lerp(1f, squashX, factor);
                float sy = Mathf.Lerp(1f, 1f / squashX, factor);
                bodyTransform.localScale = new Vector3(initialScale.x * sx, initialScale.y * sy, initialScale.z);
            }
        }

        private void AnimateFlyingFlap()
        {
            float hover = Mathf.Sin(walkTimer * 3f) * flyBobHeight;
            bodyTransform.localPosition = initialBodyPos + new Vector3(0, hover, 0);

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
                bodyTransform.localPosition = initialBodyPos;
                bodyTransform.localRotation = Quaternion.identity;
                bodyTransform.localScale = initialScale;
                if (leftLegTransform != null) leftLegTransform.localRotation = initialLeftLegRot;
                if (rightLegTransform != null) rightLegTransform.localRotation = initialRightLegRot;
                if (leftArmTransform != null) leftArmTransform.localRotation = initialLeftArmRot;
                if (rightArmTransform != null) rightArmTransform.localRotation = initialRightArmRot;
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
            float duration = 0.28f;
            float elapsed = 0f;

            Transform attackArm = rightArmTransform != null ? rightArmTransform : weaponTransform;

            // Wind up
            while (elapsed < duration * 0.4f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.4f);
                bodyTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, -12f, t));
                if (attackArm != null)
                {
                    Quaternion baseRot = rightArmTransform != null ? initialRightArmRot : initialWeaponRot;
                    attackArm.localRotation = baseRot * Quaternion.Euler(0, 0, Mathf.Lerp(0, -45f, t));
                }
                yield return null;
            }

            // Lunge & Swing forward
            elapsed = 0f;
            while (elapsed < duration * 0.6f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.6f);
                bodyTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-12f, 25f, t));
                if (attackArm != null)
                {
                    Quaternion baseRot = rightArmTransform != null ? initialRightArmRot : initialWeaponRot;
                    attackArm.localRotation = baseRot * Quaternion.Euler(0, 0, Mathf.Lerp(-45f, 55f, t));
                }
                yield return null;
            }

            // Return to rest
            elapsed = 0f;
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.15f;
                bodyTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(25f, 0, t));
                if (attackArm != null)
                {
                    Quaternion baseRot = rightArmTransform != null ? initialRightArmRot : initialWeaponRot;
                    attackArm.localRotation = Quaternion.Lerp(attackArm.localRotation, baseRot, t);
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

            bodyTransform.localRotation = Quaternion.Euler(0, 0, -80f);
            bodyTransform.localPosition = initialBodyPos + new Vector3(0, -0.1f, 0);
            if (leftLegTransform != null) leftLegTransform.localRotation = initialLeftLegRot * Quaternion.Euler(0, 0, -35f);
            if (rightLegTransform != null) rightLegTransform.localRotation = initialRightLegRot * Quaternion.Euler(0, 0, 20f);
            if (leftArmTransform != null) leftArmTransform.localRotation = initialLeftArmRot * Quaternion.Euler(0, 0, -45f);
            if (rightArmTransform != null) rightArmTransform.localRotation = initialRightArmRot * Quaternion.Euler(0, 0, -30f);
        }

        public void SetupBiped(
            Transform body,
            Transform head,
            Transform leftLeg = null,
            Transform rightLeg = null,
            Transform leftArm = null,
            Transform rightArm = null,
            Transform weapon = null,
            Transform tail = null)
        {
            style = AnimationStyle.BipedWalk;
            bodyTransform = body;
            headTransform = head;
            leftLegTransform = leftLeg;
            rightLegTransform = rightLeg;
            leftArmTransform = leftArm;
            rightArmTransform = rightArm;
            weaponTransform = weapon;
            tailTransform = tail;
        }

        public void SetupSlime(Transform body)
        {
            style = AnimationStyle.SlimeBounce;
            bodyTransform = body;
        }

        public void SetupFlying(Transform body, Transform head, Transform leftWing, Transform rightWing)
        {
            style = AnimationStyle.FlyingFlap;
            bodyTransform = body;
            headTransform = head;
            leftWingTransform = leftWing;
            rightWingTransform = rightWing;
        }
    }
}
