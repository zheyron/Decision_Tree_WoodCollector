using TMPro;
using UnityEngine;

public class NPCDebugWorldLabel : MonoBehaviour
{
    [SerializeField] private CowardCollectorNPC npc;
    [SerializeField] private TMP_Text labelText;

    void Reset()
    {
        labelText = GetComponent<TMP_Text>();
    }

    void Start()
    {
        if (labelText == null)
            labelText = GetComponent<TMP_Text>();

        if (npc == null)
            npc = GetComponentInParent<CowardCollectorNPC>();
    }

    void LateUpdate()
    {
        if (npc == null || labelText == null) return;

        bool threatNear = false;
        if (npc.threat != null)
            threatNear = Vector3.Distance(npc.transform.position, npc.threat.position) <= npc.threatDistance;

        labelText.text =
            $"Action: {npc.currentActionName}\n" +
            $"HP: {npc.HP:0}\n" +
            $"Hunger: {npc.hunger:0}\n" +
            $"Wood: {npc.woodAmount}/{npc.maxWoodCapacity}\n" +
            $"Night: {npc.isNight}\n" +
            $"Threat: {threatNear}";
    }
}