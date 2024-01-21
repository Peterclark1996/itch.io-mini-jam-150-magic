using static Util;

public struct KeyCombination
{
    public FloorName FloorName { get; }
    public Key Key1 { get; }
    public Key Key2 { get; }
    // public Key Key3 { get; }
    // public Key Key4 { get; }

    public KeyCombination(FloorName floorName, Key key1, Key key2/*, Key key3, Key key4*/) {
        FloorName = floorName;
        Key1 = key1;
        Key2 = key2;
        // Key3 = key3;
        // Key4 = key4;
    }
}
