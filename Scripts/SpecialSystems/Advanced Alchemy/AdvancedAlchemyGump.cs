using System;
using Server;
using Server.Mobiles;
using Server.Network;
using System.AdvancedAlchemy;

namespace Server.Gumps
{
    public class AdvancedAlchemyGump : Gump
    {
        public AdvancedAlchemyGump(Mobile from)
            : base(100, 100)
        {
            var pm = from as PlayerMobile;
            if (pm == null)
            {
                from.CloseGump(typeof(AdvancedAlchemyGump));
                return;
            }

            this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

			AddPage(0);
			AddBackground(15, 51, 461, 477, 5100);
            AddAlphaRegion(25, 61, 351, 223);
            AddItem(-2, 54, 11731);//Alchemy table image
            AddImage(408, 58, 5584);//Scales image

            //Mortars
			AddImage(25, 304, 1417);//Mortar 1
            AddButton(59, 298, 252, 253, 0, GumpButtonType.Reply, 0);//Add ingredient to Mortar 1

            AddImage(115, 304, 1417); //Mortar 2
            AddButton(149, 298, 252, 253, 0, GumpButtonType.Reply, 0); //Add ingredient to Mortar 2
  
            AddImage(205, 304, 1417); //Mortar 3
            AddButton(239, 298, 252, 253, 0, GumpButtonType.Reply, 0); //Add ingredient to Mortar 3
         
            AddImage(295, 304, 1417); //Mortar 4
            AddButton(329, 298, 252, 253, 0, GumpButtonType.Reply, 0); //Add ingredient to Mortar 4
           
            AddImage(385, 304, 1417); //Mortar 5
            AddButton(419, 298, 252, 253, 0, GumpButtonType.Reply, 0); //Add ingredient to Mortar 5
                            
            AddImage(25, 392, 1417); //Mortar 6
            AddButton(59, 456, 250, 251, 0, GumpButtonType.Reply, 0); //Add ingredient to Mortar 6
            
            AddImage(115, 392, 1417); //Mortar 7
            AddButton(149, 456, 250, 251, 0, GumpButtonType.Reply, 0); //Add ingredient to Mortar 7
            
            AddImage(205, 392, 1417); //Mortar 8
            AddButton(239, 456, 250, 251, 0, GumpButtonType.Reply, 0); //Add ingredient to Mortar 8
            
            AddImage(295, 392, 1417); //Mortar 9
            AddButton(329, 456, 250, 251, 0, GumpButtonType.Reply, 0); //Add ingredient to Mortar 9
            
            AddImage(385, 392, 1417); //Mortar 10
            AddButton(419, 456, 250, 251, 0, GumpButtonType.Reply, 0); //Add ingredient to Mortar 10
 
            //Select Bottle
            AddLabel(384, 221, 0, @"Select Bottle");
            AddButton(424, 248, 4014, 4016, 0, GumpButtonType.Reply, 0);
            AddItem(394, 255, 3854);//White bottle image
            AddItem(374, 260, 3838);//Yellow bottle image
            AddItem(385, 245, 3836);//Red bottle image

            //Load Recipe
            AddLabel(31, 477, 0, @"Load Recipe");
			AddButton(29, 497, 4029, 4031, 0, GumpButtonType.Reply, 0);
            AddItem(59, 495, 7985);//Scroll image
			
            //Write Recipe
            AddLabel(137, 477, 0, @"Write Recipe");
            AddButton(136, 497, 4011, 4013, 0, GumpButtonType.Reply, 0);
            AddItem(163, 496, 3636);//Blank scroll image

            //Mix Potion
			AddLabel(389, 477, 0, @"Mix Potion");
			AddButton(427, 497, 4005, 4007, 0, GumpButtonType.Reply, 0);		    
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch(info.ButtonID)
            {
                				case 0:
				{
					break;
				}
            }
        }
    }
}