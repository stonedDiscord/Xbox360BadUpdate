using IO;
using System.Diagnostics;
using System.Text;

namespace ObjLink
{
    class COFF_FILE_HEADER
    {
        /* 0x00 */ public ushort Machine;
        /* 0x02 */ public ushort NumberOfSections;
        /* 0x04 */ public uint TimeDateStamp;
        /* 0x08 */ public int PointerToSymbolTable;
        /* 0x0C */ public int NumberOfSymbols;
        /* 0x10 */ public ushort SizeOfOptionalHeader;
        /* 0x12 */ public ushort Characteristics;
    }

    class SECTION_TABLE_ENTRY
    {
        /* 0x00 */ public char[] Name;
        /* 0x08 */ public uint VirtualSize;
        /* 0x0C */ public uint VirtualAddress;
        /* 0x10 */ public int SizeOfRawData;
        /* 0x14 */ public int PointerToRawData;
        /* 0x18 */ public int PointerToRelocations;
        /* 0x1C */ public int PointerToLineNumbers;
        /* 0x20 */ public short NumberOfRelocations;
        /* 0x22 */ public short NumberOfLineNumbers;
        /* 0x24 */ public int Characteristics;

        public List<COFF_RELOCATION> Relocations = new List<COFF_RELOCATION>();

        public string GetNameClean()
        {
            return new string(this.Name).Trim('\0');
        }
    }

    class COFF_RELOCATION
    {
        /* 0x00 */ public int VirtualAddress;
        /* 0x04 */ public int SymbolTableIndex;
        /* 0x08 */ public short Type;
    }

    class SYMBOL_TABLE_ENTRY
    {
        /* 0x00 */ public byte[] Name;
        /* 0x08 */ public int Value;
        /* 0x0C */ public short SectionNumber;          // 1-based index!!!!
        /* 0x0E */ public short Type;
        /* 0x10 */ public byte StorageClass;
        /* 0x11 */ public byte NumberOfAuxSymbols;

        public byte[] AuxSymbolData;

        public string GetNameClean()
        {
            return Encoding.UTF8.GetString(this.Name).Trim('\0');
        }
    }

    internal class Program
    {
        static byte[] SaveGprCode = new byte[]
        {
            0xF9, 0xC1, 0xFF, 0x68, 0xF9, 0xE1, 0xFF, 0x70, 0xFA, 0x01, 0xFF, 0x78, 0xFA, 0x21, 0xFF, 0x80,
            0xFA, 0x41, 0xFF, 0x88, 0xFA, 0x61, 0xFF, 0x90, 0xFA, 0x81, 0xFF, 0x98, 0xFA, 0xA1, 0xFF, 0xA0,
            0xFA, 0xC1, 0xFF, 0xA8, 0xFA, 0xE1, 0xFF, 0xB0, 0xFB, 0x01, 0xFF, 0xB8, 0xFB, 0x21, 0xFF, 0xC0,
            0xFB, 0x41, 0xFF, 0xC8, 0xFB, 0x61, 0xFF, 0xD0, 0xFB, 0x81, 0xFF, 0xD8, 0xFB, 0xA1, 0xFF, 0xE0,
            0xFB, 0xC1, 0xFF, 0xE8, 0xFB, 0xE1, 0xFF, 0xF0, 0x91, 0x81, 0xFF, 0xF8, 0x4E, 0x80, 0x00, 0x20
        };

        static byte[] RestoreGprCode = new byte[]
        {
            0xE9, 0xC1, 0xFF, 0x68, 0xE9, 0xE1, 0xFF, 0x70, 0xEA, 0x01, 0xFF, 0x78, 0xEA, 0x21, 0xFF, 0x80,
            0xEA, 0x41, 0xFF, 0x88, 0xEA, 0x61, 0xFF, 0x90, 0xEA, 0x81, 0xFF, 0x98, 0xEA, 0xA1, 0xFF, 0xA0,
            0xEA, 0xC1, 0xFF, 0xA8, 0xEA, 0xE1, 0xFF, 0xB0, 0xEB, 0x01, 0xFF, 0xB8, 0xEB, 0x21, 0xFF, 0xC0,
            0xEB, 0x41, 0xFF, 0xC8, 0xEB, 0x61, 0xFF, 0xD0, 0xEB, 0x81, 0xFF, 0xD8, 0xEB, 0xA1, 0xFF, 0xE0,
            0xEB, 0xC1, 0xFF, 0xE8, 0xEB, 0xE1, 0xFF, 0xF0, 0x81, 0x81, 0xFF, 0xF8, 0x7D, 0x88, 0x03, 0xA6,
            0x4E, 0x80, 0x00, 0x20
        };

