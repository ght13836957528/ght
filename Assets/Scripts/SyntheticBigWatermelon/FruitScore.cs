namespace SyntheticBigWatermelon
{
    public class FruitScore
    {
        private int _currentScore;

        public FruitScore()
        {
            
        }

        public void AddScore(FruitConst.FruitType fruitType)
        {
            _currentScore = _currentScore + GetScoreByFruitType(fruitType);
        }

        private int GetScoreByFruitType(FruitConst.FruitType fruitType)
        {
            return 10;
        }
    }
}