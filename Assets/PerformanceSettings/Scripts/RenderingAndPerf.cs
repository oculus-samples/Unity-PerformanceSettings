// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Meta.PerformanceSettings
{
    public class RenderingAndPerf : MonoBehaviour
    {
        public Toggle TogglePrefab;

        public List<GameObject> ToShowIfDynRes;
        public List<GameObject> ToShowIfNoDynRes;
        public Slider MinDynResSlider, MaxDynResSlider;

        public RectTransform SetFramerateCapParent;
        public RectTransform SetFFRParent;
        public RectTransform SetMSAAParent;
        public RectTransform SetCPUPerfButtonsParent;
        public RectTransform SetGPUPerfButtonsParent;

        public List<GameObject> ToShowIfPassthrough;
        public OVRManager OVRManager;

        public GameObject GPUPushObjectPrefab;
        public BoxCollider[] GPUPushObjectSpawnLocs;
        public Transform GPUPushObjectsParent;
        public TMPro.TMP_Text CPUPushCounter, GPUPushCounter;
        private bool m_shouldPushCpu = false;
        public bool ShouldPushCpu { get => m_shouldPushCpu; set { if (value == m_shouldPushCpu) return; m_shouldPushCpu = value; } }

        private bool m_shouldMultithreadPushCpu = false;
        public bool ShouldMultithreadPushCpu { get => m_shouldMultithreadPushCpu; set { if (value == m_shouldMultithreadPushCpu) return; m_shouldMultithreadPushCpu = value; } }

        private float m_pushCpuMultiplier = 1;
        public float PushCpuMultiplier { get => m_pushCpuMultiplier; set { if (value == m_pushCpuMultiplier) return; m_pushCpuMultiplier = value; } }

        private int m_cpuPushVal = 0;
        private bool m_shouldPushGpu = false;
        public bool ShouldPushGpu { get => m_shouldPushGpu; set { if (value == m_shouldPushGpu) return; m_shouldPushGpu = value; } }

        private float m_pushGpuMultiplier = 0;
        public float PushGpuMultiplier { get => m_pushGpuMultiplier; set { if (value == m_pushGpuMultiplier) return; m_pushGpuMultiplier = value; } }

        public int CpuPushCycles_SingleThreaded => (int)(PushCpuMultiplier * 1E8);
        public int CpuPushCycles_MultiThreaded => (int)(PushCpuMultiplier * 1E4);
        public int CpuPush_MultiThreaded_JobCount => (int)1E4;

        private void Start()
        {
            Recalculate();
        }

        private void Recalculate()
        {
            for (var i = SetFramerateCapParent.childCount - 1; i >= 0; --i)
                Destroy(SetFramerateCapParent.GetChild(i).gameObject);

            for (var i = SetFFRParent.childCount - 1; i >= 0; --i)
                Destroy(SetFFRParent.GetChild(i).gameObject);

            for (var i = SetMSAAParent.childCount - 1; i >= 0; --i)
                Destroy(SetMSAAParent.GetChild(i).gameObject);

            for (var i = SetCPUPerfButtonsParent.childCount - 1; i >= 0; --i)
                Destroy(SetCPUPerfButtonsParent.GetChild(i).gameObject);

            for (var i = SetGPUPerfButtonsParent.childCount - 1; i >= 0; --i)
                Destroy(SetGPUPerfButtonsParent.GetChild(i).gameObject);

            var refreshRates = new List<float>(OVRManager.display.displayFrequenciesAvailable).Distinct();
            foreach (var refreshRate in refreshRates)
            {
                var refreshButton = Instantiate(TogglePrefab, SetFramerateCapParent);
                refreshButton.GetComponentInChildren<TMPro.TMP_Text>().text = refreshRate.ToString() + " Hz";
                refreshButton.SetIsOnWithoutNotify(OVRManager.display.displayFrequency == refreshRate);
                refreshButton.onValueChanged.AddListener((bool b) => { if (b) SetRefreshRate(refreshRate); });
            }

            foreach (OVRPlugin.FoveatedRenderingLevel ffrLevel in System.Enum.GetValues(typeof(OVRPlugin.FoveatedRenderingLevel)))
            {
                if (ffrLevel == OVRPlugin.FoveatedRenderingLevel.EnumSize) continue;

                var ffrButton = Instantiate(TogglePrefab, SetFFRParent);
                ffrButton.GetComponentInChildren<TMPro.TMP_Text>().text = ffrLevel.ToString();
                ffrButton.SetIsOnWithoutNotify(OVRPlugin.foveatedRenderingLevel == ffrLevel);
                ffrButton.onValueChanged.AddListener((bool b) => { if (b) SetFFRLevel(ffrLevel); });
            }

            var renderPipelineAsset = GetRenderPipelineAsset();
            foreach (UnityEngine.Rendering.Universal.MsaaQuality msaaQuality in System.Enum.GetValues(typeof(UnityEngine.Rendering.Universal.MsaaQuality)))
            {
                var msaaButton = Instantiate(TogglePrefab, SetMSAAParent);
                msaaButton.GetComponentInChildren<TMPro.TMP_Text>().text = ((int)msaaQuality).ToString() + 'x';
                msaaButton.SetIsOnWithoutNotify(renderPipelineAsset.msaaSampleCount == (int)msaaQuality);
                msaaButton.onValueChanged.AddListener((bool b) => { if (b) SetMSAAQuality(msaaQuality); });
            }

            var dynResEnabled = OVRManager.instance.enableDynamicResolution;
            foreach (var gameObject in ToShowIfDynRes)
                gameObject.SetActive(dynResEnabled);
            foreach (var gameObject in ToShowIfNoDynRes)
                gameObject.SetActive(!dynResEnabled);

            foreach (OVRPlugin.ProcessorPerformanceLevel perfLevel in System.Enum.GetValues(typeof(OVRPlugin.ProcessorPerformanceLevel)))
            {
                if (perfLevel == OVRPlugin.ProcessorPerformanceLevel.EnumSize) continue;

                var gpuButton = Instantiate(TogglePrefab, SetGPUPerfButtonsParent);
                gpuButton.GetComponentInChildren<TMPro.TMP_Text>().text = perfLevel.ToString();
                gpuButton.SetIsOnWithoutNotify(OVRPlugin.suggestedGpuPerfLevel == perfLevel);
                gpuButton.onValueChanged.AddListener((bool b) => { if (b) SetGPUPerfLevel(perfLevel); });

                var cpuButton = Instantiate(TogglePrefab, SetCPUPerfButtonsParent);
                cpuButton.GetComponentInChildren<TMPro.TMP_Text>().text = perfLevel.ToString();
                cpuButton.SetIsOnWithoutNotify(OVRPlugin.suggestedCpuPerfLevel == perfLevel);
                cpuButton.onValueChanged.AddListener((bool b) => { if (b) SetCPUPerfLevel(perfLevel); });
            }
        }

        private void Update()
        {
            if (m_shouldPushCpu)
            {
                if (m_shouldMultithreadPushCpu)
                {
                    var result = new NativeArray<int>(CpuPush_MultiThreaded_JobCount, Allocator.TempJob);

                    var pushCpuJob = new PushCPUJob()
                    {
                        Result = result,
                        NumCycles = CpuPushCycles_MultiThreaded
                    };

                    var jobHandle = pushCpuJob.Schedule(CpuPush_MultiThreaded_JobCount, 32);

                    jobHandle.Complete();
                    result.Dispose();
                }
                else
                {
                    for (var i = 0; i < CpuPushCycles_SingleThreaded; ++i)
                        if (i % 2 == 0)
                            m_cpuPushVal += i;
                        else
                            m_cpuPushVal -= i;
                }
            }
            CPUPushCounter.text = (m_shouldMultithreadPushCpu ? CpuPushCycles_SingleThreaded.ToString() : (CpuPushCycles_MultiThreaded * CpuPush_MultiThreaded_JobCount).ToString())
                + (m_shouldPushCpu ? "" : " (Disabled)");

            //push gpu
            //create new objects as needed...
            for (var i = GPUPushObjectsParent.childCount; i < (m_shouldPushGpu ? m_pushGpuMultiplier : 0); ++i)
            {
                var newObject = Instantiate(GPUPushObjectPrefab, GPUPushObjectsParent);
                var spawnVolume = GPUPushObjectSpawnLocs[Random.Range(0, GPUPushObjectSpawnLocs.Length)];
                newObject.transform.position = new Vector3(
                    Random.Range(spawnVolume.bounds.min.x, spawnVolume.bounds.max.x),
                    Random.Range(spawnVolume.bounds.min.y, spawnVolume.bounds.max.y),
                    Random.Range(spawnVolume.bounds.min.z, spawnVolume.bounds.max.z));
            }
            //delete existing objects as needed
            for (var i = GPUPushObjectsParent.childCount; i > (m_shouldPushGpu ? m_pushGpuMultiplier : 0); --i)
            {
                Destroy(GPUPushObjectsParent.GetChild(i - 1).gameObject);
            }
            GPUPushCounter.text = m_pushGpuMultiplier.ToString() + (m_shouldPushGpu ? "" : " (Disabled)");
        }

        private void SetRefreshRate(float newRefreshRate)
        {
            OVRManager.display.displayFrequency = newRefreshRate;
            _ = StartCoroutine(WaitAndRecalculate());
        }

        private void SetFFRLevel(OVRPlugin.FoveatedRenderingLevel ffrLevel)
        {
            OVRPlugin.foveatedRenderingLevel = ffrLevel;
            _ = StartCoroutine(WaitAndRecalculate());
        }

        public void ToggleDynamicFFR()
        {
            OVRPlugin.useDynamicFoveatedRendering = !OVRPlugin.useDynamicFoveatedRendering;
            _ = StartCoroutine(WaitAndRecalculate());
        }

        public void SetCPUPerfLevel(OVRPlugin.ProcessorPerformanceLevel perfLevel)
        {
            OVRPlugin.suggestedCpuPerfLevel = perfLevel;
            _ = StartCoroutine(WaitAndRecalculate());
        }

        public void SetGPUPerfLevel(OVRPlugin.ProcessorPerformanceLevel perfLevel)
        {
            OVRPlugin.suggestedGpuPerfLevel = perfLevel;
            _ = StartCoroutine(WaitAndRecalculate());
        }

        public void SetPassthroughEnabled(bool passthroughEnabled)
        {
            OVRManager.isInsightPassthroughEnabled = passthroughEnabled;
            foreach (var gameObject in ToShowIfPassthrough)
                gameObject.SetActive(passthroughEnabled);
        }

        public void SetDynamicResolutionEnabled(bool dynResEnabled)
        {
            try
            {
                OVRManager.instance.enableDynamicResolution = dynResEnabled;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            _ = StartCoroutine(WaitAndRecalculate());
        }

        public void SetMSAAQuality(UnityEngine.Rendering.Universal.MsaaQuality quality)
        {
            GetRenderPipelineAsset().msaaSampleCount = (int)quality;
            _ = StartCoroutine(WaitAndRecalculate());
        }

        public void SetRenderScale(float renderScale)
        {
            Debug.Assert(OVRManager.instance.enableDynamicResolution == false);
            TryAndSetEyeTextureResolution(renderScale);
            UnityEngine.XR.XRSettings.renderViewportScale = renderScale / UnityEngine.XR.XRSettings.eyeTextureResolutionScale;
        }

        public void SetMinDynamicResolutionScale(float minDynRes)
        {
            Debug.Assert(OVRManager.instance.enableDynamicResolution);
            TryAndSetEyeTextureResolution(minDynRes);
            OVRManager.instance.minDynamicResolutionScale = minDynRes / UnityEngine.XR.XRSettings.eyeTextureResolutionScale;
        }

        public void SetMaxDynamicResolutionScale(float maxDynRes)
        {
            Debug.Assert(OVRManager.instance.enableDynamicResolution);
            TryAndSetEyeTextureResolution(maxDynRes);
            OVRManager.instance.maxDynamicResolutionScale = maxDynRes / UnityEngine.XR.XRSettings.eyeTextureResolutionScale;
        }

        private void TryAndSetEyeTextureResolution(float renderScale)
        {
            if (UnityEngine.XR.XRSettings.eyeTextureResolutionScale < renderScale)
            {
                Debug.Log("Setting eye texture resolution from " + UnityEngine.XR.XRSettings.eyeTextureResolutionScale + " to " + renderScale);
                UnityEngine.XR.XRSettings.eyeTextureResolutionScale = renderScale;
                GetRenderPipelineAsset().renderScale = renderScale;
            }
        }

        private IEnumerator WaitAndRecalculate()
        {
            yield return new WaitForSeconds(0.1f);
            Recalculate();
        }

        private UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset GetRenderPipelineAsset()
        {
            return GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
        }
    }
}
