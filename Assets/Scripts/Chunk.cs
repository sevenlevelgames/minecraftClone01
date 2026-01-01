using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    public int chunkX;
    public int chunkZ;

    // --- Mesh 1 (Katý Bloklar) ---
    private MeshFilter solidMeshFilter;
    private MeshRenderer solidMeshRenderer;
    private MeshCollider meshCollider;
    private Mesh solidChunkMesh;
    private List<Vector3> solidVertices = new List<Vector3>();
    private List<int> solidTriangles = new List<int>();
    private List<Vector2> solidUVs = new List<Vector2>();

    // --- Mesh 2 (Bitki Örtüsü - Collider'lý) ---
    public MeshFilter foliageMeshFilter;
    public MeshCollider foliageMeshCollider;
    private Mesh foliageChunkMesh;
    private List<Vector3> foliageVertices = new List<Vector3>();
    private List<int> foliageTriangles = new List<int>();
    private List<Vector2> foliageUVs = new List<Vector2>();

    // 0=Hava, 1-5=Katý, 6=Çiçek, 7=Kömür, 8=Demir, 9=Kum, 10=Çakýl
    public byte[,,] blocks;

    void OnDestroy()
    {
        if (solidChunkMesh != null) Destroy(solidChunkMesh);
        if (foliageChunkMesh != null) Destroy(foliageChunkMesh);
    }

    void Awake()
    {
        solidMeshFilter = GetComponent<MeshFilter>();
        solidMeshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        if (foliageMeshFilter != null) { foliageMeshCollider = foliageMeshFilter.GetComponent<MeshCollider>(); }
        else { Debug.LogError("Chunk_Prefab'a 'FoliageMeshObject' eklenmemiþ veya 'foliageMeshFilter' atanmamýþ!"); }
    }

    public void GenerateChunk()
    {
        InitializeChunk();
        PopulateBlockData();
        PopulateStructures();
        UpdateSolidChunkMesh();
        UpdateFoliageChunkMesh();
    }

    // --- GÜNCELLENDÝ: ModifyBlock (Katý tanýmý deðiþti) ---
    public void ModifyBlock(int localX, int localY, int localZ, byte blockType)
    {
        if (localX < 0 || localX >= worldGenerator.chunkSize || localY < 0 || localY >= worldGenerator.chunkHeight || localZ < 0 || localZ >= worldGenerator.chunkSize) { return; }
        byte oldBlockType = blocks[localX, localY, localZ];
        blocks[localX, localY, localZ] = blockType;

        // Katý: 0 (Hava) veya 6 (Çiçek) olmayan her þey
        bool oldWasSolid = (oldBlockType != 0 && oldBlockType != 6);
        bool newIsSolid = (blockType != 0 && blockType != 6);
        bool oldWasFoliage = (oldBlockType == 6);
        bool newIsFoliage = (blockType == 6);

        if (oldWasSolid != newIsSolid)
        {
            UpdateSolidChunkMesh();
            worldGenerator.CheckNeighborChunkUpdate(new Vector3Int(localX + chunkX * worldGenerator.chunkSize, localY, localZ + chunkZ * worldGenerator.chunkSize));
        }
        if (oldWasFoliage != newIsFoliage)
        {
            UpdateFoliageChunkMesh();
        }
    }

    // --- MESH GÜNCELLEME FONKSÝYONLARI ---
    public void UpdateSolidChunkMesh()
    {
        solidVertices.Clear(); solidTriangles.Clear(); solidUVs.Clear();
        BuildMesh();
        ApplyMeshToComponents(true);
    }
    public void UpdateFoliageChunkMesh()
    {
        foliageVertices.Clear(); foliageTriangles.Clear(); foliageUVs.Clear();
        BuildFoliageMesh();
        ApplyMeshToComponents(false);
    }

    // --- InitializeChunk (Portal materyali kaldýrýldý) ---
    void InitializeChunk()
    {
        blocks = new byte[worldGenerator.chunkSize, worldGenerator.chunkHeight, worldGenerator.chunkSize];
        solidMeshRenderer.material = worldGenerator.terrainMaterial;
        if (foliageMeshFilter != null) { foliageMeshFilter.GetComponent<MeshRenderer>().material = worldGenerator.foliageMaterial; }
        foliageVertices.Clear(); foliageTriangles.Clear(); foliageUVs.Clear();
    }

    // --- PopulateBlockData (3D MAÐARA ve 3D CEVHER) (ÝÇÝ DOLU) ---
    void PopulateBlockData()
    {
        float caveScale = worldGenerator.caveNoiseScale;
        float caveOffsetX = worldGenerator.offsetX + 200f;
        float caveOffsetY = 200f;
        float caveOffsetZ = worldGenerator.offsetZ + 200f;

        float oreScale = worldGenerator.oreNoiseScale;
        float oreOffsetX = worldGenerator.offsetX + 300f;
        float oreOffsetY = 300f;
        float oreOffsetZ = worldGenerator.offsetZ + 300f;

        for (int x = 0; x < worldGenerator.chunkSize; x++)
        {
            for (int z = 0; z < worldGenerator.chunkSize; z++)
            {
                int globalX = (chunkX * worldGenerator.chunkSize) + x;
                int globalZ = (chunkZ * worldGenerator.chunkSize) + z;

                float terrainNoise = Mathf.PerlinNoise(((float)globalX + worldGenerator.offsetX) * worldGenerator.noiseScale, ((float)globalZ + worldGenerator.offsetZ) * worldGenerator.noiseScale);
                int topY = Mathf.RoundToInt(terrainNoise * worldGenerator.hillHeight) + worldGenerator.surfaceLevelY;
                int stoneTopY = topY - worldGenerator.dirtLayerDepth - 1;
                int bottomY = topY - worldGenerator.terrainDepth;
                if (stoneTopY < bottomY) { stoneTopY = bottomY; }
                bottomY = Mathf.Max(0, bottomY);

                for (int y = 0; y < worldGenerator.chunkHeight; y++)
                {
                    float globalY = (float)y;

                    if (y > topY)
                    {
                        blocks[x, y, z] = 0;
                        continue;
                    }
                    else if (y == topY)
                    {
                        blocks[x, y, z] = 2;
                        continue;
                    }
                    else if (y > stoneTopY)
                    {
                        blocks[x, y, z] = 1;
                        continue;
                    }
                    else if (y >= bottomY)
                    {
                        float caveNoiseXY = Mathf.PerlinNoise((globalX + caveOffsetX) * caveScale, (globalY + caveOffsetY) * caveScale);
                        float caveNoiseYZ = Mathf.PerlinNoise((globalY + caveOffsetY) * caveScale, (globalZ + caveOffsetZ) * caveScale);
                        float caveNoiseXZ = Mathf.PerlinNoise((globalX + caveOffsetX) * caveScale, (globalZ + caveOffsetZ) * caveScale);
                        float caveNoise = (caveNoiseXY + caveNoiseYZ + caveNoiseXZ) / 3f;

                        if (caveNoise < worldGenerator.caveThreshold)
                        {
                            blocks[x, y, z] = 0;
                        }
                        else
                        {
                            float oreNoiseXY = Mathf.PerlinNoise((globalX + oreOffsetX) * oreScale, (globalY + oreOffsetY) * oreScale);
                            float oreNoiseYZ = Mathf.PerlinNoise((globalY + oreOffsetY) * oreScale, (globalZ + oreOffsetZ) * oreScale);
                            float oreNoiseXZ = Mathf.PerlinNoise((globalX + oreOffsetX) * oreScale, (globalZ + oreOffsetZ) * oreScale);
                            float oreNoise = (oreNoiseXY + oreNoiseYZ + oreNoiseXZ) / 3f;

                            if (oreNoise > worldGenerator.coalThreshold)
                            {
                                blocks[x, y, z] = 7;
                            }
                            else if (oreNoise > worldGenerator.ironThreshold && y < worldGenerator.ironSpawnHeight)
                            {
                                blocks[x, y, z] = 8;
                            }
                            else
                            {
                                blocks[x, y, z] = 3;
                            }
                        }
                    }
                    else
                    {
                        blocks[x, y, z] = 0;
                    }
                }
            }
        }
    }

    // --- PopulateStructures (TEK TÝP AÐAÇ + ÇÝÇEK if-else MANTIÐI) (ÝÇÝ DOLU) ---
    void PopulateStructures()
    {
        int border = 3;
        for (int x = 0; x < worldGenerator.chunkSize; x++)
        {
            for (int z = 0; z < worldGenerator.chunkSize; z++)
            {
                int topY = 0;
                for (int y = worldGenerator.chunkHeight - 1; y >= 0; y--) { if (blocks[x, y, z] > 0) { topY = y; break; } }
                if (blocks[x, topY, z] == 2)
                {
                    float treeNoiseScale = 0.5f;
                    float treeNoiseVal = Mathf.PerlinNoise(((float)(chunkX * worldGenerator.chunkSize + x) + worldGenerator.offsetX) * treeNoiseScale, ((float)(chunkZ * worldGenerator.chunkSize + z) + worldGenerator.offsetZ) * treeNoiseScale);
                    if (x >= border && x < worldGenerator.chunkSize - border && z >= border && z < worldGenerator.chunkSize - border && treeNoiseVal > worldGenerator.treeNoiseThreshold)
                    {
                        BuildTree(x, topY, z);
                    }
                    else
                    {
                        float foliageNoiseVal = Mathf.PerlinNoise(((float)(chunkX * worldGenerator.chunkSize + x) + worldGenerator.offsetX) * worldGenerator.foliageNoiseScale, ((float)(chunkZ * worldGenerator.chunkSize + z) + worldGenerator.offsetZ) * worldGenerator.foliageNoiseScale);
                        if (foliageNoiseVal < worldGenerator.foliageNoiseThreshold)
                        {
                            if (topY + 1 < worldGenerator.chunkHeight) { blocks[x, topY + 1, z] = 6; }
                        }
                    }
                }
            }
        }
    }

    // --- BuildTree (Tek Tip Aðaç) (ÝÇÝ DOLU) ---
    void BuildTree(int x, int y, int z)
    {
        blocks[x, y, z] = 1;
        int trunkHeight = 5;
        for (int i = 1; i <= trunkHeight; i++) { if (y + i < worldGenerator.chunkHeight) { blocks[x, y + i, z] = 4; } }
        int leafTopY = y + trunkHeight;
        for (int ly = -1; ly <= 2; ly++)
        {
            for (int lx = -2; lx <= 2; lx++)
            {
                for (int lz = -2; lz <= 2; lz++)
                {
                    if (Mathf.Abs(lx) == 2 && Mathf.Abs(lz) == 2 && ly != 0) continue;
                    if (lx == 0 && lz == 0 && ly <= 0) continue;
                    int blockX = x + lx; int blockY = leafTopY + ly; int blockZ = z + lz;
                    if (blockY < worldGenerator.chunkHeight)
                    {
                        if (blocks[blockX, blockY, blockZ] == 0) { blocks[blockX, blockY, blockZ] = 5; }
                    }
                }
            }
        }
    }

    // --- BuildMesh (KATI) (Case 9 ve 10 eklendi) ---
    void BuildMesh()
    {
        for (int x = 0; x < worldGenerator.chunkSize; x++)
        {
            for (int y = 0; y < worldGenerator.chunkHeight; y++)
            {
                for (int z = 0; z < worldGenerator.chunkSize; z++)
                {
                    byte blockType = blocks[x, y, z];
                    if (blockType == 0 || blockType == 6) continue;

                    Vector2[] topUVs, bottomUVs, sideUVs;
                    switch (blockType)
                    {
                        case 1: topUVs = VoxelData.DirtUVs; bottomUVs = VoxelData.DirtUVs; sideUVs = VoxelData.DirtUVs; break;
                        case 2: topUVs = VoxelData.GrassTopUVs; bottomUVs = VoxelData.DirtUVs; sideUVs = VoxelData.GrassSideUVs; break;
                        case 3: topUVs = VoxelData.CobblestoneUVs; bottomUVs = VoxelData.CobblestoneUVs; sideUVs = VoxelData.CobblestoneUVs; break;
                        case 4: topUVs = VoxelData.WoodTopUVs; bottomUVs = VoxelData.WoodTopUVs; sideUVs = VoxelData.WoodSideUVs; break;
                        case 5: topUVs = VoxelData.LeavesUVs; bottomUVs = VoxelData.LeavesUVs; sideUVs = VoxelData.LeavesUVs; break;
                        case 7: topUVs = VoxelData.CoalOreUVs; bottomUVs = VoxelData.CoalOreUVs; sideUVs = VoxelData.CoalOreUVs; break;
                        case 8: topUVs = VoxelData.IronOreUVs; bottomUVs = VoxelData.IronOreUVs; sideUVs = VoxelData.IronOreUVs; break;
                        case 9: topUVs = VoxelData.SandUVs; bottomUVs = VoxelData.SandUVs; sideUVs = VoxelData.SandUVs; break;
                        case 10: topUVs = VoxelData.GravelUVs; bottomUVs = VoxelData.GravelUVs; sideUVs = VoxelData.GravelUVs; break;
                        default: topUVs = VoxelData.DirtUVs; bottomUVs = VoxelData.DirtUVs; sideUVs = VoxelData.DirtUVs; break;
                    }

                    if (!IsBlockSolid(x, y + 1, z)) AddFace(x, y, z, VoxelData.TopFaceVertices, topUVs);
                    if (!IsBlockSolid(x, y - 1, z)) AddFace(x, y, z, VoxelData.BottomFaceVertices, bottomUVs);
                    if (!IsBlockSolid(x, y, z + 1)) AddFace(x, y, z, VoxelData.FrontFaceVertices, sideUVs);
                    if (!IsBlockSolid(x, y, z - 1)) AddFace(x, y, z, VoxelData.BackFaceVertices, sideUVs);
                    if (!IsBlockSolid(x + 1, y, z)) AddFace(x, y, z, VoxelData.RightFaceVertices, sideUVs);
                    if (!IsBlockSolid(x - 1, y, z)) AddFace(x, y, z, VoxelData.LeftFaceVertices, sideUVs);
                }
            }
        }
    }

    // --- BuildFoliageMesh (BÝTKÝ ÖRTÜSÜ - "X" ÝLÜZYONU) ---
    void BuildFoliageMesh()
    {
        Vector2[] uvs = VoxelData.FoliageUVs;
        for (int x = 0; x < worldGenerator.chunkSize; x++)
        {
            for (int y = 0; y < worldGenerator.chunkHeight; y++)
            {
                for (int z = 0; z < worldGenerator.chunkSize; z++)
                {
                    if (blocks[x, y, z] == 6)
                    {
                        Vector3 v1 = new Vector3(x + 0.0f, y + 0.0f, z + 0.0f); Vector3 v2 = new Vector3(x + 0.0f, y + 1.0f, z + 0.0f);
                        Vector3 v3 = new Vector3(x + 1.0f, y + 1.0f, z + 1.0f); Vector3 v4 = new Vector3(x + 1.0f, y + 0.0f, z + 1.0f);
                        Vector3 v5 = new Vector3(x + 0.0f, y + 0.0f, z + 1.0f); Vector3 v6 = new Vector3(x + 0.0f, y + 1.0f, z + 1.0f);
                        Vector3 v7 = new Vector3(x + 1.0f, y + 1.0f, z + 0.0f); Vector3 v8 = new Vector3(x + 1.0f, y + 0.0f, z + 0.0f);
                        AddFaceToMesh(foliageVertices, foliageTriangles, foliageUVs, v1, v2, v3, v4, uvs);
                        AddFaceToMesh(foliageVertices, foliageTriangles, foliageUVs, v4, v3, v2, v1, uvs);
                        AddFaceToMesh(foliageVertices, foliageTriangles, foliageUVs, v5, v6, v7, v8, uvs);
                        AddFaceToMesh(foliageVertices, foliageTriangles, foliageUVs, v8, v7, v6, v5, uvs);
                    }
                }
            }
        }
    }

    // --- YARDIMCI FONKSÝYON: AddFaceToMesh (Foliage için) ('ufs' hatasý DÜZELTÝLDÝ) ---
    void AddFaceToMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector2[] uvCoords)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1); vertices.Add(v2); vertices.Add(v3); vertices.Add(v4);
        uvs.Add(uvCoords[0]); uvs.Add(uvCoords[1]); uvs.Add(uvCoords[2]); uvs.Add(uvCoords[3]); // Düzeltildi
        triangles.Add(vertexIndex + 0); triangles.Add(vertexIndex + 1); triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 0); triangles.Add(vertexIndex + 2); triangles.Add(vertexIndex + 3);
    }

    // --- IsBlockSolid (KATI BLOK KONTROLÜ) ---
    bool IsBlockSolid(int x, int y, int z)
    {
        if (y < 0 || y >= worldGenerator.chunkHeight) return false;
        if (x < 0 || x >= worldGenerator.chunkSize || z < 0 || z >= worldGenerator.chunkSize) return false;
        byte blockType = blocks[x, y, z];
        return blockType != 0 && blockType != 6; // 0 (Hava) veya 6 (Çiçek) DEÐÝLSE katýdýr
    }

    // AddFace (KATI MESH ÝÇÝN)
    void AddFace(int x, int y, int z, Vector3[] faceVertices, Vector2[] faceUVs)
    {
        int vertexIndex = solidVertices.Count;
        solidVertices.Add(new Vector3(x, y, z) + faceVertices[0]);
        solidVertices.Add(new Vector3(x, y, z) + faceVertices[1]);
        solidVertices.Add(new Vector3(x, y, z) + faceVertices[2]);
        solidVertices.Add(new Vector3(x, y, z) + faceVertices[3]);
        solidTriangles.Add(vertexIndex + 0); solidTriangles.Add(vertexIndex + 1); solidTriangles.Add(vertexIndex + 2);
        solidTriangles.Add(vertexIndex + 0); solidTriangles.Add(vertexIndex + 2); solidTriangles.Add(vertexIndex + 3);
        solidUVs.AddRange(faceUVs);
    }

    // --- ApplyMeshToComponents (Portal kaldýrýldý) ---
    void ApplyMeshToComponents(bool isSolid)
    {
        if (isSolid)
        {
            solidChunkMesh = new Mesh();
            solidChunkMesh.vertices = solidVertices.ToArray();
            solidChunkMesh.triangles = solidTriangles.ToArray();
            solidChunkMesh.uv = solidUVs.ToArray();
            solidChunkMesh.RecalculateNormals();
            solidMeshFilter.mesh = solidChunkMesh;
            meshCollider.sharedMesh = solidChunkMesh;
        }
        else
        {
            if (foliageMeshFilter == null) return;
            foliageChunkMesh = new Mesh();
            foliageChunkMesh.vertices = foliageVertices.ToArray();
            foliageChunkMesh.triangles = foliageTriangles.ToArray();
            foliageChunkMesh.uv = foliageUVs.ToArray();
            foliageChunkMesh.RecalculateNormals();
            foliageMeshFilter.mesh = foliageChunkMesh;
            if (foliageMeshCollider != null)
            {
                foliageMeshCollider.sharedMesh = foliageChunkMesh;
            }
        }
    }
}