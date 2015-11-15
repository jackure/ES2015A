﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Storage;


public class Resource : Building<Resource.Actions>
{
    public enum Actions { CREATED, DAMAGED, DESTROYED, COLLECTION, CREATE_UNIT };

    /// <summary>
    /// civilian creation waste some time. When units are being created status
    /// changes to RUN. Process can only be started while IDLE status.
    /// </summary>
    public enum createCivilStatus { IDLE, RUN, DISABLED };

    public Statistics statistics;

    // Constructor
    public Resource() { }

    private IGameEntity _entity;

    private createCivilStatus _createStatus;

    /// <summary>
    /// status of the create civilian option. 
    /// RUN when civilian unit is being created
    /// IDLE when option is availabe.
    /// DISABLE when option is disabled.
    /// </summary>
    /// 
    public createCivilStatus buttonCivilStatus
    {
        get
        {
            return _createStatus;
        }
    }

    /// <summary>
    /// new civil creation spends some time. makingNewCivil is true if unit 
    /// construction is in process, false otherwise.
    /// </summary>
    private bool _makingNewCivil = false;

    /// <summary>
    /// New civilian is being made.
    /// </summary>
    public bool HUD_newCivilBeingMade
    {
        get
        {
            return _makingNewCivil;
        }
    }
    /// <summary>
    /// tiem period spend making new civilian unit
    /// </summary>
    private float _constructionTime;

    /// <summary>
    /// Controls time elapsed since civil creation process was started
    /// </summary>
    private float endConstructionTime;   
    
    /// <summary>
    ///  Next update time
    /// </summary>
    private float _nextUpdate;

    /// <summary>
    // Resource could store limited amount of material when not in use
    // or when production is higher than collection
    /// </summary>
    private float _stored;

    /// <summary>
    /// sum of capacity of units collecting this resource
    /// the more units the more collectionRate.
    /// real collectionRate could be lower due to
    /// store limit.
    /// </summary>
    private float _collectionRate { get; set; }

    /// <summary>
    ///  units currently working and collecting this resource
    /// </summary>
    /// 
    public int harvestUnits { get; private set; }

    /// <summary>
    /// info of civilian units
    /// </summary>
    private UnitInfo civilInfo;

    /// <summary>
    /// list of civilian units working at this resource
    /// </summary>
    List<Unit> workersList = new List<Unit>();

    /// <summary>
    /// HUD, get current civilian units working here
    /// </summary>
    public int HUD_currentWorkers
    {
        get
        {
            return harvestUnits;
        } 
   }

    /// <summary>
    /// HUD, max production rate for this resource building and level.
    /// </summary>
    public int HUD_productionRate
    {
        get
        {
            return info.resourceAttributes.productionRate;
        }
    }

    /// <summary>
    /// HUD, max production rate for this resource building and level.
    /// </summary>
    public int HUD_currentProductionRate
    {
        get
        {
            return civilInfo.attributes.capacity * harvestUnits;
        }
    }

    /// <summary>
    /// HUD, max storing capacity for this level and resource building type
    /// </summary>
    public int HUD_storeSize
    {
        get
        {
            return info.resourceAttributes.storeSize;
        }
    }

    /// <summary>
    /// number of units created by this building. dead or alive
    /// </summary>
    public int totalUnits { get ;private set;}

    /// <summary>
    /// material amount send to player (collected) when update succes.
    /// </summary>
    private float _collectedAmount;

    /// <summary>
    /// this building can create units.
    /// unitPosition are the x,y,z map coordinates of new civilian
    /// </summary>
    private Vector3 _unitPosition;

    /// <summary>
    /// this building can create units.
    /// unitRotation is the rotation of new civilian
    /// </summary>
    private Quaternion _unitRotation;

    /// <summary>
    /// coordinates where new civilians are positioned before maxUnits limit is
   ///  reached.
    /// </summary>
    private Vector3 meetingPointInsidePosition;

    /// <summary>
    /// coordinates where new civilians are positioned after maxUnits limit is
    ///  reached.
    /// </summary>
    private Vector3 meetingPointOutsidePosition;

    /// <summary>
    /// when you create a civilian some displacement is needed to avoid units 
    /// overlap. this is the x-axis displacement
    /// </summary>
    private int _xDisplacement;

    /// <summary>
    /// when you create a civilian some displace is needed to avoid units 
    /// overlap. this is the y-axis displacement
    /// </summary>
    private int _yDisplacement;

