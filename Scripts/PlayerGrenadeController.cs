using UnityEngine;
using Unity.Netcode;

public class PlayerGrenadeController : NetworkBehaviour
{
    public GameObject handFrag;
    public GameObject handCollision;
    public GameObject handSticky;
    public GameObject handC4;

    private PlayerHealth ph;

    private void Start()
    {
        ph = GetComponent<PlayerHealth>();
        UpdateHandVisuals((int)(ph != null ? ph.grenadeType.Value : 0));

        if (ph != null)
        {
            ph.grenadeType.OnValueChanged += (oldv, newv) => UpdateHandVisuals(newv);
        }
    }

    private void OnDestroy()
    {
        if (ph != null)
            ph.grenadeType.OnValueChanged -= (oldv, newv) => UpdateHandVisuals(newv);
    }

    private void UpdateHandVisuals(int type)
    {
        if (handFrag) handFrag.SetActive(type == 0);
        if (handCollision) handCollision.SetActive(type == 1);
        if (handSticky) handSticky.SetActive(type == 2);
        if (handC4) handC4.SetActive(type == 3);
    }


}
