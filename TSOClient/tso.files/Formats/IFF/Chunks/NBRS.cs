﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using FSO.Files.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Files.Formats.IFF.Chunks
{
    /// <summary>
    /// This chunk defines all neighbours in a neighbourhood. 
    /// A neighbour is a specific version of a sim object with associated relationships and person data. (skills, person type)
    /// 
    /// These can be read within SimAntics without the avatar actually present. This is used to find and spawn suitable sims on 
    /// ped portals as visitors, and also drive phone calls to other sims in the neighbourhood.
    /// When neighbours are spawned, they assume the attributes saved here. A TS1 global call allows the game to save these attributes.
    /// </summary>
    public class NBRS : IffChunk
    {
        public List<Neighbour> Entries = new List<Neighbour>();
        public Dictionary<short, Neighbour> NeighbourByID = new Dictionary<short, Neighbour>();
        public Dictionary<uint, short> DefaultNeighbourByGUID = new Dictionary<uint, short>();

        public uint Version;

        /// <summary>
        /// Reads a NBRS chunk from a stream.
        /// </summary>
        /// <param name="iff">An Iff instance.</param>
        /// <param name="stream">A Stream object holding a OBJf chunk.</param>
        public override void Read(IffFile iff, Stream stream)
        {
            using (var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN))
            {
                io.ReadUInt32(); //pad
                Version = io.ReadUInt32(); //0x49 for latest game
                string magic = io.ReadCString(4); //SRBN
                var count = io.ReadUInt32();

                for (int i=0; i<count; i++)
                {
                    if (!io.HasMore) return;
                    var neigh = new Neighbour(io);
                    Entries.Add(neigh);
                    if (neigh.Unknown1 > 0)
                    {
                        NeighbourByID.Add(neigh.NeighbourID, neigh);
                        DefaultNeighbourByGUID[neigh.GUID] = neigh.NeighbourID;
                    }
                }
            }
            Entries = Entries.OrderBy(x => x.NeighbourID).ToList();
        }
    }

    public class Neighbour
    {
        public int Unknown1; //1
        public int Version; //0x4, 0xA
        //if 0xA, unknown3 follows
        //0x4 indicates person data size of 0xa0.. (160 bytes, or 80 entries)
        public int Unknown3; //9
        public string Name;
        public int MysteryZero;
        public int PersonMode; //0/5/9
        public short[] PersonData; //can be null

        public short NeighbourID;
        public uint GUID;
        public int UnknownNegOne; //negative 1 usually

        public Dictionary<int, List<short>> Relationships;

        public Neighbour() { }

        public Neighbour(IoBuffer io)
        {
            Unknown1 = io.ReadInt32();
            if (Unknown1 != 1) { return; }
            Version = io.ReadInt32();
            if (Version == 0xA)
            {
                //TODO: what version does this truly start?
                Unknown3 = io.ReadInt32();
                if (Unknown3 != 9) { }
            }
            Name = io.ReadNullTerminatedString();
            if (Name.Length % 2 == 0) io.ReadByte();
            MysteryZero = io.ReadInt32();
            if (MysteryZero != 0) { }
            PersonMode = io.ReadInt32();
            if (PersonMode > 0)
            {
                var size = (Version == 0x4) ? 0xa0 : 0x200;
                PersonData = new short[88];
                int pdi = 0;
                for (int i=0; i<size; i+=2)
                {
                    if (pdi >= 88)
                    {
                        io.ReadBytes(size - i);
                        break;
                    }
                    PersonData[pdi++] = io.ReadInt16();
                }
            }

            NeighbourID = io.ReadInt16();
            GUID = io.ReadUInt32();
            UnknownNegOne = io.ReadInt32();
            if (UnknownNegOne != -1) { }

            var entries = io.ReadInt32();
            Relationships = new Dictionary<int, List<short>>();
            for (int i=0; i<entries; i++)
            {
                var keyCount = io.ReadInt32();
                if (keyCount != 1) { }
                var key = io.ReadInt32();
                var values = new List<short>();
                var valueCount = io.ReadInt32();
                for (int j=0; j<valueCount; j++)
                {
                    values.Add((short)io.ReadInt32());
                }
                Relationships.Add(key, values);
            }
        }
    }
}
