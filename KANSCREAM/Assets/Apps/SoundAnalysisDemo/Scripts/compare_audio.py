import sys
# 仮想環境のsite-packagesディレクトリをモジュール検索パスに追加
sys.path.append(".venv/lib/python3.9/site-packages")
import librosa
import numpy as np
from librosa.sequence import dtw
import soundfile as sf
import os

def extract_mfcc_resampled(file_path, target_duration=2.0, n_mfcc=20):
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

def calculate_similarity(file_path1, file_path2, max_distance=100, max_volume_bonus=20):
    # MFCCを抽出
    mfcc1 = extract_mfcc_resampled(file_path1)
    mfcc2 = extract_mfcc_resampled(file_path2)

    # DTW距離を計算
    D, wp = dtw(mfcc1.T, mfcc2.T, metric='cosine')
    dtw_score = D[-1, -1]
    print(f"DTW: {dtw_score}")
    
    # DTWスコアをスケーリング
    if dtw_score <= 7:
        similarity_score = 100
    elif dtw_score >= 16:
        similarity_score = 0
    else:
        similarity_score = 100 - ((dtw_score - 7) * (100 / (16 - 7)))

    # 音量を計算
    y2, sr2 = librosa.load(file_path2, sr=None)
    volume2 = np.sum(y2**2)
    print(f"Volume2: {volume2}")

    # 音量の加点
    volume_bonus = min(max_volume_bonus, (volume2 / 10))

    # 総合スコアを計算
    total_score = similarity_score + volume_bonus
    total_score = min(total_score, max_distance)  # 最大スコアを制限

    print(f"DTW Score: {similarity_score}")
    print(f"Volume Bonus: {volume_bonus}")
    print(f"Total Score: {total_score}")

    return total_score

def extract_voice_segments(input_wav, output_wav="output.wav", top_db=30):
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
    
    distance = calculate_similarity(correct_file, recorded_file)
    distance2 = calculate_similarity(correct_file2, recorded_file)

    # 距離が小さい方をC#の標準出力に出力
    if distance < distance2:
        distance_max = distance2
    else:
        distance_max = distance

    print(f"distance: {distance_max}")