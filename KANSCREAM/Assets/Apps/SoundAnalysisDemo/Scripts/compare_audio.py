import sys
# 仮想環境のsite-packagesディレクトリをモジュール検索パスに追加
sys.path.append(".venv/lib/python3.9/site-packages")
import librosa
import numpy as np
from librosa.sequence import dtw
import soundfile as sf
import os

def extract_mfcc_resampled(file_path, target_duration=2.0, n_mfcc=13):
    y, sr = librosa.load(file_path, sr=None)
    y = librosa.effects.preemphasis(y)  # プリエンファシスフィルタを適用
    
    # 音声データの長さを取得
    current_duration = librosa.get_duration(y=y, sr=sr)

    # ターゲットの長さにリサンプリング
    stretch_rate = current_duration / target_duration
    y_resampled = librosa.effects.time_stretch(y, rate=stretch_rate)

    # MFCCを計算
    mfcc = librosa.feature.mfcc(y=y_resampled, sr=sr, n_mfcc=n_mfcc)
    return mfcc

def dtw_distance(mfcc1, mfcc2):
    #MFCCのリシェイプ
    mfcc1 = extract_mfcc_resampled(mfcc1)
    mfcc2 = extract_mfcc_resampled(mfcc2)

    print(f"mfcc_max: {np.max(mfcc1)}")
    print(f"mfcc_min: {np.min(mfcc1)}")
    # MFCCの系列間距離を計算
    if mfcc1.shape[1] != mfcc2.shape[1]:
        raise ValueError("MFCCの形状が一致しません")
    D, wp = dtw(mfcc1.T, mfcc2.T, metric='euclidean')
    print(f"D: {D[-1, -1]}")
    
    return D[-1, -1]

def extract_voice_segments(input_wav, output_wav="output.wav", top_db=20):
    # 音声データを読み込む
    y, sr = librosa.load(input_wav, sr=None)

    # 無音部分を除去（音が入っている範囲を取得）
    intervals = librosa.effects.split(y, top_db=top_db)

    # 音のある部分を結合
    y_trimmed = np.concatenate([y[start:end] for start, end in intervals])

    # 結果を保存
    sf.write(output_wav, y_trimmed, sr)

    return y_trimmed

if __name__ == "__main__":
    trimmed_audio = extract_voice_segments(sys.argv[3], "Assets/Apps/SoundAnalysisDemo/Audio/trimmed_output.wav")
    correct_file = sys.argv[1]
    correct_file2 = sys.argv[2]
    recorded_file = "Assets/Apps/SoundAnalysisDemo/Audio/trimmed_output.wav"

    if not os.path.exists(correct_file):
        print(f"Error: {correct_file} does not exist.")
        sys.exit(1)
    if not os.path.exists(correct_file2):
        print(f"Error: {correct_file2} does not exist.")
        sys.exit(1)
    if not os.path.exists(recorded_file):
        print(f"Error: {recorded_file} does not exist.")
        sys.exit(1)
    
    distance = dtw_distance(correct_file, recorded_file)
    distance2 = dtw_distance(correct_file2, recorded_file)
    print(f"Distance: {distance}")
    print(f"Distance2: {distance2}")

    # 距離が小さい方をC#の標準出力に出力
    if distance < distance2:
        distance_min = distance
    else:
        distance_min = distance2

    print(f"distance: {distance_min}")