using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class thrwsound : MonoBehaviour
{
    public AudioClip throwsnd;
    private AudioSource asrc;
    // Start is called before the first frame update
    void Start()
    {
        asrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            asrc.PlayOneShot(throwsnd);
        }
    }
}
