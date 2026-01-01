using UnityEngine;

public static class VoxelData
{
    // ... (faceTriangles ve 6 adet FaceVertices dizisi DEÐÝÞMEDÝ) ...
    public static readonly int[] faceTriangles = new int[6] { 0, 1, 2, 0, 2, 3 };
    public static readonly Vector3[] TopFaceVertices = new Vector3[4] { new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0) };
    public static readonly Vector3[] BottomFaceVertices = new Vector3[4] { new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1) };
    public static readonly Vector3[] FrontFaceVertices = new Vector3[4] { new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1) };
    public static readonly Vector3[] BackFaceVertices = new Vector3[4] { new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) };
    public static readonly Vector3[] RightFaceVertices = new Vector3[4] { new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1) };
    public static readonly Vector3[] LeftFaceVertices = new Vector3[4] { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0) };

    // --- DOKU ATLASI (UV) AYARLARI (12x1) ---

    public static readonly float TextureAtlasWidth = 12f; // Hala 12x1
    public static readonly float NormalizedTextureWidth = 1f / TextureAtlasWidth; // Artýk 1/12

    // (Blok 1-8 Atlas 1-10 arasý)
    public static readonly Vector2[] DirtUVs = new Vector2[4] { new Vector2(NormalizedTextureWidth * 0f, 0.0f), new Vector2(NormalizedTextureWidth * 0f, 1.0f), new Vector2(NormalizedTextureWidth * 1f, 1.0f), new Vector2(NormalizedTextureWidth * 1f, 0.0f) };
    public static readonly Vector2[] GrassSideUVs = new Vector2[4] { new Vector2(NormalizedTextureWidth * 1f, 0.0f), new Vector2(NormalizedTextureWidth * 2f, 0.0f), new Vector2(NormalizedTextureWidth * 2f, 1.0f), new Vector2(NormalizedTextureWidth * 1f, 1.0f) };
    public static readonly Vector2[] GrassTopUVs = new Vector2[4] { new Vector2(NormalizedTextureWidth * 2f, 0.0f), new Vector2(NormalizedTextureWidth * 2f, 1.0f), new Vector2(NormalizedTextureWidth * 3f, 1.0f), new Vector2(NormalizedTextureWidth * 3f, 0.0f) };
    public static readonly Vector2[] CobblestoneUVs = new Vector2[4] { new Vector2(NormalizedTextureWidth * 3f, 0.0f), new Vector2(NormalizedTextureWidth * 4f, 0.0f), new Vector2(NormalizedTextureWidth * 4f, 1.0f), new Vector2(NormalizedTextureWidth * 3f, 1.0f) };
    public static readonly Vector2[] WoodTopUVs = new Vector2[4] { new Vector2(NormalizedTextureWidth * 4f, 0.0f), new Vector2(NormalizedTextureWidth * 4f, 1.0f), new Vector2(NormalizedTextureWidth * 5f, 1.0f), new Vector2(NormalizedTextureWidth * 5f, 0.0f) };
    public static readonly Vector2[] WoodSideUVs = new Vector2[4] { new Vector2(NormalizedTextureWidth * 5f, 0.0f), new Vector2(NormalizedTextureWidth * 6f, 0.0f), new Vector2(NormalizedTextureWidth * 6f, 1.0f), new Vector2(NormalizedTextureWidth * 5f, 1.0f) };
    public static readonly Vector2[] LeavesUVs = new Vector2[4] { new Vector2(NormalizedTextureWidth * 6f, 0.0f), new Vector2(NormalizedTextureWidth * 7f, 0.0f), new Vector2(NormalizedTextureWidth * 7f, 1.0f), new Vector2(NormalizedTextureWidth * 6f, 1.0f) };
    public static readonly Vector2[] FoliageUVs = new Vector2[4] { new Vector2(NormalizedTextureWidth * 7f, 0.0f), new Vector2(NormalizedTextureWidth * 7f, 1.0f), new Vector2(NormalizedTextureWidth * 8f, 1.0f), new Vector2(NormalizedTextureWidth * 8f, 0.0f) };
    public static readonly Vector2[] CoalOreUVs = new Vector2[4] { new Vector2(NormalizedTextureWidth * 8f, 0.0f), new Vector2(NormalizedTextureWidth * 9f, 0.0f), new Vector2(NormalizedTextureWidth * 9f, 1.0f), new Vector2(NormalizedTextureWidth * 8f, 1.0f) };
    public static readonly Vector2[] IronOreUVs = new Vector2[4] { new Vector2(NormalizedTextureWidth * 9f, 0.0f), new Vector2(NormalizedTextureWidth * 10f, 0.0f), new Vector2(NormalizedTextureWidth * 10f, 1.0f), new Vector2(NormalizedTextureWidth * 9f, 1.0f) };

    // --- YENÝ DOKULAR (11. ve 12. Atlas Slotlarý) ---

    // 11. DOKU: KUM (Blok ID 9) - (Atlas 10/12 -> 11/12)
    public static readonly Vector2[] SandUVs = new Vector2[4]
    {
        // (BL, BR, TR, TL) - Yan yüzey sýrasý
        new Vector2(NormalizedTextureWidth * 10f, 0.0f),
        new Vector2(NormalizedTextureWidth * 11f, 0.0f),
        new Vector2(NormalizedTextureWidth * 11f, 1.0f),
        new Vector2(NormalizedTextureWidth * 10f, 1.0f)
    };

    // 12. DOKU: ÇAKILTAÞI (Blok ID 10) - (Atlas 11/12 -> 12/12)
    public static readonly Vector2[] GravelUVs = new Vector2[4]
    {
        // (BL, BR, TR, TL) - Yan yüzey sýrasý
        new Vector2(NormalizedTextureWidth * 11f, 0.0f),
        new Vector2(1f, 0.0f), // * 12f yerine 1f (daha güvenli)
        new Vector2(1f, 1.0f), // * 12f yerine 1f (daha güvenli)
        new Vector2(NormalizedTextureWidth * 11f, 1.0f)
    };

    // GÜNCELLENMÝÞ PARTÝKÜL RENKLERÝ
    public static readonly Color[] blockColors = new Color[]
    {
        new Color(0f, 0f, 0f, 0f),     // 0: Hava
        new Color(0.5f, 0.35f, 0.2f),  // 1: Toprak
        new Color(0.35f, 0.5f, 0.15f), // 2: Çimen
        new Color(0.5f, 0.5f, 0.5f),  // 3: Kýrýk Taþ
        new Color(0.4f, 0.25f, 0.1f), // 4: Odun
        new Color(0.1f, 0.4f, 0.1f),   // 5: Yaprak
        new Color(0.7f, 0.15f, 0.15f), // 6: Çiçek
        new Color(0.2f, 0.2f, 0.2f),   // 7: Kömür
        new Color(0.6f, 0.4f, 0.25f),  // 8: Demir
        new Color(0.9f, 0.85f, 0.6f),  // 9: Kum (Açýk sarý/bej)
        new Color(0.5f, 0.5f, 0.55f),  // 10: Çakýltaþý (Kýrýk Taþtan biraz farklý gri)
        new Color(1f, 1f, 1f, 1f),     // 11: (Boþta)
        new Color(1f, 1f, 1f, 1f)      // 12: (Boþta)
    };
}