﻿using System.Timers;
using Assets.Scripts.EventSystems;
using UnityEngine;
using QFSW.QC;

namespace Assets.Scripts.Entity
{
    [CommandPrefix("Entity.Virus.")]
    [RequireComponent(typeof(LineRenderer))]
    public class Virus : Enemy, IVaccineCollectedHandler, IVaccineEffectEndHandler
    {
        public float SpeedModifier = 1f;
        private float _originalSpeedModifier;

        [SerializeField] private float _range = 10f;

        private readonly byte _vertexCount = 50;
        private LineRenderer _lr;

        private bool _debugMode = false;

        private Vector2? _target = null;
        [Tooltip("By how many pixels does the virus go past the target")]
        [SerializeField] private float _overextend = 3f;

        [Tooltip("Cooldown period in ms after an attack that the virus can't attack again")]
        [SerializeField] private float _cooldownTime = 2000f;
        
        
        [ReadOnly] public bool Cooldown = false;

        public void Awake()
        {
            _originalSpeedModifier = SpeedModifier;
            _lr = GetComponent<LineRenderer>();
        }

        public void Update()
        {
            float step = Speed * SpeedModifier * Time.deltaTime;

            if (_debugMode)
            {
                if (_target != null)
                    DrawPathToTarget((Vector2)_target, Color.red);
                else if (!Cooldown)
                    DrawRangeBox();
            }

            bool isPlayerInRange = _range >= Vector2.Distance(transform.position, Player.Current.transform.position);

            // Handles movement
            if (!Cooldown && isPlayerInRange && _target == null) // If virus can atatck the player
                StartAttack();
            else if (_target != null) // If virus is in the middle of the attack
            {
                transform.position = Vector2.MoveTowards(transform.position, (Vector2)_target, step);

                // if virus has reached target, start attack cooldown
                if ((Vector2)transform.position == (Vector2)_target) 
                {
                    Timer timeoutTimer = new Timer(_cooldownTime * SpeedModifier);
                    timeoutTimer.Elapsed += OnCooldownEnd;
                    timeoutTimer.AutoReset = false;
                    timeoutTimer.Start();

                    Cooldown = true;
                    _target = null;
                }
            }
        }

        // Need to overwrite animations coming from entity
        new private void FixedUpdate() { }

        #region Debug
        [Command("Debug", MonoTargetType.All)]
        private void ToggleDebug()
        {
            _debugMode = !_debugMode;

            if (!_debugMode)
                _lr.positionCount = 0;
        }

        private void DrawPathToTarget(Vector2 target, Color color)
        {
            _lr.positionCount = 0;

            _lr.startColor = color;
            _lr.endColor = color;
            _lr.widthMultiplier = 0.1f;

            _lr.positionCount = 2;

            _lr.SetPosition(0, transform.position);
            _lr.SetPosition(1, target);

        }

        /// <summary>
        /// Draws a circle around the virus indicating its range.
        /// </summary>
        private void DrawRangeBox()
        {
            _lr.positionCount = 0;

            _lr.startColor = Color.green;
            _lr.endColor = Color.green;
            _lr.widthMultiplier = 0.1f;
            float theta = 0f;

            _lr.positionCount = _vertexCount + 1;
            for (int i = 0; i < _vertexCount; i++)
            {
                Vector2 pos = new Vector2(_range * Mathf.Cos(theta), _range * Mathf.Sin(theta));
                _lr.SetPosition(i, pos + (Vector2)transform.position);
                theta += (2f * Mathf.PI) / _vertexCount;
            }

            _lr.SetPosition(_lr.positionCount - 1, _lr.GetPosition(0));
        }
        #endregion

        /// <summary>
        /// Starts the attack and calucates the point to move to
        /// </summary>
        private void StartAttack()
        {
            // Calucate the coordinates to attack
            Vector2 player = Player.Current.transform.position;
            Vector2 virus = transform.position;

            Vector2 difference = player - virus;
            float distance = Vector2.Distance(virus, player);

            // Calculating the angle between X axis and hypotenuse (distance)
            float theda = Mathf.Acos(difference.x / distance);  // Gives the result in radians

            distance += _overextend;

            float x = Mathf.Cos(theda) * distance;
            float y = Mathf.Sin(theda) * x;


            _target = new Vector2(x, y) + virus;
        }

        #region EventHandlers
        private void OnCooldownEnd(object source, ElapsedEventArgs e)
        {
            Cooldown = false;
        }

        public void OnVaccineCollected(int _)
        {
            SpeedModifier = _originalSpeedModifier * 0.5f; 
        }

        public void OnVaccineEffectEnd()
        {
            SpeedModifier = _originalSpeedModifier;

        }
        #endregion
    }
}
