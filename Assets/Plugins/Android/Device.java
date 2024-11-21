package com.im30.lsu3d;

import android.app.Activity;
import java.util.Locale;

public class Device
{
    private Activity _unityActivity;

    private Activity getActivity()
    {
        if(null == _unityActivity) 
        { 
            try 
            {
                Class<?> classtype = Class.forName("com.unity3d.player.UnityPlayer");
                _unityActivity = (Activity) classtype.getDeclaredField("currentActivity").get(classtype);
            } 
            catch (ClassNotFoundException e) 
            {

            } 
            catch (IllegalAccessException e) 
            {

            } 
            catch (NoSuchFieldException e) 
            {
            }
        }
        return _unityActivity;
    }

    public String getCountry()
    {
        Locale locale = getActivity().getResources().getConfiguration().locale;
        return locale.getCountry();
    }
}