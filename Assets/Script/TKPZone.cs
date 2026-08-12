using UnityEngine;

/// <summary>
/// Komponen untuk satu Titik Kejadian Perkara (TKP) / lokasi baru di kota.
/// Pasang script ini pada GameObject root tiap TKP (mis. "TKP_Minimarket").
/// Building model + collider penghalang diletakkan sebagai child dari root ini.
/// </summary>
public class TKPZone : MonoBehaviour
{
    [Header("Identitas TKP")]
    public string tkpId;          // unik, contoh: "minimarket", "kafe_senja", "gang_melati"
    public string tkpName;        // nama tampil, contoh: "Minimarket TKP"

    [Header("Status Area")]
    public bool startsUnlocked = true;   // false => area terkunci sampai dibuka lewat progres
    public GameObject buildingRoot;      // root model building (bisa null kalau collider ada di child langsung)

    [Header("Referensi")]
    public BoxCollider areaTrigger;      // BoxCollider IsTrigger yang membentuk zona masuk TKP

    public bool isUnlocked { get; private set; }
    public bool playerInside { get; private set; }

    // Event dipanggil saat player masuk/keluar (bisa di-subscribe UI/manager)
    public delegate void TKPPlayerEvent(TKPZone zone);
    public event TKPPlayerEvent OnPlayerEnter;
    public event TKPPlayerEvent OnPlayerExit;

    void Awake()
    {
        isUnlocked = startsUnlocked;
        if (areaTrigger == null)
            areaTrigger = GetComponentInChildren<BoxCollider>();

        // Pastikan trigger aktif hanya kalau area sudah dibuka
        if (areaTrigger != null)
            areaTrigger.enabled = isUnlocked;

        if (TKPManager.Instance != null)
            TKPManager.Instance.RegisterZone(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isUnlocked) return;
        if (!other.CompareTag("Player") && other.GetComponent<FirstPerson>() == null) return;

        playerInside = true;
        OnPlayerEnter?.Invoke(this);
        if (TKPManager.Instance != null)
            TKPManager.Instance.SetCurrentZone(this);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") && other.GetComponent<FirstPerson>() == null) return;

        if (playerInside)
        {
            playerInside = false;
            OnPlayerExit?.Invoke(this);
            if (TKPManager.Instance != null)
                TKPManager.Instance.ClearCurrentZone(this);
        }
    }

    /// <summary>Buka area ini (aktifkan trigger + tampilkan building).</summary>
    public void Unlock()
    {
        isUnlocked = true;
        if (areaTrigger != null) areaTrigger.enabled = true;
        if (buildingRoot != null) buildingRoot.SetActive(true);
        Debug.Log("[TKP] Area dibuka: " + tkpName);
    }

    /// <summary>Kunci area ini (matikan trigger + sembunyikan building).</summary>
    public void Lock()
    {
        isUnlocked = false;
        playerInside = false;
        if (areaTrigger != null) areaTrigger.enabled = false;
        if (buildingRoot != null) buildingRoot.SetActive(false);
    }
}
