using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using DG.Tweening;
using UniRx;

public class ARAnimateTrackedPoster : MonoBehaviour
{
    readonly string CHILD_PREFAB_NAME = "PosterTrackedPrefab";

    public string assetBundlePath;

    ARTrackedImageManager m_TrackedImageManager;

    AssetBundle loadedBundle;
    GameObject childPrefab;
    Camera mainCamera;

    bool animate = false;

    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(LoadAssetBundle(assetBundlePath));
        SetReactiveObservers();
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void SetReactiveObservers()
    {
        Observable.Interval(System.TimeSpan.FromMilliseconds(1000f))
            .Subscribe(_ => {
                Debug.Log("Camera position : " + mainCamera.transform.position);
            });
    }

    IEnumerator LoadAssetBundle(string path)
    {
        using (var uwr = UnityWebRequestAssetBundle.GetAssetBundle(path))
        {
            Debug.Log("downloading asset bundle..");
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("failed to download : " + uwr.error);
            }
            else
            {
                Debug.Log("asset bundle downloaded.. loading asset bundle..");
                loadedBundle = DownloadHandlerAssetBundle.GetContent(uwr);
            }
        }

        yield return null;

        if (loadedBundle != null)
        {
            Debug.Log("Asset Bundle loaded successfully");
            childPrefab = loadedBundle.LoadAsset(CHILD_PREFAB_NAME) as GameObject;
        }
        else
        {
            Debug.Log("failed to load Asset Bundle : " + path);
        }
    }
    
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
            AnimateChildAppear();

        foreach (var trackedImage in eventArgs.updated)
            AnimateChildAppear();
    }

    void AnimateChildAppear()
    {
        if (!animate)
        {
            if (childPrefab != null)
            {
                Debug.Log("starting animation");

                animate = true;

                var child = Instantiate(childPrefab);

                child.transform.DOLocalMoveY(1f, 1f);
                child.transform.DOScale(Vector3.one, 1f)
                    .SetDelay(0.5f);
                    // .OnComplete(() => {
                    //     AnimateChildApproach(child.transform);
                    // });
            }
            else
            {
                Debug.Log("no child prefab");
            }
        }
    }

    void AnimateChildApproach(Transform child)
    {
        Debug.Log("animate approach..");
        child.SetParent(mainCamera.transform, true);
        child.DOLocalMove(new Vector3(0f, 0f, 0.05f), 0.3f).SetDelay(1f);
    }
}
