using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Player : BasePlayer
{
    /// <summary>
    /// Information regarding the current status of the player
    /// </summary>
    private status _currently;
    public status currently { get { return _currently; }}
    public enum status {IDLE, PLACING_BUILDING, SELECTED_UNITS /*...*/}

    /// <summary>
    /// Information regarding the entities of the player
    /// </summary>
    private List<IGameEntity> _activeEntities = new List<IGameEntity>();
    public List<IGameEntity> activeEntities {
        get { return new List<IGameEntity>(_activeEntities); }
    }
    
	//the list of player units in the scene
	public ArrayList currentUnits = new ArrayList ();

    // i order to mantain InformationController working
	//public ArrayList SelectedObjects = new ArrayList();
    public ArrayList SelectedObjects { get { return _selection.ToArrayList(); } }

    // Use this for initialization
    public override void Start()
    {   
        base.Start();
        _buildings = GetComponent<Managers.BuildingsManager>();
        //request the race of the player
        _selfRace = info.GetPlayerRace();
        _selection.SetRace(race);
        
    }

    // Update is called once per frame
    void Update() { }

    public override void removeEntity(IGameEntity entity)
    {
        _activeEntities.Remove(entity);
    }

    /// <summary>
    /// Add a IGameEntity to the list
    /// Player has a list with all the entities associated to him
    /// </summary>
    /// <param name="newEntity"></param>
    public override void addEntity(IGameEntity newEntity)
    {
        _activeEntities.Add(newEntity);
        Debug.Log(_activeEntities.Count + " entities");
    }

	public void FillPlayerUnits(GameObject unit) 
	{
		currentUnits.Add (unit);
	}
    
    /// <summary>
    /// Returns the count of the current associated entities
    /// </summary>
    /// <returns></returns>
    public int currentEntitiesCount()
    {
        return _activeEntities.Count;
    }

    /// <summary>
    /// Returns whether is in the current status or not
    /// </summary>
    /// <param name="check"></param>
    /// <returns></returns>
    public bool isCurrently(status check)
    {
        return _currently == check;

    }

    /// <summary>
    /// Establishes the new status of the player
    /// </summary>
    /// <param name="newStatus"></param>
    public void setCurrently(status newStatus)
    {
        _currently = newStatus;
    }

    public ArrayList getSelectedObjects()
    {
        return (ArrayList) SelectedObjects.Clone();
    }

    // <summary>
    // Getter for the resources of the player.
    // </summary>
    //public Managers.ResourcesManager resources {get { return _resources; } }
}
