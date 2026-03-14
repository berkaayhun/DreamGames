using UnityEngine;
using TMPro;

[System.Serializable]
public class LevelData
{
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public string[] grid;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Bölüm Hedefleri (Goals)")]
    public int targetBoxCount;   // Kaç kutu kırılmalı?
    public int targetVaseCount;  // Kaç vazo kırılmalı?
    public int targetStoneCount; // Kaç taş kırılmalı?

    [Header("Hedef UI (Arayüz)")]
    public TextMeshProUGUI boxTargetText;   // Sağ alttaki Kutu sayısı yazısı
    public TextMeshProUGUI vaseTargetText;  // Vazo varsa onun yazısı
    public TextMeshProUGUI stoneTargetText; // Taş varsa onun yazısı

    [Header("UI ve Oyun Durumu")]
    public TextMeshProUGUI moveText; // Ekranda sayının değişeceği o UI yazısı
    public int remainingMoves;       // Arka planda saydığımız kalan hamle
    public bool isGameOver = false;  

    [Header("Level Dosyası")]
    public TextAsset levelJSON; 

    [Header("Okunan Veriler")]
    public LevelData currentLevelData;

    void Awake()
    {
        Instance = this; 
    }

    void Start()
    {
        LoadLevel();
    }

    public void LoadLevel()
    {
        if (levelJSON != null)
        {
            // JSON'u oku ve LevelData kalıbına dök
            currentLevelData = JsonUtility.FromJson<LevelData>(levelJSON.text);
            
            // JSON'dan gelen hamle sayısını kendi sayacıma eşitle
            remainingMoves = currentLevelData.move_count;
            
            // Oyun başlar başlamaz ekrandaki yazıyı güncelle
            UpdateMoveText();
            
            Debug.Log("--- BÖLÜM YÜKLENDİ --- Hamle Sınırı: " + remainingMoves);
        }
        else
        {
            Debug.LogError("HATA: LevelManager'a JSON dosyası sürüklenmemiş!");
        }
    }

    // --- HAMLE SİSTEMİ ---
    public void DecreaseMove()
    {
        if (isGameOver) return; // Oyun bittiyse sayıyı düşürmeyi bırak

        remainingMoves--; // Sayacı 1 azalt
        UpdateMoveText(); // Ekrana yeni sayıyı yaz

        if (remainingMoves <= 0)
        {
            remainingMoves = 0; // Sayı eksiye düşmesin diye 0'a sabitliyoruz
            UpdateMoveText();
            isGameOver = true;
            Debug.Log("HAMLE BİTTİ! OYUN KİLİTLENDİ!"); 
        }
    }

    private void UpdateMoveText()
    {
        if (moveText != null) moveText.text = remainingMoves.ToString();
    }

    // --- HEDEF (GOAL) SİSTEMİ ---

    // Tahta dizilirken engelleri sayar (CubeBoard'dan çağrılır)
    public void AddGoal(string obstacleType)
    {
        if (obstacleType == "Box") targetBoxCount++;
        else if (obstacleType == "Vase") targetVaseCount++;
        else if (obstacleType == "Stone") targetStoneCount++;

        UpdateGoalUI(); // Ekrandaki yazıyı güncelle
    }

    // Engel kırıldığında sayıyı düşürür (Engellerin içinden çağrılır)
    public void DecreaseGoal(string obstacleType)
    {
        if (isGameOver) return;

        if (obstacleType == "Box") targetBoxCount--;
        else if (obstacleType == "Vase") targetVaseCount--;
        else if (obstacleType == "Stone") targetStoneCount--;

        // Hedefler eksiye düşmesin diye sıfıra sabitliyoruz
        if (targetBoxCount < 0) targetBoxCount = 0;
        if (targetVaseCount < 0) targetVaseCount = 0;
        if (targetStoneCount < 0) targetStoneCount = 0;

        UpdateGoalUI(); // Ekrandaki yazıyı güncelle
        CheckWinCondition(); // Acaba kazandık mı?
    }

    // Sağ üstteki rozetlerin içindeki yazıları değiştirir
    public void UpdateGoalUI()
    {
        if (boxTargetText != null) boxTargetText.text = targetBoxCount.ToString();
        if (vaseTargetText != null) vaseTargetText.text = targetVaseCount.ToString();
        if (stoneTargetText != null) stoneTargetText.text = targetStoneCount.ToString();
    }

    // Kazanma Kontrolü
    private void CheckWinCondition()
    {
        if (targetBoxCount == 0 && targetVaseCount == 0 && targetStoneCount == 0 && remainingMoves >= 0)
        {
            isGameOver = true;
            Debug.Log("🎉 TEBRİKLER! BÖLÜMÜ GEÇTİN!");
        }
    }
}