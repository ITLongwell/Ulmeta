using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Mobiles;
using System.Collections.Generic;
using Server.Items;
using Server.Targeting;
using System.AdvancedAlchemy;
using Server.HuePickers;
using Server.Prompts;

namespace Server.Gumps
{
    public class AlchemyItems : Gump
    {
        PlayerMobile poly;
        List<string> ItemList;
        Item m_choice;

        public AlchemyItems(Mobile from)
            : base(0, 0)
        {
            poly = from as PlayerMobile;
            int counter = 1;
            int step = 20;

            this.Closable = false;
            this.Disposable = false;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);

            AddBackground(5, 5, 395, 140 + step * AdvancedAlchemy.ItemConversion.Count, 9200);

            AddButton(329, 10, 5003, 5003, 0, GumpButtonType.Reply, 0);

            AddLabel(101, 12, 0, @"Advanced Alchemy Definitions");
            AddLabel(21, 61, 0, @"Click to target an existing item's properties:");

            AddButton(315, 60, 208, 209, 9999, GumpButtonType.Reply, 0);

            ItemList = new List<string>();

            if (AdvancedAlchemy.ItemConversion.Count > 0)
            {
                Dictionary<string, Dictionary<AlchemicProperty, PropertyLevel>>.KeyCollection keyColl = AdvancedAlchemy.ItemConversion.Keys;

                foreach (string it in keyColl)
                {
                    this.AddLabel(25, 108 + (step * counter), 0, it);
                    ItemList.Add(it);

                    this.AddLabel(208, 108 + (step * counter), 0, "View:");

                    AddButton(272, 107 + (step * counter), 247, 248, counter, GumpButtonType.Reply, 0);

                    counter++;
                }
            }
            else
                this.AddLabel(43, 85 + (step * counter), 0, "No Items have been added yet.");

        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            if (info.ButtonID == 0)
            {
                // Close gump
            }
            else if (info.ButtonID == 9999)
            {
                // add Item
                from.Target = new InternalTarget();
            }
            else
            {
                from.SendGump(new IngredientSettings(from, ItemList[info.ButtonID - 1]));
            }
        }

        private class InternalTarget : Target
        {
            public InternalTarget()
                : base(12, true, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Item)
                {
                    if (o is BaseIngredient)
                    {
                        from.SendMessage("Target is a BaseIngredient, by definition is already an ingredient.");
                        from.SendGump(new AlchemyItems(from));
                    }
                    else if ((o is ElvenAlchemyTable) || (o is BaseContainer) || (o is BaseClothing) || (o is BaseArmor) || (o is BaseWeapon) || (o is BaseTool) || (o is BaseBook) || (o is BaseTrap) || (o is BaseTalisman) || (o is BaseSuit) || (o is BaseAddon) || (o is AddonComponent))
                    {
                        from.SendMessage("This item type is not allowed to be an alchemy ingredient.");
                        from.SendGump(new AlchemyItems(from));
                    }
                    else
                        from.SendGump(new IngredientSettings(from, o as Item));
                }
                else
                {
                    from.SendMessage("That target is not an item!");
                    from.SendGump(new AlchemyItems(from));
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {

            }
        }
    }

    public class IngredientSettings : Gump
    {
        Mobile from;
        string target;
        bool IsBaseIngredient;
        List<AlchemicProperty> list = new List<AlchemicProperty>();

        public IngredientSettings(Mobile m, string name)
            : base(0, 0)
        {
            from = m;
            target = name;

            if (name.Contains("Server.Item"))
                IsBaseIngredient = false;
            else
                IsBaseIngredient = true;

            FillIn();
        }

        public IngredientSettings(Mobile m, Item o)
            : base(0, 0)
        {
            from = m;

            if (o is BaseIngredient)
            {
                target = o.Name;
                IsBaseIngredient = true;
            }
            else
            {
                target = o.GetType().FullName;
                IsBaseIngredient = false;
            }

            FillIn();
        }

        public void FillIn()
        {
            Dictionary<AlchemicProperty, PropertyLevel> props = new Dictionary<AlchemicProperty, PropertyLevel>();
            list = new List<AlchemicProperty>();

            int step = 20;
            int counter = 0;

            this.Closable = false;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);

