using Vintagestory.API.Common;
using Vintagestory.API.Config;


namespace sfanimationlibrary
{
    public class SfBBBlockToggle : BlockBehavior
    {
        public SfBBBlockToggle(Block block) : base(block)
        { }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;
            var be = world.BlockAccessor.GetBlockEntity<SfBEBlockRotate>(blockSel.Position);
 
            if (be != null)
            {
                if (be.OnInteract(blockSel, byPlayer))
                {
                    if (be.BlockOn)
                        return true;
                }
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
        }
    }
}