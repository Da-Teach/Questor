#region License/Copyright

// WhiteMagic - Injected .NET Helper Library
//     Copyright (C) 2009 Apoc
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

#region Warning Disables

#pragma warning disable 1591
// ReSharper disable InconsistentNaming

#endregion

namespace WhiteMagic.Native
{
    /// <summary>
    /// A class to extract PE header information from modules or PE files.
    /// </summary>
    public class PeHeaderParser
    {
        /// <summary>
        /// The handle, or base address, to the current PE file.
        /// </summary>
        public IntPtr ModulePtr;

        /// <summary>
        /// Creates a new instance of the PeHeaderParser class, using the specified path to a PE file.
        /// </summary>
        /// <param name="peFile"></param>
        public PeHeaderParser(string peFile)
        {
            // Yes, I know, this is some delicious copy/pasta.
            ModulePtr = Win32.LoadLibrary(peFile);

            if (ModulePtr == IntPtr.Zero)
            {
                throw new FileNotFoundException();
            }

            ParseHeaders();

            Win32.FreeLibrary(ModulePtr);
        }

        /// <summary>
        /// Creates a new instance of the PeHeaderParser class, using the handle or base address, to the specified module.
        /// </summary>
        /// <param name="hModule"></param>
        public PeHeaderParser(IntPtr hModule)
        {
            if (hModule == IntPtr.Zero)
            {
                throw new FileNotFoundException();
            }

            ModulePtr = hModule;

            ParseHeaders();
        }

        /// <summary>
        /// Retrieves the IMAGE_DOS_HEADER for this PE file.
        /// </summary>
        public ImageDosHeader DosHeader { get; private set; }
        /// <summary>
        /// Retrieves the IMAGE_NT_HEADER for this PE file. (This includes and nested structs, etc)
        /// </summary>
        public ImageNtHeader NtHeader { get; private set; }

        #region PE Header Shit

        #region Nested type: ImageDataDirectory

        [StructLayout(LayoutKind.Sequential)]
        public struct ImageDataDirectory
        {
            public uint VirtualAddress;
            public uint Size;

            public override string ToString()
            {
                return string.Format("--START DATA DIRECTORY--\n VirtualAddress: {0}, Size: {1}\n--END DATA DIRECTORY--\n",
                                     VirtualAddress.ToString("X"), Size.ToString("X"));
            }
        }

        #endregion

        #region Nested type: ImageDosHeader

        [StructLayout(LayoutKind.Sequential)]
        public struct ImageDosHeader
        {
            public UInt16 e_magic; // Magic number
            public UInt16 e_cblp; // Bytes on last page of file
            public UInt16 e_cp; // Pages in file
            public UInt16 e_crlc; // Relocations
            public UInt16 e_cparhdr; // Size of header in paragraphs
            public UInt16 e_minalloc; // Minimum extra paragraphs needed
            public UInt16 e_maxalloc; // Maximum extra paragraphs needed
            public UInt16 e_ss; // Initial (relative) SS value
            public UInt16 e_sp; // Initial SP value
            public UInt16 e_csum; // Checksum
            public UInt16 e_ip; // Initial IP value
            public UInt16 e_cs; // Initial (relative) CS value
            public UInt16 e_lfarlc; // File address of relocation table
            public UInt16 e_ovno; // Overlay number

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt16[] e_res1; // Reserved words

            public UInt16 e_oemid; // OEM identifier (for e_oeminfo)
            public UInt16 e_oeminfo; // OEM information; e_oemid specific

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public UInt16[] e_res2; // Reserved words

            public Int32 e_lfanew; // File address of new exe header
        }

        #endregion

        #region Nested type: ImageFileHeader

        [StructLayout(LayoutKind.Sequential)]
        public struct ImageFileHeader
        {
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;

            public override string ToString()
            {
                return
                    string.Format(
                        "--START FILE HEADER--\n Machine: {0}, NumberOfSections: {1}, TimeDateStamp: {2}, PointerToSymbolTable: {3}, NumberOfSymbols: {4}, SizeOfOptionalHeader: {5}, Characteristics: {6}\n--END FILE HEADER--\n",
                        Machine.ToString("X"), NumberOfSections.ToString("X"), TimeDateStamp.ToString("X"),
                        PointerToSymbolTable.ToString("X"), NumberOfSymbols.ToString("X"), SizeOfOptionalHeader.ToString("X"),
                        Characteristics.ToString("X"));
            }
        }

