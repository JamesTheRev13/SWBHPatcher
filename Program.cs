#region Instructions
// TODO: Move cheats to a separate file
var cheatsToPatch = new List<Cheat>()
{
    new Cheat
    {
        Name = "Infinite Jetpack",
        Instructions = new List<Instruction>
        {
            new() {
                Address = 0x140163480UL, // Address of the instruction to patch (taken from Ghidra)
                Bytes = [0x90, 0x90, 0x90, 0x90, 0x90] // 0x90 == NOP instruction - 5 NOPs == 5 bytes (instruction we are replacing is 5 bytes long)
            },
        },

    }
};
#endregion

var baseAddress = 0x140000000UL;
string gameExecutable = string.Empty;

Console.WriteLine("Enter the path to the game's installation directory (ex: C:\\Program Files (x86)\\Steam\\steamapps\\common\\STAR WARS Bounty Hunter):");
while (string.IsNullOrEmpty(gameExecutable))
{
    var input = Console.ReadLine() ?? string.Empty;
    //var input = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\STAR WARS Bounty Hunter";
    var fullPath = Path.Combine(input, "TangoPC.exe");

    if (!File.Exists(fullPath))
    {
        Console.WriteLine("Invalid path or game executable not found. Try again.");
    }
    else 
    {
        Console.WriteLine($"Game Path Found: {fullPath}");
        gameExecutable = fullPath;
    }
}

try
{
    // Create a backup of the original executable
    var gameDirectory = Path.GetDirectoryName(gameExecutable);
    string backupPath = Path.Combine(gameDirectory ?? throw new Exception($"Invalid Game Directory: {gameDirectory}."), "TangoPC_backup.exe");
    if (!File.Exists(backupPath))
    {
        File.Copy(gameExecutable, backupPath, overwrite: false);
        Console.WriteLine("Backup of the original executable created successfully.");
    }
    else
        Console.WriteLine("Backup of the original executable already exists. Skipping backup creation.");

    using (FileStream fs = new FileStream(gameExecutable, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
    {
        foreach (var cheat in cheatsToPatch)
        {
            Console.WriteLine($"Patching {cheat.Instructions.Count} instructions for cheat: {cheat.Name}");
            foreach (var instruction in cheat.Instructions)
            {
                // Validate the instruction
                if (instruction.Bytes == null || instruction.Bytes.Length == 0)
                {
                    var msg = $"Invalid instruction \nat Address: {instruction.Address:X} \nBytes: {instruction.Bytes}";
                    throw new Exception(msg);
                }

                var offset = instruction.GetOffset(baseAddress);
                //offset = 0x15f190; // Hardcoded offset for testing
                //instruction.Bytes = [0x90, 0x90, 0x90, 0x90, 0x90]; // Hardcoded NOP instruction for testing
                Console.WriteLine($"Patching instruction at offset: {offset:X}");

                // Validate the offset
                if (offset == -1)
                {
                    var msg = $"Invalid offset {offset:X} for instruction \nat Address: {instruction.Address:X}";
                    throw new Exception(msg);
                }
                if (offset < 0 || offset >= fs.Length)
                {
                    var msg = $"Calculated offset {offset:X} for Address {instruction.Address:X} is out of bounds.";
                    throw new Exception();
                }

                // Patch the instruction
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Write(instruction.Bytes, 0, instruction.Bytes.Length);

                // Read back the bytes to log them for debugging
                //fs.Seek(offset, SeekOrigin.Begin);
                //byte[] writtenBytes = new byte[instruction.Bytes.Length];
                //fs.Read(writtenBytes, 0, writtenBytes.Length);
                //Console.WriteLine($"Bytes written at address {instruction.Address:X}: {BitConverter.ToString(writtenBytes)}");

                Console.WriteLine($"Instruction patched successfully for Address: {instruction.Address:X}");
            }
        }
        Console.WriteLine($"TangoPC.exe patched {cheatsToPatch.Count} Mod(s) successfully.");
    }

    // Launch the game
    System.Diagnostics.Process.Start(gameExecutable);
    Console.WriteLine("Game launched successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}