using UnityEngine;

public enum PlayerRole { None, Sklave, Haendler, Senator }

[CreateAssetMenu(fileName = "PlayerRoleData", menuName = "Museum ROM/Player Role")]
public class PlayerRoleData : ScriptableObject
{
    [SerializeField] private PlayerRole _currentRole = PlayerRole.Sklave;

    // Jeder kann lesen
    public PlayerRole CurrentRole => _currentRole;

    // Jeder kann setzen – aber kontrolliert über die Methode
    public void SetRole(PlayerRole newRole)
    {
        _currentRole = newRole;
    }
}
