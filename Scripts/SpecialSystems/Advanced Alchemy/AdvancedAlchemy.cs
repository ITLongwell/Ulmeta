using Server;
using Server.Commands;
using Server.Gumps;
using Server.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Server.Items;

namespace System.AdvancedAlchemy
{
    public enum AlchemicProperty
    {
        Poisonous, Paralysis, Stimulant, Depressant, Anesthetic,
        Flammable, Inert, Irritant, Analgesic,
        Corrosive, Adhesive, Solvent, Intoxicant
    }

    public enum PropertyLevel
    {
        Low = 1, Medium = 2, High = 4, Potent = 8
    }

    public class AdvancedAlchemy
    {
        private static readonly string SavePath = "Saves\\Alchemy";
        private static readonly string SaveFile = Path.Combine(SavePath, "AdvancedAlchemy.bin");

        private static AlchemicProperty[] m_AlchemicPropertyList;
        public static AlchemicProperty[] AlchemicPropertyList
        {
            get
            {
                return m_AlchemicPropertyList;
            }
        }

        private static PropertyLevel[] m_PropertyLevelList;

        public static PropertyLevel[] PropertyLevelList
        {
            get
            {
                return m_PropertyLevelList;
            }
        }

        #region Generic Ingredients
        private static Dictionary<string, IngredientDefinition> m_BaseDefinitions;

        public static Dictionary<string, IngredientDefinition> BaseDefinitions
        {
            get
            {
                if (m_BaseDefinitions == null)
                    m_BaseDefinitions = new Dictionary<string, IngredientDefinition>();

                return m_BaseDefinitions;
            }

            set
            {
                m_BaseDefinitions = value;
            }
        }

        public static bool AddBaseDefinition(string name, int id, int hue, Dictionary<AlchemicProperty, PropertyLevel> props)
        {
            if (!BaseDefinitions.ContainsKey(name))
            {
                IngredientDefinition def = new IngredientDefinition(id, hue, props);
                BaseDefinitions.Add(name, def);
                return true;
            }
            return false;
        }

        //public static bool DeleteBaseDefinition( 

        #endregion

        #region Existing Item Ingredients
        private static Dictionary<string, Dictionary<AlchemicProperty, PropertyLevel>> m_ItemConversion;

        public static Dictionary<string, Dictionary<AlchemicProperty, PropertyLevel>> ItemConversion
        {
            get
            {
                if (m_ItemConversion == null)
                    m_ItemConversion = new Dictionary<string, Dictionary<AlchemicProperty, PropertyLevel>>();

                return m_ItemConversion;
            }

            set
            {
                m_ItemConversion = value;
            }

        }

        public static bool AddItemTarget(Item target, Dictionary<AlchemicProperty, PropertyLevel> props)
        {
            string name = target.GetType().FullName;
            return AddItemTarget(name, props);
        }

        public static bool AddItemTarget(string name, Dictionary<AlchemicProperty, PropertyLevel> props)
        {
            if (m_ItemConversion.ContainsKey(name))
                return false;
            else
            {
                m_ItemConversion.Add(name, props);
                return true;
            }
        }

        public static bool RemoveItemTarget(Item target)
        {
            string name = target.GetType().FullName;

            if (m_ItemConversion.ContainsKey(name))
            {
                m_ItemConversion.Remove(name);
                return true;
            }

            return false;
        }
        #endregion

        public static void Configure()
        {
            EventSink.WorldLoad += new WorldLoadEventHandler(Load);
        }

        public static void Initialize()
        {
            //m_BaseDefinitions = new Dictionary<string, IngredientDefinition>();
            //m_ItemConversion = new Dictionary<string, Dictionary<AlchemicProperty, PropertyLevel>>();

            var levelprops = Enum.GetValues(typeof(PropertyLevel));
            m_PropertyLevelList = new PropertyLevel[levelprops.Length];
            int counter = 0;
            foreach (PropertyLevel a in levelprops)
            {
                m_PropertyLevelList[counter] = a;
                counter++;
            }

            var alchprops = Enum.GetValues(typeof(AlchemicProperty));
            m_AlchemicPropertyList = new AlchemicProperty[alchprops.Length];
            counter = 0;
            foreach (AlchemicProperty a in alchprops)
            {
                m_AlchemicPropertyList[counter] = a;
                counter++;
            }

            EventSink.WorldSave += new WorldSaveEventHandler(Save);
            CommandSystem.Register("AlchemyCustoms", AccessLevel.GameMaster, new CommandEventHandler(BaseIngredient_OnCommand));
            CommandSystem.Register("AlchemyItems", AccessLevel.GameMaster, new CommandEventHandler(Target_OnCommand));
        }

