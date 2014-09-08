using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FeliCaNfcLibrary
{
    /// <summary>
    /// NFCカード非同期読み込みメソッド（NfcReadAsync）用イベントデータクラス
    /// </summary>
    public class NfcReadCompletedEventArgs : AsyncCompletedEventArgs
    {
        private byte[] readBytes;
        /// <summary>
        /// 読み取ったバイト列
        /// </summary>
        public byte[] ReadBytes
        {
            get
            {
                return readBytes;
            }
        }

        public NfcReadCompletedEventArgs(byte[] readBytes, Exception e, bool canceled, object state)
            : base(e, canceled, state)
        {
            this.readBytes = readBytes;
        }
    }
}
