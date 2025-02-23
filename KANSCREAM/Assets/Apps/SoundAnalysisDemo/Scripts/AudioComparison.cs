using System.Diagnostics;
using System.IO;
using UnityEngine;


public class AudioComparison
{
    public string pythonPath = ".venv/bin/python3"; // Pythonのパス
    private float similarity = 0.0f;

    public void CompareAudio()
    {
        // `StreamingAssets` フォルダのパスを取得
        string scriptPath = Path.Combine(Application.dataPath, "Apps", "SoundAnalysisDemo", "Scripts", "compare_audio.py");
        string correctFile = Path.Combine(Application.dataPath, "Apps", "SoundAnalysisDemo", "Audio", "trimmed_output_correct.wav"); // 正解データ
        string correctFile2 = Path.Combine(Application.dataPath, "Apps", "SoundAnalysisDemo", "Audio", "trimmed_output_correct2.wav"); // 正解データ
        string recordedFile = Path.Combine(Application.dataPath, "Apps", "SoundAnalysisDemo", "Audio", "recorded.wav"); // 録音データ

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{scriptPath}\" \"{correctFile}\" \"{correctFile2}\" \"{recordedFile}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(psi))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                // UnityEngine.Debug.Log("結果:" + result);

                // 結果を解析して類似度を取得
                try
                {
                    string[] lines = result.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("distance:"))
                        {
                            string similarityStr = line.Split(':')[1].Trim();
                            similarity = float.Parse(similarityStr);
                            // UnityEngine.Debug.Log("類似度: " + similarity);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError("エラーが発生しました: " + ex.Message);
                }
            }

            // 標準エラー出力を読み取る
            string error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError("Pythonエラー: " + error);
            }

            process.WaitForExit();
        }
    }

    public float GetSimilarity()
    {
        return similarity;
    }
}