        static int AlignmentIntervalFromSectionCharacteristics(int characteristics)
        {
            int alignment = 4;

            (int flag, int alignment)[] AlignmentFlagOptions = new (int flag, int alignment)[]
            {
                (0x00000000, 0),
                (0x00100000, 1),
                (0x00200000, 2),
                (0x00300000, 4),
                (0x00400000, 8),
                (0x00500000, 16),
                (0x00600000, 32),
                (0x00700000, 64),
                (0x00800000, 128),
                (0x00900000, 256),
                (0x00A00000, 512),
                (0x00B00000, 1024),
                (0x00C00000, 2048),
                (0x00D00000, 4096),
                (0x00E00000, 8192)
            };

            // Get the alignment interval from the characteristic flags.
            int alignmentIndex = (characteristics >> 20) & 0xF;
            if (alignmentIndex > 0 && alignmentIndex < AlignmentFlagOptions.Length)
                alignment = AlignmentFlagOptions[alignmentIndex].alignment;

            return alignment;
        }

        static void Main(string[] args)
        {
            // Print version info.
            Console.WriteLine("ObjLink v0.1");
            Console.WriteLine();

            // Check if the correct number of arguments have been provided.
            if (args.Length < 4)
            {
                // Print use.
                Console.WriteLine("Use: ObjLink.exe <obj file> <output file> <entrypoint> <base address> [options]");
                Console.WriteLine("   <obj file>          Object file to link");
                Console.WriteLine("   <output file>       Output file path");
                Console.WriteLine("   <entrypoint>        Entrypoint function name");
                Console.WriteLine("   <base address>      Base address for output file, must be in base-16");
                Console.WriteLine();
                Console.WriteLine("Optional:");
                Console.WriteLine("   --sym <file path>   File containing external symbol information");

                return;
            }

            // Parse command line options.
            CommandLine cmd = new CommandLine(args);
            string objFilePath = args[0];
            string outputFilePath = args[1];
            string entryPointName = args[2];
            uint baseAddress = uint.Parse(args[3], System.Globalization.NumberStyles.HexNumber);
            string symbolFilePath = cmd.GetKeyValue("--sym", false);

            // Dictionary of external symbols and their address.
            Dictionary<string, uint> ExternalSymbolLookupTable = new Dictionary<string, uint>();

            // Parse the symbol file if it was specified.
            if (symbolFilePath != null)
            {
                // Read all the entries from the symbol file and parse each one.
                string[] symbolFileEntries = File.ReadAllLines(symbolFilePath);
                for (int i = 0; i < symbolFileEntries.Length; i++)
                {
                    string[] pieces = symbolFileEntries[i].Split('=');
                    string symbolName = pieces[0].Trim();
                    uint symbolAddress = Convert.ToUInt32(pieces[1].Replace(";", "").Trim(), 16);

                    // Add the symbol to the lookup dictionary.
                    ExternalSymbolLookupTable.Add(symbolName, symbolAddress);
                }
            }

            // Open the object file for reading.
            EndianReader reader = new EndianReader(Endianness.Little, new FileStream(objFilePath, FileMode.Open, FileAccess.Read, FileShare.Read));

            // Parse the COFF file header.
            COFF_FILE_HEADER coffHeader = new COFF_FILE_HEADER();
            coffHeader.Machine = reader.ReadUInt16();
            coffHeader.NumberOfSections = reader.ReadUInt16();
            coffHeader.TimeDateStamp = reader.ReadUInt32();
            coffHeader.PointerToSymbolTable = reader.ReadInt32();
            coffHeader.NumberOfSymbols = reader.ReadInt32();
            coffHeader.SizeOfOptionalHeader = reader.ReadUInt16();
            coffHeader.Characteristics = reader.ReadUInt16();

            // Loop and read all the section headers.
            List<SECTION_TABLE_ENTRY> sectionHeaders = new List<SECTION_TABLE_ENTRY>();
            for (int i = 0; i < coffHeader.NumberOfSections; i++)
            {
                SECTION_TABLE_ENTRY sectionHdr = new SECTION_TABLE_ENTRY();
                sectionHdr.Name = reader.ReadChars(8);
                sectionHdr.VirtualSize = reader.ReadUInt32();
                sectionHdr.VirtualAddress = reader.ReadUInt32();
                sectionHdr.SizeOfRawData = reader.ReadInt32();
                sectionHdr.PointerToRawData = reader.ReadInt32();
                sectionHdr.PointerToRelocations = reader.ReadInt32();
                sectionHdr.PointerToLineNumbers = reader.ReadInt32();
                sectionHdr.NumberOfRelocations = reader.ReadInt16();
                sectionHdr.NumberOfLineNumbers = reader.ReadInt16();
                sectionHdr.Characteristics = reader.ReadInt32();

                // Save the current position in the section table.
                long position = reader.BaseStream.Position;

                // Seek to the relocation data for the section.
                reader.BaseStream.Position = sectionHdr.PointerToRelocations;

                // Loop and parse relocations for the section.
                for (int x = 0; x < sectionHdr.NumberOfRelocations; x++)
                {
                    COFF_RELOCATION reloc = new COFF_RELOCATION();
                    reloc.VirtualAddress = reader.ReadInt32();
                    reloc.SymbolTableIndex = reader.ReadInt32();
                    reloc.Type = reader.ReadInt16();

                    sectionHdr.Relocations.Add(reloc);
                }

                // Restore reader position.
                reader.BaseStream.Position = position;

                sectionHeaders.Add(sectionHdr);
            }

            // Seek to the symbol table.
            reader.BaseStream.Position = coffHeader.PointerToSymbolTable;

            int auxSymbolCount = 0;

            // Loop and read the symbol table entries.
            List<SYMBOL_TABLE_ENTRY> symbolEntries = new List<SYMBOL_TABLE_ENTRY>();
            for (int i = 0; i < coffHeader.NumberOfSymbols; i++)
            {
                SYMBOL_TABLE_ENTRY symbol = new SYMBOL_TABLE_ENTRY();

                // Check if this is an aux symbol or not.
                if (auxSymbolCount == 0)
                {
                    symbol.Name = reader.ReadBytes(8);
                    symbol.Value = reader.ReadInt32();
                    symbol.SectionNumber = reader.ReadInt16();
                    symbol.Type = reader.ReadInt16();
                    symbol.StorageClass = reader.ReadByte();
                    auxSymbolCount = symbol.NumberOfAuxSymbols = reader.ReadByte();
                }
                else
                {
                    symbol.AuxSymbolData = reader.ReadBytes(18);
                    auxSymbolCount--;
                }

                symbolEntries.Add(symbol);
            }

            // Save the position of the string table,
            int stringTableOffset = (int)reader.BaseStream.Position;
            int stringTableSize = reader.ReadInt32() - 4;

            // Loop and parse the string table.
            Dictionary<int, string> stringTable = new Dictionary<int, string>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int offset = (int)reader.BaseStream.Position - stringTableOffset;
                stringTable.Add(offset, reader.ReadNullTerminatingString());
            }


