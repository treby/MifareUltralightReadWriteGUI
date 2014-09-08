using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FeliCaNfcLibrary
{
    public delegate void NfcWriteCompletedEventHandler(object sender, NfcWriteCompletedEventArgs e);

    /// <summary>
    /// NFCカード非同期書き込みメソッド（NfcWriteAsync）用イベントデータクラス
    /// </summary>
    public class NfcWriteCompletedEventArgs// : AsyncCompletedEventArgs
    {
        // 未使用
    }
}
