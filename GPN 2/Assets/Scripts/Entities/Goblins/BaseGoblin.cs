using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class BaseGoblin : Entity, IClickable, IBuyable
{
    #region Properties
    public int Damage;
    public int Health;
    public int InitialHealth;
    public int Range;
    public int MovementRange;
    public float Cooldown;
    public virtual int Cost() => 1;

    public EntityActionManager actionManager;
    public GameObject unit_card;
    public bool isSelected;
    public bool isMovementBlocked;
    public OCCUPATION_STATE occupationState;
    private List<ArrayList> STATUSES = new List<ArrayList>();

    private List<Entity> entitiesInRange;
    public PhotonView photonView;
    private Transform parent;
    private static int entityIndex = 0;
    #endregion

    #region Constructor
    protected void Start()
    {
        actionManager = GetComponent<EntityActionManager>();
        photonView = GetComponent<PhotonView>();
        parent = transform.parent;

        entitiesInRange = new List<Entity>();
        isSelected = false;
        isMovementBlocked = false;
        occupationState = OCCUPATION_STATE.FREE;
        InitialHealth = Health;

        RenderHealth(parent);

        if (!photonView.IsMine) return;
        Vector3 cardPosition = new Vector3(-2.06f, (0.89f - (0.28f * entityIndex)), 0f);
        unit_card = Instantiate(Resources.Load<GameObject>("Prefabs/UI/unit_card"), cardPosition, Quaternion.identity);
        LocalInventory.getInstance().UpdateEntityListItem(parent.gameObject, entityIndex);
        entityIndex += 1;
        unit_card.GetComponent<UnitCard>().RenderCard(this);
    }
    #endregion

    #region Virtual Methods
    public virtual void UsePassive() { }
    public virtual void UseAbility(InputAction.CallbackContext context) { }
    #endregion

    #region Event Handlers
    public void OnClick(GameObject prevSelection = null)
    {
        bool isSelectionCard = prevSelection != null && prevSelection.GetComponent<Targetable>() != null;
        if (isSelectionCard) {
            prevSelection.GetComponent<Targetable>().UseEffect(this.gameObject);
            return;
        }

        if (!photonView.IsMine) return;
        if (HasStatus(STATUS.PARALYZED)) return;
        Entity prevEntity = prevSelection?.GetComponent<Entity>();
        bool isSelectionGoblin = prevEntity is BaseGoblin;
        BaseGoblin prevGoblin = isSelectionGoblin ? (BaseGoblin) prevEntity : null;

        bool canDeselect = isSelectionGoblin && prevEntity != this;
        bool isTargetable = isSelectionGoblin && prevGoblin.GetEntitiesInRange().Contains(this);

        if (isTargetable) return;
        if (canDeselect) prevGoblin.actionManager.Deselect();

        isSelected = isSelected ? actionManager.Deselect() : actionManager.Select();
    }

    public override void OnDamage(BaseGoblin attackingEntity, Vector3 targetPos)
    {   
        if(HasStatus(BaseGoblin.STATUS.DODGE)) return;
        if (attackingEntity is Assassin) 
        {
            AddStatus(STATUS.POISONED, 3);
        }
        if (attackingEntity is Barrel) 
        {
            AddStatus(STATUS.SLOWED, 2);
            MovementRange -= 1;
        }
        Health -= attackingEntity.Damage;
        if (Health <= 0)
        {
            OnDeath(attackingEntity);
            return;
        }
        RenderHealth(parent);
        unit_card.GetComponent<UnitCard>().RenderCard(this);
    }
    public override void OnDeath(BaseGoblin attackingEntity, Vector3? targetPos = null)
    {    
        if (!photonView.IsMine) {
            if (attackingEntity.photonView.IsMine) LocalInventory.getInstance().AddGold(2);
            Destroy(parent.gameObject);
            return;
        }
        LocalInventory.getInstance().DestroyGoblin(parent.gameObject);
        Destroy(unit_card);
        Destroy(parent.gameObject);
        entityIndex -= 1;
        if (attackingEntity.Range > 1) attackingEntity.isMovementBlocked = true;
    }
    #endregion

    #region Public Methods
    public void ClearEntitiesInRange() => entitiesInRange.Clear();
    public List<Entity> GetEntitiesInRange() => entitiesInRange;
    public Vector3 GetCurrentPos() => new Vector3(transform.position.x, transform.position.y - 0.16f, transform.position.z);
    public void AddEntityToRange(Entity entity) => entitiesInRange.Add(entity);
    public void AddStatus(STATUS status, int duration) => STATUSES.Add(new ArrayList(){status, duration});
    public bool HasStatus(STATUS status) => STATUSES.Any(s => ((STATUS)s[0]).Equals(status));
    #endregion

    private void RenderHealth(Transform parent)
    {
        Sprite healthSprite = Resources.LoadAll<Sprite>("UI_Atlas").Single(sprite => sprite.name.Equals($"{(photonView.IsMine ? "friendly" : "enemy")}_healthbar_{Health}"));
        SpriteRenderer healthbarComponent = parent.Find("healthbar").gameObject.GetComponent<SpriteRenderer>();
        healthbarComponent.sprite = healthSprite;
    }

    public void HandleStatusEffects()
    {
        STATUSES.ForEach(status => {
            status[1] = ((int) status[1]) - 1;
            switch(status[0])
            {
                case STATUS.POISONED:
                    Health -= 1;
                    break;
                case STATUS.REGENERATE:
                    if (Health < InitialHealth) Health += 1;
                    break;
                case STATUS.NONE:
                    break;
                default:
                    break;
            }

            if (!(((int) status[1]) <= 0)) return;
            if (((STATUS) status[0]) == STATUS.SLOWED) MovementRange += 1;
        });
        STATUSES.RemoveAll(status => ((int) status[1]) <= 0);
    }

    public enum STATUS
    {
        SLOWED,
        POISONED,
        TAUNTED,
        SILENT,
        SILENCED,
        DODGE,
        PARALYZED,
        MIND_CONTROLLED,
        REGENERATE,
        NONE
    }

    public enum OCCUPATION_STATE
    {
        FREE,
        CAPTURING,
        CAPTURED
    }
}