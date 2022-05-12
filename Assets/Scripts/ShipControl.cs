using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Cinemachine;

public class ShipControl : MonoBehaviour
{
    //Player control related
    [Header("Player Control Related")]
    [Space(5)]
    [SerializeField] private GameObject _playerShip;
    private Rigidbody _playerShipRigidBody;
    [Space(5)]
    [SerializeField] private float _xMouseAcceleration = 20f;
    [SerializeField] private float _yMouseAcceleration = 35f;
    [Space(5)]
    [SerializeField] private float _xInputValue;
    [SerializeField] private float _yInputValue;
    [SerializeField] private float _zInputValue;
    [Space(5)]
    //[SerializeField] private Vector2 _lastMouseInputValues;
    [SerializeField] private Vector3 _absoluteMousePosition;
    [SerializeField] private Vector3 _relativeMousePosition;
    [Space(5)]
    [SerializeField] private int _screenPixelWidth;
    [SerializeField] private int _screenPixelHeight;
    [Space(5)]
    [SerializeField] private float _mousePercentOfDisplayWidth;
    [SerializeField] private float _mousePercentOfDisplayHeight;
    [Space(5)]
    [SerializeField] private float _defaultRollSpeed = 5000f;
    [SerializeField] private float _thrustSpeed = 5000f;
    [SerializeField] private float _thrusterBoostMultiplier = 1f;
    [SerializeField] private bool _boostActive = false;
    [SerializeField] private bool _thrustActive = false;
    [SerializeField] private float _timeThrustLastActive;
    [SerializeField] private float _currentTime;

    //Camera related
    [Header("Camera Control Related")]
    [Space(5)]
    [SerializeField] private Camera _mainCamera;
    [Space(5)]
    [SerializeField] private LayerMask _followMask;
    [SerializeField] private LayerMask _cockpitMask;
    [Space(10)]
    [SerializeField] private string _currentVCam;
    [SerializeField] private CinemachineVirtualCamera _followVCam;
    [SerializeField] private CinemachineVirtualCamera _cockpitVCam;
    [SerializeField] private CinemachineVirtualCamera[] _cinematicVCams;

    //Animation related
    [Header("Animation Related")]
    [Space(5)]
    [SerializeField] private Animator _starshipAnimator;
    [SerializeField] private Animator _wingsAnimator;
    [SerializeField] private Animator _minigunAnimator1;
    [SerializeField] private Animator _minigunAnimator2;
    
    //External scripts
    [Header("References")]
    [Space(5)]
    [SerializeField] private ThrusterControl _thrusterControlScript;

    private void OnEnable()
    {
        //_lastMouseInputValues = Vector2.zero;

        _screenPixelWidth = Screen.width;
        _screenPixelHeight = Screen.height;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (_playerShip == null)
        {
            _playerShip = this.gameObject;
        }
        _playerShipRigidBody = _playerShip.GetComponent<Rigidbody>();
        DoNullChecks();
        _currentVCam = "FollowCam";
        _followVCam.Priority = 100;
        _wingsAnimator.SetBool("isEngaged", false);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }

    private void FixedUpdate()
    {
        CheckInputFixedUpdate();
    }

