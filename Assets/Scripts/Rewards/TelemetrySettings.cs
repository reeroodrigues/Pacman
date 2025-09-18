using UnityEngine;

namespace Rewards
{
    [CreateAssetMenu(menuName = "Rewards/Telemetry Settings", fileName = "TelemetrySettings")]
    public class TelemetrySettings : ScriptableObject
    {
        [Header("Geral")]
        public bool enableTelemetry = true;

        [Tooltip("Arquivo JSONL armazenado localmente (em Application.persistentDataPath).")]
        public string localFileName = "RewardsTelemetry.log";

        [Header("Envio HTTP (opcional)")]
        public bool sendToServer = false;
        public string endpointUrl = "";
        public float flushIntervalSeconds = 10f;
        public int batchSize = 20;
        public string authHeaderKey = "Authorization";
        public string authHeaderValue = "";

        [Header("Campos extras (opcionais)")]
        public bool includeDeviceId = false;
    }
}