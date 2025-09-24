using Infrastructure;
using UnityEngine;

public class DebugSetActive : MonoBehaviour
{
    private void OnEnable()
    {
        this.Log(LogType.Log, $"{gameObject.name} OnEnable");
    }
}
