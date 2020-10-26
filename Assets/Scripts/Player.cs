using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;

    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    [Range(0, 10)]
    public float mouseSensitivity = 1f;

    public float playerWidth = 0.15f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;

    private float verticalMomentum = 0;
    private bool jumpRequest;

    public float checkIncrement = 0.1f;
    public float reach = 4f;

    public Text selectedBlockText;
    public ushort selectedBlockIndex = 1;

    private Vector3Int placeBlockPos;
    private Vector3Int highlightBlockPos;

    public bool front
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth))
                || world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z + playerWidth))
               )
            {
                return true;
            }

            return false;
        }
    }
    public bool back
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth))
                || world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z - playerWidth))
               )
            {
                return true;
            }

            return false;
        }
    }
    public bool left
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z))
                || world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1, transform.position.z))
               )
            {
                return true;
            }

            return false;
        }
    }
    public bool right
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z))
                || world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1, transform.position.z))
               )
            {
                return true;
            }

            return false;
        }
    }

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        cam = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        //selectedBlockText.text = world.voxelTypes[selectedBlockIndex].voxelName + " block selected";
    }

    private void FixedUpdate()
    {
        CalculateVelocity();
        if (jumpRequest)
        {
            Jump();
        }

        transform.Rotate(Vector3.up * mouseHorizontal * mouseSensitivity);
        cam.Rotate(Vector3.right * -mouseVertical * mouseSensitivity);
        transform.Translate(velocity, Space.World);
    }

    private void Update()
    {
        GetPlayerInputs();
        GetHighlightPlaceLocation();
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        if (verticalMomentum > gravity)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }

        velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime;

        if (isSprinting)
            velocity *= sprintSpeed;
        else
            velocity *= walkSpeed;

        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
        {
            velocity.z = 0;
        }

        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
        {
            velocity.x = 0;
        }

        if (velocity.y < 0)
        {
            velocity.y = CheckDownSpeed(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = CheckUpSpeed(velocity.y);
        }
    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }

        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0)
            {
                selectedBlockIndex++;
            }
            else
            {
                selectedBlockIndex--;
            }

            if (selectedBlockIndex > world.voxelTypes.Length - 1)
            {
                selectedBlockIndex = 1;
            }
            else if (selectedBlockIndex < 1)
            {
                selectedBlockIndex = (ushort)(world.voxelTypes.Length - 1);
            }

            //selectedBlockText.text = world.voxelTypes[selectedBlockIndex].voxelName + " block selected";
        }

        if (Input.GetMouseButtonDown(0))
        {
            Chunk chunk = world.GetChunkFromVector3(highlightBlockPos);
            if (chunk != null)
            {
                Debug.Log("Destroy Block: " + highlightBlockPos);
                Debug.Log("In Chunk: " + chunk.Coord);
                chunk.EditBlock(highlightBlockPos, 0);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Chunk chunk = world.GetChunkFromVector3(placeBlockPos);
            if (chunk != null)
            {
                Debug.Log("Place Block");
                chunk.EditBlock(placeBlockPos, selectedBlockIndex);
            }
        }
    }

    private void GetHighlightPlaceLocation()
    {
        float step = checkIncrement;
        Vector3Int lastPos = new Vector3Int(Mathf.FloorToInt(cam.position.x), Mathf.FloorToInt(cam.position.y), Mathf.FloorToInt(cam.position.z));

        while (step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);

            if (world.CheckForVoxel(pos))
            {
                highlightBlockPos = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlockPos = lastPos;
                world.HighlightVoxel(highlightBlockPos);
                return;
            }

            lastPos = new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if (
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) && !left && !back)
            || (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) && !right && !back)
            || (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) && !right && !front)
            || (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) && !left && !front)
            )
        {
            isGrounded = true;
            return 0;
        }

        isGrounded = false;
        return downSpeed;
    }

    private float CheckUpSpeed(float upSpeed)
    {
        if (
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2 + upSpeed, transform.position.z - playerWidth)) && !left && !back)
            || (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2 + upSpeed, transform.position.z - playerWidth)) && !right && !back)
            || (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2 + upSpeed, transform.position.z + playerWidth)) && !right && !front)
            || (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2 + upSpeed, transform.position.z + playerWidth)) && !left && !front)
            )
        {
            verticalMomentum = 0;
            return 0;
        }
        return upSpeed;
    }
}
