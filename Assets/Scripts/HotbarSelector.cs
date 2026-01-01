using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class HotbarSelector : MonoBehaviour
{
    [Header("Slot Dizisi")]
    [Tooltip("Hiyerarþi'deki 8 adet 'Slot_BG' objeni buraya sürükle")]
    public GameObject[] hotbarSlots;

    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 15.0f;

    private RectTransform selectorRect;
    private Vector3 targetPosition;

    // 'selectedIndex'i kaldýrmýþtýk, çünkü artýk BlockInteraction'da
    // private int selectedIndex = 0; // Bu satýrýn olmamasý lazým

    // --- DEÐÝÞÝKLÝK BURADA ---
    void Awake() // Start() -> Awake() olarak deðiþtirildi
    {
        selectorRect = GetComponent<RectTransform>();

        // Baþlangýç pozisyonunu ayarla
        if (hotbarSlots.Length > 0 && hotbarSlots[0] != null)
        {
            targetPosition = hotbarSlots[0].transform.position;
            selectorRect.position = targetPosition; // Anýnda baþla
        }
        else if (hotbarSlots.Length > 0 && hotbarSlots[0] == null)
        {
            Debug.LogError("HotbarSelector'daki 'Hotbar Slots' dizisinin 0. elemaný (Element 0) boþ (None)! Lütfen Inspector'dan atayýn.");
        }
    }
    // --- DEÐÝÞÝKLÝK BÝTTÝ ---

    void Update()
    {
        // GÝRÝÞ KONTROLÜ YOK

        // TEK GÖREVÝ: Hedefe doðru yumuþakça kaymak
        selectorRect.position = Vector3.Lerp(
            selectorRect.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    // KOMUT ALMA FONKSÝYONU
    public void MoveSelector(int index, bool instant = false)
    {
        if (index < 0 || index >= hotbarSlots.Length || hotbarSlots[index] == null)
        {
            // Eðer slot[index] boþsa (Inspector'da atanmamýþsa) hata vermemesi için
            //Debug.LogWarning($"HotbarSelector: {index} index'indeki slot atanmamýþ.");
            return;
        }

        // Yeni hedefi ayarla
        targetPosition = hotbarSlots[index].transform.position;

        if (instant)
        {
            // selectorRect'in Awake() sayesinde null OLMADIÐINDAN eminiz
            selectorRect.position = targetPosition;
        }
    }
}