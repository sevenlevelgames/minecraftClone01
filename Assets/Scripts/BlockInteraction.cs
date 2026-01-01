using UnityEngine;
// using TMPro; // (Kaldýrmýþtýk)

public class BlockInteraction : MonoBehaviour
{
    [Header("Referanslar")]
    public WorldGenerator world;
    public float interactionDistance = 5f;

    [Header("Hotbar Sistemi")]
    public HotbarSelector hotbarSelector;

    // GÜNCELLENDÝ: 7. slot (index 6) = 9 (Kum)
    // 8. slot (index 7) = 10 (Çakýltaþý)
    [SerializeField] private byte[] hotbarSlotContents = new byte[8] { 2, 1, 3, 9, 10, 4, 7, 8 };
    // Sýralama: Çimen, Toprak, Taþ, KUM, ÇAKIL, Odun, Kömür, Demir

    // --- Dahili Deðiþkenler ---
    private Camera playerCamera;
    private int selectedIndex = 0;
    private byte selectedBlockType = 2;

    void Start()
    {
        playerCamera = GetComponent<Camera>();

        selectedIndex = 0;
        selectedBlockType = hotbarSlotContents[selectedIndex];

        if (hotbarSelector != null)
        {
            hotbarSelector.MoveSelector(selectedIndex, true);
        }
    }

    void Update()
    {
        HandleHotbarSelection();

        if (Input.GetMouseButtonDown(0)) { HandleRaycast(false); }
        if (Input.GetMouseButtonDown(1)) { HandleRaycast(true); }
    }

    // GÝRÝÞ MANTIÐI (DEÐÝÞMEDÝ - 8 TUÞU ZATEN DÝNLÝYORDU)
    void HandleHotbarSelection()
    {
        int newIndex = selectedIndex;
        bool inputDetected = false;

        if (Input.GetKeyDown(KeyCode.Alpha1)) { newIndex = 0; inputDetected = true; }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { newIndex = 1; inputDetected = true; }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { newIndex = 2; inputDetected = true; }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) { newIndex = 3; inputDetected = true; }
        else if (Input.GetKeyDown(KeyCode.Alpha5)) { newIndex = 4; inputDetected = true; }
        else if (Input.GetKeyDown(KeyCode.Alpha6)) { newIndex = 5; inputDetected = true; }
        else if (Input.GetKeyDown(KeyCode.Alpha7)) { newIndex = 6; inputDetected = true; }
        else if (Input.GetKeyDown(KeyCode.Alpha8)) { newIndex = 7; inputDetected = true; }

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput > 0f) { newIndex++; inputDetected = true; }
        else if (scrollInput < 0f) { newIndex--; inputDetected = true; }

        if (inputDetected)
        {
            int maxIndex = hotbarSlotContents.Length - 1;
            if (newIndex > maxIndex) { newIndex = 0; }
            if (newIndex < 0) { newIndex = maxIndex; }

            if (newIndex != selectedIndex)
            {
                selectedIndex = newIndex;
                selectedBlockType = hotbarSlotContents[selectedIndex];

                if (hotbarSelector != null)
                {
                    hotbarSelector.MoveSelector(selectedIndex, false);
                }
            }
        }
    }
    // Blok Kýrma/Koyma (GÜNCELLENDÝ: GetBlockID kullanýr)
    // Blok Kýrma/Koyma (GÜNCELLENDÝ: Yerleþtirirken de yerçekimini tetikler)
    void HandleRaycast(bool isPlacing)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            Vector3 targetPoint;
            byte blockType;
            Vector3Int blockPos;

            if (isPlacing)
            {
                // --- BLOK KOYMA ---
                targetPoint = hit.point + hit.normal * 0.01f;
                blockPos = Vector3Int.FloorToInt(targetPoint);
                blockType = selectedBlockType;
            }
            else
            {
                // --- BLOK KIRMA ---
                targetPoint = hit.point - hit.normal * 0.01f;
                blockPos = Vector3Int.FloorToInt(targetPoint);

                // Partikül Kodu
                byte blockID = world.GetBlockID(blockPos);
                if (blockID > 0)
                {
                    world.SpawnBreakEffect(blockPos, blockID);
                }

                blockType = 0; // Kýrma
            }

            // 1. ÖNCE bloðu deðiþtir/kýr (Veriyi güncelle)
            world.SetBlock(blockPos, blockType);

            // 2. SONRA yerçekimini kontrol et
            if (isPlacing == false)
            {
                // Eðer blok KIRDIYSAK, üstündeki sütunu kontrol et
                world.CheckForFallingBlocks(blockPos);
            }
            // --- YENÝ EKLENEN KISIM ---
            else if (blockType == 9 || blockType == 10) // Kum(9) veya Çakýl(10) MI YERLEÞTÝRDÝK?
            {
                // Eðer Kum/Çakýl yerleþtirdiysek, YERLEÞTÝRDÝÐÝMÝZ bloðun kendisini kontrol et
                world.StartFalling(blockPos, blockType);
            }
            // --- YENÝ KISMIN SONU ---
        }
    }
}