using UnityEngine;

public class MobileDetector : MonoBehaviour
{
    [SerializeField] private GameObject mobileObject;

    private void Awake()
    {
        if (mobileObject == null)
            return;

        mobileObject.SetActive(Application.isMobilePlatform);
    }
}