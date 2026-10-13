using UnityEngine;

namespace EchoFall.Movement
{
    // Optional Resources/EchoAudio asset. Replace individual placeholders without touching gameplay.
    [CreateAssetMenu(menuName="Echo-Fall/Audio Palette")]
    public sealed class AudioPalette : ScriptableObject
    {
        public AudioClip ambience,exploration,combat,boss,memory,ending;
        public AudioClip step,jump,land,dash,slash,charged,impact,deflect,signal,ui;
    }
}
