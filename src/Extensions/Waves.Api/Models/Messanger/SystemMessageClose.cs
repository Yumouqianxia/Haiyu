using System;
using System.Collections.Generic;
using System.Text;
using Waves.Api.Models;

namespace Waves.Api.Models.Messanger
{
    public class SystemMessageClose
    {
        public SystemMessagerModel Message { get; set; }
        public SystemMessageClose(SystemMessagerModel message)
        {
            this.Message = message;
        }
    }
}