        #endregion

        #region Nested type: ImageNtHeader

        [StructLayout(LayoutKind.Sequential)]
        public struct ImageNtHeader
        {
            public uint Signature;
            public ImageFileHeader FileHeader;
            public ImageOptionalHeader OptionalHeader;

            public override string ToString()
            {
                return string.Format("Signature: {0},\n{1}\n{2}", Signature.ToString("X"), FileHeader, OptionalHeader);
            }
        }

        #endregion

        #region Nested type: ImageOptionalHeader

        [StructLayout(LayoutKind.Sequential)]
        public struct ImageOptionalHeader
        {
            //
            // Standard fields.
            //

            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;
            public uint BaseOfData;

            //
            // NT additional fields.
            //

            public uint ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public uint SizeOfStackReserve;
            public uint SizeOfStackCommit;
            public uint SizeOfHeapReserve;
            public uint SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public ImageDataDirectory[] DataDirectory;

            public override string ToString()
            {
                var dataDir = new StringBuilder();
                foreach (ImageDataDirectory directory in DataDirectory)
                {
                    dataDir.Append(directory.ToString());
                }
                return
                    string.Format(
                        "-- START OPTIONAL HEADER --\n Magic: {0}, MajorLinkerVersion: {1}, MinorLinkerVersion: {2}, SizeOfCode: {3}, SizeOfInitializedData: {4}, SizeOfUninitializedData: {5}, AddressOfEntryPoint: {6}, BaseOfCode: {7}, BaseOfData: {8}, ImageBase: {9}, SectionAlignment: {10}, FileAlignment: {11}, MajorOperatingSystemVersion: {12}, MinorOperatingSystemVersion: {13}, MajorImageVersion: {14}, MinorImageVersion: {15}, MajorSubsystemVersion: {16}, MinorSubsystemVersion: {17}, Win32VersionValue: {18}, SizeOfImage: {19}, SizeOfHeaders: {20}, CheckSum: {21}, Subsystem: {22}, DllCharacteristics: {23}, SizeOfStackReserve: {24}, SizeOfStackCommit: {25}, SizeOfHeapReserve: {26}, SizeOfHeapCommit: {27}, LoaderFlags: {28}, NumberOfRvaAndSizes: {29}, DataDirectory: {30}\n--END OPTIONAL HEADER--\n",
                        Magic.ToString("X"), MajorLinkerVersion.ToString("X"), MinorLinkerVersion.ToString("X"),
                        SizeOfCode.ToString("X"), SizeOfInitializedData.ToString("X"), SizeOfUninitializedData.ToString("X"),
                        AddressOfEntryPoint.ToString("X"), BaseOfCode.ToString("X"), BaseOfData.ToString("X"), ImageBase.ToString("X"),
                        SectionAlignment.ToString("X"), FileAlignment.ToString("X"),
                        MajorOperatingSystemVersion.ToString("X"), MinorOperatingSystemVersion.ToString("X"),
                        MajorImageVersion.ToString("X"), MinorImageVersion.ToString("X"),
                        MajorSubsystemVersion.ToString("X"), MinorSubsystemVersion.ToString("X"), Win32VersionValue.ToString("X"),
                        SizeOfImage.ToString("X"), SizeOfHeaders.ToString("X"), CheckSum.ToString("X"),
                        Subsystem.ToString("X"), DllCharacteristics.ToString("X"), SizeOfStackReserve.ToString("X"),
                        SizeOfStackCommit.ToString("X"), SizeOfHeapReserve.ToString("X"), SizeOfHeapCommit.ToString("X"),
                        LoaderFlags.ToString("X"), NumberOfRvaAndSizes.ToString("X"), dataDir);
            }
        }

        #endregion

        #endregion

        private void ParseHeaders()
        {
            DosHeader = (ImageDosHeader) Marshal.PtrToStructure(ModulePtr, typeof (ImageDosHeader));

            if (DosHeader.e_magic == PeHeaderConstants.IMAGE_DOS_SIGNATURE)
            {
                NtHeader =
                    (ImageNtHeader)
                    Marshal.PtrToStructure(new IntPtr(ModulePtr.ToInt32() + DosHeader.e_lfanew), typeof (ImageNtHeader));
            }
        }

