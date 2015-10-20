﻿using UnityEngine;
using System.Collections;
using Utils;
namespace Managers
{
    public class BuildingsManager : MonoBehaviour
    {

        Player player;
        UserInput inputs;
        private ConstructionGrid grid;
        private bool _placing = false;
        private GameObject newBuilding;
        bool move = true;
        float yoffset = 0f;

        // Use this for initialization
        void Start()
        {
            player = GetComponent<Player>();
            inputs = GetComponent<UserInput>();
            grid = GetComponent<ConstructionGrid>();

        }

        // Update is called once per frame
        void Update()
        {

            //TODO : (hermetico) test and remove after merging
            #region testcreatebuilding
            bool TEST = false;
            if (TEST) { TEST = false; _createBuilding_("Men-Sawmill"); }
            #endregion

            if (_placing)
            {
                relocate();

            }

        }

        /// <summary>
        /// Starts creating a building, required the name of the building ex: 'elf-farm'
        /// </summary>
        /// <param name="name"></param>
        public void _createBuilding_(string name)
        {
            if (!_placing)
            {
                GameObject newBuilding;
                newBuilding = (GameObject)Resources.Load("Prefabs/Buildings/Resources/" + name, typeof(GameObject));
                newBuilding = (GameObject)Instantiate(newBuilding, new Vector3(0, 0, 0), Quaternion.identity);
                this.createBuilding(newBuilding);

            }

        }
        
        /// <summary>
        /// Starts creating a building, required the name of the building ex: 'elf-farm'
        /// </summary>
        /// <param name="name"></param>
        public void createBuilding(string path)
        {
            if (!_placing)
            {
                GameObject newBuilding;
                newBuilding = (GameObject)Resources.Load( path, typeof(GameObject));
                newBuilding = (GameObject)Instantiate(newBuilding, new Vector3(0, 0, 0), Quaternion.identity);
                this.createBuilding(newBuilding);

            }

        }

        /// <summary>
        /// Starts creating a building. The param newBuilding must be instantiated previously
        /// </summary>
        /// <param name="newBuilding"></param>
        public void createBuilding(GameObject newBuilding)
        {
            if (!_placing)
            {
                this.newBuilding = newBuilding;
                newBuilding.AddComponent<CollisionDetector>();
                this.newBuilding.GetComponent<Rigidbody>().detectCollisions = false;
                //newBuilding.GetComponent<Collider>().isTrigger = true; //HACK : (hermetico) controlar colision objeto con resto
                _placing = true;
                //Cursor.visible = false;
                player.setCurrently(Player.status.PLACING_BUILDING);
            }
        }

        /// <summary>
        /// Discretizes the location through ConstructioGrid
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Vector3 adaptLocation(Vector3 location)
        {
            
            location = grid.discretizeMapCoords(location);
            return location;
        }

        /// <summary>
        /// Checks if is valid locatoin through Constructiongrid
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool checkLocation(Vector3 location)
        {
            bool check = false;

            check = grid.isNewPositionAbleForConstrucction(location)
                && !(newBuilding.AddComponent<CollisionDetector>().isThereCollision);

            return check;

        }

        /// <summary>
        /// Places the building, checking if is a suitable place
        /// </summary>
        public void placeBuilding()
        {
            // 1. getPoint
            Vector3 toLocation = inputs.FindHitPoint();

            // 2. offsetPoint
            toLocation = locationOffset(toLocation);

            // 3. discretize
            toLocation = adaptLocation(toLocation);

            // alter the color if is not a valid location
            if (checkLocation(toLocation))
            {

                //TODO : (hermetico) restar recursos necesarios para crear el building
                Destroy(newBuilding.GetComponent<CollisionDetector>());
                grid.reservePosition(toLocation);
                newBuilding.transform.position = toLocation;
                this.newBuilding.GetComponent<Rigidbody>().detectCollisions = false;
                IGameEntity destination = (IGameEntity)newBuilding.GetComponent<Unit>();
                player.addEntityToList(destination);
                
                // remaining operations
                _finishPlacing();
            }


        }

