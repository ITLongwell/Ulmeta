using System;
using Server;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Items;
using System.IO;
using Server.Commands;
using System.AdvancedAlchemy;
using Server.Engines.Craft;

namespace Server.Items
{

    public enum AdvancedPotionEffect
    {
        Heal,
        Regen,
        Harm,
        Cure,
        Poison,
        Agility,
        Slow,
        Strength,
        Weaken,
        Mind,
        Mindless,
        Refresh,
        Burden,
        Nightsight,
        Blindness,
        Explosion,
        Conflagration,
        ConfusionBlast,
        Invisibility,
        ExplodingTar,
        Duration,
        SkillMod,
        Mute
    }

    public enum AdvancedPotionEffectLevel
    {
        Lesser = 1, Normal = 3, Greater = 9, Potent = 27
    }

    public class AdvancedPotion : Item //, ICraftable
    {
        private Dictionary<AdvancedPotionEffect, AdvancedPotionEffectLevel> m_properties;
        public Dictionary<AdvancedPotionEffect, AdvancedPotionEffectLevel> Properties
        {
            get
            {
                if (m_properties == null)
                    m_properties = new Dictionary<AdvancedPotionEffect, AdvancedPotionEffectLevel>();

                return m_properties;
            }
        }

        private List<SkillMod> m_skillmods;
        public List<SkillMod> Skillmods
        {
            get
            {
                if (m_skillmods == null)
                    m_skillmods = new List<SkillMod>();

                return m_skillmods;
            }
        }

        private class EffectTimer : Timer
        {
            Mobile from;

            public EffectTimer
                (TimeSpan delay, Mobile m)
                : base(delay)
            {
                Priority = TimerPriority.OneSecond;
                from = m;
            }

            protected override void OnTick()
            {
            }
        }

        public AdvancedPotion( Dictionary<AdvancedPotionEffect, AdvancedPotionEffectLevel> props, List<SkillMod> skills, int id, string name ) : this( props, id, name )
        {
            m_skillmods = skills;
        }


        public AdvancedPotion( Dictionary<AdvancedPotionEffect, AdvancedPotionEffectLevel> props, int id, string name): base( id )
        {
            m_properties = props;
            Name = name;
        }

        public AdvancedPotion(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1); //Version
            writer.Write(m_properties.Count());

            foreach (KeyValuePair<AdvancedPotionEffect, AdvancedPotionEffectLevel> kvp in m_properties)
            {
                writer.Write((int)kvp.Key);
                writer.Write((int)kvp.Value);
            }

        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version > 0)
            {
                int count = reader.ReadInt();

                m_properties = new Dictionary<AdvancedPotionEffect, AdvancedPotionEffectLevel>();

                for (int i = 0; i < count; i++)
                {
                    AdvancedPotionEffect adv = (AdvancedPotionEffect)reader.ReadInt();
                    AdvancedPotionEffectLevel pot = (AdvancedPotionEffectLevel)reader.ReadInt();
                    m_properties.Add(adv, pot);
                }
            }
        }
    }

}
