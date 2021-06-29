using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraController : MonoBehaviour {
    private Transform _cameraPivot;
    private Transform _cameraRig;

    private Vector3 _pivotPos;
    private float _camDistance = 10f;
    private Vector3 _localRotation;

    [SerializeField] private float _mouseSensitivity = 4f;
    [SerializeField] private float _scrollSensitivity = 2f;
    [SerializeField] private float _cameraMoveDampening = 10f;
    [SerializeField] private float _cameraScrollDampening = 0.1f;

    [SerializeField] private float _cameraMinDistance = 1.5f;
    [SerializeField] private float _cameraMaxDistance = 100f;

    public static UnityEvent CameraUpdate = new UnityEvent();

    // Start is called before the first frame update
    void Start() {
        _cameraRig = transform;
        _cameraPivot = transform.parent;

        // Set Pivot to Grid Center
    }

    bool updated;
    void LateUpdate() {
        return;
        updated = false;
        // Rotation
        if ( Input.GetAxis( "Horizontal" ) != 0 || Input.GetAxis( "Vertical" ) != 0 ) {
            _localRotation.x += Input.GetAxis( "Horizontal" ) * _mouseSensitivity;
            _localRotation.y += Input.GetAxis( "Vertical" ) * _mouseSensitivity;
            updated = true;
        } else if ( Input.GetMouseButton( 0 ) && ( Input.GetAxis( "Mouse X" ) != 0 || Input.GetAxis( "Mouse Y" ) != 0 ) ) {
            _localRotation.x += Input.GetAxis( "Mouse X" ) * _mouseSensitivity;
            _localRotation.y += Input.GetAxis( "Mouse Y" ) * _mouseSensitivity;
            updated = true;
        }

        _localRotation.y = Mathf.Clamp( _localRotation.y, 0f, 90f );

        // Zoom
        if ( Input.GetAxis( "Mouse ScrollWheel" ) != 0f ) {
            float scroll = Input.GetAxis( "Mouse ScrollWheel" ) * _scrollSensitivity;
            scroll *= ( _camDistance * 0.3f );

            _camDistance += scroll * -1f;
            _camDistance = Mathf.Clamp( _camDistance, _cameraMinDistance, _cameraMaxDistance );
            updated = true;

        }

        // Apply Rotation
        Quaternion rot = Quaternion.Euler( _localRotation.y, _localRotation.x, 0 );
        _cameraRig.rotation = Quaternion.Lerp( _cameraRig.rotation, rot, Time.deltaTime * _cameraMoveDampening );

        // Apply Zoom
        if ( _cameraRig.localPosition.y != _camDistance * -1f ) {
            _cameraRig.localPosition = new Vector3( 0f, Mathf.Lerp( _cameraRig.localPosition.y, _camDistance * 1f, Time.deltaTime * _cameraScrollDampening ), 0f );
        }

        if ( updated ) CameraUpdate.Invoke();

    }
}
