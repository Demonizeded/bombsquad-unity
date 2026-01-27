using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class animnade : MonoBehaviour
{
    bool isrotnt = false;
    Quaternion orgpos;
    // Start is called before the first frame update
    void Start()
    {
        orgpos = gameObject.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButtonDown(0) && isrotnt == false)
        {
            StartCoroutine(MoveNade());
        }

    }

    private System.Collections.IEnumerator MoveNade()
    {
        isrotnt = true;
        Quaternion targetpos = orgpos * Quaternion.Euler(15, 0, 0);
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            gameObject.transform.rotation = Quaternion.Lerp(orgpos, targetpos, t);
            yield return null;
        }
        t = 0f;
        yield return new WaitForSeconds(0.1f);
        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            gameObject.transform.rotation = Quaternion.Lerp(targetpos, orgpos, t);
            
            yield return null;
        }
        gameObject.transform.localRotation = Quaternion.Euler(0,0,0);

        isrotnt = false;
    }
}
