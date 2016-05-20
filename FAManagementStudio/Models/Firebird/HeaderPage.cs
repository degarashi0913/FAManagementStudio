using System.Runtime.InteropServices;

namespace FAManagementStudio.Models.Firebird
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HeaderPage
    {
        public Pag hdr_header;
        // Page size of database
        [MarshalAs(UnmanagedType.U2)]
        public ushort hdr_page_size;
        // Version of on-disk structure
        [MarshalAs(UnmanagedType.U2)]
        public ushort hdr_ods_version;
        // Page number of PAGES relation
        [MarshalAs(UnmanagedType.U4)]
        public uint hdr_PAGES;
        // Page number of next hdr page
        [MarshalAs(UnmanagedType.U4)]
        public uint hdr_next_page;
        // Oldest interesting transaction
        [MarshalAs(UnmanagedType.U4)]
        public uint hdr_oldest_transaction;
        // Oldest transaction thought active
        [MarshalAs(UnmanagedType.U4)]
        public uint hdr_oldest_active;
        // Next transaction id
        [MarshalAs(UnmanagedType.U4)]
        public uint hdr_next_transaction;
        // sequence number of file
        [MarshalAs(UnmanagedType.U2)]
        public ushort hdr_sequence;
        // Flag settings, see below
        [MarshalAs(UnmanagedType.U2)]
        public ushort hdr_flags;
        // Date/time of creation
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] hdr_creation_date;
        // Next attachment id
        [MarshalAs(UnmanagedType.I4)]
        public int hdr_attachment_id;
        // Event count for shadow synchronization
        [MarshalAs(UnmanagedType.I4)]
        public int hdr_shadow_count;
        // CPU database was created on
        [MarshalAs(UnmanagedType.U1)]
        public byte hdr_cpu;
        // OS database was created under
        [MarshalAs(UnmanagedType.U1)]
        public byte hdr_os;
        // Compiler of engine on which database was created
        [MarshalAs(UnmanagedType.U1)]
        public byte hdr_cc;
        // Cross-platform database transfer compatibility flags
        [MarshalAs(UnmanagedType.U1)]
        public byte hdr_compatibility_flags;
        // Update version of ODS
        [MarshalAs(UnmanagedType.U2)]
        public ushort hdr_ods_minor;
        // offset of HDR_end in page
        [MarshalAs(UnmanagedType.U2)]
        public ushort hdr_end;
        // Page buffers for database cache
        [MarshalAs(UnmanagedType.U4)]
        public uint hdr_page_buffers;
        // Oldest snapshot of active transactions
        [MarshalAs(UnmanagedType.U4)]
        public uint hdr_oldest_snapshot;
        // The amount of pages in files locked for backup
        [MarshalAs(UnmanagedType.I4)]
        public int hdr_backup_pages;
        // Page at which processing is in progress
        [MarshalAs(UnmanagedType.U4)]
        public uint hdr_crypt_page;
        // Last page to crypt
        [MarshalAs(UnmanagedType.U4)]
        public uint hdr_top_crypt;
        // Name of plugin used to crypt this DB
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string hdr_crypt_plugin;
        // Stuff to be named later - reserved for minor changes
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] hdr_misc;
        // Misc data
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
        public string hdr_data;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Pag
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte pag_type;
        [MarshalAs(UnmanagedType.U1)]
        public byte pag_flags;
        // not used but anyway present because of alignment rules
        [MarshalAs(UnmanagedType.U2)]
        public ushort pag_reserved;
        [MarshalAs(UnmanagedType.U4)]
        public uint pag_generation;
        [MarshalAs(UnmanagedType.U4)]
        public uint pag_scn;
        //for validation
        [MarshalAs(UnmanagedType.U4)]
        public uint pag_pageno;
    }
}
