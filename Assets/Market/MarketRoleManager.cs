using UnityEngine;
using UnityEngine.AI;

public class MarketRoleManager : MonoBehaviour
{
		[SerializeField] private PlayerRoleData roleData;
		[SerializeField] private GameObject sklavePlayer;
		[SerializeField] private GameObject exitGate;
		[SerializeField] private GameObject baeckerNPC;

		[Header("Nur für die Senator-Rolle")]
		[SerializeField] private GameObject senatorNPC;
		[SerializeField] private GameObject introManager;
		[SerializeField] private GameObject sklaveZumKauf1;
		[SerializeField] private GameObject sklaveZumKauf2;

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

				if (roleData.CurrentRole == PlayerRole.Senator)
				{
						if (senatorNPC != null)
								senatorNPC.SetActive(false);

						if (introManager != null)
								introManager.SetActive(false);

						DeaktiviereSklavenBewegung(sklaveZumKauf1);
						DeaktiviereSklavenBewegung(sklaveZumKauf2);
				}
				else
				{
						SetzeSklavenKaufbar(sklaveZumKauf1, false);
						SetzeSklavenKaufbar(sklaveZumKauf2, false);
				}

        _xrOriginSwitcher.FindAndDisableXROrigin();
    }

    void OnDestroy()
    {
    		_xrOriginSwitcher.RestoreXROrigin();
    }

    private void DeaktiviereSklavenBewegung(GameObject sklave)
    {
				if (sklave == null) return;

				NPCNavigator navigator = sklave.GetComponent<NPCNavigator>();
				if (navigator != null)
						navigator.enabled = false;

				NavMeshAgent agent = sklave.GetComponent<NavMeshAgent>();
				if (agent != null)
						agent.enabled = false;
    }

    private void SetzeSklavenKaufbar(GameObject sklave, bool kaufbar)
    {
				if (sklave == null) return;

				SklaveKauf kauf = sklave.GetComponent<SklaveKauf>();
				if (kauf != null)
						kauf.SetKaufbar(kaufbar);
    }
}
