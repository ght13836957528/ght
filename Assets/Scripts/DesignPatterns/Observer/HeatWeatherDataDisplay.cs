using UnityEngine;
public class HeatWeatherDataDisplay : Observer, DisplayElement
{
    private int _degree;
    private WeatherData _mWeatherData;

    public HeatWeatherDataDisplay(WeatherData weatherData)
    {
        _mWeatherData = weatherData;
        weatherData.RegisterObserver(this);
    }

    public void Update()
    {
        _degree = _mWeatherData.GetDegree();
        Display();
    }

    public void Display()
    {
        Debug.Log("degree===" + _degree);
    }
}