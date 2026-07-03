using UnityEngine;

public class MarketRoleManager : MonoBehaviour
{
		[SerializeField] private PlayerRoleData roleData;
		[SerializeField] private GameObject sklavePlayer;
		[SerializeField] private GameObject exitGate;
		[SerializeField] private GameObject baeckerNPC;
		
    private XROriginSwitcher _xrOriginSwitcher = new XROriginSwitcher();

    void Start()
    {
				if (roleData.CurrentRole == PlayerRole.Sklave) {
						_xrOriginSwitcher.xrOriginTag = "Player";
				}
				else {
						_xrOriginSwitcher.xrOriginTag = "Sklave";
						if (sklavePlayer != null && sklavePlayer.CompareTag("Player"))
								sklavePlayer.tag = "Untagged";
						NextSceneCollider ncs = exitGate.GetComponent<NextSceneCollider>();
						ncs.sceneNameToLoad = "RaumSenator";
				}

				if (baeckerNPC != null)
						baeckerNPC.SetActive(roleData.CurrentRole != PlayerRole.Haendler);

        _xrOriginSwitcher.FindAndDisableXROrigin();
    }

    void OnDestroy()
    {
    		_xrOriginSwitcher.RestoreXROrigin();
    }
}
