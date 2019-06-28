using System.Collections.Generic;

public class GameConfigEntry : ICSVDeserializable
{
    private string _mKey;
    private string _mValue;

    public string Key
    {
        get
        {
            return _mKey;
        }
    }

    public string Value
    {
        get
        {
            return _mValue;
        }
    }

    public void CSVDeserialize(Dictionary<string, string[]> data, int index)
    {
        _mKey = data["Key"][index];
        _mValue = data["Value"][index];
    }
}
