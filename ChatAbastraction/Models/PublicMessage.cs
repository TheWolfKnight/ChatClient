using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ChatAbastraction.Interfaces;

namespace ChatAbastraction.Models {
    public class PublicMessage : IMessage {

        public string SenderName = null!;
        public string MessageData = null!;

        public PublicMessage(string senderName, string messageData) {
            this.SenderName = senderName;
            this.MessageData = messageData;
        }

        public string GetMessage() {
            throw new NotImplementedException();
        }

    }
}
