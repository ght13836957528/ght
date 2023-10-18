using UnityEngine;
/// <summary>
/// 观察者模式
/// </summary>
public class InitObserverEntrance : MonoBehaviour
{
    private HeatWeatherDataDisplay _display;

    private void Start()
    {
        WeatherData weatherData = new WeatherData();
        _display = new HeatWeatherDataDisplay(weatherData);
        weatherData.SetDegree(5);
        weatherData.SetDegree(10);
    }
}