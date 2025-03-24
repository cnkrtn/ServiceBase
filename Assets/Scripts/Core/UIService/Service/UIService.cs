using System.Collections.Generic;
using System.Threading.Tasks;
using Core.AddressableService.Interface;
using Core.EventService.Events;
using Core.EventService.Interface;
using Core.EventService.Keys;
using Core.GameService.Interface;
using Core.UIService.Interface;
using Core.UIService.Keys;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.UIService.Service
{
    
    public class UIService : IUIService
    {
        private  IAddressableService _addressableService;
        private IEventService _eventService;
        private GameObject _loadingScreen;
        private GameObject _loadingScreenSmall;
        private GameObject _fadeScreen;
        private IGameService _gameService;
        private bool _returnPopUpValue;
        private bool _returnPopUpValueSet;
        private bool _sliderValueSet;
        private bool _sliderValueSetAvailable;
        private int _sliderValue;
        private Dictionary<string, GameObject> _screens = new Dictionary<string, GameObject>();
        private int _enabledUIList;
        private int _sortOrder = Constants.Constants.SortOderStartValue;
        private bool _orderStart;
        private bool _orderReturn;
        public int SortOrder => _sortOrder;
        public void AddSortOrder(bool add)
        {
            CalculateSortOrder(add);
        }
        
        public void AddOnUI()
        {
            _enabledUIList++;
            _gameService.SetOnUI(true);
        }

        public void RemoveOnUI()
        {
            _enabledUIList--;
            if (_enabledUIList == 0)
            {
                _gameService.SetOnUI(false);         
            }
        }
        public async Task Inject()
        {
            _gameService = ReferenceLocator.Instance.GameService;
            _eventService = ReferenceLocator.Instance.EventService;
            _addressableService = ReferenceLocator.Instance.AddressableService;
            await Init();
        }
        private async Task Init()
        {
            _loadingScreen = await _addressableService.LoadObject(UIKeys.KEY_LOADING_SCREEN_UI);
            _loadingScreenSmall = await _addressableService.LoadObject(UIKeys.KEY_LOADING_SCREEN_SMALL_UI);
            _fadeScreen = await _addressableService.LoadObject(UIKeys.KEY_FADE_UI);
        }

        public void ShowFadeScreen()
        {
            _fadeScreen.SetActive(true);
            _eventService.Fire(EventKeys.EVENT_ON_FADE_START,new OnFadeStart(true));
        }

        public void RemoveFadeScreen()
        {
            _fadeScreen.SetActive(false);
        }
        
        public void ShowLoadingScreen()
        {
            _loadingScreen.SetActive(true);
        }

        public void RemoveLoadingScreen()
        {
            _loadingScreen.SetActive(false);
        }
        
        public async Task<GameObject> ShowScreen(string key)
        {
            if (_gameService.LoadingScreen) return null;
            _gameService.SetLoadingScreen(true);
            var loadObject = await _addressableService.LoadObject(key);
            _screens[key] = loadObject;
            _gameService.SetLoadingScreen(false);
            return loadObject;
        }
        
        public async Task ShowScreenOrder<T>(string key, T initData)
        {
            if (_gameService.LoadingScreen) return;
            while (_orderStart)
            {
                await Task.Delay(100);
            }
            _orderStart = true;
            _orderReturn = true;
            _gameService.SetLoadingScreen(true);
            var loadObject = await _addressableService.LoadObject(key);
            _screens[key] = loadObject;
            _gameService.SetLoadingScreen(false);
            _eventService.Fire(EventKeys.EVENT_ON_UI_INIT,new OnUIInit<T>(loadObject.GetInstanceID(),initData));
            while (_orderReturn)
            {
                await Task.Delay(100);
            }
            _orderStart = false;
        }

        public void FinishOrder()
        {
            _orderReturn = false;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public async Task ShowScreen<T>(string key, T initData)
        {
            if (_gameService.LoadingScreen) return;
            _gameService.SetLoadingScreen(true);
            var loadObject = await _addressableService.LoadObject(key);
            _screens[key] = loadObject;
            _gameService.SetLoadingScreen(false);
            _eventService.Fire(EventKeys.EVENT_ON_UI_INIT,new OnUIInit<T>(loadObject.GetInstanceID(),initData));
            
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public async Task ShowScreen<T, T2>(string key, T data, T2 extraData)
        {
            if (_gameService.LoadingScreen)
                return;
            _gameService.SetLoadingScreen(true);
            var loadObject = await _addressableService.LoadObject(key);
            _screens[key] = loadObject;
            _gameService.SetLoadingScreen(false);
            _eventService.Fire(EventKeys.EVENT_ON_UI_INIT,new OnUIInit<T,T2>(loadObject.GetInstanceID(),data,extraData));
        }
        
        public async Task ShowScreen<T, T2,T3>(string key, T data, T2 extraData,T3 extraPlusData)
        {
            if (_gameService.LoadingScreen)
                return;
            _gameService.SetLoadingScreen(true);
            var loadObject = await _addressableService.LoadObject(key);
            _screens[key] = loadObject;
            _gameService.SetLoadingScreen(false);
            _eventService.Fire(EventKeys.EVENT_ON_UI_INIT,new OnUIInit<T,T2,T3>(loadObject.GetInstanceID(),data,extraData,extraPlusData));
        }




        public void ShowLoadingScreenSmall()
        {
            _loadingScreenSmall.SetActive(true);
        }

        public void RemoveLoadingScreenSmall()
        {
            _loadingScreenSmall.SetActive(false);
        }

        public async Task<int> ShowSlider(int maxValue,int minValue)
        {
            var instantiatedObject = await _addressableService.LoadObject(UIKeys.KEY_SLIDER_UI);
            _screens[UIKeys.KEY_SLIDER_UI] = instantiatedObject;
            _eventService.Fire(EventKeys.EVENT_ON_UI_INIT,new OnUIInit<int,int>(instantiatedObject.GetInstanceID(),maxValue,minValue));
            _eventService.Subscribe<OnReturn>(EventKeys.EVENT_ON_RETURN,OnReturn);
            while (!_sliderValueSet)
            {
                await Task.Delay(100);
            }
            _sliderValueSet = false;
            return _sliderValueSetAvailable ? _sliderValue : Constants.Constants.SliderNotReturnValue;
        }
        
        public async void ShowPopUpScreen(string text)
        {
            var instantiatedObject = await _addressableService.LoadObject(UIKeys.KEY_POPUP_UI);
            _screens[UIKeys.KEY_POPUP_UI] = instantiatedObject;
            _eventService.Fire(EventKeys.EVENT_ON_UI_INIT,new OnUIInit<string>(instantiatedObject.GetInstanceID(),text));
        }

        public async Task<bool> ShowReturnPopUpScreen(string text)
        {
            var instantiatedObject = await _addressableService.LoadObject(UIKeys.KEY_RETURN_POPUP_UI);
            _screens[UIKeys.KEY_RETURN_POPUP_UI] = instantiatedObject;
            _eventService.Fire(EventKeys.EVENT_ON_UI_INIT,new OnUIInit<string>(instantiatedObject.GetInstanceID(),text));
            _eventService.Subscribe<OnReturn>(EventKeys.EVENT_ON_RETURN,OnReturn);
            while (!_returnPopUpValueSet)
            {
                await Task.Delay(100);
            }
            _returnPopUpValueSet = false;
            return _returnPopUpValue;
        }

        public void RemoveScreen(string screenKey)
        {
            if (_screens.ContainsKey(screenKey))
                Object.Destroy(_screens[screenKey]);
        }

        public void RemoveAllScreens()
        {
            foreach (var screen in _screens)
            {
                Object.Destroy(_screens[screen.Key]);
            }
        }
        
        private void OnReturn(OnReturn obj)
        {
            if (obj.IsSlider)
            {
                _eventService.UnSubscribe<OnReturn>(EventKeys.EVENT_ON_RETURN,OnReturn);
                _sliderValueSet = true;
                _sliderValueSetAvailable = obj.ValueSetAvailable;
                _sliderValue = obj.ValueSlider;
                return;
            }
            _eventService.UnSubscribe<OnReturn>(EventKeys.EVENT_ON_RETURN,OnReturn);
            _returnPopUpValue = obj.Value;
            _returnPopUpValueSet = true;
        }

        private void CalculateSortOrder(bool open)
        {
            _sortOrder += open ? 1 : -1;
        }
    
    }
}
