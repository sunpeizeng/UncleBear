using System.Collections.Generic;

public interface ICSVDeserializable
{
    void CSVDeserialize(Dictionary<string, string[]> data, int index);
}