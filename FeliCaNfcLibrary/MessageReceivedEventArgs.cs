using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FeliCaNfcLibrary
{
    // Windowsメッセージを持つイベント引数
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Windowsメッセージ
        /// </summary>
        private readonly Message _message;
        public Message Message
        {
            get
            {
                return _message;
            }
        }

        public MessageReceivedEventArgs(Message message)
        {
            _message = message;
        }
    }
}
