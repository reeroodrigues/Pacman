using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Rewards
{
    public sealed class RewardService
    {
        private static readonly Lazy<RewardService> _lazy = new Lazy<RewardService>(() => new RewardService());
        public static RewardService I => _lazy.Value;
        public event Action<bool> OnConfigLoaded;
        private RewardService() { }

        public bool Enabled { get; set; } = false;
        public RewardConfig Config { get; private set; }

        public int LowStockThreshold { get; set; } = 10;
        public event Action<int> OnLowStock;
        public event Action<int> OnLowStockCleared;
        private bool _lowStockActive = false;

        private struct RuntimeBand { public float min, max; }
        private RuntimeBand[] _autoBands;
        
        private Dictionary<string, int> _runtimeStock;
        private Dictionary<string, int> _campaignStock;

        private string _configSignature;
        public string TopPrizeItemId = "labubu";
        private string SavePath => Path.Combine(Application.persistentDataPath, "RewardsDailyStock.json");
        private string CampaignPath => Path.Combine(Application.persistentDataPath, "RewardsCampaignStock.json");
        
        [Serializable] private class CampaignItemDTO { public string id; public int remaining; }

        public bool NoHasOtherPrizes { get; private set; } 
        [Serializable] private class CampaignDTO
        {
            public string configSignature;
            public CampaignItemDTO[] items;
        }

        public void LoadConfig(RewardConfig localAsset)
        {
            Config = localAsset;
            _autoBands = null;
            _configSignature = BuildConfigSignature(Config);

            BuildRuntimeStockFromConfig();
            LoadOrResetDailyStock();
            LoadOrInitCampaignStock();

            if (Config != null && Config.autoPercentBands)
                ComputeAutoBands();

            CheckLowStockAndMaybeFire(forceEmit: true);
        }

        public void ComputeAutoBands()
        {
            _autoBands = null;
            if (Config == null || Config.categories == null || Config.categories.Length == 0) return;

            int n = Config.categories.Length;
            _autoBands = new RuntimeBand[n];

            float[] catStock = new float[n];
            float total = 0f;
            for (int i = 0; i < n; i++)
            {
                var cat = Config.categories[i];
                float s = 0f;
                if (cat.items != null)
                    for (int j = 0; j < cat.items.Length; j++)
                        s += Mathf.Max(0, cat.items[j].initialDailyStock);
                catStock[i] = s;
                total += s;
            }

            float acc = 0f;
            if (total <= 0.00001f)
            {
                float width = 100f / n;
                for (int i = n - 1; i >= 0; i--)
                {
                    float min = acc, max = acc + width;
                    acc = max;
                    _autoBands[i] = new RuntimeBand { min = min, max = max };
                }
                _autoBands[0].max = 100f;
            }
            else
            {
                for (int i = n - 1; i >= 0; i--)
                {
                    float width = (catStock[i] / total) * 100f;
                    float min = acc, max = acc + width;
                    acc = max;
                    _autoBands[i] = new RuntimeBand { min = min, max = max };
                }
                _autoBands[0].max = 100f;
            }

            const float eps = 0.0001f;
            for (int i = 0; i < n; i++)
            {
                var b = _autoBands[i];
                b.min = Mathf.Clamp(b.min, 0f, 100f);
                b.max = Mathf.Clamp(b.max, 0f, 100f);
                if (b.max < b.min) b.max = b.min;
                if (i < n - 1 && Mathf.Abs(_autoBands[i + 1].min - b.max) < eps)
                    _autoBands[i + 1].min = b.max;
                _autoBands[i] = b;
            }
        }

        public RewardConfig.Category ChooseCategory(float percent)
        {
            if (Config == null || Config.categories == null || Config.categories.Length == 0) return null;

            int n = Config.categories.Length;
            if (percent >= 100f) return Config.categories[0];

            float p = Mathf.Max(0f, percent);
            const float eps = 0.0001f;

            if (Config.autoPercentBands)
            {
                if (_autoBands == null) ComputeAutoBands();
                if (_autoBands == null) return Config.categories[n - 1];

                for (int i = n - 1; i >= 0; i--)
                {
                    var b = _autoBands[i];
                    if (p + eps >= b.min && p <= b.max + eps) return Config.categories[i];
                }
                for (int i = 0; i < n; i++)
                    if (_autoBands[i].min <= p + eps) return Config.categories[i];

                return Config.categories[n - 1];
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    var band = Config.categories[i].manualBand;
                    Debug.Log($"[Rewards] Band[{i}] {Config.categories[i].name}: {band.min}..{band.max}");
                    if (p + eps >= band.min && p <= band.max + eps) return Config.categories[i];
                }
                for (int i = 0; i < n; i++)
                    if (Config.categories[i].manualBand.min <= p + eps) return Config.categories[i];

                return Config.categories[n - 1];
            }
        }

        private int GetCategoryIndex(RewardConfig.Category cat)
        {
            if (Config?.categories == null || cat == null) return -1;
            for (int i = 0; i < Config.categories.Length; i++)
                if (ReferenceEquals(Config.categories[i], cat)) return i;
            return -1;
        }

        private RewardConfig.Item DrawItemFromCategoryIndex(int catIndex)
        {
            if (Config == null || Config.categories == null) return null;
            if (catIndex < 0 || catIndex >= Config.categories.Length) return null;

            var cat = Config.categories[catIndex];
            if (cat?.items == null || cat.items.Length == 0) return null;

            EnsureRuntimeStock();

            List<RewardConfig.Item> pool = new List<RewardConfig.Item>(cat.items.Length);
            for (int i = 0; i < cat.items.Length; i++)
            {
                var it = cat.items[i];
                if (it == null || string.IsNullOrEmpty(it.id)) continue;
                if (IsIgnoredInEvaluation(it.id)) continue;

                if (_runtimeStock.TryGetValue(it.id, out int remaining) && remaining > 0)
                    pool.Add(it);
            }

            if (pool.Count == 0) return null;

            int idx = UnityEngine.Random.Range(0, pool.Count);
            return pool[idx];
        }
        
        private static bool IdEq(string a, string b) =>
            !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b) &&
            a.Equals(b, StringComparison.OrdinalIgnoreCase);
        
        private static bool IsIgnoredInEvaluation(string itemId)
        {
            return !string.IsNullOrEmpty(itemId)
                   && itemId.Equals("canetazero", StringComparison.OrdinalIgnoreCase);
        }
        
        private bool OnlyTopPrizeAvailable()
        {
            var cfg = Config;
            if (cfg == null || cfg.categories == null) return false;

            EnsureRuntimeStock();

            bool topHas = false;

            foreach (var c in cfg.categories)
            {
                var items = c?.items;
                if (items == null) continue;

                for (int i = 0; i < items.Length; i++)
                {
                    var it = items[i];
                    if (it == null || string.IsNullOrEmpty(it.id)) continue;
                    if (IsIgnoredInEvaluation(it.id)) continue; // ignora canetazero

                    _runtimeStock.TryGetValue(it.id, out var remaining);

                    if (IdEq(it.id, TopPrizeItemId))
                    {
                        if (remaining > 0) topHas = true; // labubu disponível
                    }
                    else
                    {
                        if (remaining > 0) return false; // ainda existe outro prêmio qualquer
                    }
                }
            }

            // True somente se NÃO há nenhum outro prêmio e o Labubu tem estoque (>0)
            return topHas;
        }
        
        private RewardConfig.Item FindEligibleInCategory(int catIndex)
        {
            if (Config == null || Config.categories == null) return null;
            if (catIndex < 0 || catIndex >= Config.categories.Length) return null;

            var cat = Config.categories[catIndex];
            var items = cat?.items;
            if (items == null || items.Length == 0) return null;

            EnsureRuntimeStock();

            for (int i = 0; i < items.Length; i++)
            {
                var it = items[i];
                if (it == null || string.IsNullOrEmpty(it.id)) continue;
                if (IsIgnoredInEvaluation(it.id)) continue;

                if (_runtimeStock.TryGetValue(it.id, out int remaining) && remaining > 0)
                    return it;
            }
            return null;
        }
        
        public RewardResult Evaluate(int score)
        {
            if (Config == null || Config.categories == null || Config.categories.Length == 0)
                return default;
            
            if (OnlyTopPrizeAvailable())
            {
                if (TryForceItem(TopPrizeItemId, out var rrTop))
                    return rrTop;
                // se não conseguiu (ex: 0 no labubu), segue o fluxo normal
            }

            var max = Mathf.Max(1, Config.maxScore);
            var percent = (score / (float)max) * 100f;

            Debug.Log($"[Rewards] Evaluate score={score} max={Config?.maxScore} percent={(score / (float)Mathf.Max(1, Config.maxScore))*100f:F6} auto={Config?.autoPercentBands}");
            
            var chosenCat = ChooseCategory(percent);
            if (chosenCat == null) return default;

            var startIndex = GetCategoryIndex(chosenCat);
            if (startIndex < 0) startIndex = 0;

            RewardConfig.Item item = null;
            var finalCatIndex = startIndex;

            // 1) tenta na categoria que pontuou
            item = FindEligibleInCategory(startIndex);

            // 2) tenta para frente (categorias de baixo)
            if (item == null)
            {
                for (int i = startIndex + 1; i < Config.categories.Length; i++)
                {
                    item = FindEligibleInCategory(i);
                    if (item != null) { finalCatIndex = i; break; }
                }
            }

            // 3) tenta para trás (categoria de cima)
            if (item == null)
            {
                for (int i = startIndex - 1; i >= 0; i--)
                {
                    item = FindEligibleInCategory(i);
                    if (item != null) { finalCatIndex = i; break; }
                }
            }

            if (item == null) return default;

            var finalCat = Config.categories[finalCatIndex];
            return new RewardResult
            {
                categoryId = finalCat.id,
                categoryName = finalCat.name,
                itemId = item?.id,
                itemName = item?.name,
                sprite = item?.sprite,
                percentUsed = percent
            };
        }

        public void Decrement(RewardResult result)
        {
            if (string.IsNullOrEmpty(result.itemId)) return;
            EnsureRuntimeStock();

            if (_runtimeStock.TryGetValue(result.itemId, out int remaining) && remaining > 0)
            {
                _runtimeStock[result.itemId] = remaining - 1;
                SaveDailyStock();
                CheckLowStockAndMaybeFire();
            }
        }

        public int TotalRemaining()
        {
            EnsureRuntimeStock();
            int total = 0;
            foreach (var kv in _runtimeStock) total += Mathf.Max(0, kv.Value);
            return total;
        }

        public int RemainingForItem(string itemId)
        {
            EnsureRuntimeStock();
            return (!string.IsNullOrEmpty(itemId) && _runtimeStock.TryGetValue(itemId, out int r)) ? Mathf.Max(0, r) : 0;
        }

        public void ResetRuntimeStock()
        {
            BuildRuntimeStockFromConfig();
            SaveDailyStock();
            CheckLowStockAndMaybeFire(forceEmit: true);
        }

        public void ForceResetToday()
        {
            BuildRuntimeStockFromConfig();
            SaveDailyStock();
            CheckLowStockAndMaybeFire(forceEmit: true);
        }

        private void BuildRuntimeStockFromConfig()
        {
            _runtimeStock = new Dictionary<string, int>(StringComparer.Ordinal);
            if (Config == null || Config.categories == null) return;

            foreach (var c in Config.categories)
            {
                if (c?.items == null) continue;
                foreach (var it in c.items)
                {
                    if (it == null || string.IsNullOrEmpty(it.id)) continue;
                    _runtimeStock[it.id] = Mathf.Max(0, it.initialDailyStock);
                }
            }
        }

        private void EnsureRuntimeStock()
        {
            if (_runtimeStock == null)
                BuildRuntimeStockFromConfig();
        }
        
        private void EnsureCampaignStock()
        {
            if (_campaignStock == null)
                LoadOrInitCampaignStock();
        }

        private void LoadOrInitCampaignStock()
        {
            try
            {
                _campaignStock = new Dictionary<string, int>(StringComparer.Ordinal);
                bool loadedFromFile = false;

                if (File.Exists(CampaignPath))
                {
                    var json = File.ReadAllText(CampaignPath, Encoding.UTF8);
                    var dto = JsonUtility.FromJson<CampaignDTO>(json);
                    if (dto != null && dto.items != null && dto.configSignature == _configSignature)
                    {
                        foreach (var it in dto.items)
                            if (!string.IsNullOrEmpty(it.id))
                                _campaignStock[it.id] = it.remaining;
                        loadedFromFile = true;
                    }
                }

                if (!loadedFromFile)
                {
                    if (Config != null && Config.categories != null)
                    {
                        foreach (var c in Config.categories)
                        {
                            if (c?.items == null) continue;
                            foreach (var it in c.items)
                            {
                                if (it == null || string.IsNullOrEmpty(it.id)) continue;
                                _campaignStock[it.id] = it.totalCampaignStock;
                            }
                        }
                    }
                }

                SaveCampaignStock();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"RewardService LoadOrInitCampaignStock falhou: {e.Message}");
                _campaignStock = new Dictionary<string, int>(StringComparer.Ordinal);
                if (Config != null && Config.categories != null)
                {
                    foreach (var c in Config.categories)
                    {
                        if (c?.items == null) continue;
                        foreach (var it in c.items)
                        {
                            if (it == null || string.IsNullOrEmpty(it.id)) continue;
                            _campaignStock[it.id] = it.totalCampaignStock;
                        }
                    }
                }
                SaveCampaignStock();
            }
        }

        private void SaveCampaignStock()
        {
            try
            {
                EnsureCampaignStock();
                var dto = new CampaignDTO { configSignature = _configSignature };

                var list = new List<CampaignItemDTO>(_campaignStock.Count);
                foreach (var kv in _campaignStock)
                    list.Add(new CampaignItemDTO { id = kv.Key, remaining = kv.Value });
                dto.items = list.ToArray();

                var json = JsonUtility.ToJson(dto, prettyPrint: true);
                File.WriteAllText(CampaignPath, json, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"RewardService SaveCampaignStock falhou: {e.Message}");
            }
        }

        [Serializable] private class ItemStockDTO { public string id; public int remaining; }
        [Serializable] private class DailyStockDTO { public string date; public string configSignature; public ItemStockDTO[] items; }

        private string TodayKey() => DateTime.Now.ToString("yyyy-MM-dd");

        private string BuildConfigSignature(RewardConfig cfg)
        {
            if (cfg == null || cfg.categories == null) return "null";
            var sb = new StringBuilder(1024);
            sb.Append("v2|max=").Append(cfg.maxScore)
              .Append("|auto=").Append(cfg.autoPercentBands ? "1" : "0")
              .Append("|cats=");
            for (int i = 0; i < cfg.categories.Length; i++)
            {
                var c = cfg.categories[i];
                if (c == null) continue;
                sb.Append($"[{c.id}|{c.name}|band:{c.manualBand.min}-{c.manualBand.max}|items:");
                if (c.items != null)
                {
                    for (int j = 0; j < c.items.Length; j++)
                    {
                        var it = c.items[j];
                        if (it == null) continue;
                        sb.Append($"({it.id}|{it.name}|{it.initialDailyStock}|{it.totalCampaignStock})");
                    }
                }
                sb.Append("]");
            }
            return sb.ToString();
        }

        private void LoadOrResetDailyStock()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    BuildRuntimeStockFromConfig();
                    SaveDailyStock();
                    return;
                }

                var json = File.ReadAllText(SavePath, Encoding.UTF8);
                var dto = JsonUtility.FromJson<DailyStockDTO>(json);

                var today = TodayKey();
                if (dto == null || string.IsNullOrEmpty(dto.date) || dto.date != today
                    || string.IsNullOrEmpty(dto.configSignature) || dto.configSignature != _configSignature)
                {
                    BuildRuntimeStockFromConfig();
                    SaveDailyStock();
                    return;
                }

                var map = new Dictionary<string, int>(StringComparer.Ordinal);
                if (dto.items != null)
                {
                    for (int i = 0; i < dto.items.Length; i++)
                    {
                        var it = dto.items[i];
                        if (it == null || string.IsNullOrEmpty(it.id)) continue;
                        map[it.id] = Mathf.Max(0, it.remaining);
                    }
                }

                foreach (var key in new List<string>(_runtimeStock.Keys))
                {
                    if (map.TryGetValue(key, out int persisted))
                        _runtimeStock[key] = Mathf.Max(0, persisted);
                }

                SaveDailyStock();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"RewardService LoadOrResetDailyStock falhou: {e.Message}");
                BuildRuntimeStockFromConfig();
                SaveDailyStock();
            }
        }

        private void SaveDailyStock()
        {
            try
            {
                EnsureRuntimeStock();
                var dto = new DailyStockDTO
                {
                    date = TodayKey(),
                    configSignature = _configSignature
                };

                var list = new List<ItemStockDTO>(_runtimeStock.Count);
                foreach (var kv in _runtimeStock)
                    list.Add(new ItemStockDTO { id = kv.Key, remaining = Mathf.Max(0, kv.Value) });
                dto.items = list.ToArray();

                var json = JsonUtility.ToJson(dto, prettyPrint: true);
                File.WriteAllText(SavePath, json, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"RewardService SaveDailyStock falhou: {e.Message}");
            }
        }
        
        public void SetCampaignRemaining(string itemId, int newRemaining)
        {
            EnsureCampaignStock();
            if (string.IsNullOrEmpty(itemId)) return;

            _campaignStock[itemId] = newRemaining;
            SaveCampaignStock();
        }

        public int CampaignRemainingForItem(string itemId)
        {
            EnsureCampaignStock();
            return (!string.IsNullOrEmpty(itemId) && _campaignStock.TryGetValue(itemId, out var r)) ? r : 0;
        }

        public int CampaignRemaining(string itemId) => CampaignRemainingForItem(itemId);

        public bool TopUpToday(string itemId, int quantity)
        {
            EnsureRuntimeStock();
            EnsureCampaignStock();

            if (string.IsNullOrEmpty(itemId)) return false;
            if (!_runtimeStock.ContainsKey(itemId)) _runtimeStock[itemId] = 0;
            if (!_campaignStock.TryGetValue(itemId, out var camp)) camp = 0;
            
            int allowed = quantity;

            _campaignStock[itemId] = camp - allowed;
            _runtimeStock[itemId] += allowed;

            SaveCampaignStock();
            SaveDailyStock();
            CheckLowStockAndMaybeFire();
            return true;
        }


        public bool SetTodayFromCampaign(string itemId, int newTodayValue)
        {
            EnsureRuntimeStock();
            EnsureCampaignStock();

            if (string.IsNullOrEmpty(itemId)) return false;

            _runtimeStock.TryGetValue(itemId, out var today);
            int delta = newTodayValue - today;
            if (delta <= 0)
            {
                _runtimeStock[itemId] = Mathf.Max(0, newTodayValue);
                SaveDailyStock();
                return true;
            }
            return TopUpToday(itemId, delta);
        }

        private void CheckLowStockAndMaybeFire(bool forceEmit = false)
        {
            int remaining = TotalRemaining();
            int threshold = Mathf.Max(0, LowStockThreshold);
            bool isLow = remaining <= threshold;

            if (forceEmit)
            {
                _lowStockActive = isLow;
                if (isLow) OnLowStock?.Invoke(remaining);
                else OnLowStockCleared?.Invoke(remaining);
                return;
            }

            if (isLow != _lowStockActive)
            {
                _lowStockActive = isLow;
                if (isLow) OnLowStock?.Invoke(remaining);
                else OnLowStockCleared?.Invoke(remaining);
            }
        }

        public void EmitCurrentLowStock()
        {
            CheckLowStockAndMaybeFire(forceEmit: true);
        }

        public struct RewardResult
        {
            public int categoryId;
            public string categoryName;
            public string itemId;
            public string itemName;
            public Sprite sprite;
            public float percentUsed;
        }

        public bool TryForceItem(string targetItemId, out RewardResult result)
        {
            result = default;
            if (Config == null || Config.categories == null || string.IsNullOrEmpty(targetItemId))
                return false;

            EnsureRuntimeStock();

            for (int i = 0; i < Config.categories.Length; i++)
            {
                var cat = Config.categories[i];
                var items = cat?.items;
                if (items == null) continue;

                for (int j = 0; j < items.Length; j++)
                {
                    var it = items[j];
                    if (it == null || string.IsNullOrEmpty(it.id)) continue;

                    if (string.Equals(it.id, targetItemId, StringComparison.OrdinalIgnoreCase))
                    {
                        if (RemainingForItem(it.id) <= 0) return false;

                        result = new RewardResult
                        {
                            categoryId = cat.id,
                            categoryName = cat.name,
                            itemId = it.id,
                            itemName = it.name,
                            sprite = it.sprite,
                            percentUsed = -1f
                        };
                        return true;
                    }
                }
            }
            return false;
        }

        public bool DecrementByItemId(string targetItemId)
        {
            if (TryForceItem(targetItemId, out var rr))
            {
                Decrement(rr);
                return true;
            }
            return false;
        }
        
        public int TotalRemainingForItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;

            int today = 0;
            if (_runtimeStock != null && _runtimeStock.TryGetValue(itemId, out var t))
                today = Mathf.Max(0, t);

            int camp = 0;
            if (_campaignStock != null && _campaignStock.TryGetValue(itemId, out var c))
                camp = c;

            return today + camp;
        }

        public int TotalRemainingAll()
        {
            int sum = 0;
            
            if (Config != null && Config.categories != null)
            {
                foreach (var c in Config.categories)
                {
                    if (c?.items == null) continue;
                    foreach (var it in c.items)
                    {
                        if (it == null || string.IsNullOrEmpty(it.id)) continue;
                        sum += TotalRemainingForItem(it.id);
                    }
                }
            }
            
            return sum;
        }

        public IEnumerator LoadConfigAsync(RewardConfig localAsset)
        {
            bool loadedRemote = false;

            if (localAsset != null && localAsset.useRemote && !string.IsNullOrEmpty(localAsset.remoteUrl))
            {
                using (var req = UnityWebRequest.Get(localAsset.remoteUrl))
                {
                    req.timeout = 6;
#if UNITY_2020_3_OR_NEWER
                    yield return req.SendWebRequest();
                    bool ok = req.result == UnityWebRequest.Result.Success;
#else
                    yield return req.SendWebRequest();
                    bool ok = !req.isNetworkError && !req.isHttpError;
#endif
                    if (ok && !string.IsNullOrEmpty(req.downloadHandler.text))
                    {
                        try
                        {
                            var sc = CreateRuntimeConfigFromJson(req.downloadHandler.text);
                            if (sc != null)
                            {
                                Config = sc;
                                _autoBands = null;
                                _configSignature = BuildConfigSignature(Config);

                                BuildRuntimeStockFromConfig();
                                LoadOrResetDailyStock();
                                LoadOrInitCampaignStock();

                                if (Config.autoPercentBands) ComputeAutoBands();
                                CheckLowStockAndMaybeFire(forceEmit: true);

                                loadedRemote = true;
                                OnConfigLoaded?.Invoke(true);
                                yield break;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"RewardService: JSON remoto inválido. {e.Message}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"RewardService: falha ao baixar config remota ({req.error})");
                    }
                }
            }

            LoadConfig(localAsset);
            OnConfigLoaded?.Invoke(false);
            yield break;
        }

        [Serializable] private class RemoteConfigDTO
        {
            public int maxScore = 370;
            public bool autoPercentBands = true;
            public RemoteCategoryDTO[] categories;
        }
        [Serializable] private class RemoteCategoryDTO
        {
            public int id = 1;
            public string name = "Categoria";
            public RewardConfig.Band manualBand;
            public RemoteItemDTO[] items;
        }
        [Serializable] private class RemoteItemDTO
        {
            public string id;
            public string name;
            public int initialDailyStock = 0;
            public int totalCampaignStock = 0;
        }

        private RewardConfig CreateRuntimeConfigFromJson(string json)
        {
            var dto = JsonUtility.FromJson<RemoteConfigDTO>(json);
            if (dto == null || dto.categories == null || dto.categories.Length == 0) return null;

            var sc = ScriptableObject.CreateInstance<RewardConfig>();
            sc.hideFlags = HideFlags.DontSave;
            sc.maxScore = Mathf.Max(1, dto.maxScore);
            sc.autoPercentBands = dto.autoPercentBands;
            sc.useRemote = true;

            sc.categories = new RewardConfig.Category[dto.categories.Length];
            for (int i = 0; i < dto.categories.Length; i++)
            {
                var cDto = dto.categories[i];
                var cat = new RewardConfig.Category
                {
                    id = cDto.id,
                    name = string.IsNullOrEmpty(cDto.name) ? $"Categoria {i + 1}" : cDto.name,
                    manualBand = cDto.manualBand,
                    items = new RewardConfig.Item[(cDto.items != null) ? cDto.items.Length : 0]
                };

                if (cDto.items != null)
                {
                    for (int j = 0; j < cDto.items.Length; j++)
                    {
                        var it = cDto.items[j];
                        cat.items[j] = new RewardConfig.Item
                        {
                            id = it.id,
                            name = it.name,
                            initialDailyStock = Mathf.Max(0, it.initialDailyStock),
                            totalCampaignStock = Mathf.Max(0, it.totalCampaignStock),
                            sprite = null
                        };
                    }
                }

                sc.categories[i] = cat;
            }
            return sc;
        }

#if UNITY_EDITOR
        public (float min, float max)[] GetComputedAutoBandsForDebug()
        {
            if (_autoBands == null) return null;
            var arr = new (float, float)[_autoBands.Length];
            for (int i = 0; i < _autoBands.Length; i++)
                arr[i] = (_autoBands[i].min, _autoBands[i].max);
            return arr;
        }
#endif
    }
}