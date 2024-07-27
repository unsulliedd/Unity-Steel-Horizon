using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private float fakeLoadSpeed = 0.45f; // Bu deðer yüklenme hýzýný ayarlar

    private void Start()
    {
        progressBar.value = 0.40f;
        StartCoroutine(UpdateProgressBar());
    }

    private IEnumerator UpdateProgressBar()
    {
        while (SaveGameManager.Instance.WorldSceneOperation == null)
        {
            yield return null;
        }

        while (SaveGameManager.Instance.WorldSceneOperation.progress < 0.9f)
        {
            float targetProgress = Mathf.Clamp01(SaveGameManager.Instance.WorldSceneOperation.progress / 0.9f);
            while (progressBar.value < targetProgress)
            {
                progressBar.value += fakeLoadSpeed * Time.deltaTime;
                yield return null;
            }
            yield return null;
        }

        while (progressBar.value < 1f)
        {
            progressBar.value += fakeLoadSpeed * Time.deltaTime;
            yield return null;
        }

        progressBar.value = 1.0f;
    }
}