    /// <summary>
    ///  x, y, z coordinates of our building
    /// </summary>
    private Vector3 _center
    {
        get
        {
            return transform.position;
        }     
    }
    /// <summary>
    /// current player
    /// </summary>
    private Player player;
    
    /// <summary>
    /// check if starter unit was created. We need to wait until resource is built
    /// </summary>
    public bool hasDefaultUnit {get; private set;}


    private readonly object syncLock = new object();
    bool hasCreatedCivil = false;
    List<GameObject> pendingProducers = new List<GameObject>();
    List<GameObject> pendingWanderers = new List<GameObject>();


    /// <summary>
    /// check if collecting unit type matchs rigth resource type
    /// </summary>
    /// <param name="unitType"></param>
    /// <param name="type"></param>
    /// <returns>
    /// true if resource and unit type match,
    /// false otherwise
    /// </returns>
    private bool match(UnitTypes unitType, BuildingTypes type)
    {
        return unitType == UnitTypes.CIVIL;
    }

    /// <summary>
    /// civilians units collect resources each production cicle.
    /// the sum of units capacity is the total amount of materials they can 
    /// take from the store and send to player. 
    /// </summary>
    private void collect()
    {
        
        if (_collectionRate > _stored)
        {
            // collect all stored resources
            _collectedAmount = _stored;           
            _stored = 0;
        }
        else
        {
            // collection capacity lower than stored materials. some materials
            //remain at store until new collection cycle.
            _collectedAmount = _collectionRate;
            _stored -= _collectedAmount;
        }
        sendResource(_collectedAmount);
        return;
    }

    /// <summary>
    /// after civilians sends last batch produced they are able to take the 
    /// new production and store it for the next production cycle
    /// </summary>
    private void produce()
    {
        float remainingSpace = info.resourceAttributes.storeSize - _stored;

        // Production rate bigger than remaining store space means we will 
        // lose part or whole production!!

        if (info.resourceAttributes.productionRate >= remainingSpace)
        {
            _stored = info.resourceAttributes.storeSize;
        }
        else
        {
            _stored += info.resourceAttributes.productionRate;
        }
        return;
    }

    /// <summary>
    /// New goods produced are sent to player.
    /// Method triger an event sending object goods with amount of materials 
    /// transferred. gold production is sent too.
    /// </summary>
    /// <param name="amount">materials amount produced</param>
    /// 
    /// TODO: now we are using two diferent ways to increase player resources
    /// 1- Create classe goods and send it to player using event.
    /// 2- Direct use of addAmount method.
    /// 
    /// we must change this behaviour, only one way will be the right one.

    private void sendResource(float amount)
    {

        if (amount  > 0.0)
        {
            Goods goods = new Goods();
            goods.amount = amount;

            // TODO: 
            // BUG: Null reference when we try to add material amount to player.

            if (type.Equals(BuildingTypes.FARM))
            {
                //Player.getOwner(_entity).resources.AddAmount(WorldResources.Type.FOOD, amount); 
                goods.type = Goods.GoodsType.FOOD;
            }
            else if(type.Equals(BuildingTypes.MINE))
            {
                //BasePlayer.getOwner(_entity).resources.AddAmount(WorldResources.Type.METAL, amount);
                goods.type = Goods.GoodsType.METAL;
            }
            else
            {
                // BasePlayer.getOwner(_entity).resources.AddAmount(WorldResources.Type.WOOD, amount);
                goods.type = Goods.GoodsType.WOOD;
            }
            fire(Actions.COLLECTION, goods);
        }         
    }

