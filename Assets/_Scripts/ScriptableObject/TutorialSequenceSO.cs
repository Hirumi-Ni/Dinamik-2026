using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TutorialStepData
{
    [Tooltip("Gambar screenshot/ilustrasi yang sudah di-highlight secara manual")]
    public Sprite slideImage;

    [Tooltip("Indeks posisi dari array dialogPositions di TutorialManager (misal: 0 = Bawah, 1 = Atas, 2 = Kanan)")]
    public int dialogPositionIndex = 0;

    [Tooltip("Pose karakter saat menjelaskan slide ini")]
    public AvatarPose characterPose = AvatarPose.Explaining;

    [TextArea(3, 5)]
    public string dialogText;
}

[CreateAssetMenu(fileName = "NewTutorialSequence", menuName = "Game Data/Tutorial Sequence")]
public class TutorialSequenceSO : ScriptableObject
{
    public List<TutorialStepData> steps = new List<TutorialStepData>();
}