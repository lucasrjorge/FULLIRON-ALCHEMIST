using TMPro;
using UnityEngine;

namespace Fulliron.Alchemy
{
    public class CalcinationStation : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] AlchemyDatabase database;
        [SerializeField] ItemSocket itemSocket;
        [SerializeField] SimpleButton pumpButton;
        [SerializeField] SimpleButton sealButton;
        [SerializeField] GaugeBar heatGauge;
        [SerializeField] TMP_Text statusText;
        [SerializeField] Light fireLight;
        [SerializeField] Transform residueSpawn;
        [SerializeField] AlchemicalItem residueItemPrefab;
        [SerializeField] GrimoireManager grimoire;

        [Header("Ajustes")]
        [SerializeField, Min(1f)] float maxHeat = 100f;
        [SerializeField, Min(0f)] float heatGain = 15f;
        [SerializeField, Min(0f)] float heatDecay = 8f;
        [SerializeField, Min(0.001f)] float exposureRate = 0.1f;
        [SerializeField, Range(0f, 1f)] float stage1 = 0.15f;
        [SerializeField, Range(0f, 1f)] float stage2 = 0.45f;
        [SerializeField, Range(0f, 1f)] float stage3 = 0.75f;
        [SerializeField, Range(0f, 1f)] float strongThreshold = 0.5f;

        float heat;
        float exposure;
        AlchemicalItem tracked;
        bool finished;

        float NormalizedHeat => maxHeat > 0f ? Mathf.Clamp01(heat / maxHeat) : 0f;

        void Start()
        {
            if (grimoire == null)
                grimoire = GrimoireManager.Instance;

            if (pumpButton != null)
                pumpButton.OnPressed.AddListener(AddHeat);
            if (sealButton != null)
                sealButton.OnPressed.AddListener(Seal);

            UpdateVisuals();
        }

        void OnDestroy()
        {
            if (pumpButton != null)
                pumpButton.OnPressed.RemoveListener(AddHeat);
            if (sealButton != null)
                sealButton.OnPressed.RemoveListener(Seal);
        }

        void Update()
        {
            heat = Mathf.Max(0f, heat - heatDecay * Time.deltaTime);

            var item = itemSocket != null ? itemSocket.CurrentItem : null;
            if (item == null)
            {
                tracked = null;
                exposure = 0f;
                finished = false;
                UpdateVisuals();
                return;
            }

            if (tracked != item)
            {
                tracked = item;
                exposure = 0f;
                finished = false;
            }

            if (!finished)
            {
                exposure += NormalizedHeat * exposureRate * Time.deltaTime;
                if (exposure >= 1f)
                {
                    exposure = 1f;
                    ApplyStage(true);
                }
            }

            UpdateVisuals();
        }

        public void AddHeat()
        {
            heat = Mathf.Min(maxHeat, heat + heatGain);
            PrototypeLog.Log($"Calcinatio: fole acionado. Calor = {heat:0}.");
        }

        public void Seal()
        {
            ApplyStage(false);
        }

        void ApplyStage(bool automatic)
        {
            if (tracked == null || tracked.Component == null)
            {
                SetStatus("Coloque um componente no cadinho.");
                return;
            }

            if (finished)
                return;

            if (database == null)
            {
                SetStatus("Banco de alquimia ausente.");
                return;
            }

            var profile = database.FindProfile(tracked.Component, tracked.State, ProcessType.Calcinatio);
            if (profile == null)
            {
                SetStatus("Este componente não tem perfil de Calcinatio.");
                PrototypeLog.Log($"Calcinatio: sem perfil para {tracked.DisplayName}.");
                return;
            }

            var outcome = profile.FindOutcome(exposure);
            if (outcome == null || outcome.output == null)
            {
                SetStatus("Nenhum resultado de Calcinatio para este estágio.");
                return;
            }

            if (outcome.outputState == ComponentState.Raw)
            {
                SetStatus("O componente ainda está cru.");
                return;
            }

            var input = tracked.Component;
            bool strong = NormalizedHeat >= strongThreshold;
            var residue = strong ? input.CalcinationResidueStrong : input.CalcinationResidueWeak;
            if (residue == null)
                residue = strong ? input.CalcinationResidueWeak : input.CalcinationResidueStrong;

            float intensity = NormalizedHeat;
            tracked.ApplyProcess(ProcessType.Calcinatio, intensity, outcome);
            finished = true;

            if (residue != null && residueItemPrefab != null && residueSpawn != null)
            {
                var residueItem = Instantiate(
                    residueItemPrefab,
                    residueSpawn.position + Random.insideUnitSphere * 0.03f,
                    residueSpawn.rotation);
                residueItem.SetData(residue, ComponentState.Residue);
                grimoire?.DiscoverComponent(residue);
                PrototypeLog.Log($"Calcinatio: resíduo gerado: {residue.DisplayName}.");
            }

            grimoire?.DiscoverComponent(outcome.output);
            SetStatus((automatic ? "Passou do ponto: " : "Selado: ") + outcome.output.DisplayName + ". Retire o componente.");
        }

        void UpdateVisuals()
        {
            float normalized = NormalizedHeat;

            if (heatGauge != null)
                heatGauge.SetValue(normalized, $"Calor {heat:0}\n{StageName()}");

            if (fireLight != null)
            {
                fireLight.intensity = 0.2f + normalized * 3f;
                fireLight.color = Color.Lerp(new Color(1f, 0.35f, 0.08f), new Color(1f, 0.8f, 0.35f), normalized);
            }

            if (statusText == null)
                return;

            if (tracked == null || tracked.Component == null)
            {
                statusText.text = "Coloque um componente no cadinho.\nBOMBEAR aquece; SELAR recolhe.";
                return;
            }

            if (finished)
                return;

            if (database != null && database.FindProfile(tracked.Component, tracked.State, ProcessType.Calcinatio) == null)
            {
                statusText.text = "Sem perfil de Calcinatio para " + tracked.DisplayName + ".";
                return;
            }

            if (exposure < 0.001f && heat < 1f)
            {
                statusText.text = "BOMBEAR para aquecer.\n" + StageName();
                return;
            }

            statusText.text = StageName() + "\nExposição " + (exposure * 100f).ToString("0") + "%";
        }

        string StageName()
        {
            if (exposure < stage1) return "Cru";
            if (exposure < stage2) return "Calcinado";
            if (exposure < stage3) return "Supercalcinado";
            return "Cinza";
        }

        void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
            PrototypeLog.Log("Calcinatio: " + message);
        }
    }
}
