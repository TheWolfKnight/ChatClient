using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ChatAbastraction.Interfaces;

namespace ChatAbastraction.Models {
    public class PrivateMessage : IMessage {

        public string SenderName = null!;
        public string ReciverName = null!;
        public string MessageData = null!;

        public PrivateMessage(string senderName, string reciverName, string messageData) {
            this.SenderName = senderName;
            this.ReciverName = reciverName;
            this.MessageData = messageData;
        }

        public string GetMessage() {
            throw new NotImplementedException();
        }
    }
}
