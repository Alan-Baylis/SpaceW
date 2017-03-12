﻿using SpaceEngine.AtmosphericScattering;
using SpaceEngine.Core.Bodies;

using System;

using UnityEngine;

public static class EventManager
{
    public static CelestialBodyEvents CelestialBodyEvents = new CelestialBodyEvents();
}

public sealed class CelestialBodyEvents
{
    public EventHolder<CelestialBody, Atmosphere> OnAtmosphereBaked = new EventHolder<CelestialBody, Atmosphere>();
    public EventHolder<CelestialBody, Atmosphere, AtmosphereBase> OnAtmospherePresetChanged = new EventHolder<CelestialBody, Atmosphere, AtmosphereBase>();
}

#region Event Holders

public class EventHolder
{
    public event Action OnEvent;

    public void Invoke()
    {
        if (OnEvent != null)
            OnEvent();
    }

    public void SafeInvoke()
    {
        try
        {
            Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("EventManager: {0}:{1}", ex.GetType().Name, ex.Message));
        }
    }
}

public class EventHolder<T>
{
    public event Action<T> OnEvent;

    public void Invoke(T arg1)
    {
        if (OnEvent != null)
            OnEvent(arg1);
    }

    public void SafeInvoke(T arg1)
    {
        try
        {
            Invoke(arg1);
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("EventManager: {0}:{1}", ex.GetType().Name, ex.Message));
        }
    }
}

public class EventHolder<T1, T2>
{
    public event Action<T1, T2> OnEvent;

    public void Invoke(T1 arg1, T2 arg2)
    {
        if (OnEvent != null)
            OnEvent(arg1, arg2);
    }

    public void SafeInvoke(T1 arg1, T2 arg2)
    {
        try
        {
            Invoke(arg1, arg2);
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("EventManager: {0}:{1}", ex.GetType().Name, ex.Message));
        }
    }
}

public class EventHolder<T1, T2, T3>
{
    public event Action<T1, T2, T3> OnEvent;

    public void Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        if (OnEvent != null)
            OnEvent(arg1, arg2, arg3);
    }

    public void SafeInvoke(T1 arg1, T2 arg2, T3 arg3)
    {
        try
        {
            Invoke(arg1, arg2, arg3);
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("EventManager: {0}:{1}", ex.GetType().Name, ex.Message));
        }
    }
}

public class EventHolder<T1, T2, T3, T4>
{
    public event Action<T1, T2, T3, T4> OnEvent;

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        if (OnEvent != null)
            OnEvent(arg1, arg2, arg3, arg4);
    }

    public void SafeInvoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        try
        {
            Invoke(arg1, arg2, arg3, arg4);
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("EventManager: {0}:{1}", ex.GetType().Name, ex.Message));
        }
    }
}

#endregion