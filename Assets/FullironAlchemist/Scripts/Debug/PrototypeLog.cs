using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fulliron.Alchemy
{
    public class PrototypeLog : MonoBehaviour
    {
        public static PrototypeLog Instance { get; private set; }

        [SerializeField] TMP_Text display;
        [SerializeField, Min(1)] int maxLines = 10;

        readonly List<string> lines = new List<string>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public static void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Debug.Log("[Fulliron] " + message);

            if (Instance != null)
                Instance.Append(line);
        }

        void Append(string line)
        {
            lines.Add(line);
            while (lines.Count > maxLines)
                lines.RemoveAt(0);

            if (display != null)
                display.text = string.Join("\n", lines);
        }

        [ContextMenu("Limpar log")]
        public void Clear()
        {
            lines.Clear();
            if (display != null)
                display.text = string.Empty;
        }
    }
}