    void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_currentVCam == "FollowCam")
            {
                SwitchCamera("CockpitCam");
            }
            else if (_currentVCam == "CockpitCam")
            {
                SwitchCamera("FollowCam");
            }
            else if (_currentVCam == "CinematicCam")
            {
                SwitchCamera("FollowCam");
            }
        }

        _currentTime = Time.time;
    }

    void CheckInputFixedUpdate()
    {
        ApplyKeyboardInput();
    }

    void CheckPlayerIdle()
    {
        //Determine if player has been idle for 5 seconds


        //if so, start cinematic vcams

    }

    private void ApplyKeyboardInput()
    {
        bool wActive;
        bool sActive;
        bool aActive;
        bool dActive;
        float zInput = 0;


        if (Input.GetKey(KeyCode.LeftShift))
        {
            _boostActive = true;
        } else
        {
            _boostActive = false;
        }

        if (_boostActive)
        {
            _thrusterBoostMultiplier = 4f;
        } else
        {
            _thrusterBoostMultiplier = 1f;
        }
        
        if (Input.GetKey(KeyCode.W))
        {
            AddThrust(transform.forward * _thrustSpeed * _thrusterBoostMultiplier);
            wActive = true;
            _thrustActive = true;
            _timeThrustLastActive = _currentTime;
        }
        else 
        { 
            wActive = false;
            _thrustActive = false;

            AddSlowingThrust(transform.forward);
        }

        if (Input.GetKey(KeyCode.S))
        {
            //Back Thrust
            if (Time.time - _timeThrustLastActive < 5f)
            {
                AddThrust(-transform.forward * (_thrustSpeed * 0.1f));
            }
            
            sActive = true;
        }
        else { sActive = false; }

        if (wActive == false && sActive == false)
        {
            //No forward/back thrust input
            if (_wingsAnimator.GetBool("isEngaged") == true)
            {
                AnimateDisengageWings();
            }
        }
        else if (wActive == true && sActive == false) //forward not back thrust input
        {
            if (_wingsAnimator.GetBool("isEngaged") == false)
            {
                AnimateEngageWings();
            }
        }

        if (Input.GetKey(KeyCode.A))
        {
            //Yaw Left
            zInput = _defaultRollSpeed * Time.deltaTime;
            aActive = true;
        }
        else
        {
            aActive = false;
        }

        if (Input.GetKey(KeyCode.D))
        {
            //Yaw Right
            zInput = -_defaultRollSpeed * Time.deltaTime;
            dActive = true;
        }
        else
        { dActive = false; }

        if (aActive)
        {
            _thrusterControlScript.EnableThrusterSpotlight(0);
            _thrusterControlScript.EnableThrusterSpotlight(3);
        }
        
        if (dActive)
        {
            _thrusterControlScript.EnableThrusterSpotlight(1);
            _thrusterControlScript.EnableThrusterSpotlight(2);
        }
        
        if (!aActive)
        {
            _thrusterControlScript.DisableThrusterSpotlight(0);
            _thrusterControlScript.DisableThrusterSpotlight(3);
        }

        if (!dActive)
        {
            _thrusterControlScript.DisableThrusterSpotlight(1);
            _thrusterControlScript.DisableThrusterSpotlight(2);
        }

        if ((_thrustActive) || ((_currentTime - _timeThrustLastActive) < 9f) && _currentTime > 5f)
        {
            ApplyMouseInput(zInput);
        } else
        {
            ApplyMouseInput(zInput, true);
        }
    }

    private void ApplyMouseInput(float zInput, bool rollOnly = false)
    {
        Vector2 pitchYaw = GetMouseInput();
        float xInput = pitchYaw.x;
        float yInput = pitchYaw.y;

        RotatePlayerShip(xInput, yInput, zInput, rollOnly);
    }

    private Vector2 GetMouseInput()
    {
        Vector2 newInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        SetMousePercentOfScreen();

        if (_mousePercentOfDisplayWidth > 0.9f)
        {
            newInput.x += 1f;
        }
        else if (_mousePercentOfDisplayWidth < 0.1f)
        {
            newInput.x -= 1f;
        } else if (_mousePercentOfDisplayWidth < 0.5f)
        {
            newInput.x = -1f * _mousePercentOfDisplayWidth;
        } else if (_mousePercentOfDisplayWidth > 0.5f)
        {
            newInput.x = 1f * _mousePercentOfDisplayWidth;
        }

        if (_mousePercentOfDisplayHeight > 0.9f)
        {
            newInput.y += 1f;
        }
        else if (_mousePercentOfDisplayHeight < 0.1f)
        {
            newInput.y -= 1f;
        } else if (_mousePercentOfDisplayHeight < 0.5f)
        {
            newInput.y = -1f * _mousePercentOfDisplayHeight;
        } else if (_mousePercentOfDisplayHeight > 0.5f)
        {
            newInput.y = 1f * _mousePercentOfDisplayHeight;
        }

        if (_mousePercentOfDisplayWidth > 0.47f && _mousePercentOfDisplayWidth < 0.53f)
        {
            newInput.x = 0;
        }

        if (_mousePercentOfDisplayHeight > 0.47f && _mousePercentOfDisplayHeight < 0.53f)
        {
            newInput.y = 0;
        }

        //_xInputValue = newInput.x;
        //_yInputValue = newInput.y;

        //Round and clamp values to reduce AABB errors
        newInput.x = Mathf.Round(newInput.x * 100f) * 0.01f;
        newInput.y = Mathf.Round(newInput.y * 100f) * 0.01f;
        newInput.x = Mathf.Clamp(newInput.x, -1.0f, 1.0f);
        newInput.y = Mathf.Clamp(newInput.y, -1.0f, 1.0f);

        _xInputValue = newInput.x;
        _yInputValue = newInput.y;

        //_lastMouseInputValues = newInput;

        //return _lastMouseInputValues;
        return newInput;
    }

    private void RotatePlayerShip(float xInput = 0, float yInput = 0, float zInput = 0, bool rollOnly = false)
    {
        //Round value to reduce AABB errors
        zInput = Mathf.Round(zInput * 100f) * 0.01f;

        _zInputValue = zInput;

        //roll
        transform.Rotate(Vector3.forward, zInput * Time.deltaTime);

        //yaw, pitch
        if (!rollOnly)
        {
            transform.Rotate(Vector3.up, xInput * _xMouseAcceleration * Time.deltaTime);
            transform.Rotate(Vector3.right, yInput * _yMouseAcceleration * Time.deltaTime);
        }
    }

    void  SetMousePercentOfScreen()
    {
        _absoluteMousePosition = Input.mousePosition;
        _mousePercentOfDisplayWidth = _absoluteMousePosition.x / _screenPixelWidth;
        _mousePercentOfDisplayHeight = _absoluteMousePosition.y / _screenPixelHeight; 
    }

    private void AddThrust(Vector3 newAddForce)
    {
        _playerShipRigidBody.AddForce(newAddForce, ForceMode.Force);
    }

    private void AddSlowingThrust(Vector3 newAddForce)
    {
        float timeSinceLastThrust = Time.time - _timeThrustLastActive;

        if (_timeThrustLastActive > 0f)
        {
            switch (timeSinceLastThrust)
            {
                case < 1f:
                    AddThrust(transform.forward * (_thrustSpeed * 0.6f));
                    break;
                case < 3f:
                    AddThrust(transform.forward * (_thrustSpeed * 0.4f));
                    break;
                case < 5f:
                    AddThrust(transform.forward * (_thrustSpeed * 0.2f));
                    break;
                case < 9f:
                    AddThrust(transform.forward * (_thrustSpeed * 0.1f));
                    break;
            }
        }
    }

    public void ReceiveForce(Vector3 newAddForce)
    {
        _playerShipRigidBody.AddForce(newAddForce, ForceMode.Impulse);
    }

    void SwitchCamera(string CameraName)
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

    private void AnimateEngageWings()
    {
        _wingsAnimator.SetTrigger("Engage");
        _wingsAnimator.SetBool("isEngaged", true);
    }

    private void AnimateDisengageWings()
    {
        _wingsAnimator.SetTrigger("Disengage");
        _wingsAnimator.SetBool("isEngaged", false);
    }

    private void AnimateCockpitControls(string newThrusterPosition, string newJoystickPosition)
    {
        //newThrusterPositions = "none" | "forward" | "back"
        //newJoystickPositions = "none" | "forward" | "back" | "left" | "right" | "forward-left" | "forward-right" | "back-left" | "back-right"
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
        if (_playerShip == null)
        {
            Debug.Log("ShipControl::DoNullChecks()::_playerShip is null!");
        }
        if (_wingsAnimator == null)
        {
            Debug.Log("ShipControl::DoNullChecks()::_wingsAnimator is null!");
        }
        if (_minigunAnimator1 == null)
        {
            Debug.Log("ShipControl::DoNullChecks()::_mingunAnimator1 is null!");
        }
        if (_minigunAnimator2 == null)
        {
            Debug.Log("ShipControl::DoNullChecks()::_minigunAnimator2 is null!");
        }
        //if (_starshipAnimator == null)
        //{
        //    Debug.Log("ShipControl::DoNullChecks()::_starshipAnimator is null!");
        //}
    }
}
