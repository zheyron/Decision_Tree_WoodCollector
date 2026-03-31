using UnityEngine;
using UnityEngine.InputSystem;

public class CowardCollectorNPC : MonoBehaviour
{
    [Header("Required by exercise")]
    public float HP = 100f;
    public int woodAmount = 0;
    public bool isNight = false;
    public Transform storage;

    [Header("World references")]
    public Transform treePoint;
    public Transform foodPoint;
    public Transform safeZone;
    public Transform refugePoint;
    public Transform threat;

    [Header("Config")]
    public float moveSpeed = 3f;
    public float decisionInterval = 0.5f;
    public float lowHPThreshold = 35f;
    public float injuredThreshold = 70f;
    public int maxWoodCapacity = 3;
    public float threatDistance = 5f;
    public float hunger = 0f;
    public float hungerThreshold = 60f;
    public float hungerIncreasePerSecond = 5f;
    public float arriveDistance = 0.2f;
    public float fleeDuration = 2f;
    public float hideDuration = 2f;

    [Header("Debug")]
    public string currentActionName = "None";

    [Header("Debug Visuals")]
    public bool drawDebug = true;
    public bool drawAlways = true;
    public float pointGizmoSize = 0.35f;

    private ITreeNode root;
    private float decisionTimer = 0f;
    private Transform currentDestination;

    private float actionTimer = 0f;
    private bool actionLocked = false;

    private enum NPCAction
    {
        None,
        Flee,
        Hide,
        SearchFood,
        Rest,
        DepositWood,
        CollectWood,
        Sleep
    }

    private NPCAction currentAction = NPCAction.None;

    void Start()
    {
        BuildTree();
    }