        [Usage("AlchemyItems")]
        [Description("View existing or target new non-custom ingredients")]
        private static void Target_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new AlchemyItems(e.Mobile));
        }

        [Usage("AlchemyCustoms")]
        [Description("Creates new customized items to add to ingredients by user input.")]
        private static void BaseIngredient_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new AlchemyCustoms(e.Mobile));
        }

        static void Load()
        {
            if (!File.Exists(SaveFile))
                return;

            try
            {
                using (FileStream stream = new FileStream(SaveFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFileReader reader = new BinaryFileReader(new BinaryReader(stream));

                    Deserialize(reader);

                    reader.Close();
                }
            }

            catch { }
        }

        static void Save(WorldSaveEventArgs args)
        {
            try
            {
                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);

                BinaryFileWriter writer = new BinaryFileWriter(SaveFile, true);

                Serialize(writer);

                writer.Close();
            }

            catch { }

        }

        public static void Serialize(BinaryFileWriter writer)
        {
            writer.Write((int)0); //Version

            writer.Write(m_BaseDefinitions.Count);

            foreach (KeyValuePair<string, IngredientDefinition> kvp in m_BaseDefinitions)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value.ItemID);
                writer.Write(kvp.Value.Hue);
                writer.Write(kvp.Value.Props.Count);

                foreach (KeyValuePair<AlchemicProperty, PropertyLevel> kv in kvp.Value.Props)
                {
                    writer.Write((int)kv.Key);
                    writer.Write((int)kv.Value);
                }
            }

            writer.Write(ItemConversion.Count);

            foreach (KeyValuePair<string, Dictionary<AlchemicProperty, PropertyLevel>> kvp in m_ItemConversion)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value.Count);

                foreach (KeyValuePair<AlchemicProperty, PropertyLevel> kv in kvp.Value)
                {
                    writer.Write((int)kv.Key);
                    writer.Write((int)kv.Value);
                }
            }

        }

        public static void Deserialize(BinaryFileReader reader)
        {
            int version = reader.ReadInt();
            m_ItemConversion = new Dictionary<string, Dictionary<AlchemicProperty, PropertyLevel>>();
            m_BaseDefinitions = new Dictionary<string, IngredientDefinition>();

            if (version >= 0)
            {
                int count = reader.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    string name = reader.ReadString();
                    int ItemID = reader.ReadInt();
                    int Hue = reader.ReadInt();
                    int count2 = reader.ReadInt();

                    Dictionary<AlchemicProperty, PropertyLevel> dict = new Dictionary<AlchemicProperty, PropertyLevel>();
                    for (int j = 0; j < count2; j++)
                    {
                        AlchemicProperty alch = (AlchemicProperty)reader.ReadInt();
                        PropertyLevel level = (PropertyLevel)reader.ReadInt();
                        dict.Add(alch, level);
                    }
                    IngredientDefinition filler = new IngredientDefinition(ItemID, Hue, dict);
                    m_BaseDefinitions.Add(name, filler);
                }

                count = reader.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    string name = reader.ReadString();
                    int count2 = reader.ReadInt();

                    Dictionary<AlchemicProperty, PropertyLevel> dict = new Dictionary<AlchemicProperty, PropertyLevel>();
                    for (int j = 0; j < count2; j++)
                    {
                        AlchemicProperty alch = (AlchemicProperty)reader.ReadInt();
                        PropertyLevel level = (PropertyLevel)reader.ReadInt();
                        dict.Add(alch, level);
                    }

                    m_ItemConversion.Add(name, dict);
                }

            }
        }

    }

    public class IngredientDefinition
    {
        private int m_ItemIDdef;
        private int m_huedef;

        private Dictionary<AlchemicProperty, PropertyLevel> m_propdef;

        public int ItemID { get { return m_ItemIDdef; } }
        public int Hue { get { return m_huedef; } }
        public Dictionary<AlchemicProperty, PropertyLevel> Props { get { return m_propdef; } }

        public IngredientDefinition(int ItemIDdef, int huedef, Dictionary<AlchemicProperty, PropertyLevel> propdef)
        {
            m_ItemIDdef = ItemIDdef;
            m_huedef = huedef;
            m_propdef = propdef;
        }
    }

    public class BaseIngredient : Item
    {
        Dictionary<AlchemicProperty, PropertyLevel> properties;

        public BaseIngredient() : base()
        {
        }

        [Constructable]
        public BaseIngredient(String name) : base()
        {
            if (AdvancedAlchemy.BaseDefinitions.ContainsKey(name))
            {
                IngredientDefinition def = AdvancedAlchemy.BaseDefinitions[name];
                this.Name = name;
                this.ItemID = def.ItemID;
                this.Hue = def.Hue;
                this.properties = def.Props;
                this.Stackable = true;
            }
            else
                this.Delete();
        }

        public virtual CustomHuePicker CustomHuePicker
        {
            get
            {
                return null;
            }
        }


        public BaseIngredient(Serial serial) : base(serial) { }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version > 0)
            {
                int count = reader.ReadInt();

                properties = new Dictionary<AlchemicProperty, PropertyLevel>();

                for (int i = 0; i < count; i++)
                {
                    AlchemicProperty al = (AlchemicProperty)reader.ReadInt();
                    PropertyLevel pr = (PropertyLevel)reader.ReadInt();
                    properties.Add(al, pr);
                }

            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);


            writer.Write(1); //Version

            if (properties == null)
                properties = new Dictionary<AlchemicProperty, PropertyLevel>();

            writer.Write(properties.Count());

            foreach (KeyValuePair<AlchemicProperty, PropertyLevel> kvp in properties)
            {
                writer.Write((int)kvp.Key);
                writer.Write((int)kvp.Value);
            }

        }
    }

}
