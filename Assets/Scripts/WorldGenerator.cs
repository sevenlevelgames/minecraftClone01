using UnityEngine;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    [Header("Chunk Ayarlarý")]
    public int chunkSize = 16;
    public int chunkHeight = 128;
    [Tooltip("Oyuncunun etrafýnda kaç chunk'lýk bir kare oluþturulacaðý.")]
    public int renderDistance = 4;

    [Header("Arazi Ayarlarý (Global)")]
    public int surfaceLevelY = 64;
    public int terrainDepth = 30;
    public int hillHeight = 10;
    public int dirtLayerDepth = 5;
    public float noiseScale = 0.1f;

    [HideInInspector] public float offsetX;
    [HideInInspector] public float offsetZ;

    [Header("Aðaç Ayarlarý")]
    [Range(0.0f, 1.0f)]
    public float treeNoiseThreshold = 0.85f;

    [Header("Bitki Örtüsü Ayarlarý")]
    [Range(0.0f, 1.0f)]
    public float foliageNoiseThreshold = 0.5f;
    public float foliageNoiseScale = 0.3f;

    [Header("Maðara ve Cevher Ayarlarý")]
    public float caveNoiseScale = 0.09f;
    [Range(0.0f, 1.0f)]
    public float caveThreshold = 0.55f;
    public float oreNoiseScale = 0.4f;
    [Range(0.0f, 1.0f)]
    public float coalThreshold = 0.75f;
    [Range(0.0f, 1.0f)]
    public float ironThreshold = 0.85f;
    public int ironSpawnHeight = 50;

    [Header("Referanslar")]
    public Transform player;
    public GameObject chunkPrefab;
    public Material terrainMaterial;
    public Material foliageMaterial;

    [Header("Efektler")]
    public GameObject blockBreakFXPrefab; // "BlockBreakFX" prefab (Kýrma)
    // "FallingBlockFXPrefab" referansý SÝLÝNDÝ

    // --- Dinamik Dünya Deðiþkenleri ---
    private Dictionary<Vector2, Chunk> activeChunks = new Dictionary<Vector2, Chunk>();
    private Vector2Int currentPlayerChunk;
    private List<Vector2> chunksToUnload = new List<Vector2>();

    void Start()
    {
        offsetX = Random.Range(-1000f, 1000f);
        offsetZ = Random.Range(-1000f, 1000f);

        if (player == null)
        {
            Debug.LogError("WorldGenerator: 'Player' referansý atanmamýþ! Lütfen Inspector'dan atayýn.");
            this.enabled = false;
            return;
        }
        UpdateChunks();
    }

    void Update()
    {
        Vector2Int playerChunkPos = GetChunkCoordFromWorldPos(player.position);
        if (playerChunkPos != currentPlayerChunk)
        {
            currentPlayerChunk = playerChunkPos;
            UpdateChunks();
        }
    }

    // --- DÜNYA YÖNETÝMÝ ---
    void UpdateChunks()
    {
        chunksToUnload.Clear();
        foreach (Vector2 chunkCoord in activeChunks.Keys)
        {
            if (Vector2.Distance(chunkCoord, currentPlayerChunk) > renderDistance)
            {
                chunksToUnload.Add(chunkCoord);
            }
        }
        foreach (Vector2 chunkCoord in chunksToUnload)
        {
            DestroyChunk(chunkCoord);
        }

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2 checkCoord = new Vector2(currentPlayerChunk.x + x, currentPlayerChunk.y + z);
                if (!activeChunks.ContainsKey(checkCoord))
                {
                    CreateChunk((int)checkCoord.x, (int)checkCoord.y);
                }
            }
        }
    }

    void CreateChunk(int chunkX, int chunkZ)
    {
        Vector3 chunkPosition = new Vector3(chunkX * chunkSize, 0, chunkZ * chunkSize);
        GameObject newChunkObject = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity);
        newChunkObject.transform.parent = this.transform;
        newChunkObject.name = $"Chunk ({chunkX}, {chunkZ})";
        Chunk newChunk = newChunkObject.GetComponent<Chunk>();
        newChunk.worldGenerator = this;
        newChunk.chunkX = chunkX;
        newChunk.chunkZ = chunkZ;
        newChunk.GenerateChunk();
        activeChunks.Add(new Vector2(chunkX, chunkZ), newChunk);
    }

    void DestroyChunk(Vector2 chunkCoord)
    {
        if (activeChunks.TryGetValue(chunkCoord, out Chunk chunkToDestroy))
        {
            Destroy(chunkToDestroy.gameObject);
            activeChunks.Remove(chunkCoord);
        }
    }

    // --- BLOK ETKÝLEÞÝMÝ ÝÇÝN YARDIMCI FONKSÝYONLAR ---

    Vector2Int GetChunkCoordFromWorldPos(Vector3 worldPos)
    {
        int chunkX = Mathf.FloorToInt(worldPos.x / chunkSize);
        int chunkZ = Mathf.FloorToInt(worldPos.z / chunkSize);
        return new Vector2Int(chunkX, chunkZ);
    }

    public Chunk GetChunkFromWorldPos(Vector3Int worldPos)
    {
        Vector2Int coord = GetChunkCoordFromWorldPos(worldPos);
        activeChunks.TryGetValue(coord, out Chunk chunk);
        return chunk;
    }

    public byte GetBlockID(Vector3Int worldPos)
    {
        Chunk chunk = GetChunkFromWorldPos(worldPos);
        if (chunk != null)
        {
            int localX = worldPos.x - (chunk.chunkX * chunkSize);
            int localY = worldPos.y;
            int localZ = worldPos.z - (chunk.chunkZ * chunkSize);
            if (localX < 0) localX += chunkSize;
            if (localZ < 0) localZ += chunkSize;
            if (localY >= 0 && localY < chunkHeight)
            {
                return chunk.blocks[localX, localY, localZ];
            }
        }
        return 0; // Hava
    }

    public void SetBlock(Vector3Int worldPos, byte blockType)
    {
        Chunk targetChunk = GetChunkFromWorldPos(worldPos);
        if (targetChunk == null) return;
        int localX = worldPos.x - (targetChunk.chunkX * chunkSize);
        int localY = worldPos.y;
        int localZ = worldPos.z - (targetChunk.chunkZ * chunkSize);
        if (localX < 0) localX += chunkSize;
        if (localZ < 0) localZ += chunkSize;
        targetChunk.ModifyBlock(localX, localY, localZ, blockType);
    }

    public void CheckNeighborChunkUpdate(Vector3Int worldPos)
    {
        int localX = worldPos.x % chunkSize;
        int localZ = worldPos.z % chunkSize;
        if (localX < 0) localX += chunkSize;
        if (localZ < 0) localZ += chunkSize;
        if (localX == 0) { Chunk c = GetChunkFromWorldPos(worldPos + Vector3Int.left); if (c != null) c.UpdateSolidChunkMesh(); }
        else if (localX == chunkSize - 1) { Chunk c = GetChunkFromWorldPos(worldPos + Vector3Int.right); if (c != null) c.UpdateSolidChunkMesh(); }
        if (localZ == 0) { Chunk c = GetChunkFromWorldPos(worldPos + Vector3Int.back); if (c != null) c.UpdateSolidChunkMesh(); }
        else if (localZ == chunkSize - 1) { Chunk c = GetChunkFromWorldPos(worldPos + Vector3Int.forward); if (c != null) c.UpdateSolidChunkMesh(); }
    }

    public void SpawnBreakEffect(Vector3Int worldPos, byte blockID)
    {
        if (blockBreakFXPrefab == null || blockID == 0)
            return;
        Vector3 effectPos = new Vector3(worldPos.x + 0.5f, worldPos.y + 0.5f, worldPos.z + 0.5f);
        GameObject fxObject = Instantiate(blockBreakFXPrefab, effectPos, Quaternion.identity);
        ParticleSystem ps = fxObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            if (blockID < VoxelData.blockColors.Length)
            {
                Color blockColor = VoxelData.blockColors[blockID];
                main.startColor = new ParticleSystem.MinMaxGradient(blockColor);
            }
        }
    }

    // --- YERÇEKÝMÝ KONTROLÜ (ANA) ---
    public void CheckForFallingBlocks(Vector3Int brokenPos)
    {
        Vector3Int currentPos = brokenPos + Vector3Int.up;
        while (currentPos.y < chunkHeight)
        {
            byte blockID = GetBlockID(currentPos);
            if (blockID == 9 || blockID == 10) // Kum veya Çakýl
            {
                StartFalling(currentPos, blockID);
                currentPos += Vector3Int.up;
            }
            else
            {
                break;
            }
        }
    }

    // --- YERÇEKÝMÝ KONTROLÜ (YARDIMCI - "Sahte Tween" SÝLÝNDÝ) ---
    public void StartFalling(Vector3Int fallStartPos, byte blockID)
    {
        Vector3Int posToFallTo = fallStartPos;

        // 1. Düþülecek en alçak 'Hava' bloðunu bul
        for (int y = fallStartPos.y - 1; y >= 0; y--)
        {
            Vector3Int checkPos = new Vector3Int(fallStartPos.x, y, fallStartPos.z);
            byte block = GetBlockID(checkPos);
            if (block == 0)
            {
                posToFallTo = checkPos;
            }
            else
            {
                break;
            }
        }

        // 2. Bloðu taþý (eðer düþecek yer varsa)
        if (posToFallTo != fallStartPos)
        {
            // --- VERÝYÝ ANINDA GÜNCELLE ---
            SetBlock(posToFallTo, blockID); // YENÝ yerine yerleþtir
            SetBlock(fallStartPos, 0);      // ESKÝ yerinden sil

            // --- GÖRSEL ANÝMASYON ("SAHTE TWEEN") KODU SÝLÝNDÝ ---
        }
    }
}