            if (IsBaseIngredient)
            {
                if (AdvancedAlchemy.BaseDefinitions.ContainsKey(target) && AdvancedAlchemy.BaseDefinitions[target].Props != null)
                {
                    AddItem(34, 50, AdvancedAlchemy.BaseDefinitions[target].ItemID);
                    AddBackground(6, 22, 394, 163 + (step * AdvancedAlchemy.BaseDefinitions[target].Props.Count), 9200);
                }
                else
                {
                    AdvancedAlchemy.BaseDefinitions[target] = new IngredientDefinition(0, 0, new Dictionary<AlchemicProperty, PropertyLevel>());
                    AddBackground(6, 22, 394, 163, 9200);
                }

                AddItem(21, 72, AdvancedAlchemy.BaseDefinitions[target].ItemID, AdvancedAlchemy.BaseDefinitions[target].Hue);

                props = AdvancedAlchemy.BaseDefinitions[target].Props;

                foreach (AlchemicProperty alch in props.Keys)
                {
                    this.AddLabel(106, 106 + (step * counter), 0, alch.ToString() + ", " + props[alch].ToString());
                    AddButton(328, 105 + (step * counter), 5531, 5532, (int)Buttons.Delete + 3 * counter, GumpButtonType.Reply, 0);
                    list.Add(alch);
                    counter++;
                }

            }
            else
            {
                if (AdvancedAlchemy.AddItemTarget(target, new Dictionary<AlchemicProperty,PropertyLevel>()) )
                    AddBackground(6, 22, 394, 163, 9200);
                else
                    AddBackground(6, 22, 394, 163 + (step * AdvancedAlchemy.ItemConversion[target].Count), 9200);


                Type t = Type.GetType(target);
                object it = Activator.CreateInstance(t);
                AddItem(21, 72, ((Item)it).ItemID, ((Item)it).Hue);
                ((Item)it).Delete();

                props = AdvancedAlchemy.ItemConversion[target];

                foreach (AlchemicProperty alch in props.Keys)
                {
                    this.AddLabel(106, 106 + (step * counter), 0, alch.ToString() + ", " + props[alch].ToString());
                    AddButton(328, 105 + (step * counter), 5531, 5532, (int)Buttons.Delete + 3 * counter, GumpButtonType.Reply, 0);
                    list.Add(alch);
                    counter++;
                }
            }

            AddLabel(26, 34, 0, @"Alchemic Properties of " + target);
            AddButton(361, 30, 1151, 1152, (int)Buttons.Close, GumpButtonType.Reply, 0);
            AddButton(104, 135 + (step * counter), 5534, 5535, (int)Buttons.Add, GumpButtonType.Reply, 0);
        }

        public enum Buttons
        {
            Delete = 1,
            Add = 2,
            Close = 3,
        }


        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            int response = info.ButtonID;
            int multiple = 0;
            while (response > 3)
            {
                multiple++;
                response = response - 3;
            }