        #region Nested type: PeHeaderConstants

        /// <summary>
        /// Contains constants ripped from WinNT.h
        /// </summary>
        public class PeHeaderConstants
        {
            public const int IMAGE_DOS_SIGNATURE = 0x5A4D;
            public const int IMAGE_FILE_32BIT_MACHINE = 0x0100;
            public const int IMAGE_FILE_AGGRESIVE_WS_TRIM = 0x0010;
            public const int IMAGE_FILE_BYTES_REVERSED_HI = 0x8000;
            public const int IMAGE_FILE_BYTES_REVERSED_LO = 0x0080;
            public const int IMAGE_FILE_DEBUG_STRIPPED = 0x0200;
            public const int IMAGE_FILE_DLL = 0x2000;
            public const int IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002;
            public const int IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020;
            public const int IMAGE_FILE_LINE_NUMS_STRIPPED = 0x0004;
            public const int IMAGE_FILE_LOCAL_SYMS_STRIPPED = 0x0008;
            public const int IMAGE_FILE_MACHINE_ALPHA = 0x0184;
            public const int IMAGE_FILE_MACHINE_ALPHA64 = 0x0284;
            public const int IMAGE_FILE_MACHINE_AM33 = 0x01d3;
            public const int IMAGE_FILE_MACHINE_AMD64 = 0x8664;
            public const int IMAGE_FILE_MACHINE_ARM = 0x01c0;
            public const int IMAGE_FILE_MACHINE_CEE = 0xC0EE;
            public const int IMAGE_FILE_MACHINE_CEF = 0x0CEF;
            public const int IMAGE_FILE_MACHINE_EBC = 0x0EBC;
            public const int IMAGE_FILE_MACHINE_I386 = 0x014c;
            public const int IMAGE_FILE_MACHINE_IA64 = 0x0200;
            public const int IMAGE_FILE_MACHINE_M32R = 0x9041;
            public const int IMAGE_FILE_MACHINE_MIPS16 = 0x0266;
            public const int IMAGE_FILE_MACHINE_MIPSFPU = 0x0366;
            public const int IMAGE_FILE_MACHINE_MIPSFPU16 = 0x0466;
            public const int IMAGE_FILE_MACHINE_POWERPC = 0x01F0;
            public const int IMAGE_FILE_MACHINE_POWERPCFP = 0x01f1;
            public const int IMAGE_FILE_MACHINE_R10000 = 0x0168;
            public const int IMAGE_FILE_MACHINE_R3000 = 0x0162;
            public const int IMAGE_FILE_MACHINE_R4000 = 0x0166;
            public const int IMAGE_FILE_MACHINE_SH3 = 0x01a2;
            public const int IMAGE_FILE_MACHINE_SH3DSP = 0x01a3;
            public const int IMAGE_FILE_MACHINE_SH3E = 0x01a4;
            public const int IMAGE_FILE_MACHINE_SH4 = 0x01a6;
            public const int IMAGE_FILE_MACHINE_SH5 = 0x01a8;
            public const int IMAGE_FILE_MACHINE_THUMB = 0x01c2;
            public const int IMAGE_FILE_MACHINE_TRICORE = 0x0520;
            public const int IMAGE_FILE_MACHINE_UNKNOWN = 0;
            public const int IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x0169;
            public const int IMAGE_FILE_NET_RUN_FROM_SWAP = 0x0800;
            public const int IMAGE_FILE_RELOCS_STRIPPED = 0x0001;
            public const int IMAGE_FILE_REMOVABLE_RUN_FROM_SWAP = 0x0400;
            public const int IMAGE_FILE_SYSTEM = 0x1000;
            public const int IMAGE_FILE_UP_SYSTEM_ONLY = 0x4000;
            public const int IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b;
            public const int IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b;
            public const int IMAGE_NT_SIGNATURE = 0x50450000;
            public const int IMAGE_OS2_SIGNATURE = 0x4E45;
            public const int IMAGE_OS2_SIGNATURE_LE = 0x4C45;
            public const int IMAGE_ROM_OPTIONAL_HDR_MAGIC = 0x107;
            public const int IMAGE_SIZEOF_FILE_HEADER = 20;
        }

        #endregion
    }
}