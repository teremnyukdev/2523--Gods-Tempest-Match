using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Application.Services.ApplicationState
{
    public class ApplicationStateService
    {
        private static ApplicationStateMonoHelper _helper;
        
        public event Action ApplicationQuitEvent;
        public event Action<bool> ApplicationPauseEvent;

        public void Initialize()
        {
            Reset();
            
            GameObject applicationStateHelper = new GameObject("ApplicationStateHelper");
            _helper = applicationStateHelper.AddComponent<ApplicationStateMonoHelper>();
            _helper.ApplicationQuitEvent += NotifyApplicationQuitEvent;
            _helper.ApplicationPauseEvent += NotifyApplicationPauseEvent;
            Object.DontDestroyOnLoad(_helper);
        }

        private void Reset()
        {
            if (_helper)
                Object.Destroy(_helper.gameObject);
            
            ApplicationQuitEvent = null;
            ApplicationPauseEvent = null;
        }

        public void Dispose()
        {
            if(_helper == null)
                return;

            _helper.ApplicationQuitEvent -= NotifyApplicationQuitEvent;
            _helper.ApplicationPauseEvent -= NotifyApplicationPauseEvent;
        }

        private void NotifyApplicationQuitEvent()
        {
            ApplicationQuitEvent?.Invoke();
        }

        private void NotifyApplicationPauseEvent(bool isPause)
        {
            ApplicationPauseEvent?.Invoke(isPause);
        }
    }
}