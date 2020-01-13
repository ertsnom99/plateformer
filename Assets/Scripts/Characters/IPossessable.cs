using UnityEngine;

public interface IPossessable
{
    // Returns the possession state after calling this method
    bool Possess(PossessionPower possessingScript, PlayerController possessingController);
    // Returns the possession state after calling this method
    // HACK: Not sure if this method should just return if "IsPossessed"
    GameObject Unpossess(bool centerColliderToPos = false, Vector2? forceRespawnPos = null);
}
