using UnityEngine;

namespace Small_Corner_Map.Helpers
{
    internal class Utils
    {
        internal static void RecursiveFind(Transform current, string targetName, List<Transform> result)
        {
            if (current.name == targetName)
            {
                result.Add(current);
            }
            for (int i = 0; i < current.childCount; i++)
            {
                RecursiveFind(current.GetChild(i), targetName, result);
            }
        }
    }
}
