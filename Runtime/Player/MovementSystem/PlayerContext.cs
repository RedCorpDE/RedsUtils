using UnityEngine;
using UnityEngine.InputSystem;

namespace RedsUtils.Player.MovementSystem
{
    public sealed class PlayerContext
    {
        public readonly GameObject GameObject;
        public readonly Transform Transform;
        public readonly CharacterController CharacterController;
        public readonly Camera MainCamera;
        public readonly PlayerInput PlayerInput;
        public readonly LocomotionMotor Motor;

        public PlayerContext(
            GameObject go,
            CharacterController characterController,
            Camera mainCamera,
            PlayerInput playerInput)
        {
            GameObject = go;
            Transform = go.transform;
            CharacterController = characterController;
            MainCamera = mainCamera;
            PlayerInput = playerInput;
            Motor = new LocomotionMotor(characterController);
        }
    }
}