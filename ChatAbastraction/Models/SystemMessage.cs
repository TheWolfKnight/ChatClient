using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ChatAbastraction.Interfaces;

namespace ChatAbastraction.Models {
    public class SystemMessage : IMessage {

        public string OriginalSource = null!;
        public string HeaderData = null!;

        public string GetMessage() {
            throw new NotImplementedException();
        }

    }
}
