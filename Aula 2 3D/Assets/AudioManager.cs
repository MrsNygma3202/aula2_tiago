using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
   priva void Awake()
   {
       if (instance == null)
       {
           instance = this;
               DontDestroyOnLoad(gameObject);
       }
       else
       {
           Destroy(gameObject);
       }
   } 
}