        /// <summary>
        /// Cancel the placing of the building and returns player to idle status
        /// </summary>
        public void cancelPlacing()
        {
            if (_placing)
            {
                Destroy(newBuilding);
                // remaining operations
                _finishPlacing();
            }
        }

        /// <summary>
        /// Common operations when placing or cancelling placing
        /// </summary>

        private void _finishPlacing()
        {
            //Cursor.visible = true;
            _placing = false;
            player.setCurrently(Player.status.IDLE);
        }

        /// <summary>
        /// Moves the building to the mouse position
        /// </summary>
        private void relocate()
        {

            // 1. getPoint
            Vector3 toLocation = inputs.FindHitPoint();

            // 2. offsetPoint
            toLocation = locationOffset(toLocation);
            
            // 3. discretize
            toLocation = adaptLocation(toLocation);

            // 4. check and move alter the color if is not a valid location
            newBuilding.transform.position = toLocation;
            if (checkLocation(toLocation))
            {
                _drawState(Color.green);
            }
            else
            {
                _drawState(Color.red);
            }

        }


        /// <summary>
        /// Draws a surrounding box based on the collider
        /// </summary>
        /// <param name="lineColor"></param>
        private void _drawState(Color lineColor)
        {
            Vector3 boundPoint1, boundPoint2, boundPoint3, boundPoint4, boundPoint5, boundPoint6, boundPoint7, boundPoint8;
            Bounds bounds = newBuilding.GetComponent<Collider>().bounds;
            boundPoint1 = bounds.min;
            boundPoint2 = bounds.max;
            boundPoint3 = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
            boundPoint4 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
            boundPoint5 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
            boundPoint6 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
            boundPoint7 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
            boundPoint8 = new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);


            // rectangular cuboid
            // top of rectangular cuboid (6-2-8-4)
            Debug.DrawLine(boundPoint6, boundPoint2, lineColor);
            Debug.DrawLine(boundPoint2, boundPoint8, lineColor);
            Debug.DrawLine(boundPoint8, boundPoint4, lineColor);
            Debug.DrawLine(boundPoint4, boundPoint6, lineColor);

            // bottom of rectangular cuboid (3-7-5-1)
            Debug.DrawLine(boundPoint3, boundPoint7, lineColor);
            Debug.DrawLine(boundPoint7, boundPoint5, lineColor);
            Debug.DrawLine(boundPoint5, boundPoint1, lineColor);
            Debug.DrawLine(boundPoint1, boundPoint3, lineColor);

            // legs (6-3, 2-7, 8-5, 4-1)
            Debug.DrawLine(boundPoint6, boundPoint3, lineColor);
            Debug.DrawLine(boundPoint2, boundPoint7, lineColor);
            Debug.DrawLine(boundPoint8, boundPoint5, lineColor);
            Debug.DrawLine(boundPoint4, boundPoint1, lineColor);

        }

        
        /// <summary>
        /// Moves the location based on the collider, because if we move the object on with the center on the hitpoint
        /// the next hitPoint will be the same object that we are placing, and not the surface of the terrain
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Vector3 locationOffset(Vector3 location) {
            Bounds newBounds = newBuilding.GetComponent<Collider>().bounds;
            Vector3 min = newBounds.min;
            Vector3 max = newBounds.max;
            // estos calculos dependen de la camara principal
            // movemos el punto a partir del tamaño del objeto
            // para no situar el objeto encima del raton
            location.x += ((max.x - min.x) / 2f);
            location.z -= ((max.z - min.z) / 2f);
            location.y += ((max.y - min.y) / 2f);

            // ahora hacemos un raicasting, para que el objeto no quede entre
            // del terreno
            return location;// inputs.FindHitPoint(location);
        }
        
    }
    public class CollisionDetector : MonoBehaviour
    {
        private bool _isThereCollision;
        public bool isThereCollision{ get { return _isThereCollision; } }
        void OnCollisionEnter(Collision collisionInfo)
        {
            _isThereCollision = true;
            collisionInfo.gameObject.SetActive(true);
        }
        void OnCollisionExit(Collision collisionInfo)
        {
            _isThereCollision = false;
        }
    }
}