﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class LightHouseRevealer : MonoBehaviour
{

    private Storage.Races _race;
    public enum Direction { CLOCK, COUNTERCLOCK };
    public Direction direction = Direction.CLOCK;

    private float RevealerAngleStep = 1f;
    private float RevealerOrbitatingRadius = 50;
    private  float _revealerSphereRadius;

    private Vector3 _basePosition;


    private bool _orbitating = false;
    private Vector3 _center;
    private GameObject _light;
    private FOWEntity _fow;
    private GameObject _target;
    private Vector3 _lastPosition;
    private Vector3 _lastTargetPosition;
    private float _offset = 1f;
    

    void Awake(){ }

    void Start()
    {

        //moves the revealer to the orbitating position
        transform.position = transform.parent.position;
        transform.Translate(new Vector3(RevealerOrbitatingRadius, 0f, 0f));
        _basePosition = transform.position;

        _race = transform.parent.GetComponent<Barrack>().getRace();
        _center = transform.parent.position;
        _fow = GetComponent<FOWEntity>();
        _fow.Activate(_race);
        _revealerSphereRadius = _fow.Range * .5f;
        _light = transform.parent.FindChild("Light").gameObject;
        _lastPosition = new Vector3(9999f, 9999f, 9999f);
        _orbitating = true;
    }



    void Update()
    {
        if (_orbitating)
        {
            Vector3 rotateDirection = direction == Direction.CLOCK ? Vector3.up : Vector3.down;
            transform.RotateAround(_center, rotateDirection, RevealerAngleStep);
            _light.transform.LookAt(transform);
        }
        /*
        else if (_target) 
        {
            // checks if the target is moving by comparing the last and the current position, so we need to readjust the orbitation
            if (_lastTargetPosition != _target.transform.position)
            {
                _orbitating = true;
                return;
            }
        }
        */
        if (_target)
        {
            Debug.DrawLine(transform.parent.position,_target.transform.position, Color.yellow);
            Debug.DrawLine(transform.position, _target.transform.position, Color.red);

            if (!CloserToTarget())
            {
                // the target is fixed, we are orbitating in wrong direction
                if (EnoughDifference(_lastTargetPosition, _target.transform.position))
                {
                    ToggleDirection();
                    _orbitating = true;
                }
                else
                {
                    _orbitating = false;
                }
            }

            UpdatePositions();
        }

        
    }


    /// <summary>
    /// Toggles the orbitating direction
    /// </summary>
    public void ToggleDirection()
    {
        direction = direction == Direction.CLOCK ? Direction.COUNTERCLOCK : Direction.CLOCK;
        Debug.Log("Now rotating " + direction);
    }

    /// <summary>
    /// Is the revealer orbitating or not
    /// </summary>
    /// <param name="_set"></param>
    public void SetOrbitating(bool _set)
    {
        _orbitating = _set;
    }


    /// <summary>
    /// Stops orbitating if it's enemy
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerEnter(Collider col)
    {
        GameObject obj = col.gameObject;
        IGameEntity entity = obj.GetComponent<IGameEntity>();
        if (entity != null)
        {
            if (IsEnemy(entity))
            {
                _target = obj;
                _lastTargetPosition = _target.transform.position;
            }

        }
    }

    /// <summary>
    ///  Just start orbitating
    /// </summary>
    /// <param name="col"></param>
    void OnTriggerExit(Collider col)
    {
        GameObject obj = col.gameObject;
        IGameEntity entity = obj.GetComponent<IGameEntity>();
        if (entity != null)
        {
            if (IsEnemy(entity))
            {
                _target = null;
                _orbitating = true;
                RestartLastPosition();
            }

        }
    }


    /// <summary>
    /// Returns if the current movement has positioned us closer to the target
    /// </summary>
    /// <returns></returns>
    private bool CloserToTarget()
    {
        Vector3 we, he, now, then;

        we = transform.position;
        he = _target.transform.position;
        now = we - he;
        then = _lastPosition - he;

        return now.sqrMagnitude < then.sqrMagnitude;

    }


    /// <summary>
    /// Updates the target and the revealer position
    /// </summary>
    private void UpdatePositions()
    {
        _lastPosition = transform.position;
        _lastTargetPosition = _target.transform.position;
    }


    /// <summary>
    /// Restarts the last position
    /// </summary>
    private void RestartLastPosition()
    {
        _lastPosition = new Vector3(9999f, 9999f, 9999f);
    }


    /// <summary>
    /// Checks if two vectors are different by applying an offset
    /// </summary>
    /// <param name="now"></param>
    /// <param name="then"></param>
    /// <returns></returns>
    private bool EnoughDifference(Vector3 now, Vector3 then)
    {
        //Debug.Log(Vector3.Distance(now, then) + " " + _offset);
        return Vector3.Distance(now, then) > _offset;
    }

    /// <summary>
    /// Returns if its enemy by checking if it's unity and a different race
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private bool IsEnemy(IGameEntity entity)
    {

        return (entity.info.isUnit && entity.info.race != _race);
        //return entity.info.isUnit;
    }
}

