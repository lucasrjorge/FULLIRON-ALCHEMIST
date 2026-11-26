using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fulliron.Alchemy
{
    [Serializable]
    public class ProcessStep
    {
        public ProcessType process;
        [Range(0f, 1f)] public float intensity;
        [Min(0f)] public float duration;
        public string outputId;
        public float time;
    }

    [Serializable]
    public class ProcessHistory
    {
        [SerializeField] List<ProcessStep> steps = new List<ProcessStep>();

        public IReadOnlyList<ProcessStep> Steps
        {
            get
            {
                EnsureInitialized();
                return steps;
            }
        }

        public void Add(ProcessType process, float intensity, float duration, string outputId)
        {
            EnsureInitialized();
            steps.Add(new ProcessStep
            {
                process = process,
                intensity = Mathf.Clamp01(intensity),
                duration = Mathf.Max(0f, duration),
                outputId = outputId,
                time = Time.time
            });
        }

        public void Clear()
        {
            EnsureInitialized();
            steps.Clear();
        }

        void EnsureInitialized()
        {
            if (steps == null)
                steps = new List<ProcessStep>();
        }
    }
}
