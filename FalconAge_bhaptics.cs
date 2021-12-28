using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;
using HarmonyLib;
using UnityEngine;

using MyBhapticsTactsuit;


namespace FalconAge_bhaptics
{
    public class FalconAge_bhaptics : MelonMod
    {
        public static TactsuitVR tactsuitVr;
        public static bool whipHandIsRight = true;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }

        #region Explosions
        [HarmonyPatch(typeof(ExploderBase), "Explode")]
        public class bhaptics_BaseExplosion
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("ExplosionBelly");
            }
        }

        [HarmonyPatch(typeof(DroneEnemy), "OnExplodeTimer")]
        public class bhaptics_DroneExplosion
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("ExplosionBelly");
            }
        }
/*
        [HarmonyPatch(typeof(DroneWander), "OnDefeated")]
        public class bhaptics_DroneWanderExplosion
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("ExplosionBelly");
            }
        }
*/
        [HarmonyPatch(typeof(GroundEnemySpider), "OnBombExploded")]
        public class bhaptics_SpiderExplosion
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("ExplosionBelly");
            }
        }

        [HarmonyPatch(typeof(HeavySentryProjectile), "Explode")]
        public class bhaptics_HeavySentryExplosion
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("ExplosionBelly");
            }
        }
        #endregion

        #region Melee combat

        [HarmonyPatch(typeof(WhipTargetGenericEvents), "WhipPullInternal")]
        public class bhaptics_WhipPull
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.WhipPull(whipHandIsRight);
                tactsuitVr.StopWhip();
            }
        }

        [HarmonyPatch(typeof(WhipRope), "SetEndPoints")]
        public class bhaptics_WhipRope
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StopWhip();
                //tactsuitVr.LOG("Noise: " + __instance.noiseAmplitudeScale.ToString());
            }
        }

        [HarmonyPatch(typeof(PlayerToolMelee), "OnTriggerEnter")]
        public class bhaptics_Melee
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerToolMelee __instance, Collider coll)
            {
                string[] excludedTriggers = { "WhistleNearFace", "HeadLockArea", "PouchMotionTrigger", "MeleeTrigger", "Tools", "Coll", "Whistle" };
                string myCollider;
                bool rightHand = true;
                try
                {
                    rightHand = !__instance.myHand.IsLeftHand;
                }
                catch { tactsuitVr.LOG("Hand not found!"); return; }
                try
                {
                    myCollider = coll.attachedRigidbody.name;
                }
                catch { myCollider = "Invalid"; }
                if (excludedTriggers.Any(myCollider.Contains)) { return; }
                whipHandIsRight = rightHand;
                tactsuitVr.SwordRecoil(rightHand);
            }
        }

        #endregion

        #region World interactions
        [HarmonyPatch(typeof(LandedBehaviour), "OnLanded")]
        public class bhaptics_BirdLand
        {
            [HarmonyPostfix]
            public static void Postfix(LandedBehaviour __instance, FlyToTarget landTarget)
            {
                bool landedOnHand = false;
                bool fingerLandings = false;
                bool isRightHand = false;
                try
                {
                    if (landTarget.landTrigger == "landend_hand") { landedOnHand = true; }
                    fingerLandings = __instance.UsesFingerLandings;
                }
                catch { tactsuitVr.LOG("Landed nowhere?"); return; }
                if (landedOnHand) { tactsuitVr.BirdLand(isRightHand); }
            }
        }

        [HarmonyPatch(typeof(PlayerEatableObject), "EatObject")]
        public class bhaptics_PlayerEats
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Eating");
            }
        }
        #endregion

        #region Health threads

        [HarmonyPatch(typeof(PlayerDamageable), "Update")]
        public class bhaptics_UpdatePlayerHealth
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerDamageable __instance)
            {
                float currentHealth;
                try { currentHealth = __instance.CurrentHealth; }
                catch { return; }
                if (currentHealth == 0f) { tactsuitVr.StopHeartBeat(); return; }
                //tactsuitVr.LOG("Player health: " + currentHealth.ToString());
                if (currentHealth <= 0.4f) { tactsuitVr.StartHeartBeat(); }
                else { tactsuitVr.StopHeartBeat(); }
            }
        }

        [HarmonyPatch(typeof(BirdDamageable), "Update")]
        public class bhaptics_UpdateBirdHealth
        {
            [HarmonyPostfix]
            public static void Postfix(BirdDamageable __instance)
            {
                bool birdExhausted = false;
                float birdHealth = 1.0f;
                int needles = 0;
                try
                {
                    birdExhausted = __instance.Exhausted;
                    birdHealth = __instance.currentHealthFraction;
                    needles = __instance.needleCount;
                }
                catch { return; }
                if ((birdExhausted) | (birdHealth <= 0.25f) | (needles >= 1))
                { tactsuitVr.StartTingle(); }
                else { tactsuitVr.StopTingle(); }
            }
        }

        [HarmonyPatch(typeof(PersistentStats), "IncrementHealthBits")]
        public class bhaptics_IncrementHealth
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Healing");
            }
        }

        [HarmonyPatch(typeof(PlayerDamageable), "KillPlayer")]
        public class bhaptics_KillPlayer
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StopThreads();
            }
        }

        [HarmonyPatch(typeof(PlayerAutoDamage), "DamagePlayer")]
        public class bhaptics_DamagePlayer
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("Impact");
                //tactsuitVr.LOG("DamagePlayer");
            }
        }
        #endregion
    
    }
}
