using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton yang melacak semua TKP di kota dan area mana yang sudah terbuka.
/// Player bisa "muter kota" bebas (FirstPerson) dan masuk ke TKP yang sudah dibuka.
/// </summary>
public class TKPManager : MonoBehaviour
{
    public static TKPManager Instance { get; private set; }

    [Header("Daftar TKP (otomatis terisi saat Awake)")]
    [SerializeField] private List<TKPZone> allZones = new List<TKPZone>();

    [Header("UI (opsional)")]
    public UnityEngine.UI.Text zoneLabel;   // text yang nunjukin TKP sedang dikunjungi

    public TKPZone currentZone { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterZone(TKPZone zone)
    {
        if (!allZones.Contains(zone))
            allZones.Add(zone);
    }

    public void SetCurrentZone(TKPZone zone)
    {
        currentZone = zone;
        UpdateLabel();
    }

    public void ClearCurrentZone(TKPZone zone)
    {
        if (currentZone == zone)
        {
            currentZone = null;
            UpdateLabel();
        }
    }

    /// <summary>Buka TKP berdasarkan id (mis. setelah quest selesai).</summary>
    public void UnlockTKP(string id)
    {
        var z = allZones.FirstOrDefault(x => x.tkpId == id);
        if (z != null) z.Unlock();
    }

    public bool IsUnlocked(string id)
    {
        var z = allZones.FirstOrDefault(x => x.tkpId == id);
        return z != null && z.isUnlocked;
    }

    public List<TKPZone> GetAllZones() => allZones;

    private void UpdateLabel()
    {
        if (zoneLabel == null) return;
        zoneLabel.text = currentZone != null
            ? "Lokasi: " + currentZone.tkpName
            : "Lokasi: (jalan kota)";
    }
}
