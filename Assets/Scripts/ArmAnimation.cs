using UnityEngine;
using System.Collections; // Coroutine'ler için bu gerekli

// Artýk bir RectTransform gerektirmiyor, çünkü script boþ bir objede olacak
public class ArmAnimation : MonoBehaviour
{
    [Header("Animasyon Hedefleri")]
    [Tooltip("Animasyonu yapýlacak olan El 'Image' objesinin RectTransform'u")]
    public RectTransform playerArmRect; // GÜNCELLENDÝ: Artýk GetComponent deðil, atanacak

    [Tooltip("Elin 'vurma' anýndaki zirve pozisyonu (Boþ bir obje)")]
    public RectTransform attackTarget;

    [Header("Animasyon Hýzý")]
    [Tooltip("Vuruþun hýzý. YÜKSEK = HIZLI vuruþ.")]
    public float animationSpeed = 10f;

    // --- Dahili Deðiþkenler ---
    private Vector3 idlePosition;       // Elin varsayýlan konumu
    private Quaternion idleRotation;    // Elin varsayýlan dönüþü

    private bool isAnimating = false; // Zaten bir vuruþ animasyonunda mý?

    // Awake() yerine Start() kullanmak, UI pozisyonlarýnýn oturmasý için daha güvenlidir
    void Start()
    {
        // 1. Gerekli referanslarýn atanýp atanmadýðýný kontrol et
        if (playerArmRect == null || attackTarget == null)
        {
            Debug.LogError("ArmAnimation: 'Player Arm Rect' veya 'Attack Target' Inspector'dan atanmamýþ!");
            this.enabled = false; // Script'i devre dýþý býrak
            return;
        }

        // 2. Baþlangýçtaki (dinlenme) pozisyonumuzu ve rotasyonumuzu hafýzaya al
        idlePosition = playerArmRect.position;
        idleRotation = playerArmRect.rotation;
    }

    // GÜNCELLENDÝ: Input (Giriþ) dinlemesi eklendi
    void Update()
    {
        // Eðer Sol Týk VEYA Sað Týk basýldýysa...
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // ...ve zaten bir animasyon oynamýyorsa
            if (!isAnimating)
            {
                // Animasyonu baþlat
                StartCoroutine(PunchAnimationSequence());
            }
        }
    }

    // --- ANÝMASYON KORUTÝNÝ (Coroutine) ---
    // Bu fonksiyon, animasyonu zamana yayarak yapar
    private IEnumerator PunchAnimationSequence()
    {
        isAnimating = true; // Animasyonu kilitle

        // --- 1. VURUÞ (ÝLERÝ GÝDÝÞ) ---

        Vector3 startPos = idlePosition;
        Quaternion startRot = idleRotation;
        Vector3 targetPos = attackTarget.position;
        Quaternion targetRot = attackTarget.rotation;

        float duration = 1f / (animationSpeed * 1.5f); // Gidiþ
        float time = 0;

        while (time < duration)
        {
            float t = time / duration;
            t = t * t; // "Savrulma" (Ease-In)

            // GÜNCELLENDÝ: 'armRect' yerine 'playerArmRect' kullanýlýyor
            playerArmRect.position = Vector3.Lerp(startPos, targetPos, t);
            playerArmRect.rotation = Quaternion.Slerp(startRot, targetRot, t);

            time += Time.deltaTime;
            yield return null;
        }

        playerArmRect.position = targetPos;
        playerArmRect.rotation = targetRot;

        // --- 2. GERÝ ÇEKME (ÝLK YERE DÖNÜÞ) ---

        time = 0;
        duration = 1f / animationSpeed; // Dönüþ

        startPos = playerArmRect.position; // Mevcut pozisyondan baþla
        startRot = playerArmRect.rotation;
        targetPos = idlePosition; // Dinlenme pozisyonuna dön
        targetRot = idleRotation;

        while (time < duration)
        {
            float t = time / duration;
            t = 1 - (1 - t) * (1 - t); // "Savrulma" (Ease-Out)

            playerArmRect.position = Vector3.Lerp(startPos, targetPos, t);
            playerArmRect.rotation = Quaternion.Slerp(startRot, targetRot, t);

            time += Time.deltaTime;
            yield return null;
        }

        playerArmRect.position = idlePosition;
        playerArmRect.rotation = idleRotation;

        isAnimating = false; // Kilidi aç
    }

    // 'TriggerPunch()' fonksiyonuna artýk gerek yok,
    // ama public olarak býrakmak test için faydalý olabilir.
    public void TriggerPunch()
    {
        if (!isAnimating)
        {
            StartCoroutine(PunchAnimationSequence());
        }
    }
}