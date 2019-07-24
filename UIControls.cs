using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIControls : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler {

    #region variables
    private static UIControls uiControls;
    public static UIControls Instance { get { return uiControls; } }

    //  Bools for choosing controls
    //  1 for DUAL JOYSTICKS
    //  2 for GYROSCOPE
    //  3 for CONTROLLER
    [HideInInspector] public bool[] controlChoice = new bool[3];

    [Header ("Dual Joystick Components")]
    public Transform DualJoystickControl;
    public Image DualJoystickMove;
    public Image DualJoystickMoveKnob;
    public Image DualJoystickRotate;
    public Image DualJoystickRotateKnob;
    public ButtonHandler DualJoystickSwitch;
    private bool djShoot;
    public bool DJShoot { get { return djShoot; } set { djShoot = value; } } //    No button handler, it is integrated into the Rotate Joystick

    [Header ("Gyroscope Components")]
    public Transform GyroControl;
    public Image GyroJoystick;
    public Image GyroJoystickKnob;
    public ButtonHandler GyroSwitch;
    public ButtonHandler GyroShoot;
    private Gyroscope gyro;
    public Gyroscope Gyro { get { return gyro; } set { gyro = value; } }

    private Vector3 inputVector;
    private Vector3 inputVectorMove;
    public Vector3 InputVectorMove { get { return inputVectorMove; } set { inputVectorMove = value; } }
    private Vector3 inputVectorRotate;
    public Vector3 InputVectorRotate { get { return inputVectorRotate; } set { inputVectorRotate = value; } }
    #endregion

    #region initialization
    //  Singleton initialization
    private void Awake () {
        if (!Instance) {
            uiControls = this;
        } else if (this != Instance) {
            Destroy (this.gameObject);
        }
    }
    #endregion

    #region touch
    public virtual void OnPointerDown (PointerEventData ped) {
        OnDrag (ped);
    }

    public virtual void OnDrag (PointerEventData ped) {
        if (controlChoice[1]) {
            Vector2 posMove;
            Vector2 posRotate;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle (DualJoystickMove.rectTransform, ped.position, ped.pressEventCamera, out posMove) && (ped.position.x < Screen.width / 2)) {
                DJMove (posMove);
            }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle (DualJoystickRotate.rectTransform, ped.position, ped.pressEventCamera, out posRotate) && (ped.position.x > Screen.width / 2)) {
                DJRotate (posRotate);
                DJShoot = true;
            }
        } else if (controlChoice[2]) {
            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle (GyroJoystick.rectTransform, ped.position, ped.pressEventCamera, out pos) && (ped.position.x < Screen.width / 2)) {
                GyroRotate (pos);
            }
        }
    }

    public virtual void OnPointerUp (PointerEventData ped) {
        if (controlChoice[1]) {
            //  Reset the input vectors
            inputVectorMove = Vector3.zero;
            inputVectorRotate = Vector3.zero;
            //  Reset the image positions
            DualJoystickMoveKnob.rectTransform.anchoredPosition = Vector3.zero;
            DualJoystickRotateKnob.rectTransform.anchoredPosition = Vector3.zero;
            //  Stop shooting
            DJShoot = false;
        } else if (controlChoice[2]) {
            inputVector = Vector3.zero;
            GyroJoystickKnob.rectTransform.anchoredPosition = Vector3.zero;
        }
    }
    #endregion

    #region joysticks
    private void DJMove (Vector2 posMove) {
        posMove.x = (posMove.x / DualJoystickMove.rectTransform.sizeDelta.x);
        posMove.y = (posMove.y / DualJoystickMove.rectTransform.sizeDelta.y);

        inputVectorMove = new Vector3 (posMove.x * 2 - 1, posMove.y * 2 - 1, 0);

        inputVectorMove = (inputVectorMove.magnitude > 1.0f) ? inputVectorMove.normalized : inputVectorMove;

        //   Move Joystick Image
        DualJoystickMoveKnob.rectTransform.anchoredPosition = new Vector3 (inputVectorMove.x * (DualJoystickMove.rectTransform.sizeDelta.x / 3), inputVectorMove.y * (DualJoystickMove.rectTransform.sizeDelta.y / 3));
    }

    private void DJRotate (Vector2 posRotate) {
        posRotate.x = (posRotate.x / DualJoystickRotate.rectTransform.sizeDelta.x);
        posRotate.y = (posRotate.y / DualJoystickRotate.rectTransform.sizeDelta.y);

        inputVectorRotate = new Vector3 (posRotate.x * 2 + 1, posRotate.y * 2 - 1, 0);

        inputVectorRotate = (inputVectorRotate.magnitude > 1.0f) ? inputVectorRotate.normalized : inputVectorRotate;

        //   Move Joystick Image
        DualJoystickRotateKnob.rectTransform.anchoredPosition = new Vector3 (inputVectorRotate.x * (DualJoystickRotate.rectTransform.sizeDelta.x / 3), inputVectorRotate.y * (DualJoystickRotate.rectTransform.sizeDelta.y / 3));
    }

    private void GyroRotate (Vector2 pos) {
        pos.x = (pos.x / GyroJoystick.rectTransform.sizeDelta.x);
        pos.y = (pos.y / GyroJoystick.rectTransform.sizeDelta.y);

        inputVector = new Vector3 (pos.x * 2 - 1, pos.y * 2 - 1, 0);

        inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

        //   Move Joystick Image
        GyroJoystickKnob.rectTransform.anchoredPosition = new Vector3 (inputVector.x * (GyroJoystick.rectTransform.sizeDelta.x / 3), inputVector.y * (GyroJoystick.rectTransform.sizeDelta.y / 3));
    }
    #endregion

    #region send input information
    public float Horizontal () {
        //  Returns horizontal axis from Dual Joystick
        if (controlChoice[1]) {
            if (inputVectorMove.x != 0) {
                return inputVectorMove.x;
            }
        }
        //  Returns Horizontal axis from left Stick of XBOX One Controller
        else {
            return Input.GetAxis ("XBOXONE LEFTSICK HORIZONTAL");
        }
        return 0f;
    }

    public float Vertical () {
        //  Returns vertical axis from Dual Joystick
        if (controlChoice[1]) {
            if (inputVectorMove.y != 0) {
                return inputVectorMove.y;
            }
        }
        //  Returns vertical axis from left Stick of XBOX One Controller
        else {
            return Input.GetAxis ("XBOXONE LEFTSTICK VERTICAL");
        }
        return 0f;
    }

    public float GyroPitch () {
        Quaternion quatGyro = Gyro.attitude;
        float pitch = -Mathf.Asin (2 * (quatGyro.x * quatGyro.z - quatGyro.w * quatGyro.y));
        return pitch;
    }

    public float GyroYaw () {
        Quaternion quatGyro = Gyro.attitude;
        float yaw = -Mathf.Atan2 (1 - 2 * (quatGyro.z * quatGyro.z + quatGyro.w * quatGyro.w), 2 * (quatGyro.x * quatGyro.w + quatGyro.y * quatGyro.z));
        return yaw - 1.5f;
    }

    public float HorizontalRotate () {
        //  Returns horizontal axis from Dual Joystick
        if (controlChoice[1]) {
            if (inputVectorRotate.x != 0) {
                return inputVectorRotate.x;
            }
        }
        //  Returns horizontal axis from Gyro Joystick
        else if (controlChoice[2]) {
            if (inputVector.x != 0) {
                return inputVector.x;
            }
        }
        //  Returns Horizontal axis from Right Stick of XBOX One Controller
        else {
            return Input.GetAxis ("XBOXONE RIGHTSTICK HORIZONTAL");
        }
        return 0f;
    }

    public float VerticalRotate () {
        //  Returns vertical axis from Dual Joystick
        if (controlChoice[1]) {
            if (inputVectorRotate.y != 0) {
                return inputVectorRotate.y;
            }
        }
        //  Returns vertical axis from Gyro Joystick
        else if (controlChoice[2]) {
            if (inputVector.y != 0) {
                return inputVector.y;
            }
        }
        //  Returns vertical axis from Right Stick of XBOX One Controller
        else {
            return Input.GetAxis ("XBOXONE RIGHTSTICK VERTICAL");
        }
        return 0f;
    }

    public bool Switch () {
        if (controlChoice[1]) {
            return DualJoystickSwitch.ReturnBool ();
        } else if (controlChoice[2]) {
            return GyroSwitch.ReturnBool ();
        } else if (Input.GetAxis ("XBOXONE LT") > 0.5) {
            return true;
        }
        return false;
    }

    public bool Shoot () {
        if (controlChoice[1]) {
            return DJShoot;
        } else if (controlChoice[2]) {
            return GyroShoot.ReturnBool ();
        } else if (Input.GetAxis ("XBOXONE RT") > 0.5) {
            return true;
        }
        return false;
    }
    #endregion
}
