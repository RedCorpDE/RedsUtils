using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RedsUtils.Player.MovementSystem
{
    
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerComplex : PlayerSimple
    {
        [SerializeField] private LocomotionSet locomotionSet;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerInput input;

        private readonly List<(LocomotionSet.Entry entry, LocomotionRuntime runtime)> _runtimes = new();
        private PlayerContext _ctx;

        private void Awake()
        {
            
            _ctx = new PlayerContext(gameObject, characterController, mainCamera, input);

            BuildRuntimes();
        }

        private void OnEnable()
        {
            foreach (var pair in _runtimes)
                if (pair.entry.enabledByDefault)
                    pair.runtime.Enable();
        }

        private void OnDisable()
        {
            foreach (var pair in _runtimes)
                pair.runtime.Disable();
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            _ctx.Motor.BeginFrame();

            foreach (var pair in _runtimes)
                pair.runtime.Tick(dt);

            _ctx.Motor.Apply(dt);
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
