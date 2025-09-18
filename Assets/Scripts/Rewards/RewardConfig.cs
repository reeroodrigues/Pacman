using UnityEngine;

namespace Rewards
{
    [CreateAssetMenu(menuName = "Rewards/Reward Config", fileName = "RewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        [Header("Geral")]
        [Tooltip("Pontuação máxima usada para calcular a % do jogador (score/maxScore*100).")]
        public int maxScore = 2770;

        [Tooltip("Se verdadeiro, as faixas de % são calculadas automaticamente pela proporção de estoque das categorias")]
        public bool autoPercentBands = true;

        [Tooltip("Usar configuração remota (JSON)")]
        public bool useRemote = false;

        [Tooltip("URL do JSON remoto.")]
        public string remoteUrl = "";

        [Space(8)]
        [Tooltip("Categorias ordenadas da MELHOR (topo) para a PIOR (base)")]
        public Category[] categories = new Category[0];

        [System.Serializable]
        public class Category
        {
            public int id = 1;
            public string name = "Categoria 1";

            [Tooltip("Itens disponíveis nesta categoria")]
            public Item[] items = new Item[0];

            [Tooltip("Faixa manual de % (usada quando autoPercentBands = false)")]
            public Band manualBand;
        }

        [System.Serializable]
        public class Item
        {
            public string id = "item_id";
            public string name = "Item Name";
            public Sprite sprite;
            
            [Tooltip("Estoque diário inicial para este item.")]
            [Min(0)] public int initialDailyStock = 0; // Campo para o estoque do DIA
            
            [Tooltip("Estoque total da campanha (stock geral).")]
            [Min(0)] public int totalCampaignStock = 0; // Campo para o estoque TOTAL
        }

        [System.Serializable]
        public struct Band
        {
            [Tooltip("Porcentagem mínima")]
            public float min;
            [Tooltip("Porcentagem máxima")]
            public float max;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (maxScore < 1) maxScore = 1;
            
            if (categories != null)
            {
                for (int i = 0; i < categories.Length; i++)
                {
                    var c = categories[i];
                    
                    if (c.manualBand.max < c.manualBand.min)
                    {
                        (c.manualBand.max, c.manualBand.min) = (c.manualBand.min, c.manualBand.max);
                    }
                    
                    if (c.items != null)
                    {
                        for (int j = 0; j < c.items.Length; j++)
                        {
                            if (c.items[j].initialDailyStock < 0)
                                c.items[j].initialDailyStock = 0;
                            
                            if (c.items[j].totalCampaignStock < 0)
                                c.items[j].totalCampaignStock = 0;
                        }
                    }
                }
            }
        }
#endif
    }
}