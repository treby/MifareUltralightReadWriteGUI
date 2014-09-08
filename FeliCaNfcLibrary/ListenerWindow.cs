using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FeliCaNfcLibrary
{
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

    /// <summary>
    /// Windowメッセージ監視用ウィンドウ
    /// </summary>
    public class ListenerWindow : Form
    {
        /// <summary>
        /// 監視中のウィンドウメッセージを受信した
        /// </summary>
        public event MessageReceivedEventHandler MessageReceived;
        private void onMessageReceived(MessageReceivedEventArgs e)
        {
            if(MessageReceived != null)
                MessageReceived(null, e);
        }

        private const Int32 MAX_MESSAGES = 2;
        private UInt32[] messageSet = new UInt32[MAX_MESSAGES];
        private Int32 registeredMessage = 0;

        /// <summary>
        /// 監視するウィンドウメッセージを追加
        /// </summary>
        /// <param name="message">登録するWindowsメッセージ</param>
        public bool WatchMessage(UInt32 message)
        {
            // メッセージ登録数に空きがあるなら登録
            if (registeredMessage < messageSet.Length)
            {
                messageSet[registeredMessage] = message;
                registeredMessage++;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Control.CreateParams プロパティ：
        /// 　コントロール ハンドルが作成されるときに必要な作成パラメータを取得します。
        /// </summary>
        protected override CreateParams CreateParams
        {
	        get 
	        {
                const Int32 WS_EX_TOOLWINDOW = 0x80;
                const Int64 WS_POPUP = 0x80000000;
                const Int32 WS_VISIBLE = 0x10000000;
                const Int32 WS_SYSMENU = 0x80000;
                const Int32 WS_MAXIMIZEBOX = 0x10000;

                CreateParams cp = base.CreateParams;
                cp.ExStyle = WS_EX_TOOLWINDOW;
                cp.Style = unchecked((Int32)WS_POPUP) | WS_VISIBLE | WS_SYSMENU | WS_MAXIMIZEBOX;
                cp.Width = 0;
                cp.Height = 0;

                return cp;
	        }
        }

        /// <summary>
        /// ウィンドウへ送信されたメッセージを処理する、
        /// アプリケーション定義のコールバック関数
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            bool handleMessage = false;

            for (Int32 i = 0; i < registeredMessage; i++)
            {
                if(messageSet[i] == m.Msg)
                {
                    handleMessage = true;
                }
            }

            if(handleMessage)
            {
                // イベント発生
                onMessageReceived(new MessageReceivedEventArgs(m));
            }

            base.WndProc(ref m);
            return ;
        }
    }
}
