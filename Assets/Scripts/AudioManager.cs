using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public enum AudioChannel { Master, Sfx, Music };

    public float masterVolumePercent { get; private set; }
    public float sfxVolumPercent { get; private set; }
    public float musicVolumePercent { get; private set; }

    AudioSource[] musicSources;
    AudioSource sfx2DSource;
    int activeMusicSourceIndex;

    private static AudioManager _instance;
    public static AudioManager instance {get {return _instance;} }

    Transform audioListener;
    Transform playerTransform;

    SoundLibrary library;

    void Awake() {
        //保证只有一个AudioManager的实例
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);         
        } else {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++) {
                GameObject newMusicSource = new GameObject("Music source" + (1 + i));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }

            GameObject newSfx2Dsource = new GameObject("2D Sfx source");
            sfx2DSource = newSfx2Dsource.AddComponent<AudioSource>();
            newSfx2Dsource.transform.parent = transform;

            audioListener = FindObjectOfType<AudioListener>().transform;
            if (FindObjectOfType<Player>() != null) {
                playerTransform = FindObjectOfType<Player>().transform;
            }
            library = GetComponent<SoundLibrary>();

            masterVolumePercent = PlayerPrefs.GetFloat("master vol", 1);
            sfxVolumPercent = PlayerPrefs.GetFloat("sfx vol", 1f);
            musicVolumePercent = PlayerPrefs.GetFloat("music vol", 0.5f);
        }
    }

    void Update() {
        if (playerTransform != null) {
            audioListener.position = playerTransform.position;
        }
    }

    //设置音量
    public void SetVolume(float volumePercent, AudioChannel channel) {
        switch (channel) {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx:
                sfxVolumPercent = volumePercent;
                break;
            case AudioChannel.Music:
                sfxVolumPercent = volumePercent;
                break;
        }

        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        //保存用户偏好
        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumPercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1) {
        //在1到0之间改变
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].volume = musicVolumePercent * masterVolumePercent;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossFade(fadeDuration));
    }

    public void PlaySound(AudioClip clip, Vector3 pos) {
        if (clip != null) {
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumPercent * masterVolumePercent);
        }
    }

    public void PlaySound(string soundName, Vector3 pos) {
        PlaySound(library.GetClipFromName(soundName), pos);
    }

    public void PlaySound2D(string soundName) {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumPercent * masterVolumePercent);
    }

    IEnumerator AnimateMusicCrossFade(float duration) {
        float percent = 0;

        while (percent < 1) {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);
            yield return null;
        }
    }
}