            Console.WriteLine($"Searching for symbol '{entryPointName}'...");

            // Find the symbol entry that matches the entry point name.
            int entryPointSymbolIndex = -1;
            for (int i = 0; i < symbolEntries.Count; i++)
            {
                // Check if this symbol has matching name.
                if (GetSymbolName(symbolEntries[i]) == entryPointName)
                {
                    entryPointSymbolIndex = i;
                    break;
                }

                // Skip aux symbol entries.
                i += symbolEntries[i].NumberOfAuxSymbols;
            }

            // Make sure we found a symbol with matching name.
            if (entryPointSymbolIndex == -1)
            {
                Console.WriteLine($"Failed to find symbol for name '{entryPointName}'!");
                return;
            }


            string GetSymbolName(SYMBOL_TABLE_ENTRY symbol)
            {
                // Check if the symbol name is short enough to be inline or not.
                if (BitConverter.ToInt32(symbol.Name, 0) == 0)
                {
                    // Fetch the name from the string table.
                    int offset = BitConverter.ToInt32(symbol.Name, 4);
                    return stringTable[offset];
                }
                else
                {
                    return symbol.GetNameClean();
                }
            }


            // Create the output file.
            FileStream outputFs = new FileStream(outputFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            EndianReader outputReader = new EndianReader(Endianness.Big, outputFs);
            EndianWriter outputWriter = new EndianWriter(Endianness.Big, outputFs);

            // Create a list of symbols that need to be added to the output binary and symbols that need relocations.
            Queue<int> functionsToAdd = new Queue<int>();
            Queue<int> dataToAdd = new Queue<int>();
            Queue<int> externsToAdd = new Queue<int>();
            HashSet<int> symbolsAdded = new HashSet<int>();
            HashSet<int> relocationQueue = new HashSet<int>();

            // Dictionary of symbols and their final virtual address.
            Dictionary<string, uint> SymbolLookupTable = new Dictionary<string, uint>();
            Dictionary<int, uint> SectionVATable = new Dictionary<int, uint>();

            // Add the symbol for main to the functions list.
            functionsToAdd.Enqueue(entryPointSymbolIndex);

            // Recursively process functions that need to be added to the output file.
            while (functionsToAdd.Count > 0)
            {
                // Get the top item in the queue.
                int debugSymbolIndex = functionsToAdd.Dequeue();
                if (symbolsAdded.Contains(debugSymbolIndex) == true)
                    continue;

                // Check the name of the section this symbol points to.
                Debug.Assert(sectionHeaders[symbolEntries[debugSymbolIndex].SectionNumber].GetNameClean() == ".debug$S");

                // Save the name of the symbol using the .debug$S symbol.
                string symbolName = GetSymbolName(symbolEntries[debugSymbolIndex]);

                // Find a symbol that points to the .text section for the entry point function.
                int sectionIndex = symbolEntries[debugSymbolIndex].SectionNumber - 1;
                int symbolIndex = symbolEntries.FindIndex(s => s.SectionNumber == sectionIndex);

                // Calculate the address the symbol will be written to in the output file.
                sectionIndex = symbolEntries[symbolIndex].SectionNumber;
                uint symbolBaseAddress = baseAddress + (uint)outputWriter.BaseStream.Position;

                Console.WriteLine($"Adding 0x{symbolBaseAddress:X08} '{symbolName}'");
                SymbolLookupTable.Add(symbolName, symbolBaseAddress);
                SectionVATable.Add(sectionIndex, symbolBaseAddress);

                // Seek to and read the data for this function.
                reader.BaseStream.Position = sectionHeaders[sectionIndex].PointerToRawData;
                byte[] functionData = reader.ReadBytes(sectionHeaders[sectionIndex].SizeOfRawData);

                // Write the function data to the output file.
                outputWriter.Write(functionData);

                // Loop through the relocations for this section and queue additional functions and data as needed.
                for (int i = 0; i < sectionHeaders[sectionIndex].Relocations.Count; i++)
                {
                    // Check the section index to determine if the relocation points to external data or not.
                    int refIndex = sectionHeaders[sectionIndex].Relocations[i].SymbolTableIndex;
                    if (refIndex != 0)
                    {
                        if (symbolEntries[refIndex].SectionNumber != 0)
                        {
                            // Check the relocation type.
                            if (symbolEntries[refIndex].Type == 0x20)
                                functionsToAdd.Enqueue(refIndex);
                            else
                                dataToAdd.Enqueue(refIndex);
                        }
                        else
                        {
                            // Add the symbol to the list of externs.
                            externsToAdd.Enqueue(refIndex);
                        }
                    }
                }

                // Add this function to the list of symbols that have been added.
                symbolsAdded.Add(debugSymbolIndex);
            }

            // Create the __savegprlr/__restgprlr sleds.
            uint saveGprCodeBaseAddress = baseAddress + (uint)outputWriter.BaseStream.Position;
            outputWriter.Write(SaveGprCode);
            for (int i = 14; i <= 31; i++)
            {
                SymbolLookupTable.Add($"__savegprlr_{i}", saveGprCodeBaseAddress);
                saveGprCodeBaseAddress += 4;
            }

            uint restoreGprCodeBaseAddress = baseAddress + (uint)outputWriter.BaseStream.Position;
            outputWriter.Write(RestoreGprCode);
            for (int i = 14; i <= 31; i++)
            {
                SymbolLookupTable.Add($"__restgprlr_{i}", restoreGprCodeBaseAddress);
                restoreGprCodeBaseAddress += 4;
            }

            // Loop through all the external symbols and create fake import stubs for them.
            bool missingExterns = false;
            while (externsToAdd.Count > 0)
            {
                // Get the top item in the queue.
                int symbolIndex = externsToAdd.Dequeue();
                string name = GetSymbolName(symbolEntries[symbolIndex]);

                // Check if we've already resolved this symbol.
                if (SymbolLookupTable.ContainsKey(name) == true)
                    continue;

                // Check we have an extern address for this symbol.
                if (ExternalSymbolLookupTable.ContainsKey(name) == false)
                {
                    // Flag that we have missing externs.
                    Console.WriteLine($"ERROR: unresolved external symbol '{name}'");
                    missingExterns = true;

                    // Add a faux entry so we don't spew multiple errors for the same symbol name.
                    SymbolLookupTable.Add(name, 0);
                    continue;
                }

                // Resolve the extern symbol address.
                uint externAddress = ExternalSymbolLookupTable[name];

                // Add the symbol to the list of resolved symbols.
                uint stubAddress = baseAddress + (uint)outputWriter.BaseStream.Position;
                Console.WriteLine($"Adding 0x{stubAddress:X08} '{name}' (import) 0x{externAddress:X08}");
                SymbolLookupTable.Add(name, stubAddress);

                // Create a fake import stub for the external symbol.
                outputWriter.Write((uint)0x3D600000 | ((externAddress >> 16) & 0xFFFF));        // lis   r11, 0xAABB
                outputWriter.Write((uint)0x616B0000 | (externAddress & 0xFFFF));                // ori   r11, r11, 0xCCDD
                outputWriter.Write((uint)0x7D6903A6);                                           // mtctr r11
                outputWriter.Write((uint)0x4E800420);                                           // bctr
            }

            // If we had missing external symbols bail out now.
            if (missingExterns == true)
                return;

            // Add any data referenced to the output file.
            outputWriter.AlignToBoundary(16);
            while (dataToAdd.Count > 0)
            {
                // Get the top item in the queue and make sure we haven't already added it.
                int symbolIndex = dataToAdd.Dequeue();
                if (symbolsAdded.Contains(symbolIndex) == true)
                    continue;

                // Check if the section has already been written to file.
                int sectionIndex = symbolEntries[symbolIndex].SectionNumber - 1;
                if (SectionVATable.ContainsKey(sectionIndex) == false)
                {
                    // Check if the symbol has strict alignment requirements.
                    int alignment = AlignmentIntervalFromSectionCharacteristics(sectionHeaders[sectionIndex].Characteristics);
                    outputWriter.AlignToBoundary(alignment);

                    uint sectionBaseAddress = baseAddress + (uint)outputWriter.BaseStream.Position;

                    // Seek to and read the data for this symbol.
                    reader.BaseStream.Position = sectionHeaders[sectionIndex].PointerToRawData;
                    byte[] functionData = reader.ReadBytes(sectionHeaders[sectionIndex].SizeOfRawData);
                    //string test = Encoding.UTF8.GetString(functionData);

                    // Write the data to the output file.
                    outputWriter.Write(functionData);
                    outputWriter.AlignToBoundary(4);

                    SectionVATable.Add(sectionIndex, sectionBaseAddress);
                }

                // Calculate the address the symbol will located at in memory.
                uint symbolBaseAddress = SectionVATable[sectionIndex] + (uint)symbolEntries[symbolIndex].Value;

                string symbolName = GetSymbolName(symbolEntries[symbolIndex]);
                Console.WriteLine($"Adding 0x{symbolBaseAddress:X08} '{symbolName}'");
                SymbolLookupTable.Add(symbolName, symbolBaseAddress);

                // Add this function to the list of symbols that have been added.
                symbolsAdded.Add(symbolIndex);
            }
            outputWriter.AlignToBoundary(16);

            // Loop through every symbol added and process relocations.
            Console.WriteLine("Processing relocations...");
            for (int i = 0; i < symbolsAdded.Count; i++)
            {
                // Get the name and address of the symbol.
                int symbolIndex = symbolsAdded.ElementAt(i);
                string symbolName = GetSymbolName(symbolEntries[symbolIndex]);
                uint symbolAddress = SymbolLookupTable[symbolName];
                int symbolBaseOffset = (int)(symbolAddress - baseAddress);

                // Get the section index for this symbol and process relocations.
                int sectionIndex = symbolEntries[symbolIndex].SectionNumber - 1;
                for (int x = 0; x < sectionHeaders[sectionIndex].Relocations.Count; x++)
                {
                    // Get the name and address of the symbol being referenced.
                    COFF_RELOCATION reloc = sectionHeaders[sectionIndex].Relocations[x];
                    if (reloc.Type == 0x12)
                    {
                        Debug.Assert(reloc.SymbolTableIndex == 0);
                        continue;
                    }

                    string refSymbolName = GetSymbolName(symbolEntries[reloc.SymbolTableIndex]);
                    uint refSymbolAddress = SymbolLookupTable[refSymbolName];
                    int refSymbolOffset = (int)(refSymbolAddress - baseAddress);

                    int relocOffset = symbolBaseOffset + reloc.VirtualAddress;
                    outputReader.BaseStream.Position = relocOffset;

                    // Check the relocation type and handle accordingly.
                    switch (reloc.Type)
                    {
                        case 0x0006:
                            {
                                // IMAGE_REL_PPC_REL24  - A 24-bit PC-relative offset to the symbol’s location.
                                int pcOffset = refSymbolOffset - relocOffset;
                                //Debug.Assert((pcOffset & 0xFF000000) == 0);

                                // Mask in the 24-bit PC-relative offset.
                                uint opcode = outputReader.ReadUInt32();
                                opcode = (uint)((opcode & ~0x03FFFFFC) | (pcOffset & 0x03FFFFFC));

                                // Write new opcode value.
                                outputWriter.BaseStream.Position = relocOffset;
                                outputWriter.Write(opcode);
                                break;
                            }
                        case 0x0010:
                            {
                                // IMAGE_REL_PPC_REFHI - The high 16 bits of the target’s 32-bit VA. This is used for the first instruction in a
                                // two-instruction sequence that loads a full address. This relocation must be immediately followed by a PAIR relocation
                                // whose SymbolTableIndex contains a signed 16-bit displacement that is added to the upper 16 bits that was taken from
                                // the location that is being relocated.

                                // Mask in the upper 16-bits of the target symbol address.
                                uint opcode = outputReader.ReadUInt32();
                                opcode = (uint)((opcode & 0xFFFF0000) | ((refSymbolAddress >> 16) & 0xFFFF));

                                // Write new opcode value.
                                outputWriter.BaseStream.Position = relocOffset;
                                outputWriter.Write(opcode);
                                break;
                            }
                        case 0x0011:
                            {
                                // IMAGE_REL_PPC_REFLO - The low 16 bits of the target’s VA.

                                // Mask in the lower 16-bits of the target symbol address.
                                uint opcode = outputReader.ReadUInt32();
                                opcode = (uint)((opcode & 0xFFFF0000) | (refSymbolAddress & 0xFFFF));

                                // Write new opcode value.
                                outputWriter.BaseStream.Position = relocOffset;
                                outputWriter.Write(opcode);
                                break;
                            }
                        case 0x0012:
                            {
                                // IMAGE_REL_PPC_PAIR - A relocation that is valid only when it immediately follows a REFHI or SECRELHI relocation.
                                // Its SymbolTableIndex contains a displacement and not an index into the symbol table.

                                // Do we need to do anything here?
                                Debug.Assert(reloc.SymbolTableIndex == 0);
                                break;
                            }
                        default:
                            {
                                throw new NotSupportedException($"Relocation type 0x{reloc.Type:X04} not supported");
                            }
                    }
                }
            }

            // Object file successfully linked.
            Console.WriteLine("Successfully linked object file");

            // Close output file.
            outputReader.Close();
            outputWriter.Close();
            outputFs.Close();
        }
    }
}