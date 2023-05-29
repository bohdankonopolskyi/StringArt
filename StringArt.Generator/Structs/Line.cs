namespace StringArt.Generator.Structs;

public struct Line
{
    public Line(int start, int end)
    {
        Start = start;
        End = end;
    }

    public int Start { get; set; }
    public int End { get; set; }
}