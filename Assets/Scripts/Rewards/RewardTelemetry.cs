using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Rewards
{
    public class RewardTelemetry : MonoBehaviour
    {
        public static RewardTelemetry I { get; private set; }

        [SerializeField] private TelemetrySettings settings;

        private string _sessionId;
        private string _filePath;
        private readonly Queue<string> _pending = new Queue<string>();
        private float _nextFlushTime;

        void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);

            _sessionId = Guid.NewGuid().ToString("N");
            if (settings == null) { Debug.LogWarning("[Telemetry] Settings não atribuídos."); return; }

            _filePath = Path.Combine(Application.persistentDataPath, string.IsNullOrEmpty(settings.localFileName) ? "RewardsTelemetry.log" : settings.localFileName);
            _nextFlushTime = Time.realtimeSinceStartup + Mathf.Max(2f, settings.flushIntervalSeconds);
        }

        void Update()
        {
            if (settings == null || !settings.enableTelemetry) return;
            if (!settings.sendToServer || string.IsNullOrEmpty(settings.endpointUrl)) return;

            if (Time.realtimeSinceStartup >= _nextFlushTime && _pending.Count > 0)
            {
                _nextFlushTime = Time.realtimeSinceStartup + Mathf.Max(2f, settings.flushIntervalSeconds);
                StartCoroutine(FlushBatch());
            }
        }
        
        public static void LogRewardGranted(RewardService.RewardResult res, int stockBefore, int stockAfter, int totalRemaining, string cause)
        {
            if (I == null || I.settings == null || !I.settings.enableTelemetry) return;
            I.InternalLogReward(res, stockBefore, stockAfter, totalRemaining, cause);
        }
        
        [Serializable]
        private class RewardGrantedDTO
        {
            public string ts;
            public string session;
            public string scene;
            public string cause;

            public int score;
            public float percent;

            public int categoryId;
            public string categoryName;

            public string itemId;
            public string itemName;

            public int stockBefore;
            public int stockAfter;
            public int totalRemaining;

            public string appVersion;
            public string platform;
            public string deviceId;
        }

        private void InternalLogReward(RewardService.RewardResult res, int stockBefore, int stockAfter, int totalRemaining, string cause)
        {
            var dto = new RewardGrantedDTO
            {
                ts             = DateTime.UtcNow.ToString("o"),
                session        = _sessionId,
                scene          = SceneManager.GetActiveScene().name,
                cause          = cause ?? "unknown",

                score          = Mathf.RoundToInt((res.percentUsed / 100f) * Mathf.Max(1, (I != null && RewardService.I.Config != null ? RewardService.I.Config.maxScore : 370))),
                percent        = res.percentUsed,

                categoryId     = res.categoryId,
                categoryName   = res.categoryName,

                itemId         = res.itemId,
                itemName       = res.itemName,

                stockBefore    = stockBefore,
                stockAfter     = stockAfter,
                totalRemaining = totalRemaining,

                appVersion     = Application.version,
                platform       = Application.platform.ToString(),
                deviceId       = (settings.includeDeviceId ? SystemInfo.deviceUniqueIdentifier : null)
            };

            var json = JsonUtility.ToJson(dto, prettyPrint: false);
            AppendLineLocal(json);
            
            if (settings.sendToServer && !string.IsNullOrEmpty(settings.endpointUrl))
                _pending.Enqueue(json);
        }

        private void AppendLineLocal(string jsonLine)
        {
            try
            {
                using (var fs = new FileStream(_filePath, File.Exists(_filePath) ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                    sw.WriteLine(jsonLine);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Telemetry] Falha ao gravar JSONL: {e.Message}");
            }
        }

        private System.Collections.IEnumerator FlushBatch()
        {
            if (_pending.Count == 0) yield break;

            int count = Mathf.Min(settings.batchSize <= 0 ? 20 : settings.batchSize, _pending.Count);
            var arr = new string[count];
            for (int i = 0; i < count; i++) arr[i] = _pending.Dequeue();
            
            var payload = "[" + string.Join(",", arr) + "]";
            using (var req = new UnityWebRequest(settings.endpointUrl, UnityWebRequest.kHttpVerbPOST))
            {
                var body = Encoding.UTF8.GetBytes(payload);
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(settings.authHeaderKey) && !string.IsNullOrEmpty(settings.authHeaderValue))
                    req.SetRequestHeader(settings.authHeaderKey, settings.authHeaderValue);

                req.timeout = 8;
                yield return req.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
                bool ok = req.result == UnityWebRequest.Result.Success;
#else
                bool ok = !req.isNetworkError && !req.isHttpError;
#endif
                if (!ok)
                {
                    Debug.LogWarning($"[Telemetry] Falha POST ({req.responseCode}): {req.error}");
                    foreach (var line in arr) _pending.Enqueue(line);
                }
            }
        }
    }
}
