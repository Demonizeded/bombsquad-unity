using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.VisualScripting;


public class Abilities : NetworkBehaviour
{
    [SerializeField] private Button TankButton;
    [SerializeField] private GameObject Tankbuttongobj;
    [SerializeField] private Button FlashButton;
    [SerializeField] private GameObject Flashbuttongobj;
    [SerializeField] private Button VampButton;
    [SerializeField] private GameObject VampButtonobj;

    [SerializeField] private Button GunnerButton;
    [SerializeField] private GameObject GunnerButtongobj;

    [SerializeField] private Button TestButton;
    [SerializeField] private GameObject TestButtongobj;

    public bool AlrChosen = false;
    int ab1;
    int ab2;
    int ab3;
    void Start()
    {
        
        SkillDraw();
    }

    IEnumerator SkillDrawButWait()
    {
        yield return new WaitForSeconds(1f);
    }

    public void SkillDraw()
    {
        StartCoroutine(SkillDrawButWait());
        ab1 = UnityEngine.Random.Range(1, 4);
        ab2 = UnityEngine.Random.Range(4, 5);
        ab3 = UnityEngine.Random.Range(5, 6);

        switch (ab1)
        {
            case 1:
                Flashbuttongobj.SetActive(true);
                break;
            case 2:
                VampButtonobj.SetActive(true);
                break;
            case 3:
                Tankbuttongobj.SetActive(true);
                break;
        }

        switch (ab2)
        {
            case 4:
                GunnerButtongobj.SetActive(true);
                break;
        }

        switch (ab3)
        {
            case 5:
                TestButtongobj.SetActive(true);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        TankButton.onClick.AddListener(Tank);
        FlashButton.onClick.AddListener(Flash);
        VampButton.onClick.AddListener(Vamp);
        GunnerButton.onClick.AddListener(Gunner);
        TestButton.onClick.AddListener(Test);

    }

    void Test()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Vamp()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        UnlockGrenades(4);
    }

    void Gunner()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        UnlockGrenades(3);
    }
    void Flash()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        UnlockGrenades(2);
    }
    void Tank()
    {


        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        UnlockGrenades(1);

        
    }

    void UnlockGrenades(int Aclass)
    {
        var plrthrw = GetComponentInParent<GrenadeThrower>();

        AlrChosen = true;
        plrthrw.alrchose = AlrChosen;
        Cursor.lockState = CursorLockMode.Locked;

        switch (Aclass)
        {
            case 1://tank
                var plrhl = GetComponentInParent<PlayerHealth>();
                plrhl.ClassSelectionServerRpc(1);
            break;

            case 2://flash
                var plrmv = GetComponentInParent<PlrMovement>();
                plrmv.AblitiesMovementServerRpc(1);
            break;

            case 3://gunner
                plrthrw.NadeAblitiesServerRpc(1);
            break;

            case 4:
                var plrhl2 = GetComponentInParent<PlayerHealth>();
                plrhl2.ClassSelectionServerRpc(2);
                break;
        }
    }


}
