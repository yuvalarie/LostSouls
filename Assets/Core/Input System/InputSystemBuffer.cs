using System;

namespace Core.Input_System
{
    public sealed class InputSystemBuffer
    {
        // Lazy<T> ensures thread-safety and deferred initialization
        private static readonly Lazy<InputSystemBuffer> _lazyInstance =
            new Lazy<InputSystemBuffer>(() => new InputSystemBuffer());

        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static InputSystemBuffer Instance => _lazyInstance.Value;

        // Private ctor prevents external instantiation
        private InputSystemBuffer()
        {
            InputSystem = new InputSystem_Actions();
            InputSystem.Enable();
        }
        
        public InputSystem_Actions InputSystem { get; private set; }
    }
}