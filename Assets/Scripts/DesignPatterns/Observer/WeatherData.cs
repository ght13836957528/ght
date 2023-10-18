public class WeatherData : Observable
{
    private int _degree;
    private void MeasurementChanged()
    {
        SetChanged();
    }

    public void SetDegree(int degree)
    {
        _degree = degree;
        MeasurementChanged();
    }

    public int GetDegree()
    {
        return _degree;
    }
}