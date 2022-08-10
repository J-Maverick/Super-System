using UnityEngine;

public static class MiscExtensions
{
    
    public static bool HasComponent <T>(this GameObject obj) where T:Component
        {
        return obj.GetComponent<T>() != null;
        }
    

}
