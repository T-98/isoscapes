public class Cell
{
    public bool isWater;

    public Cell(string type)
    {
        if(type == "water")this.isWater = true;
    }
}