    void Update()
    {
        UpdateNeeds();

        if (drawDebug)
        {
            DrawRuntimeDebug();
        }

        HandleActionTimer();
        MoveToDestination();
        ResolveArrival();

        if (actionLocked)
            return;

        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0f)
        {
            root.Execute();
            decisionTimer = decisionInterval;
        }
    }

    private void BuildTree()
    {
        ActionNode fleeAction = new ActionNode(FleeToSafeZone);
        ActionNode hideAction = new ActionNode(HideFromThreat);
        ActionNode searchFoodAction = new ActionNode(GoSearchFood);
        ActionNode restAction = new ActionNode(GoRest);
        ActionNode depositWoodAction = new ActionNode(GoDepositWood);
        ActionNode collectWoodAction = new ActionNode(GoCollectWood);
        ActionNode sleepAction = new ActionNode(GoSleep);

        QuestionNode woodAtNightNode = new QuestionNode(HasWood, depositWoodAction, sleepAction);
        QuestionNode maxWoodNode = new QuestionNode(IsWoodFull, depositWoodAction, collectWoodAction);
        QuestionNode nightNode = new QuestionNode(IsNightCheck, woodAtNightNode, maxWoodNode);
        QuestionNode injuredNode = new QuestionNode(IsInjured, restAction, nightNode);
        QuestionNode hungryNode = new QuestionNode(IsHungry, searchFoodAction, injuredNode);

        QuestionNode threatResponseNode = new QuestionNode(HasEnoughHPToFlee, fleeAction, hideAction);
        QuestionNode threatNode = new QuestionNode(IsThreatNear, threatResponseNode, hungryNode);

        root = threatNode;
    }

    private void UpdateNeeds()
    {
        hunger += hungerIncreasePerSecond * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0f, 100f);

        if (Keyboard.current != null)
        {
            if (Keyboard.current.nKey.wasPressedThisFrame)
                isNight = !isNight;

            if (Keyboard.current.hKey.wasPressedThisFrame)
                HP = Mathf.Max(0f, HP - 10f);

            if (Keyboard.current.jKey.wasPressedThisFrame)
                HP = Mathf.Min(100f, HP + 10f);
        }
    }

    private void HandleActionTimer()
    {
        if (!actionLocked) return;

        actionTimer -= Time.deltaTime;

        if (actionTimer <= 0f)
        {
            actionLocked = false;

            if (currentAction == NPCAction.Flee || currentAction == NPCAction.Hide)
            {
                currentDestination = null;
            }

            currentAction = NPCAction.None;
            currentActionName = "Reevaluando árbol";
        }
    }

    private void MoveToDestination()
    {
        if (currentDestination == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = currentDestination.position;
        targetPos.y = currentPos.y;

        transform.position = Vector3.MoveTowards(
            currentPos,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }

    private void ResolveArrival()
    {
        if (currentDestination == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = currentDestination.position;
        targetPos.y = currentPos.y;

        if (Vector3.Distance(currentPos, targetPos) > arriveDistance) return;

        switch (currentAction)
        {
            case NPCAction.Flee:
                currentDestination = safeZone;
                break;

            case NPCAction.SearchFood:
                hunger = 0f;
                currentDestination = null;
                break;

            case NPCAction.Rest:
                HP = Mathf.Min(100f, HP + 25f);
                currentDestination = null;
                break;

            case NPCAction.DepositWood:
                woodAmount = 0;
                currentDestination = null;
                break;

            case NPCAction.CollectWood:
                woodAmount = Mathf.Min(woodAmount + 1, maxWoodCapacity);
                currentDestination = null;
                break;

            case NPCAction.Sleep:
                isNight = false;
                HP = Mathf.Min(100f, HP + 10f);
                currentDestination = null;
                break;
        }
    }

    private bool IsThreatNear()
    {
        if (threat == null) return false;
        return Vector3.Distance(transform.position, threat.position) <= threatDistance;
    }

    private bool HasEnoughHPToFlee() => HP > lowHPThreshold;
    private bool IsHungry() => hunger >= hungerThreshold;
    private bool IsInjured() => HP < injuredThreshold;
    private bool IsNightCheck() => isNight;
    private bool HasWood() => woodAmount > 0;
    private bool IsWoodFull() => woodAmount >= maxWoodCapacity;

    private void FleeToSafeZone()
    {
        if (actionLocked && currentAction == NPCAction.Flee)
            return;

        currentAction = NPCAction.Flee;
        currentActionName = "Huir a zona segura";
        currentDestination = safeZone;

        actionLocked = true;
        actionTimer = fleeDuration;
    }

    private void HideFromThreat()
    {
        if (actionLocked && currentAction == NPCAction.Hide)
            return;

        currentAction = NPCAction.Hide;
        currentActionName = "Mantener distancia / Esconderse";
        currentDestination = null;

        actionLocked = true;
        actionTimer = hideDuration;
    }

    private void GoSearchFood()
    {
        currentAction = NPCAction.SearchFood;
        currentActionName = "Buscar comida";
        currentDestination = foodPoint;
    }

    private void GoRest()
    {
        currentAction = NPCAction.Rest;
        currentActionName = "Curarse / Descansar";
        currentDestination = refugePoint;
    }

    private void GoDepositWood()
    {
        currentAction = NPCAction.DepositWood;
        currentActionName = "Ir a storage y dejar madera";
        currentDestination = storage;
    }

    private void GoCollectWood()
    {
        currentAction = NPCAction.CollectWood;
        currentActionName = "Ir al árbol y recolectar madera";
        currentDestination = treePoint;
    }

    private void GoSleep()
    {
        currentAction = NPCAction.Sleep;
        currentActionName = "Ir a refugio y dormir";
        currentDestination = refugePoint;
    }

    #region Debug
    private Color GetActionColor()
    {
        switch (currentAction)
        {
            case NPCAction.Flee: return Color.red;
            case NPCAction.Hide: return new Color(1f, 0.5f, 0f);
            case NPCAction.SearchFood: return Color.yellow;
            case NPCAction.Rest: return Color.magenta;
            case NPCAction.DepositWood: return Color.blue;
            case NPCAction.CollectWood: return Color.green;
            case NPCAction.Sleep: return Color.cyan;
            default: return Color.white;
        }
    }

    private void DrawRuntimeDebug()
    {
        Color actionColor = GetActionColor();

        if (currentDestination != null)
        {
            Debug.DrawLine(transform.position, currentDestination.position, actionColor);
        }

        if (threat != null)
        {
            bool danger = Vector3.Distance(transform.position, threat.position) <= threatDistance;
            Color threatColor = danger ? Color.red : Color.green;
            Debug.DrawLine(transform.position, threat.position, threatColor);
        }

        Debug.DrawLine(transform.position, transform.position + Vector3.up * 2f, actionColor);
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug || !drawAlways) return;
        DrawGizmosInternal();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebug || drawAlways) return;
        DrawGizmosInternal();
    }

    private void DrawGizmosInternal()
    {
        Color actionColor = Application.isPlaying ? GetActionColor() : Color.white;

        Gizmos.color = actionColor;
        Gizmos.DrawWireSphere(transform.position, 0.4f);

        Color threatRadiusColor = Color.yellow;

        if (threat != null)
        {
            bool danger = Vector3.Distance(transform.position, threat.position) <= threatDistance;
            threatRadiusColor = danger ? Color.red : Color.green;
        }

        Gizmos.color = threatRadiusColor;
        Gizmos.DrawWireSphere(transform.position, threatDistance);

        if (currentDestination != null)
        {
            Gizmos.color = actionColor;
            Gizmos.DrawLine(transform.position, currentDestination.position);
            Gizmos.DrawSphere(currentDestination.position, 0.15f);
        }

        if (threat != null)
        {
            bool danger = Vector3.Distance(transform.position, threat.position) <= threatDistance;
            Gizmos.color = danger ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, threat.position);
            Gizmos.DrawWireSphere(threat.position, 0.5f);
        }

        DrawPointGizmo(treePoint, new Color(0f, 0.8f, 0f));
        DrawPointGizmo(storage, Color.blue);
        DrawPointGizmo(foodPoint, Color.yellow);
        DrawPointGizmo(safeZone, Color.red);
        DrawPointGizmo(refugePoint, Color.cyan);
    }

    private void DrawPointGizmo(Transform point, Color color)
    {
        if (point == null) return;

        Gizmos.color = color;
        Gizmos.DrawSphere(point.position, pointGizmoSize);
        Gizmos.DrawWireSphere(point.position, pointGizmoSize + 0.08f);
    }
    #endregion
}