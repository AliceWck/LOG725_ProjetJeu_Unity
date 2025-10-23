using UnityEngine;

public interface ILightSource
{
    bool IsPlayerInLight(Vector3 playerPosition);
}