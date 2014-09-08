using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace FeliCaNfcLibrary
{
    public class MessageHandler
    {
        public bool bRet;
        private UInt32 target_number;
        private UInt32 card_find_message;
        private UInt32 card_enable_message;
        private static felica_nfc_dll_wrapper FeliCaNfcDllWrapperClass = new felica_nfc_dll_wrapper();

        /// <summary>
        /// Windows メッセージの登録
        /// </summary>
        public MessageHandler(UInt32 findMsg, UInt32 enableMsg)
        {
            card_find_message = findMsg;
            card_enable_message = enableMsg;
        }

        /// <summary>
        /// Type A カード用のメッセージハンドラ
        /// </summary>
        public void messageHandlerFuncForTypeA(object sender, MessageReceivedEventArgs e)
        {
            bRet = false;

            // カードを検知した(felicalib_nfc_start_poll_modeに対応するWindowsメッセージ)
            if (e.Message.Msg == card_find_message)
            {
                IntPtr pDevInfo = e.Message.LParam;
                IntPtr pDeviceData_A;
                if (IntPtr.Size == 8)
                {
                    pDeviceData_A = (IntPtr)((Int64)pDevInfo
                        + (Int64)Marshal.OffsetOf(typeof(DEVICE_INFO), "dev_info"));
                }
                else
                {
                    pDeviceData_A = (IntPtr)((Int32)pDevInfo
                        + (Int32)Marshal.OffsetOf(typeof(DEVICE_INFO), "dev_info"));
                }

                DEVICE_DATA_NFC_14443A_18092_106K DeviceData_A = (DEVICE_DATA_NFC_14443A_18092_106K)Marshal.PtrToStructure(pDeviceData_A, typeof(DEVICE_DATA_NFC_14443A_18092_106K));

                target_number = DeviceData_A.target_number;
                bRet = FeliCaNfcDllWrapperClass.FeliCaLibNfcStartDevAccess(target_number);

                if (bRet == false)
                {
                    // ToDo : エラー処理
                    System.Windows.Forms.MessageBox.Show("Failed: FeliCaLibNfcStartDevAccess");
                    System.Windows.Forms.Application.Exit();
                    return;
                }
            }
            else if (e.Message.Msg == card_enable_message)
            {
                bRet = true;
                System.Windows.Forms.Application.Exit();            // ←これでApplication.Run()から先に進める？
                return;
            }

            return;
        }
    }
}
