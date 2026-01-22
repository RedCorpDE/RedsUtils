using System;
using System.Collections.Generic;
using System.Linq;
using RedsUtils.Player.MovementSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RedsUtils.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    [AddComponentMenu("Utilities/Player/Simple Player")]
    public sealed class PlayerComplex : PlayerSimple
    {

        [Header("Config")]
        [SerializeField] private LocomotionSet locomotionSet;

        [Header("Refs")]
        [SerializeField] private Camera mainCamera;

        private readonly List<(LocomotionSet.Entry entry, LocomotionRuntime runtime)> _runtimes = new();
        private PlayerContext _ctx;

        private void Awake()
        {
            var cc = GetComponent<CharacterController>();
            var pi = GetComponent<PlayerInput>();

            _ctx = new PlayerContext(gameObject, cc, mainCamera, pi);

            BuildRuntimes();
        }

        private void OnEnable()
        {
            foreach (var pair in _runtimes)
            {
                if (pair.entry.enabledByDefault)
                    pair.runtime.Enable();
            }
        }

        private void OnDisable()
        {
            foreach (var pair in _runtimes)
                pair.runtime.Disable();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (var pair in _runtimes)
                pair.runtime.Tick(dt);
        }

        private void FixedUpdate()
        {
            float fdt = Time.fixedDeltaTime;
            foreach (var pair in _runtimes)
                pair.runtime.FixedTick(fdt);
        }

        private void BuildRuntimes()
        {
            _runtimes.Clear();
            if (locomotionSet == null) return;

            foreach (var entry in locomotionSet.modules
                         .Where(e => e.module != null)
                         .OrderBy(e => e.order))
            {
                var runtime = entry.module.CreateRuntime(_ctx);
                _runtimes.Add((entry, runtime));
            }
        }
        
    }
}