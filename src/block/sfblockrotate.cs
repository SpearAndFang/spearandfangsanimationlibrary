using System.Text;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Util;
using System.Linq; 


namespace sfanimationlibrary
{
    public class SfBlockRotate : BlockDisplayCase
    {

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            var be = world.BlockAccessor.GetBlockEntity(selection.Position) as SfBEBlockRotate;

            if (be != null)
            {
                if (!be.IsSwitchEnabled())
                {
                    return new WorldInteraction[] { };
                }
                else
                {
                    return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "sfanimationlibrary:click-to-toggle",
                        MouseButton = EnumMouseButton.Right
                        //HotKeyCode = "shift"
                    }
                    };
                }
            }
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            var be = GetBlockEntity<SfBEBlockRotate>(blockSel);
            if (be != null && be.OnInteract(blockSel, byPlayer))
            {
                return true;
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            var dsc = new StringBuilder();
            
            var testBlock = world.BlockAccessor.GetBlock(pos, BlockLayersAccess.Default);
            bool switchEnabled = false;
            if (testBlock.Attributes != null)
            {
                if (testBlock.Attributes["switchEnabled"].Exists)
                { switchEnabled = testBlock.Attributes["switchEnabled"].AsBool(); }
            }

            if (switchEnabled)
            {
                bool blockOn = false;
                var state = "off";
                if (world.BlockAccessor.GetBlockEntity(pos) is SfBEBlockRotate be)
                { blockOn = be.BlockOn; }
                if (blockOn)
                { state = "on"; }
                dsc.AppendLine(Lang.GetMatching("sfanimationlibrary:toggle-state") + ": " + Lang.GetMatching("sfanimationlibrary:toggle-" + state));
                dsc.AppendLine();
            }
            var ingame = base.GetPlacedBlockInfo(world, pos, forPlayer);
            if (ingame != null)
            {
                if (ingame.Contains("\r\n"))
                {
                    var desc = ingame.Split("\r\n");
                    var lastElement = desc.Last();
                    dsc.AppendLine(lastElement);
                }
            }
            return dsc.ToString();
        }
    }
}