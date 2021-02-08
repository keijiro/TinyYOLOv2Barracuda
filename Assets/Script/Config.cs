namespace TinyYoloV2
{
    public static class Config
    {
        public const int ImageSize = 416;
        public const int CellsInRow = 13;
        public const int AnchorCount = 5;
        public const int ClassCount = 20;

        public const int InputSize = ImageSize * ImageSize * 3;
        public const int TotalCells = CellsInRow * CellsInRow;
        public const int OutputPerCell = AnchorCount * (5 + ClassCount);
        public const int MaxDetection = TotalCells * AnchorCount;

        public static string[] _labels = new[]
        {
            "Plane", "Bicycle", "Bird", "Boat",
            "Bottle", "Bus", "Car", "Cat",
            "Chair", "Cow", "Table", "Dog",
            "Horse", "Motorbike", "Person", "Plant",
            "Sheep", "Sofa", "Train", "TV"
        };

        public static string GetLabel(int index)
          => _labels[index];
    }
}
