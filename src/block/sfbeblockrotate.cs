using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using System.Diagnostics;
using Vintagestory.API.Util;


namespace sfanimationlibrary
{
 
    public class SfBEBlockRotate : BlockEntityDisplayCase
    {
        protected ICoreServerAPI sapi;
 
        protected ILoadedSound ambientSound;
        private string rotationSound = "";
        private float rotationSoundVolume = 0.5f;
        private string switchSound = "";
        private float switchSoundVolume = 0.5f;
        private string rotationAxis = "x"; //x, y, z
        private bool switchEnabled = false;
        private float updateFrequency = 100.0f;
        private float rotationMultiplier = 1.0f;

        public bool BlockOn { get; set; }
        
        public bool IsSwitchEnabled()
        {
            return this.switchEnabled;
        }

        public float RotAngle = 0f;

        public override string InventoryClassName => "genericblockrotation";
        protected new InventoryGeneric inventory; 
        public override InventoryBase Inventory => this.inventory;

        public SfBEBlockRotate()
        {
            this.inventory = new InventoryGeneric(1, null, null);
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            this.inventory.LateInitialize("genericblockrotation" + "-" + this.Pos.X + "/" + this.Pos.Y + "/" + this.Pos.Z, api);

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

                if (this.Block.Attributes["rotationAxis"].Exists)
                { this.rotationAxis = (this.Block.Attributes["rotationAxis"].AsString()).ToLower(); }

                if (this.Block.Attributes["switchEnabled"].Exists)
                { this.switchEnabled = this.Block.Attributes["switchEnabled"].AsBool(); }

                if (this.Block.Attributes["updateFrequency"].Exists)
                { this.updateFrequency = this.Block.Attributes["updateFrequency"].AsFloat(); }

                if (this.Block.Attributes["rotationMultiplier"].Exists)
                { this.rotationMultiplier = this.Block.Attributes["rotationMultiplier"].AsFloat(); }

                if (this.updateFrequency <= 0f)
                { this.updateFrequency = 100.0f; }

                if (this.rotationMultiplier <= 0f)
                { this.rotationMultiplier = 0.1f; }
                else { this.rotationMultiplier = this.rotationMultiplier * 0.1f; }
            }
         
            RegisterGameTickListener(OnBlockTick, (int)(this.updateFrequency));

            if (api.Side == EnumAppSide.Client)
            {
                if (this.rotationSound != "")
                {
                    ambientSound = ((IClientWorldAccessor)api.World).LoadSound(new SoundParams()
                    {
                        Location = new AssetLocation(this.rotationSound),
                        ShouldLoop = true,
                        Position = Pos.ToVec3f().Add(0.5f, 0.25f, 0.5f),
                        DisposeOnFinish = false,
                        Volume = this.rotationSoundVolume,
                        Range = 8
                    });
                }
            }
        }


        public void ToggleAmbientSound(bool blockon)
        {
            if (ambientSound == null)
            { return; }

            if (blockon)
            {
                if (!ambientSound.IsPlaying)
                {
                    ambientSound.Start();
                    return;
                }
            }
            else
            {
                if (ambientSound.IsPlaying)
                { ambientSound.Stop(); }
            }
        }


        private void OnBlockTick(float dt)
        {
            if (Api.Side == EnumAppSide.Client)
            {
                ToggleAmbientSound(BlockOn);
            }

            if (Api.Side == EnumAppSide.Server)
            {
                if (this.switchEnabled == false)
                {
                    BlockOn = true;
                }
                if (BlockOn)
                {
                    RotAngle = RotAngle + (1 * this.rotationMultiplier);
                    if (RotAngle > 360)
                    { RotAngle = 0; }
                    
                }
                else
                { BlockOn = false; }
                MarkDirty(true);
            }
        }


        public bool OnInteract(BlockSelection blockSel, IPlayer byPlayer)
        {
            if (this.switchEnabled)
            {
                //Api.World.PlaySoundAt(new AssetLocation(this.switchSound), Pos, 0, byPlayer, false, 16, this.switchSoundVolume); //1.20
                Api.World.PlaySoundAt(new AssetLocation(this.switchSound), Pos.X, Pos.Y, Pos.Z, byPlayer, false, 16, this.switchSoundVolume); //1.19
                if (!BlockOn)
                { BlockOn = true; }
                else
                { BlockOn = false; }
                MarkDirty(true);
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
            RotAngle = tree.GetFloat("rotangle");
        }


        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetBool("on", BlockOn);
            tree.SetFloat("rotangle", RotAngle);
        }

       
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            var rotY = this.Block.Shape.rotateY;
            MeshData mesh;
            (Api as ICoreClientAPI).Tesselator.TesselateBlock(this.Block, out mesh); //not the default mesh

            float x = 0f; float y = 0f; float z = 0f;

  
            if (this.rotationAxis == "x")
            {
                 y = RotAngle; 
                mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, y, 0); //90 * GameMath.DEG2RAD);
                if (rotY == 0 || rotY == 180)
                {
                    mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, this.Block.Shape.rotateY * GameMath.DEG2RAD, 0); //orient based on direction last
                }

            }
            else if (this.rotationAxis == "y")
            {
                if (rotY == 0 || rotY == 180)
                {
                    mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, this.Block.Shape.rotateY * GameMath.DEG2RAD, 0); //orient based on direction last
                }
                if (rotY == 90)
                { z = RotAngle; }
                else if (rotY == 270)
                { z = -RotAngle; }

                else if (rotY == 0)
                { x = -RotAngle; }
                else
                { x = RotAngle; }
                mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), x, 0, z); 
            }
            else //z
            {
                if (rotY == 0 || rotY == 180 ) 
                { z = RotAngle; }
                else if ( rotY == 90 )
                { x = RotAngle; }
                else
                { x = -RotAngle; }
                mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), x, 0, z); 
                if (rotY == 0 || rotY == 180)
                {
                   mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, this.Block.Shape.rotateY * GameMath.DEG2RAD, 0); //orient based on direction last
                }
            }
            mesher.AddMeshData(mesh);
            return true;
        }
    }
}