    public void createCivilian()
    {
        if (!_makingNewCivil)
        {
            _makingNewCivil = true;
            _createStatus = createCivilStatus.RUN;
            endConstructionTime = Time.time + _constructionTime;
        }
        
    }
    /// <summary>
    /// Method create civilian unit.
    /// If capacity limit of building is not reached unit is positioned inside 
    /// building limits otherwise unit is positioned outside, 
    /// just at desired meeting Point.
    /// civilian sex is randomly selected(last parameter of createUnit method).
    /// </summary>
    /// <returns>civilian GameObject</returns>
    private void newCivilian()
    {

        // TODO set desired rotation, now unit rotation equals building rotation!!
        // TODO  ---create gameobject meetingPointInside and meetingPointOutside
        // attached to resource building design--- just Waiting for designners team.

        //---unComment next two lines when meeting point objects are created---
        //GameObject meetingPointInside = this.GetComponent(meetingPointInside);
        //GameObject meetingPointOutside = this.GetComponent(meetingPointOutside);

        // only one civilian is placed inside building limits until designners team
        // enabled some space to place units.

        //if (harvestUnits < info.resourceAttributes.maxUnits)
        if(harvestUnits < 1)
        {
            // TODO get inside meeting point and calculate position
            //unitPosition = this.GetComponent(meetingPointInside).transform.position;

            // Units distributed in rows of 5 elements
            
            _xDisplacement = harvestUnits % 5;
            _yDisplacement = harvestUnits / 5;
            _unitPosition.Set(_center.x + _xDisplacement, _center.y , _center.z + _yDisplacement );
            
            // Method createUnit from Info returns GameObject Instance;
            GameObject gob = Info.get.createUnit(race, UnitTypes.CIVIL, _unitPosition, _unitRotation, -1);

            Unit civil = gob.GetComponent<Unit>();
            civil.role = Unit.Roles.PRODUCING;            
            BasePlayer.getOwner(this).addEntity(civil);
            fire(Actions.CREATE_UNIT, civil);

            totalUnits++;
            harvestUnits++;
            workersList.Add(civil);
            

            _collectionRate += Info.get.of(race, UnitTypes.CIVIL).attributes.capacity;
        }
        else
        {
            // TODO get outside meeting point and calculate position
            _xDisplacement = (totalUnits - harvestUnits) % 5;
            _yDisplacement = (totalUnits - harvestUnits) / 5;
            _unitPosition.Set(_center.x + 10 + _xDisplacement, _center.y  , _center.z + 10 + _yDisplacement);
            GameObject gob = Info.get.createUnit(race, UnitTypes.CIVIL, _unitPosition, _unitRotation, -1);

            Unit civil = gob.GetComponent<Unit>();
            civil.role = Unit.Roles.WANDERING;

            BasePlayer.getOwner(this).addEntity(civil);
            fire(Actions.CREATE_UNIT, civil);

            totalUnits++;

        }
        _createStatus = createCivilStatus.IDLE;
        _makingNewCivil = false;
    }

    /// <summary>
    /// Recruit a Explorer from building. you need to do this to take away worker
   ///  from building. production decrease when you remove workers
   /// </summary>
    private void recruitExplorer(Unit worker)
    {
        if (harvestUnits > 0)
        {
            _collectionRate -= worker.info.attributes.capacity;
            harvestUnits--;

            worker.role = Unit.Roles.WANDERING;
            workersList.Remove(worker);
        }

        // No workers
        if (harvestUnits == 0)
        {
            setStatus(EntityStatus.IDLE);
        }
        // TODO: Some alert message if you try to remove unit when no unit at building
    }

    /// <summary>
    /// Recruit a worker. you can use a explorer as a worker. beware of building maxUnits.
    /// </summary>
    private void recruitWorker(Unit explorer)
    {
       
        if (harvestUnits < info.resourceAttributes.maxUnits)
        {
            _collectionRate -= explorer.info.attributes.capacity;
            harvestUnits++;

            explorer.role = Unit.Roles.PRODUCING;
            workersList.Add(explorer);
        }
        if (harvestUnits == 1)
        {
            setStatus(EntityStatus.WORKING);
        }
        Debug.Log(" You are trying to recruit worker but building capacity is full"); 
    }

    private WorldResources.Type getResourceType()
    {
        switch (type) {
            case BuildingTypes.FARM:
                return WorldResources.Type.FOOD;
            case BuildingTypes.MINE:
                return WorldResources.Type.METAL;
            case BuildingTypes.SAWMILL:
                return WorldResources.Type.WOOD;
            default:
                throw new Exception("That resource type does not exist!");
        }
    }

    /// <summary>
    /// when collider interact with other gameobject method checks if 
    /// gameobject is a civilian unit. Civilians units are recruited as workers
    /// while limit of workers are not reached.  
    /// </summary>
    /// <param name="other">collider gameobject interacting with our own collider</param>
    void OnTriggerEnter(Collider other)
    {
        
        // space enough to hold new civil
        
        if (harvestUnits < info.resourceAttributes.maxUnits)
        {
            IGameEntity entity = other.gameObject.GetComponent<IGameEntity>();

            if (entity.info.isUnit)
            {
                if (entity.info.isCivil)
                {
                    recruitWorker((Unit)entity);
                }
            }
        }
    }

