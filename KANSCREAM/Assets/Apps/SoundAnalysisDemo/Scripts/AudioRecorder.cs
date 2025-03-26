using UnityEngine;
using System.Collections;
using System.IO;

public class AudioRecorder : MonoBehaviour
{
    private AudioClip recording;
    private bool isRecording = false;
    private string filePath;
    public int frequency = 44100; // サンプリング周波数
    private float recordingDuration = 3.0f; // 録音時間を3秒に設定
    private float similarity = 0.0f; 

    AudioComparison audioComparison;

    void Start()
    {
        audioComparison = new AudioComparison();
        // 利用可能なマイクデバイスの名前を取得して表示
        foreach (var device in Microphone.devices)
        {
            Debug.Log("マイクデバイス: " + device);
        }
    }

    public void StartRecording()
    {
        // マイクデバイスがMacBook AirのマイクかYeti Stereo Microphoneにマイクデバイスに文字列を代入
        string micDevice =  "Yeti Stereo Microphone";
        foreach (var device in Microphone.devices)
        {
            if (device == "Yeti Stereo Microphone")
            {
                micDevice = device;
                break;
            }else if (device == "MacBook Airのマイク")
            {
                micDevice = device;
            }
        }
        isRecording = true;
        filePath = Path.Combine(Application.dataPath, "Apps", "SoundAnalysisDemo", "Audio", "recorded.wav");
        recording = Microphone.Start(micDevice, false, (int)recordingDuration, frequency);
        StartCoroutine(StopRecordingAfterDuration(recordingDuration));
        Debug.Log("録音開始...");
    }

    IEnumerator StopRecordingAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopRecording();
    }

    void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        Microphone.End(null);
        SaveWav(filePath, recording);
        Debug.Log("録音停止...");
        
        audioComparison.CompareAudio();
        similarity = audioComparison.GetSimilarity();
    }

    void SaveWav(string filename, AudioClip clip)
    {
        var samples = clip.samples * clip.channels;
        var floatData = new float[samples];
        clip.GetData(floatData, 0);

        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                // WAV ヘッダー
                writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF")); // RIFF チャンク
                writer.Write(36 + samples * 2); // ファイルサイズ
                writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE")); // WAVEフォーマット
                writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt ")); // fmt チャンク
                writer.Write(16); // fmt のチャンクサイズ
                writer.Write((short)1); // フォーマット（1 = PCM）
                writer.Write((short)clip.channels); // チャンネル数
                writer.Write(frequency); // サンプリング周波数
                writer.Write(frequency * clip.channels * 2); // バイトレート
                writer.Write((short)(clip.channels * 2)); // ブロックアライン
                writer.Write((short)16); // サンプルあたりのビット数（16bit）
                writer.Write(System.Text.Encoding.UTF8.GetBytes("data")); // data チャンク
                writer.Write(samples * 2); // data サイズ

                // PCM データ書き込み
                for (int i = 0; i < samples; i++)
                {
                    short intSample = (short)(floatData[i] * short.MaxValue);
                    writer.Write(intSample);
                }
            }

            File.WriteAllBytes(filename, memoryStream.ToArray());
        }
    }

    public float GetSimilarity()
    {
        return similarity;
    }
}