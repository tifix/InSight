using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * put it on camera base
 */

public class CameraController : MonoBehaviour
{
    public GameObject player_character;

    [SerializeField] private AnimationCurve mouse_sensitivity;
    [SerializeField] private AnimationCurve FOV_curve;
    [SerializeField] private float pitch_sensitivity_multiplier = 0.4f;
    public bool invert_mouse = false;
    private float FOV;
    public float pitch;
    private float yaw;


    public void Awake()
    {
        Cursor.lockState=CursorLockMode.Locked;
        Cursor.visible = false;
        FOV = Camera.main.fieldOfView;
    }

    void Update()
    {
        CameraCloserIfWallBlocked();

        //if mouse is used to rotate camera
        if (Cursor.lockState == CursorLockMode.Locked) 
        {
            Vector2 delta_mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) ;
            float move_magnitude = mouse_sensitivity.Evaluate(delta_mouse.magnitude);

            float yaw_add = move_magnitude * delta_mouse.x ;
            float pitch_add;
            if (invert_mouse) pitch_add = move_magnitude * delta_mouse.y;
            else pitch_add = move_magnitude * pitch_sensitivity_multiplier * -delta_mouse.y;

            pitch = Mathf.Clamp(pitch += pitch_add, -40, 40);

            transform.eulerAngles=new Vector3( 0, yaw += yaw_add, pitch); //
        }

        //currently disabled due to being trippy as FUCK
        //UpdateFOVBySpeed();
    }
    void CameraCloserIfWallBlocked() 
    {
        Transform T_cam= Camera.main.transform;
        Vector3 temp = T_cam.localPosition;

        if(Physics.Raycast(transform.position, T_cam.position - transform.position, out RaycastHit hit, 2.7f, LayerMask.GetMask("Terrain")))
        {
            temp.x = -hit.distance;
            Camera.main.transform.localPosition = temp;
        }
        else temp.x = -2.7f;// camera should not boom (come closer) if there isn't a wall to obstruct it
        Camera.main.transform.localPosition = temp;
    }


    void UpdateFOVBySpeed() 
    {

        Vector3 velocity_flattened = new Vector3(Player.instance.rb.velocity.x, 0, Player.instance.rb.velocity.z);    //discarding the y parameter as jumping would make FOV too erratic
        Camera.main.fieldOfView = Mathf.Lerp(FOV-7,FOV+15, FOV_curve.Evaluate(velocity_flattened.magnitude));
    }
}