            switch (response)
            {
                case (int)Buttons.Delete:
                    {
                        if (IsBaseIngredient)
                            AdvancedAlchemy.BaseDefinitions[target].Props.Remove(list[multiple]);
                        else
                            AdvancedAlchemy.ItemConversion[target].Remove(list[multiple]);

                        from.SendGump(new IngredientSettings(from, target));
                        break;
                    }
                case (int)Buttons.Add:
                    {
                        from.SendGump(new AddProperty(target, IsBaseIngredient, -1, -1));
                        break;
                    }
                case (int)Buttons.Close:
                    {
                        if (IsBaseIngredient)
                            from.SendGump(new AlchemyCustoms(from));
                        else
                            from.SendGump(new AlchemyItems(from));

                        break;
                    }
                default:
                    {
                        if (IsBaseIngredient)
                            from.SendGump(new AlchemyCustoms(from));
                        else
                            from.SendGump(new AlchemyItems(from));

                        break;
                    }

            }
        }
    }

    public class AddProperty : Gump
    {
        string target;
        bool IsBaseIngredient;
        AlchemicProperty alchchoice;
        PropertyLevel levelchoice;
        int alchposition, levelposition;

        public AddProperty(string o, bool Custom, int alch, int level)
            : base(0, 0)
        {
            target = o;
            IsBaseIngredient = Custom;

            if (alch == -1)
            {
                alchchoice = AdvancedAlchemy.AlchemicPropertyList[0];
                levelchoice = AdvancedAlchemy.PropertyLevelList[0];
                alchposition = 0;
                levelposition = 0;
            }
            else
            {
                alchchoice = AdvancedAlchemy.AlchemicPropertyList[alch];
                levelchoice = AdvancedAlchemy.PropertyLevelList[level];
                alchposition = alch;
                levelposition = level;
            }

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);
            AddBackground(5, 21, 196, 208, 9200);
            AddLabel(42, 38, 0, @"Alchemic Property");
            AddLabel(50, 104, 0, @"Property Level");

            AddLabel(50, 64, 0, alchchoice.ToString());
            AddLabel(50, 128, 0, levelchoice.ToString());

            AddButton(66, 184, 247, 248, (int)Buttons.Okay, GumpButtonType.Reply, 0);
            AddButton(158, 65, 4005, 4006, (int)Buttons.NextProp, GumpButtonType.Reply, 0);
            AddButton(10, 65, 4014, 4015, (int)Buttons.PrevProp, GumpButtonType.Reply, 0);
            AddButton(158, 129, 4005, 4006, (int)Buttons.NextLevel, GumpButtonType.Reply, 0);
            AddButton(13, 127, 4014, 4015, (int)Buttons.PrevLevel, GumpButtonType.Reply, 0);
        }

        public enum Buttons
        {
            Okay,
            NextProp,
            PrevProp,
            NextLevel,
            PrevLevel
        }




        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case (int)Buttons.Okay:
                    {
                        if (IsBaseIngredient)
                        {
                            if (!AdvancedAlchemy.BaseDefinitions[target].Props.ContainsKey((AlchemicProperty)alchchoice))
                                AdvancedAlchemy.BaseDefinitions[target].Props.Add((AlchemicProperty)alchchoice, (PropertyLevel)levelchoice);
                            else
                                from.SendMessage("That BaseIngredient already has the property " + alchchoice.ToString());
                        }
                        else
                        {
                            if (!AdvancedAlchemy.ItemConversion[target].ContainsKey((AlchemicProperty)alchchoice))
                                AdvancedAlchemy.ItemConversion[target].Add((AlchemicProperty)alchchoice, (PropertyLevel)levelchoice);
                            else
                                from.SendMessage("That ingredient already has the property " + alchchoice.ToString());
                        }

                        from.SendGump(new IngredientSettings(from, target));
                        break;
                    }
                case (int)Buttons.NextProp:
                    {
                        if (alchchoice == AdvancedAlchemy.AlchemicPropertyList[AdvancedAlchemy.AlchemicPropertyList.Length - 1])
                            alchposition = 0;
                        else
                            alchposition++;

                        from.SendGump(new AddProperty(target, IsBaseIngredient, alchposition, levelposition));
                        break;
                    }
                case (int)Buttons.PrevProp:
                    {
                        if (alchchoice == AdvancedAlchemy.AlchemicPropertyList[0])
                            alchposition = AdvancedAlchemy.AlchemicPropertyList.Length - 1;
                        else
                            alchposition--;

                        from.SendGump(new AddProperty(target, IsBaseIngredient, alchposition, levelposition));
                        break;
                    }
                case (int)Buttons.NextLevel:
                    {
                        if (levelchoice == AdvancedAlchemy.PropertyLevelList[AdvancedAlchemy.PropertyLevelList.Length - 1])
                            levelposition = 0;
                        else
                            levelposition++;

                        from.SendGump(new AddProperty(target, IsBaseIngredient, alchposition, levelposition));
                        break;
                    }
                case (int)Buttons.PrevLevel:
                    {
                        if (levelchoice == AdvancedAlchemy.PropertyLevelList[0])
                            levelposition = AdvancedAlchemy.PropertyLevelList.Length - 1;
                        else
                            levelposition--;

                        from.SendGump(new AddProperty(target, IsBaseIngredient, alchposition, levelposition));
                        break;
                    }

            }
        }
    }

    public class AlchemyCustoms : Gump
    {
        PlayerMobile poly;
        List<string> ItemList;
        Item m_choice;

        public AlchemyCustoms(Mobile from)
            : base(0, 0)
        {
            poly = from as PlayerMobile;
            int counter = 1;
            int step = 20;

            this.Closable = false;
            this.Disposable = false;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);

            AddBackground(5, 5, 395, 140 + step * AdvancedAlchemy.BaseDefinitions.Count, 9200);

            AddButton(329, 10, 5003, 5003, 0, GumpButtonType.Reply, 0);

            AddLabel(71, 12, 0, @"Advanced Alchemy Custom Definitions");
            AddLabel(21, 61, 0, @"Click to define a new custom item:");

            AddButton(315, 60, 208, 209, 9999, GumpButtonType.Reply, 0);

            ItemList = new List<string>();

            if (AdvancedAlchemy.BaseDefinitions.Count > 0)
            {
                Dictionary<string, IngredientDefinition>.KeyCollection keyColl = AdvancedAlchemy.BaseDefinitions.Keys;

                foreach (string it in keyColl)
                {
                    this.AddLabel(25, 108 + (step * counter), 0, it);
                    ItemList.Add(it);

                    this.AddLabel(208, 108 + (step * counter), 0, "View:");

                    AddButton(272, 107 + (step * counter), 247, 248, counter, GumpButtonType.Reply, 0);

                    counter++;
                }
            }
            else
                this.AddLabel(43, 85 + (step * counter), 0, "No custom items have been added yet.");

        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            if (info.ButtonID == 0)
            {
                // Close gump
            }
            else if (info.ButtonID == 9999)
            {
                from.SendGump(new DefineBaseIngredient(null, 0, 0, null));
                // add Item
            }
            else
            {
                from.SendGump(new IngredientSettings(from, ItemList[info.ButtonID - 1]));
            }
        }

    }

    public class DefineBaseIngredient : Gump
    {
        int m_id, m_hue;
        BaseIngredient ItemToAdd;
        string m_name;
        bool m_confirmed;

        public DefineBaseIngredient(bool confirmed, string name, int id, int hue, BaseIngredient item)
            : base(0, 0)
        {
            m_id = id;
            m_hue = hue;
            ItemToAdd = item;
            m_name = name;
            m_confirmed = confirmed;
            this.FillIn();
        }

        public DefineBaseIngredient(string name, int id, int hue, BaseIngredient item)
            : base(0, 0)
        {
            m_id = id;
            m_hue = hue;
            ItemToAdd = item;
            m_name = name;
            m_confirmed = false;
            this.FillIn();
        }

        public void FillIn()
        {
            if (ItemToAdd == null)
                ItemToAdd = new BaseIngredient();

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);
            AddBackground(60, 47, 279, 157, 9200);
            AddLabel(96, 60, 0, @"Define New BaseIngredient");

            if (m_name == null || m_name == "")
            {
                AddLabel(162, 89, 0, @"Name:");
                AddTextEntry(210, 89, 118, 20, 1071, (int)Buttons.Name, @"");
                //AddLabel(162, 89, 0, @"Select Name:");
                //AddButton(256, 89, 247, 248, (int)Buttons.Name, GumpButtonType.Reply, 0);
            }
            else
            {
                AddLabel(162, 89, 0, "Name: " + m_name);
            }

            if (m_id == 0)
            {
                AddLabel(160, 118, 0, @"ItemID:");
                AddTextEntry(210, 118, 118, 20, 1071, (int)Buttons.ItemID, @"");
                //AddButton(256, 118, 247, 248, (int)Buttons.ID, GumpButtonType.Reply, 0);
            }
            else
            {
                AddLabel(160, 118, 0, @"ItemID: " + m_id.ToString());
                AddItem(84, 113, m_id, m_hue);
                AddLabel(170, 147, 0, @"Select Hue:");
                AddButton(249, 145, 4025, 248, (int)Buttons.Hue, GumpButtonType.Reply, 0);
            }

            AddButton(305, 53, 5052, 5053, (int)Buttons.Cancel, GumpButtonType.Reply, 0);

            if (m_confirmed)
                AddButton(216, 175, 5202, 5203, (int)Buttons.Save, GumpButtonType.Reply, 0);
            else
                AddButton(125, 175, 5204, 5205, (int)Buttons.Apply, GumpButtonType.Reply, 0);
        }

        public enum Buttons
        {
            ItemID = 1,
            Name,
            Apply,
            Hue,
            Save,
            Cancel
        }


        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case (int)Buttons.Apply:
                    {
                        try
                        {
                            string nameString = info.GetTextEntry((int)Buttons.Name).Text;

                            string itemIdString = info.GetTextEntry((int)Buttons.ItemID).Text;

                            if (String.IsNullOrEmpty(nameString))
                            {
                                sender.Mobile.SendMessage("You must enter a name for the ingredient.");
                                sender.Mobile.SendGump(new DefineBaseIngredient(this.m_name, this.m_id, this.m_hue, this.ItemToAdd)); return;
                            }
                            else if (String.IsNullOrEmpty(itemIdString))
                            {
                                sender.Mobile.SendMessage("You must enter an itemID for the ingredient.");
                                sender.Mobile.SendGump(new DefineBaseIngredient(this.m_name, this.m_id, this.m_hue, this.ItemToAdd)); return;
                            }
                            else
                            {
                                sender.Mobile.SendGump(new DefineBaseIngredient(true, nameString, Convert.ToInt32(itemIdString), this.m_hue, this.ItemToAdd)); return;
                            }

                        }

                        catch
                        {
                            sender.Mobile.SendMessage("Invalid text in name or itemID entry.");
                            sender.Mobile.SendGump(new DefineBaseIngredient(this.m_name, this.m_id, this.m_hue, this.ItemToAdd));
                            return;
                        }
                    }
                case (int)Buttons.Hue:
                    {
                        if (ItemToAdd.CustomHuePicker == null)
                            from.SendHuePicker(new InternalPicker(this, from));
                        else
                            from.SendGump(new CustomHuePickerGump(from, ItemToAdd.CustomHuePicker, new CustomHuePickerCallback(SetBaseIngredientHue), this));
                        break;
                    }
                case (int)Buttons.Save:
                    {
                        if (!AdvancedAlchemy.AddBaseDefinition(m_name, m_id, m_hue, new Dictionary<AlchemicProperty, PropertyLevel>()))
                            from.SendMessage("That name has already been defined as a BaseIngredient.");

                        from.SendGump(new AlchemyCustoms(from));
                        ItemToAdd.Delete();
                        break;
                    }
                case (int)Buttons.Cancel:
                    {
                        from.SendGump(new AlchemyCustoms(from));
                        ItemToAdd.Delete();
                        break;
                    }
                default:
                    {
                        from.SendGump(new AlchemyCustoms(from));
                        ItemToAdd.Delete();
                        break;
                    }

            }
        }

        private static void SetBaseIngredientHue(Mobile from, object state, int hue)
        {
            ((DefineBaseIngredient)state).m_hue = hue;
            from.SendGump(new DefineBaseIngredient(true, ((DefineBaseIngredient)state).m_name, ((DefineBaseIngredient)state).m_id, ((DefineBaseIngredient)state).m_hue, ((DefineBaseIngredient)state).ItemToAdd));
        }

        private class InternalPicker : HuePicker
        {
            private DefineBaseIngredient m_ingredient;
            private Mobile m_from;
            public InternalPicker(DefineBaseIngredient ing, Mobile from)
                : base(ing.m_id)
            {
                this.m_from = from;
                this.m_ingredient = ing;
            }

            public override void OnResponse(int hue)
            {
                m_from.SendGump(new DefineBaseIngredient(true, m_ingredient.m_name, m_ingredient.m_id, hue, m_ingredient.ItemToAdd));
            }
        }
    }

}
