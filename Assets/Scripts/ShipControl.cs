using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ShipControl : MonoBehaviour
{
    //Player control related


    //Camera related
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _followMask;
    [SerializeField] private LayerMask _cockpitMask;
    [SerializeField] private string _currentVCam;
    [SerializeField] private CinemachineVirtualCamera _followVCam;
    [SerializeField] private CinemachineVirtualCamera _cockpitVCam;
    [SerializeField] private CinemachineVirtualCamera[] _cinematicVCams;


    // Start is called before the first frame update
    void Start()
    {
        DoNullChecks();
        _currentVCam = "FollowCam";
        _followVCam.Priority = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_currentVCam == "FollowCam")
            {
                SwitchCamera("CockpitCam");
            } else if(_currentVCam == "CockpitCam")
            {
                SwitchCamera("FollowCam");
            } else if(_currentVCam == "CinematicCam")
            {
                SwitchCamera("FollowCam");
            }
        }
    }

    void SwitchCamera (string CameraName)
    {


        switch (CameraName)
        {
            case "FollowCam":
                _mainCamera.cullingMask = _followMask;
                _currentVCam = "FollowCam";
                _cockpitVCam.Priority = 10;

                if (_cinematicVCams != null)
                {
                    foreach (CinemachineVirtualCamera vCams in _cinematicVCams)
                    {
                        vCams.Priority = 10;
                    }
                }
               
                _followVCam.Priority = 100;

                //Switch engine background audio
                _followVCam.GetComponent<AudioSource>().enabled = true;
                _cockpitVCam.GetComponent<AudioSource>().enabled = false;

                //turn off music background audio

                break;
            case "CockpitCam":
                _mainCamera.cullingMask = _cockpitMask;
                _currentVCam = "CockpitCam";
                _followVCam.Priority = 10;
                //_followVCam.enabled = false;

                if (_cinematicVCams != null)
                {
                    foreach (CinemachineVirtualCamera vCams in _cinematicVCams)
                    {
                        vCams.Priority = 10;
                    }
                }

                _cockpitVCam.Priority = 100;

                //Switch engine background audio
                _cockpitVCam.GetComponent<AudioSource>().enabled = true;
                _followVCam.GetComponent<AudioSource>().enabled = false;

                //turn off music background audio

                break;
            case "CinematicCam":
                _mainCamera.cullingMask = _followMask;
                _currentVCam = "CinematicCam";
                _followVCam.Priority = 10;
                _cockpitVCam.Priority = 10;

                //set priority for cinematic vcams

                //Switch engine background audio
                _followVCam.GetComponent<AudioSource>().enabled = true;
                _cockpitVCam.GetComponent<AudioSource>().enabled = false;

                //Turn on Music background audio

                break;
        }
    }

    void CheckPlayerIdle()
    {
        //Determine if player has been idle for 5 seconds


        //if so, start cinematic vcams

    }

    void DoNullChecks()
    {
        if (_mainCamera == null)
        {
            Debug.Log("ShipControl::DoNullChecks()::_mainCamera is null!");
        }
        if (_followVCam == null)
        {
            Debug.Log("ShipControl::DoNullChecks()::_followVCam is null!");
        }
        if (_cockpitVCam == null)
        {
            Debug.Log("ShipControl::DoNullChecks()::_cockpitVCam is null!");
        }
        if (_cinematicVCams == null)
        {
            Debug.Log("ShipControl::DoNullChecks()::_cinematicVCams is null!");
        }
    }
}
