using UnityEngine;
using Ami.BroAudio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    #region [Sounds]
    [Header("Music/BGM")]
    public SoundID BGM;

    [Header("SFX_Gameplay")]
    public SoundID SFX_Burn;
    public SoundID SFX_Card1;
    public SoundID SFX_Card2;
    public SoundID SFX_Card3;
    public SoundID SFX_CorrectPair;
    public SoundID SFX_FireExtinguished;
    public SoundID SFX_FireExtinguished2;
    public SoundID SFX_Fuse;
    public SoundID SFX_GlassBreaking;
    public SoundID SFX_ScoreCounting;

    
    [Header("SFX_Item")]
    public SoundID SFX_Elixir;
    public SoundID SFX_Garlic;
    public SoundID SFX_Hourglass;
    public SoundID SFX_Shine;

    [Header("SFX_Shop")]
    public SoundID SFX_BookFlip;
    public SoundID SFX_Stamp;
    public SoundID SFX_WaterPouring;
    
    [Header("SFX_UI")]
    public SoundID SFX_ButtonClick;
    public SoundID SFX_Hover1;
    public SoundID SFX_Hover2;
    public SoundID SFX_Hover3;

  


     
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayAudio(BGM);
    }

    public void PlayAudio(SoundID sound) => BroAudio.Play(sound);
    public void StopAudio(SoundID sound) => BroAudio.Stop(sound);
}