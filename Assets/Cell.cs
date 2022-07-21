public class Cell
{
    public bool isWater;
    public bool isSand;
    public bool isGrass;
    public bool isMountain;
    public bool hasObject = false;

    public Cell(string type)
    {
        if(type == "water")this.isWater = true;
        if (type == "sand") this.isSand = true;
        if (type == "grass") this.isGrass = true;
        if (type == "mountain") this.isMountain = true;
    }
}