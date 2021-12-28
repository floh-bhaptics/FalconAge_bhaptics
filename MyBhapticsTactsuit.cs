using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;

namespace MyBhapticsTactsuit
{
    public class TactsuitVR
    {
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        private static ManualResetEvent HeartBeat_mrse = new ManualResetEvent(false);
        private static ManualResetEvent Tingle_mrse = new ManualResetEvent(false);
        private static ManualResetEvent Whip_mrse = new ManualResetEvent(false);
        public Dictionary<String, FileInfo> FeedbackMap = new Dictionary<String, FileInfo>();

        private static bHaptics.RotationOption defaultRotationOption = new bHaptics.RotationOption(0.0f, 0.0f);

        public void HeartBeatFunc()
        {
            while (true)
            {
                HeartBeat_mrse.WaitOne();
                bHaptics.SubmitRegistered("HeartBeat");
                Thread.Sleep(1000);
            }
        }

        public void TingleFunc()
        {
            while (true)
            {
                Tingle_mrse.WaitOne();
                bHaptics.SubmitRegistered("NeckTingleShort");
                Thread.Sleep(2050);
            }
        }

        public void WhipFunc()
        {
            while (true)
            {
                Whip_mrse.WaitOne();
                bHaptics.SubmitRegistered("Whip_R");
                Thread.Sleep(1050);
            }
        }

        public TactsuitVR()
        {
            LOG("Initializing suit");
            if (!bHaptics.WasError)
            {
                suitDisabled = false;
            }
            RegisterAllTactFiles();
            LOG("Starting HeartBeat and NeckTingle thread...");
            Thread HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.Start();
            Thread TingleThread = new Thread(TingleFunc);
            TingleThread.Start();
            Thread WhipThread = new Thread(WhipFunc);
            WhipThread.Start();
        }

        public void LOG(string logStr)
        {
#pragma warning disable CS0618 // Typ oder Element ist veraltet
            MelonLogger.Msg(logStr);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
        }



        void RegisterAllTactFiles()
        {
            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\bHaptics";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.tact", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                // LOG("Trying to register: " + prefix + " " + fullName);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    bHaptics.RegisterFeedbackFromTactFile(prefix, tactFileStr);
                    LOG("Pattern registered: " + prefix);
                }
                catch (Exception e) { LOG(e.ToString()); }

                FeedbackMap.Add(prefix, Files[i]);
            }
            systemInitialized = true;
            //PlaybackHaptics("HeartBeat");
        }

        public void PlaybackHaptics(String key, float intensity = 1.0f, float duration = 1.0f)
        {
            if (FeedbackMap.ContainsKey(key))
            {
                bHaptics.ScaleOption scaleOption = new bHaptics.ScaleOption(intensity, duration);
                bHaptics.SubmitRegistered(key, key, scaleOption, defaultRotationOption);
                // LOG("Playing back: " + key);
            }
            else
            {
                LOG("Feedback not registered: " + key);
            }
        }

        public void PlayBackHit(String key, float xzAngle, float yShift)
        {
            bHaptics.ScaleOption scaleOption = new bHaptics.ScaleOption(1f, 1f);
            bHaptics.RotationOption rotationOption = new bHaptics.RotationOption(xzAngle, yShift);
            bHaptics.SubmitRegistered(key, key, scaleOption, rotationOption);
        }

        public void GunRecoil(bool isRightHand, float intensity = 1.0f )
        {
            float duration = 1.0f;
            var scaleOption = new bHaptics.ScaleOption(intensity, duration);
            var rotationFront = new bHaptics.RotationOption(0f, 0f);
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }
            string keyArm = "Recoil" + postfix;
            string keyVest = "RecoilVest" + postfix;
            bHaptics.SubmitRegistered(keyArm, keyArm, scaleOption, rotationFront);
            bHaptics.SubmitRegistered(keyVest, keyVest, scaleOption, rotationFront);
        }
        public void SwordRecoil(bool isRightHand, float intensity = 1.0f)
        {
            float duration = 1.0f;
            var scaleOption = new bHaptics.ScaleOption(intensity, duration);
            var rotationFront = new bHaptics.RotationOption(0f, 0f);
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }
            string keyArm = "Sword" + postfix;
            string keyVest = "SwordVest" + postfix;
            bHaptics.SubmitRegistered(keyArm, keyArm, scaleOption, rotationFront);
            bHaptics.SubmitRegistered(keyVest, keyVest, scaleOption, rotationFront);
        }
        public void WhipPull(bool isRightHand, float intensity = 1.0f)
        {
            float duration = 1.0f;
            var scaleOption = new bHaptics.ScaleOption(intensity, duration);
            var rotationFront = new bHaptics.RotationOption(0f, 0f);
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }
            string keyArm = "WhipPull" + postfix;
            string keyVest = "WhipPullVest" + postfix;
            bHaptics.SubmitRegistered(keyArm, keyArm, scaleOption, rotationFront);
            bHaptics.SubmitRegistered(keyVest, keyVest, scaleOption, rotationFront);
        }

        public void BirdLand(bool isRightHand)
        {
            string key = "BirdLand";
            if (bHaptics.IsDeviceConnected(bHaptics.DeviceType.Tactosy_hands)) { key += "Hands"; }
            else { key += "Arms"; }
            if (isRightHand) { key += "_R"; }
            else { key += "_L"; }
            bHaptics.SubmitRegistered(key);
        }

        public void HeadShot(float hitAngle)
        {
            if (bHaptics.IsDeviceConnected(bHaptics.DeviceType.Tactal)) { PlaybackHaptics("HitInTheFace"); }
            else { PlayBackHit("BulletHit", hitAngle, 0.5f); }
        }

        public void StartHeartBeat()
        {
            HeartBeat_mrse.Set();
        }

        public void StopHeartBeat()
        {
            HeartBeat_mrse.Reset();
        }

        public void StartTingle()
        {
            Tingle_mrse.Set();
        }

        public void StopTingle()
        {
            Tingle_mrse.Reset();
        }
        public void StartWhip(bool isRightHand)
        {
            Whip_mrse.Set();
        }

        public void StopWhip()
        {
            Whip_mrse.Reset();
        }


        public bool IsPlaying(String effect)
        {
            return bHaptics.IsPlaying(effect);
        }

        public void StopHapticFeedback(String effect)
        {
            bHaptics.TurnOff(effect);
        }

        public void StopAllHapticFeedback()
        {
            StopThreads();
            foreach (String key in FeedbackMap.Keys)
            {
                bHaptics.TurnOff(key);
            }
        }

        public void StopThreads()
        {
            StopHeartBeat();
            StopTingle();
            StopWhip();
        }


    }
}