    /// <summary>
    /// If unit inside building is attacked and killed we must recalculate 
    /// collection rate and current harvestUnits. No harvestUnits means no
   ///  production or collection so IDLE status.
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay(Collider other)
    {

        IGameEntity entity = other.gameObject.GetComponent<IGameEntity>();
        if ((entity.info.isUnit)&&(entity.info.isCivil))
        {
            if (entity.status == EntityStatus.DEAD)
            {
                _collectionRate -= entity.info.attributes.capacity;
                harvestUnits--;
                if (harvestUnits == 0)
                {
                    setStatus(EntityStatus.IDLE);
                }
            }
        }
    }

    /// <summary>
    /// when collider interaction with other gameobject ends method checks if
    /// gameobject is civilian unit. Civilians units are recruited as explorers
    /// and fired as workers.
    /// </summary>
    /// <param name="other">collider gameobject interacting with our own collider</param>
    void OnTriggerExit(Collider other)

    {
        // get entity
        IGameEntity entity = other.gameObject.GetComponent<IGameEntity>();

        if (harvestUnits < info.resourceAttributes.maxUnits)
        {
            if (entity.info.isUnit)
            {
                if (entity.info.isCivil)
                {
                    recruitExplorer((Unit)entity);
                }
            }  
        }
    }

    /// <summary>
    /// Workers can be attacked inside buildings???. waiting for design decission
    /// </summary>
    /// <param name="unit"></param>
    private void onUnitDestroy(Unit unit)
    {
        
    }


    /// <summary>
    /// When building is destroyed civilian workers turns into explorers
    /// </summary>
    public override void OnDestroy()
    {        
        foreach (Unit unit in workersList)
        {
            unit.role = Unit.Roles.WANDERING;
            harvestUnits--;
        }
            base.OnDestroy();
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    override public void Awake()
    {       
        _nextUpdate = 0;
        _stored = 0;
        _collectionRate = 0;
        harvestUnits = 0;
        _xDisplacement = 0;
        _yDisplacement = 0;
        _info = Info.get.of(race, type);
        totalUnits = 0;
        _unitRotation = transform.rotation;
        hasDefaultUnit = false;
        civilInfo = Info.get.of(this.race, UnitTypes.CIVIL);
        _makingNewCivil = false;
        _constructionTime = 10;
        _entity = this.GetComponent<IGameEntity>();

        // Call Building start
        base.Awake();
    }

    override public void Start()
    {
        // Setup base
        base.Start();
        this.GetComponent<Rigidbody>().isKinematic = false;

        GameObject gameInformationObject = GameObject.Find("GameInformationObject");
        GameObject gameController = GameObject.Find("GameController");
        ResourcesPlacer res_pl = gameController.GetComponent<ResourcesPlacer>();

        if (Player.getOwner(_entity).race.Equals(gameInformationObject.GetComponent<GameInformation>().GetPlayerRace()))
        {
            register(Actions.COLLECTION, res_pl.onCollection);
            register(Actions.CREATED, res_pl.onStatisticsCreated);
        }

        statistics = new Statistics(getResourceType(), (int)info.resourceAttributes.updateInterval, info.resourceAttributes.storeSize);

        fire(Actions.CREATED, statistics);
    }


    // Update is called once per frame
    // when updated, collecting units load materials from store and send it to
    // player.After they finish sending materials, production cycle succes.
    // new produced materials can be stored but not collected until
    // next update.
    override public void Update()
    {
        base.Update();

        switch (status)
        {

            case EntityStatus.IDLE:

                if (!hasDefaultUnit)
                {
                    createCivilian();
                    hasDefaultUnit = true;
                }
                break;

            case EntityStatus.WORKING:

                if (Time.time > _nextUpdate)
                {
                    _nextUpdate = Time.time + info.resourceAttributes.updateInterval;
                    collect();
                    produce();
                         
                }
                break;
        }
        if (_createStatus == createCivilStatus.RUN)
        {
            if (Time.time > endConstructionTime)
            {
                _createStatus = createCivilStatus.DISABLED;
                newCivilian();
                setStatus(EntityStatus.WORKING);
                
            }

        }
    
    }
}
