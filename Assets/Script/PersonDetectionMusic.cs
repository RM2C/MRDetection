using UnityEngine;

namespace PassthroughCameraSamples.MultiObjectDetection
{
	// 検出結果に応じてオーディオ付きPrefabを生成/破棄する最小実装
	public class PersonDetectionLogger : MonoBehaviour
	{
		// 検出結果（BoxDrawn）を提供するUI管理コンポーネント
		[SerializeField] private SentisInferenceUiManager m_uiInference;
		// 人検出時に生成するオーディオPrefab（AudioSource付き）
		[SerializeField] private GameObject m_personAudioPrefab;
		// 時計検出時に生成するオーディオPrefab（AudioSource付き）
		[SerializeField] private GameObject m_clockAudioPrefab;

		// 生成済みのオーディオインスタンス（人）
		private GameObject m_personAudioInstance;
		// 生成済みのオーディオインスタンス（時計）
		private GameObject m_clockAudioInstance;

		private void Update()
		{
			// 参照未設定なら何もしない
			if (m_uiInference == null) return;

			// 今フレームでの検出有無を判定（時計優先）
			var personDetected = false;
			var clockDetected = false;
			foreach (var box in m_uiInference.BoxDrawn)
			{
				var name = box.ClassName;
				if (name == "clock") { clockDetected = true; }
				else if (name == "person") { personDetected = true; }
				if (clockDetected && personDetected) break;
			}

			// 優先度: clock > person > none
			if (clockDetected)
			{
				EnsureClockAudio();
				DestroyPersonAudio();
			}
			else if (personDetected)
			{
				EnsurePersonAudio();
				DestroyClockAudio();
			}
			else
			{
				DestroyPersonAudio();
				DestroyClockAudio();
			}
		}

		// 人検出時のインスタンス生成（未生成のときのみ）
		private void EnsurePersonAudio()
		{
			if (m_personAudioInstance != null) return;
			if (m_personAudioPrefab == null) { Debug.LogWarning("Person audio prefab not assigned"); return; }
			m_personAudioInstance = Instantiate(m_personAudioPrefab, transform);
			ConfigureAndPlay(m_personAudioInstance);
		}

		// 時計検出時のインスタンス生成（未生成のときのみ）
		private void EnsureClockAudio()
		{
			if (m_clockAudioInstance != null) return;
			if (m_clockAudioPrefab == null) { Debug.LogWarning("Clock audio prefab not assigned"); return; }
			m_clockAudioInstance = Instantiate(m_clockAudioPrefab, transform);
			ConfigureAndPlay(m_clockAudioInstance);
		}

		// 生成したオブジェクトのAudioSource設定と再生
		private static void ConfigureAndPlay(GameObject go)
		{
			if (go == null) return;
			var audio = go.GetComponent<AudioSource>();
			if (audio == null) { Debug.LogWarning("Instantiated object has no AudioSource"); return; }
			audio.spatialBlend = 0f; // 2D再生（距離減衰なし）
			audio.loop = true;       // 検出中は鳴り続ける
			if (audio.clip == null)
			{
				Debug.LogWarning("AudioSource clip is not assigned");
				return;
			}
			if (!audio.isPlaying) audio.Play();
		}

		// 人オーディオの破棄（インスタンスが存在する場合のみ）
		private void DestroyPersonAudio()
		{
			if (m_personAudioInstance == null) return;
			Destroy(m_personAudioInstance);
			m_personAudioInstance = null;
		}

		// 時計オーディオの破棄（インスタンスが存在する場合のみ）
		private void DestroyClockAudio()
		{
			if (m_clockAudioInstance == null) return;
			Destroy(m_clockAudioInstance);
			m_clockAudioInstance = null;
		}
	}
}
