using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    private Vector3 OffsetVect3; // Offset Vector3 between the player and the flashlight

    //public GameObject FollowCam; // Camera that follows the player

    [SerializeField] private float MoveSpeed = 13f; // Move speed of the flashlight

    public Light FlashLight; // Flashlight component

    private bool FlashLightIsOn = true
        ;


    // audio
    public AudioSource Source;

    public AudioClip FlashLightOnSound;
    public AudioClip FlashLightOffSound;



    // Start is called before the first frame update
    void Start()
    {
        //OffsetVect3 = transform.position - FollowCam.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = FollowCam.transform.position + OffsetVect3; // Light looks at camera + offset position

        //transform.rotation = Quaternion.Slerp(transform.rotation, FollowCam.transform.rotation, Time.deltaTime * MoveSpeed); // Smooth rotation to follow the camera

        if(Input.GetKeyDown(KeyCode.F)) // If F key is pressed
        {
            if(FlashLightIsOn == false)
            {
                FlashLight.enabled = true; // Turn on the flashlight
                FlashLightIsOn = true;

                // Audio
                Source.PlayOneShot(FlashLightOnSound);
            }
            else if(FlashLightIsOn == true)
            {
                FlashLight.enabled = false;
                FlashLightIsOn = false;

                // Audio
                Source.PlayOneShot(FlashLightOffSound);
            }
                
        }


    }
}
