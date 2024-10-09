public class Cheat
{
    public string Name { get; set; } = string.Empty;
    public List<Instruction> Instructions { get; set; } = new List<Instruction>();
}

public class Instruction
{
    public ulong Address { get; set; }
    public byte[]? Bytes { get; set; }
    public long Offset { get; set; }
    public long GetOffset(ulong baseAddress) => (long)(Address - baseAddress);
    public ulong CalculateNextAddress() => Address + (ulong)Bytes.Length;
}
