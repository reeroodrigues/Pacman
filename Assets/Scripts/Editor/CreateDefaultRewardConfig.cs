#if UNITY_EDITOR
using Rewards;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class CreateDefaultRewardConfig
    {
        [MenuItem("Rewards/Create Default RewardConfig")]
        public static void Create()
        {
            var asset = ScriptableObject.CreateInstance<RewardConfig>();
            asset.maxScore = 370;
            asset.autoPercentBands = true;

            asset.categories = new RewardConfig.Category[]
            {
                new RewardConfig.Category{
                    id=1, name="Categoria 1 (Top)",
                    manualBand = new RewardConfig.Band{ min=100f, max=100f },
                    items = new RewardConfig.Item[]{
                        new RewardConfig.Item{ id="bubulab", name="Bubulab", initialDailyStock=1 }
                    }
                },
                new RewardConfig.Category{
                    id=2, name="Categoria 2",
                    manualBand = new RewardConfig.Band{ min=82f, max=99f },
                    items = new RewardConfig.Item[]{
                        new RewardConfig.Item{ id="mochila", name="Mochila", initialDailyStock=20 },
                        new RewardConfig.Item{ id="estojo",  name="Estojo",  initialDailyStock=20 },
                        new RewardConfig.Item{ id="caneta",  name="Caneta",  initialDailyStock=25 },
                    }
                },
                new RewardConfig.Category{
                    id=3, name="Categoria 3 (Base)",
                    manualBand = new RewardConfig.Band{ min=0f, max=81f },
                    items = new RewardConfig.Item[]{
                        new RewardConfig.Item{ id="lapis",     name="Lápis",     initialDailyStock=50 },
                        new RewardConfig.Item{ id="borracha",  name="Borracha",  initialDailyStock=30 },
                    }
                }
            };

            var path = "Assets/RewardConfig.asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            Debug.Log($"RewardConfig criado em: {path}");
        }
    }
}
#endif
