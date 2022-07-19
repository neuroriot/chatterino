﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public struct UserInfoData
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string MessageId { get; set; }
        public TwitchChannel Channel { get; set; }
    }
}
