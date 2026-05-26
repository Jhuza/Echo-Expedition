//Based on UnityStandardAssets.Utility.FPSCounter
//using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WorldSpaceTransitions
{
    [RequireComponent(typeof(Text))]
    public class FPS : MonoBehaviour
    {
        const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0;
        private int m_CurrentFps;

        private Text text;


        void Awake()
        {
            text = GetComponent<Text>();
        }

        private void Start()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        }


        void Update()
        {
            // measure average frames per second
            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;
                text.text = $"{m_CurrentFps} fps";
            }
        }
    }
}