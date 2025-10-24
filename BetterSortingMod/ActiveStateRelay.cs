using System;
using UnityEngine;

namespace BetterSortingMod
{
    /// <summary>
    /// Relays Unity active-state changes (OnEnable / OnDisable) to callbacks.
    /// Use this to eliminate per-frame polling for visibility sync.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class ActiveStateRelay : MonoBehaviour
    {
        /// <summary>
        /// Fired when this component becomes enabled and active in the hierarchy.
        /// </summary>
        public event Action<ActiveStateRelay> BecameActive;

        /// <summary>
        /// Fired when this component becomes disabled or inactive in the hierarchy.
        /// </summary>
        public event Action<ActiveStateRelay> BecameInactive;

        /// <summary>
        /// Fired on any active state change (true: enabled/active, false: disabled/inactive).
        /// </summary>
        public event Action<ActiveStateRelay, bool> ActiveStateChanged;

        private Action _onActive;
        private Action _onInactive;
        private bool _configured;

        /// <summary>
        /// Returns true if this component is active and enabled.
        /// </summary>
        public bool IsActive => isActiveAndEnabled;

        /// <summary>
        /// Ensure an <see cref="ActiveStateRelay"/> exists on the given GameObject.
        /// </summary>
        /// <exception cref="ArgumentNullException">If <paramref name="go"/> is null.</exception>
        public static ActiveStateRelay Ensure(GameObject go)
        {
            if (go == null) throw new ArgumentNullException(nameof(go));
            var relay = go.GetComponent<ActiveStateRelay>();
            if (relay == null) relay = go.AddComponent<ActiveStateRelay>();
            return relay;
        }

        /// <summary>
        /// Ensure an <see cref="ActiveStateRelay"/> exists on the given component's GameObject.
        /// </summary>
        /// <exception cref="ArgumentNullException">If <paramref name="host"/> is null.</exception>
        public static ActiveStateRelay Ensure(Component host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            return Ensure(host.gameObject);
        }

        /// <summary>
        /// Configure callbacks for active / inactive transitions.
        /// </summary>
        /// <param name="onActive">Invoked on OnEnable (when the object is active in hierarchy).</param>
        /// <param name="onInactive">Invoked on OnDisable (or object deactivation in hierarchy).</param>
        /// <param name="invokeImmediately">
        /// If true, immediately invokes the corresponding callback according to the current state.
        /// </param>
        public void Configure(Action onActive, Action onInactive, bool invokeImmediately = false)
        {
            _onActive = onActive;
            _onInactive = onInactive;
            _configured = true;

            if (invokeImmediately)
            {
                if (isActiveAndEnabled) HandleBecameActive();
                else HandleBecameInactive();
            }
        }

        private void OnEnable()
        {
            HandleBecameActive();
        }

        private void OnDisable()
        {
            // OnDisable is also called during teardown; guard not needed.
            HandleBecameInactive();
        }

        private void HandleBecameActive()
        {
            ActiveStateChangedSafe(true);
            if (_configured) _onActive?.Invoke();
            BecameActive?.Invoke(this);
        }

        private void HandleBecameInactive()
        {
            ActiveStateChangedSafe(false);
            if (_configured) _onInactive?.Invoke();
            BecameInactive?.Invoke(this);
        }

        private void ActiveStateChangedSafe(bool active)
        {
            try
            {
                ActiveStateChanged?.Invoke(this, active);
            }
            catch (Exception)
            {
                // Swallow to avoid breaking Unity event loop if external subscribers throw.
            }
        }

        private void OnDestroy()
        {
            // Prevent holding references
            _onActive = null;
            _onInactive = null;
            BecameActive = null;
            BecameInactive = null;
            ActiveStateChanged = null;
        }
    }
}
