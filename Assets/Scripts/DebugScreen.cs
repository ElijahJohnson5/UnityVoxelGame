using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;

    float frameRate;
    float timer;

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();
    }

    void Update()
    {
        string debugText = "Minecraft\n";
        debugText += frameRate + " fps\n\n";
        debugText += "X: " + (world.player.transform.position.x) + " Y: " + world.player.transform.position.y + " Z: " + (world.player.transform.position.z) + "\n";
        debugText += "Chunk: " + (world.playerChunkCoord.x) + " / " + (world.playerChunkCoord.z) + "\n";
        text.text = debugText;

        float timelapse = Time.smoothDeltaTime;
        timer = timer <= 0 ? 1 : timer -= timelapse;

        if (timer <= 0)
        {
            frameRate = (int)(1f / timelapse);
        }
    }
}
