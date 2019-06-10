using UnityEngine;

public interface IPossessable
{
    // Returns the possession state after calling this method
    bool Possess(Possession possessingScript);
    // Returns the possession state after calling this method
    bool Unpossess(bool centerColliderToPos = false, Vector2? forceRespawnPos = null);
}
