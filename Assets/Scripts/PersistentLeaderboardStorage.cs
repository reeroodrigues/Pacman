using System;
using System.Collections.Generic;
using Tools.Leaderboard.Models;
using Tools.Leaderboard.Storage;
using UnityEngine;

public class PersistentLeaderboardStorage : ILeaderboardStorage
{
    private readonly string _saveKey;

    public PersistentLeaderboardStorage(string saveKey)
    {
        _saveKey = saveKey;
    }

    public void SaveLeaderboard(List<LeaderboardEntry> entries)
    {
        var serializableEntries = new List<SerializableLeaderboardEntry>();
        
        foreach (var entry in entries)
        {
            serializableEntries.Add(new SerializableLeaderboardEntry(entry));
        }
        
        var data = new SerializableLeaderboardData { entries = serializableEntries };
        var json = JsonUtility.ToJson(data, true);
        
        PlayerPrefs.SetString(_saveKey, json);
        PlayerPrefs.Save();
        
        Debug.Log($"[PersistentLeaderboardStorage] Saved {entries.Count} entries to PlayerPrefs with key '{_saveKey}'");
    }

    public List<LeaderboardEntry> LoadLeaderboard()
    {
        var entries = new List<LeaderboardEntry>();
        
        if (PlayerPrefs.HasKey(_saveKey))
        {
            var json = PlayerPrefs.GetString(_saveKey);
            Debug.Log($"[PersistentLeaderboardStorage] Loading from PlayerPrefs with key '{_saveKey}'");
            
            var data = JsonUtility.FromJson<SerializableLeaderboardData>(json);
            
            if (data != null && data.entries != null)
            {
                foreach (var serializableEntry in data.entries)
                {
                    entries.Add(serializableEntry.ToLeaderboardEntry());
                }
                
                Debug.Log($"[PersistentLeaderboardStorage] Loaded {entries.Count} entries from PlayerPrefs");
            }
            else
            {
                Debug.LogWarning("[PersistentLeaderboardStorage] Failed to deserialize data from PlayerPrefs");
            }
        }
        else
        {
            Debug.Log($"[PersistentLeaderboardStorage] No data found in PlayerPrefs with key '{_saveKey}'");
        }
        
        return entries;
    }

    public void ClearLeaderboard()
    {
        PlayerPrefs.DeleteKey(_saveKey);
        PlayerPrefs.Save();
        Debug.Log($"[PersistentLeaderboardStorage] Cleared leaderboard data from PlayerPrefs");
    }

    [Serializable]
    private class SerializableLeaderboardData
    {
        public List<SerializableLeaderboardEntry> entries = new List<SerializableLeaderboardEntry>();
    }

    [Serializable]
    private class SerializableLeaderboardEntry
    {
        public string playerId;
        public string playerName;
        public string email;
        public string phoneNumber;
        public int score;
        public string timeStamp;
        public int rank;
        public string eventId;

        public SerializableLeaderboardEntry() { }

        public SerializableLeaderboardEntry(LeaderboardEntry entry)
        {
            playerId = entry.PlayerId;
            playerName = entry.PlayerName;
            email = entry.Email;
            phoneNumber = entry.PhoneNumber;
            score = entry.Score;
            timeStamp = entry.TimeStamp.ToString("o");
            rank = entry.Rank;
            eventId = entry.EventId;
        }

        public LeaderboardEntry ToLeaderboardEntry()
        {
            var entry = new LeaderboardEntry
            {
                PlayerId = playerId,
                PlayerName = playerName,
                Email = email,
                PhoneNumber = phoneNumber,
                Score = score,
                Rank = rank,
                EventId = eventId
            };

            if (DateTime.TryParse(timeStamp, out DateTime parsedTime))
            {
                entry.TimeStamp = parsedTime;
            }
            else
            {
                entry.TimeStamp = DateTime.UtcNow;
            }

            return entry;
        }
    }
}
