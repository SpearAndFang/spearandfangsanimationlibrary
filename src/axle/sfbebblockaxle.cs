using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;
using Vintagestory.GameContent;


namespace sfanimationlibrary
{
 
    public class SfBEBBlockMPAxle : BEBehaviorMPBase
    {
        BlockEntityGeneric sfbeaxle;
        float resistance;
        private Vec3f center = new Vec3f(0.5f, 0.5f, 0.5f);
        BlockFacing[] orients = new BlockFacing[2];
        string orientations;
        protected ILoadedSound ambientSound;

        private string rotationSound = "";
        private float rotationSoundVolume = 0.5f;

        public SfBEBBlockMPAxle(BlockEntity blockentity) : base(blockentity)
        {
            sfbeaxle = blockentity as BlockEntityGeneric;
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

            if (this.Block.Attributes["rotationSound"].Exists)
            { this.rotationSound = this.Block.Attributes["rotationSound"].AsString(); }

            if (this.Block.Attributes["rotationSoundVolume"].Exists)
            { this.rotationSoundVolume = this.Block.Attributes["rotationSoundVolume"].AsFloat(); }

            sfbeaxle.RegisterGameTickListener(OnEveryMs, 500);

            orientations = Block.Variant["rotation"];
            switch (orientations)
            {
                case "ns":
                    AxisSign = new int[] { 0, 0, -1 };
                    orients[0] = BlockFacing.NORTH;
                    orients[1] = BlockFacing.SOUTH;
                    break;

                case "we":
                    AxisSign = new int[] { -1, 0, 0 };
                    orients[0] = BlockFacing.WEST;
                    orients[1] = BlockFacing.EAST;
                    break;

                case "ud":
                    AxisSign = new int[] { 0, 1, 0 };
                    orients[0] = BlockFacing.DOWN;
                    orients[1] = BlockFacing.UP;
                    break;
            }
        }


        private void OnEveryMs(float dt)
        {
            resistance = GameMath.Clamp(resistance + dt / 20, 0, 3);
            UpdateBreakSounds();
        }

        public void UpdateBreakSounds()
        {
            if (Api.Side != EnumAppSide.Client) return;

            if (this.rotationSound != "")
            {

                if (resistance > 0 && network != null && network.Speed > 0.1)
                {
                    if (ambientSound == null || !ambientSound.IsPlaying)
                    {
                        ambientSound = ((IClientWorldAccessor)Api.World).LoadSound(new SoundParams()
                        {
                            Location = new AssetLocation(this.rotationSound),
                            ShouldLoop = true,
                            Position = Position.ToVec3f().Add(0.5f, 0.25f, 0.5f),
                            DisposeOnFinish = false,
                            Volume = this.rotationSoundVolume
                        });

                        ambientSound.Start();
                    }

                    ambientSound.SetPitch(GameMath.Clamp(network.Speed * 1.5f + 0.2f, 0.5f, 1));
                }
                else
                {
                    ambientSound?.FadeOut(1, (s) => { ambientSound.Stop(); });
                }
            }
        }


        public override float GetResistance()
        {
            return 0.0005f;
        }


        public static bool IsAttachedToBlock(IBlockAccessor blockaccessor, Block block, BlockPos Position)
        {
            string orientations = block.Variant["rotation"];
            BlockPos bposd = new BlockPos(Position.X, Position.Y - 1, Position.Z, 0);
            BlockPos bposu = new BlockPos(Position.X, Position.Y + 1, Position.Z, 0);
            if (orientations == "ns" || orientations == "we")
            {
                // Up or down
                if (
                    blockaccessor.GetBlock(bposd).SideSolid[BlockFacing.UP.Index] ||
                    blockaccessor.GetBlock(bposu).SideSolid[BlockFacing.DOWN.Index]
                ) return true;

                // Front or back
                BlockFacing frontFacing = orientations == "ns" ? BlockFacing.WEST : BlockFacing.NORTH;
                return
                    blockaccessor.GetBlock(Position.AddCopy(frontFacing)).SideSolid[frontFacing.Opposite.Index] ||
                    blockaccessor.GetBlock(Position.AddCopy(frontFacing.Opposite)).SideSolid[frontFacing.Index]
                ;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    BlockFacing face = BlockFacing.HORIZONTALS[i];
                    BlockPos bpost = new BlockPos(Position.X + face.Normali.X, Position.Y, Position.Z + face.Normali.Z, 0);
                    Block blockNeib = blockaccessor.GetBlock(bpost);
                    if (blockNeib.SideSolid[face.Opposite.Index])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected virtual bool AddStands => true;


    
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            base.GetBlockInfo(forPlayer, sb);
            if (Api.World.EntityDebugMode)
            {
                string orientations = Block.Variant["orientation"];
                sb.AppendLine(string.Format(Lang.Get("Orientation: {0}", orientations)));
            }
        }
    }
}