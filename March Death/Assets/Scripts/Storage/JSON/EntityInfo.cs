﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public enum EntityType { UNIT, BUILDING, RESOURCE };
    public enum Races { MEN, ELVES, DWARFS, LIZARDMEN, GREENSKINS, CHAOS, SKAVEN, UNDEAD, OGRES };

    public abstract class EntityInfo
    {
        public EntityType entityType = EntityType.UNIT;
        public Races race = 0;
        public string name = "";

        public EntityResources resources;
        public abstract EntityAttributes attributes { get; set; }
        public abstract List<EntityAbility> abilities { get; set; }

        public abstract T getType<T>() where T : struct, IConvertible;

        /// <summary>
        /// Returns true if the entity is a unit, false otherwise
        /// </summary>
        public bool isUnit
        {
            get
            {
                return entityType == EntityType.UNIT;
            }
        }

        /// <summary>
        /// Returns true if the unit is civil, false otherwise
        /// </summary>
        public bool isCivil
        {
            get
            {
                return isUnit && ((UnitInfo)this).type == UnitTypes.CIVIL;
            }
        }

        /// <summary>
        /// Returns true if the unit is of the army, false otherwise
        /// </summary>
        public bool isArmy
        {
            get
            {
                return isUnit && !isCivil;
            }
        }

        /// <summary>
        /// Returns true if the entity is a building, false otherwise
        /// </summary>
        public bool isBuilding
        {
            get
            {
                return entityType == EntityType.BUILDING;
            }
        }

        /// <summary>
        /// Returns true if the entity is a resource, false otherwise
        /// </summary>
        public bool isResource
        {
            get
            {
                return isBuilding && ((((BuildingInfo) this).type == BuildingTypes.FARM) || (((BuildingInfo) this).type == BuildingTypes.SAWMILL) || (((BuildingInfo) this).type == BuildingTypes.MINE));
            }
        }

        /// <summary>
        /// Returns true if the entity is a resource, false otherwise
        /// </summary>
        public bool isBarrack
        {
            get
            {
                return isBuilding && ((((BuildingInfo)this).type == BuildingTypes.BARRACK));
            }
        }

        /// <summary>
        /// Returns true if the entity is an Archery building, false otherwise
        /// </summary>
        public bool isArchery
        {
            get
            {
                return ((((BuildingInfo)this).type == BuildingTypes.ARCHERY));
            }
        }

        /// <summary>
        /// Returns true if the entity is an Archery building, false otherwise
        /// </summary>
        public bool isStable
        {
            get
            {
                return ((((BuildingInfo)this).type == BuildingTypes.STABLE));
            }
        }

        /// <summary>
        /// Returns true if the entity is Stronghold building, false otherwise
        /// </summary>
        public bool isStronghold
        {
            get
            {
                return ((((BuildingInfo)this).type == BuildingTypes.STRONGHOLD));
            }
        }

        /// <summary>
        /// If this info describes a unit, returns the UnitAttributes class, otherwise it returns null
        /// It should always be used either by first checking isUnit, or checking if returned value is not null
        /// </summary>
        public UnitAttributes unitAttributes
        {
            get
            {
                if (!isUnit)
                {
                    return null;
                }

                return (UnitAttributes)this.attributes;
            }
        }

        /// <summary>
        /// If this info describes a resource, returns the ResourceAttributes class, otherwise it returns null
        /// It should always be used either by first checking isResource, or checking if returned value is not null
        /// </summary>
        public ResourceAttributes resourceAttributes
        {
            get
            {
                if (!isResource)
                {
                    return null;
                }

                return (ResourceAttributes)this.attributes;
            }
        }

        /// <summary>
        /// If this info describes a building, returns the BuildingAttributes class, otherwise it returns null
        /// It should always be used either by first checking isBuilding, or checking if returned value is not null
        /// </summary>
        public BuildingAttributes buildingAttributes
        {
            get
            {
                if (!isBuilding)
                {
                    return null;
                }

                return (BuildingAttributes)this.attributes;
            }
        }


        /// <summary>
        /// If this info describes a barrack, returns the BarrackAttributes class, otherwise it returns null
        /// It should always be used either by first checking isBarrack, or checking if returned value is not null
        /// </summary>
        public BarrackAttributes barrackAttributes
        {
            get
            {
                if (!isBarrack)
                {
                    return null;
                }

                return (BarrackAttributes)this.attributes;
            }
        }

        /// <summary>
        /// If this info describes an archery, returns the ArcheryAttributes class, otherwise it returns null
        /// It should always be used either by first checking isArchery, or checking if returned value is not null
        /// </summary>
        public ArcheryAttributes archeryAttributes
        {
            get
            {
                if (!isArchery)
                {
                    return null;
                }

                return (ArcheryAttributes)this.attributes;
            }
        }

        /// <summary>
        /// If this info describes a stable, returns the stableAttributes class, otherwise it returns null
        /// It should always be used either by first checking isStable, or checking if returned value is not null
        /// </summary>
        public StableAttributes stableAttributes
        {
            get
            {
                if (!isStable)
                {
                    return null;
                }

                return (StableAttributes)this.attributes;
            }
        }
    }
}
