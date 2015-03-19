using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Gumps;

namespace Server.Items
{
    [Furniture]
    [Flipable(0x2DD3, 0x2DD4)]
    public class AdvancedAlchemyTable : Item
    {
        [Constructable]
        public AdvancedAlchemyTable()
            : base(0x2DD3)
        {
            this.Name = "Alchemy table";
            this.Weight = 15.0;
        }

        public AdvancedAlchemyTable(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendGump(new AdvancedAlchemyGump(from));
        }


        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}