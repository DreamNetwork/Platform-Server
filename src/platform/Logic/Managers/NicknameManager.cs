using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatusPlatform.Networking;

namespace StatusPlatform.Logic.Managers
{
    // TODO
    public class NicknameManager : Manager
    {
        public override bool HandlePacket(Client sourceClient, Message message)
        {
            return false;
        }
    }
}
