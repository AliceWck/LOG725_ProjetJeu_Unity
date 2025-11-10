using UnityEngine;

public interface ILightSource
{
    bool IsPlayerInLight(Vector3 playerPosition);

    Vector3 GetLightPosition();

    bool IsGuardianLight();
}