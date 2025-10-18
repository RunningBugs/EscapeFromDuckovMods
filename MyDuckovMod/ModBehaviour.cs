using UnityEngine;

namespace MyDuckovMod
{
    // This class is discovered via: info.ini name => namespace => ModBehaviour class
    // Example: name=MyDuckovMod => loads MyDuckovMod.ModBehaviour
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        void Awake()
        {
            Debug.Log("MyDuckovMod loaded");
        }

        void OnEnable()
        {
            // Register to game events here if needed
        }

        void OnDisable()
        {
            // Unregister events here
        }
    }
}

