using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace sfanimationlibrary
{
    public class sfanimationlibraryModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
			//block
            api.RegisterBlockClass("sfblockrotate", typeof(SfBlockRotate));
            //api.RegisterBlockBehaviorClass("sfbbblocktoggle", typeof(SfBBBlockToggle));
            api.RegisterBlockEntityClass("sfbeblockrotate", typeof(SfBEBlockRotate));
			
			//animutil
			api.RegisterBlockClass("sfblockanimated", typeof(SfBlockAnimated));
            //api.RegisterBlockBehaviorClass("sfanimatedtoggle", typeof(SfBBAnimatedToggle));
            api.RegisterBlockEntityClass("sfbeblockanimated", typeof(SfBEBlockAnimated));

            //axle
            api.RegisterBlockClass("sfblockaxle", typeof(SfBlockAxle));
            api.RegisterBlockEntityBehaviorClass("sfbebblockmpaxle", typeof(SfBEBBlockMPAxle));
        }
    }
}
