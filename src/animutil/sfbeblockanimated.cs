using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using System.Diagnostics;


namespace sfanimationlibrary
{
    //loosely based on the riftward
    public class SfBEBlockAnimated : BlockEntityDisplayCase
    {
        protected ICoreServerAPI sapi;
        protected ILoadedSound ambientSound;

        private string rotationSound = "";
        private float rotationSoundVolume = 0.5f;
        private string switchSound = "";
        private float switchSoundVolume = 0.5f;
        private bool switchEnabled = false;
        private float easeInSpeedModifier = 1.0f;
        private float easeOutSpeedModifier = 2.0f;
        private float animationSpeed = 1.0f;
        private string animatorName = "blockanimator";
        private string animationName = "spin";

        public bool BlockOn { get; set; }

        public bool IsSwitchEnabled()
        {
            return this.switchEnabled;
        }

        public override string InventoryClassName => "genericanimated";
        protected new InventoryGeneric inventory; 
        public override InventoryBase Inventory => this.inventory;

        public SfBEBlockAnimated()
        {
            this.inventory = new InventoryGeneric(1, null, null);
        }

        BlockEntityAnimationUtil animUtil
        {
            get { return GetBehavior<BEBehaviorAnimatable>().animUtil; }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            this.inventory.LateInitialize("genericanimated" + "-" + this.Pos.X + "/" + this.Pos.Y + "/" + this.Pos.Z, api);

            if (this.Block.Attributes != null)
            {
                if (this.Block.Attributes["rotationSound"].Exists)
                { this.rotationSound = this.Block.Attributes["rotationSound"].AsString(); }

                if (this.Block.Attributes["rotationSoundVolume"].Exists)
                { this.rotationSoundVolume = this.Block.Attributes["rotationSoundVolume"].AsFloat(); }

                if (this.Block.Attributes["switchSound"].Exists)
                { this.switchSound = this.Block.Attributes["switchSound"].AsString(); }

                if (this.Block.Attributes["switchSoundVolume"].Exists)
                { this.switchSoundVolume = this.Block.Attributes["switchSoundVolume"].AsFloat(); }

                if (this.Block.Attributes["switchEnabled"].Exists)
                { this.switchEnabled = this.Block.Attributes["switchEnabled"].AsBool(); }

                if (this.Block.Attributes["easeInSpeedModifier"].Exists)
                { this.easeInSpeedModifier = this.Block.Attributes["easeInSpeedModifier"].AsFloat(); }

                if (this.Block.Attributes["easeOutSpeedModifier"].Exists)
                { this.easeOutSpeedModifier = this.Block.Attributes["easeOutSpeedModifier"].AsFloat(); }

                if (this.Block.Attributes["animationSpeed"].Exists)
                { this.animationSpeed = this.Block.Attributes["animationSpeed"].AsFloat(); }

                if (this.Block.Attributes["animatorName"].Exists)
                { this.animatorName = this.Block.Attributes["animatorName"].AsString(); }

                if (this.Block.Attributes["animationName"].Exists)
                { this.animationName = this.Block.Attributes["animationName"].AsString(); }
            }

            if (this.animationSpeed <= 0.0f)
            { this.animationSpeed = 1f; }
            this.animationSpeed = this.animationSpeed * 0.0001f;


            sapi = api as ICoreServerAPI;
            if (sapi != null)
            {
                RegisterGameTickListener(OnServerTick, 5000);
            }

            if (sapi == null)
            {
                float rotY = Block.Shape.rotateY;
                animUtil?.InitializeAnimator(this.animatorName, null, null, new Vec3f(0, rotY, 0));
                return;
            }

            if (BlockOn || this.switchEnabled == false)
            {
                Activate();
            }

        }


        public void ToggleAmbientSound(bool blockon)
        {
            if (Api?.Side != EnumAppSide.Client) return;

            if (blockon)
            {
                if (ambientSound == null || !ambientSound.IsPlaying)
                {
                    ambientSound = ((IClientWorldAccessor)Api.World).LoadSound(new SoundParams()
                    {
                        Location = new AssetLocation(this.rotationSound),
                        ShouldLoop = true,
                        Position = Pos.ToVec3f().Add(0.5f, 0.5f, 0.5f),
                        DisposeOnFinish = false,
                        Volume = 0f,
                        Range = 6,
                        SoundType = EnumSoundType.Ambient
                    });

                    if (ambientSound != null)
                    {
                        ambientSound.Start();
                        ambientSound.FadeTo(this.rotationSoundVolume, 1f, (s) => { });
                        ambientSound.PlaybackPosition = ambientSound.SoundLengthSeconds * (float)Api.World.Rand.NextDouble();
                    }
                }
                else
                {
                    if (ambientSound.IsPlaying) ambientSound.FadeTo(this.rotationSoundVolume, 1f, (s) => { });
                }
            }
            else
            {
                ambientSound?.FadeOut(0.5f, (s) => { s.Dispose(); ambientSound = null; });
            }
        }


        private void OnServerTick(float dt)
        {
            if (BlockOn) Activate();
            else Deactivate();
        }


        public void Activate()
        {
            
            if (Api == null) return;
            BlockOn = true;

            var meta = new AnimationMetaData() 
             { 
                 Animation = this.animationName, 
                 Code = this.animationName, 
                 EaseInSpeed = this.easeInSpeedModifier, 
                 EaseOutSpeed = this.easeOutSpeedModifier,
                 //SupressDefaultAnimation = true,
                 AnimationSpeed = this.animationSpeed
            };

            
            meta.AnimationSpeed = meta.AnimationSpeed * 500000;
            // We could theoretically also override these easily enough
            //meta.EaseInSpeed = meta.EaseInSpeed * 100000;
            //meta.EaseOutSpeed = meta.EaseOutSpeed * 100000;

            animUtil?.StartAnimation(meta);
            MarkDirty(true);
            ToggleAmbientSound(true);
        }


        public void Deactivate()
        {
            animUtil?.StopAnimation(this.animationName);
            BlockOn = false;
            ToggleAmbientSound(false);
            MarkDirty(true);
        }


        public bool OnInteract(BlockSelection blockSel, IPlayer byPlayer)
        {
            if (this.switchEnabled)
            {
                Api.World.PlaySoundAt(new AssetLocation(this.switchSound), Pos.X, Pos.Y, Pos.Z, byPlayer, false, 16, this.switchSoundVolume); //1.19
                if (BlockOn)
                { Deactivate(); }
                else
                { Activate(); }
            }
            return true;
        }


        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            ambientSound?.Dispose();
        }


        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            ambientSound?.Dispose();
        }


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            BlockOn = tree.GetBool("on");
            
            if (BlockOn) Activate();
            else Deactivate();

        }


        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetBool("on", BlockOn);
        }

        /*
        //moved to the block class
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);

            var state = "off";
            if (BlockOn)
            { state = "on"; }

            dsc.AppendLine(Lang.GetMatching("sfanimationlibrary:toggle-state") + ": " + Lang.GetMatching("sfanimationlibrary:toggle-" + state));
            if (this.switchEnabled)
            {
                dsc.AppendLine(Lang.GetMatching("sfanimationlibrary:click-to-toggle"));
            }
        }
        */
    }
}