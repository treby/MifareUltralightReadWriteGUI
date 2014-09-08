using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;
using System.Collections.Specialized;

namespace FeliCaNfcLibrary
{
    public delegate void ProgressChangedEventHandler(ProgressChangedEventArgs e);
    public delegate void NfcReadCompletedEventHandler(object sender, NfcReadCompletedEventArgs e);

    /// <summary>
    /// NFCカードに読み書きするライブラリ
    /// </summary>
    public class FeliCaNfc
    {
        [DllImport("User32.dll")]
        extern static UInt32 RegisterWindowMessage(String lpString);

        private static felica_nfc_dll_wrapper FeliCaNfcDllWrapperClass = new felica_nfc_dll_wrapper();
        ListenerWindow lw = null;
        String msg_str_of_find = "find";
        String msg_str_of_enable = "enable";
        UInt32 card_find_message = 0;
        UInt32 card_enable_message = 0;

        UInt32 target_number;

        // 非同期処理用フィールド
        private HybridDictionary userStateToLifetime = new HybridDictionary();
        private delegate void NfcReadWorkerEventHandler(AsyncOperation asyncOp);
        private delegate void NfcWriteWorkerEventHandler(byte[] pages, byte[] data, AsyncOperation asyncOp);
       // private delegate void NfcWriteDelegate(byte Addr, byte[] Data);
        private SendOrPostCallback onNfcReadCompletedDelegate;
        private SendOrPostCallback onNfcWriteCompletedDelegate;

        private const Int32 BUFSIZ = 512;

        private object taskId = null;
        /// <summary>
        /// 処理中か
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return taskId != null;
            }
        }

        #region イベント
        /// <summary>
        /// カードの読み取りを完了した
        /// </summary>
        public event NfcReadCompletedEventHandler NfcReadCompleted;
        protected void OnNfcReadCompleted(NfcReadCompletedEventArgs e)
        {
            if (NfcReadCompleted != null)
            {
                NfcReadCompleted(this, e);
            }
        }

        /// <summary>
        /// カードの書き込みを完了した
        /// </summary>
        public event AsyncCompletedEventHandler NfcWriteCompleted;
        protected void OnNfcWriteCompleted(AsyncCompletedEventArgs e)
        {
            if (NfcWriteCompleted != null)
            {
                NfcWriteCompleted(this, e);
            }
        }

        /// <summary>
        /// 進捗状況が変化した
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;
        protected void OnProgressChanged(ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(e);
            }
        }
        #endregion // イベント

        // コンストラクタ
        public FeliCaNfc()
        {
            InitializeDelegates();
        }
        protected virtual void InitializeDelegates()
        {
            onNfcReadCompletedDelegate = new SendOrPostCallback(ReadCompleted);
            onNfcWriteCompletedDelegate = new SendOrPostCallback(WriteCompleted);
        }

        #region NfcReadAsync
        /// <summary>
        /// NFCカードへの非同期読み込み（認証、暗号化なし）を行う
        /// </summary>
        public void NfcReadAsync()      // StringBuilder port_name, UInt32 target_device, UInt32 stop_mode
        {
            if (this.IsBusy) throw new ApplicationException("ライブラリは使用中です。");

            this.taskId = Guid.NewGuid();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(taskId);

            lock (userStateToLifetime.SyncRoot)
            {
                userStateToLifetime[taskId] = asyncOp;
            }

            NfcReadWorkerEventHandler workerDelegate = new NfcReadWorkerEventHandler(NfcReadWorker);
            workerDelegate.BeginInvoke(asyncOp, null, null);
        }
        // NFCカード読み取りの実処理
        private void NfcReadWorker(AsyncOperation asyncOp)
        {
            // byte[] readBytes = null;
            byte[] readBytes = new byte[72];
            Exception e = null;

            if (!TaskCanceled(asyncOp.UserSuppliedState))
            {
                try
                {
                    StringBuilder port_name = new StringBuilder("USB0");

                    const Int32 DEVICE_TYPE_NFC_14443A_18092_106K = 0x00000001;
                    UInt32 target_device = DEVICE_TYPE_NFC_14443A_18092_106K;

                    NfcSetUp(port_name, target_device);

                    Application.Run(lw);    // felicalib_nfc_start_dev_access : カードの使用権の獲得

                    // felicalib_nfc_thru : デバイス（カード）コマンド発行
                    byte[] command_packet_data = new byte[] { 0x30, 0x00 };
                    UInt16 command_packet_data_length = (UInt16) command_packet_data.Length;
                    byte[] response_packet_data = new byte[BUFSIZ];
                    UInt16 response_packet_data_length = 0x00;

                    if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcThru(
                        command_packet_data,
                        command_packet_data_length,
                        response_packet_data,
                        ref response_packet_data_length))
                    {
                        throw new ApplicationException("Failed! : FeliCaLibNfcThru");
                    }

                    Array.Copy(response_packet_data, readBytes, response_packet_data_length);

                    // 2回目
                    command_packet_data = new byte[] { 0x30, 0x04 };
                    command_packet_data_length = (UInt16)command_packet_data.Length;

                    if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcThru(
                        command_packet_data,
                        command_packet_data_length,
                        response_packet_data,
                        ref response_packet_data_length))
                    {
                        throw new ApplicationException("Failed! : FeliCaLibNfcThru");
                    }
                    
                    Array.Copy(response_packet_data, 0, readBytes, 16, response_packet_data_length);

                    // 3回目
                    command_packet_data = new byte[] { 0x30, 0x08 };
                    command_packet_data_length = (UInt16)command_packet_data.Length;

                    if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcThru(
                        command_packet_data,
                        command_packet_data_length,
                        response_packet_data,
                        ref response_packet_data_length))
                    {
                        throw new ApplicationException("Failed! : FeliCaLibNfcThru");
                    }

                    Array.Copy(response_packet_data, 0, readBytes, 32, response_packet_data_length);

                    // 4回目
                    command_packet_data = new byte[] { 0x30, 0x0C };
                    command_packet_data_length = (UInt16)command_packet_data.Length;

                    if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcThru(
                        command_packet_data,
                        command_packet_data_length,
                        response_packet_data,
                        ref response_packet_data_length))
                    {
                        throw new ApplicationException("Failed! : FeliCaLibNfcThru");
                    }

                    Array.Copy(response_packet_data, 0, readBytes, 48, response_packet_data_length);

                    UInt32 RE_NOTIFICATION_SAME_DEVICE = 0x00;
                    UInt32 stop_mode = RE_NOTIFICATION_SAME_DEVICE;

                    NfcClosing(stop_mode);
                }
                catch (Exception ex)
                {
                    ErrorRoutine();
                    e = ex;
                }
            }

            this.ReadCompletionMethod(readBytes, e, TaskCanceled(asyncOp.UserSuppliedState), asyncOp);
        }
        // NfcReadAsync 完了
        private void ReadCompletionMethod(Byte[] readBytes, Exception exception, bool canceled, AsyncOperation asyncOp)
        {
            if (!canceled)
            {
                lock (userStateToLifetime.SyncRoot)
                {
                    userStateToLifetime.Remove(asyncOp.UserSuppliedState);
                }
            }

            NfcReadCompletedEventArgs e = new NfcReadCompletedEventArgs(readBytes, exception, canceled, asyncOp.UserSuppliedState);

            asyncOp.PostOperationCompleted(onNfcReadCompletedDelegate, e);
        }
        private void ReadCompleted(object operationState)
        {
            NfcReadCompletedEventArgs e = operationState as NfcReadCompletedEventArgs;
            this.taskId = null;

            // イベント発生
            OnNfcReadCompleted(e);
        }
        #endregion // NfcReadAsync
        
        #region NfcWriteAsync
        /// <summary>
        /// NFCカードへの非同期書き込み（認証、暗号化なし）を行う
        /// </summary>
        public void NfcWriteAsync(Int32 flgs, byte[] dataToWrite)      // StringBuilder port_name, UInt32 target_device, UInt32 stop_mode
        {
            if (this.IsBusy) throw new ApplicationException("ライブラリは使用中です。");

            this.taskId = Guid.NewGuid();
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(taskId);

            lock (userStateToLifetime.SyncRoot)
            {
                userStateToLifetime[taskId] = asyncOp;
            }

            List<byte> pagesList = new List<byte>();

            if ((flgs & 0x0001) != 0) pagesList.Add(0x04);
            if ((flgs & 0x0002) != 0) pagesList.Add(0x05);
            if ((flgs & 0x0004) != 0) pagesList.Add(0x06);
            if ((flgs & 0x0008) != 0) pagesList.Add(0x07);
            if ((flgs & 0x0010) != 0) pagesList.Add(0x08);
            if ((flgs & 0x0020) != 0) pagesList.Add(0x09);
            if ((flgs & 0x0040) != 0) pagesList.Add(0x0A);
            if ((flgs & 0x0080) != 0) pagesList.Add(0x0B);
            if ((flgs & 0x0100) != 0) pagesList.Add(0x0C);
            if ((flgs & 0x0200) != 0) pagesList.Add(0x0D);
            if ((flgs & 0x0400) != 0) pagesList.Add(0x0E);
            if ((flgs & 0x0800) != 0) pagesList.Add(0x0F);

            byte[] pages = pagesList.ToArray();

            NfcWriteWorkerEventHandler workerDelegate = new NfcWriteWorkerEventHandler(NfcWriteWorker);
            workerDelegate.BeginInvoke(pages, dataToWrite, asyncOp, null, null);
        }
        private void NfcWriteWorker(byte[] pages, byte[] data, AsyncOperation asyncOp)
        {
            Exception e = null;

            StringBuilder port_name = new StringBuilder("USB0");

            const Int32 DEVICE_TYPE_NFC_14443A_18092_106K = 0x00000001;
            UInt32 target_device = DEVICE_TYPE_NFC_14443A_18092_106K;

            UInt32 RE_NOTIFICATION_SAME_DEVICE = 0x00;
            UInt32 stop_mode = RE_NOTIFICATION_SAME_DEVICE;

            if (!TaskCanceled(asyncOp.UserSuppliedState))
            {
                try
                {
                    NfcSetUp(port_name, target_device);

                    Application.Run(lw);    // felicalib_nfc_start_dev_access : カードの使用権の獲得

                    // felicalib_nfc_thru : デバイス（カード）コマンド発行
                    for (int i = 0; i < pages.Length; i++ )
                    {
                        byte[] command_packet_data = new byte[] { 0xA2, pages[i], data[i * 4], data[i * 4 + 1], data[i * 4 + 2], data[i * 4 + 3] };
                        UInt16 command_packet_data_length = (UInt16)command_packet_data.Length;
                        byte[] response_packet_data = new byte[BUFSIZ];
                        UInt16 response_packet_data_length = 0x00;

                        if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcThru(
                            command_packet_data,
                            command_packet_data_length,
                            response_packet_data,
                            ref response_packet_data_length))
                        {
                            throw new ApplicationException("Failed! : FeliCaLibNfcThru");
                        }
                    }
                    
                    NfcClosing(stop_mode);
                }
                catch (Exception ex)
                {
                    ErrorRoutine();
                    e = ex;
                }
            }

            this.WriteCompletionMethod(e, TaskCanceled(asyncOp.UserSuppliedState), asyncOp);
        }
        private void WriteCompletionMethod(Exception exception, bool canceled, AsyncOperation asyncOp)
        {
            if (!canceled)
            {
                lock (userStateToLifetime.SyncRoot)
                {
                    userStateToLifetime.Remove(asyncOp.UserSuppliedState);
                }
            }

            AsyncCompletedEventArgs e = new AsyncCompletedEventArgs(exception, canceled, asyncOp.UserSuppliedState);

            asyncOp.PostOperationCompleted(onNfcWriteCompletedDelegate, e);
        }
        private void WriteCompleted(object operationState)
        {
            AsyncCompletedEventArgs e = operationState as AsyncCompletedEventArgs;
            this.taskId = null;

            OnNfcWriteCompleted(e);
        }
        #endregion

        // ListenerWindowでウィンドウメッセージを取得
        private void lw_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Msg == card_find_message)
            {
                IntPtr pDevInfo = e.Message.LParam;
                IntPtr pDeviceData;
                if (IntPtr.Size == 8)
                {
                    pDeviceData = (IntPtr)((Int64)pDevInfo
                        + (Int64)Marshal.OffsetOf(typeof(DEVICE_INFO), "dev_info"));
                }
                else
                {
                    pDeviceData = (IntPtr)((Int32)pDevInfo
                        + (Int32)Marshal.OffsetOf(typeof(DEVICE_INFO), "dev_info"));
                }

                // TypeA 用の記述
                DEVICE_DATA_NFC_14443A_18092_106K DeviceData_A =
                    (DEVICE_DATA_NFC_14443A_18092_106K)
                    Marshal.PtrToStructure(pDeviceData,
                    typeof(DEVICE_DATA_NFC_14443A_18092_106K));

                // FeliCa 用の記述
                DEVICE_DATA_NFC_18092_212_424K DeviceData_FeliCa =
                    (DEVICE_DATA_NFC_18092_212_424K)
                    Marshal.PtrToStructure(pDeviceData,
                    typeof(DEVICE_DATA_NFC_18092_212_424K));

                // TypeA 用の記述
                target_number = DeviceData_A.target_number;

                // felicalib_nfc_start_dev_access : カードの使用権の獲得
                if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcStartDevAccess(target_number))
                {
                    ErrorRoutine();
                    lw.Dispose();
                    throw new ApplicationException("Failed! : FeliCaLibNfcStartDevAccess");
                }
            }
            else if (e.Message.Msg == card_enable_message)
            {
                lw.Dispose();             // これによりApplication.Run(lw) から先に進める！
                return;
            }

            return;
        }

        #region 非同期処理用メソッド
        private bool TaskCanceled(object taskId)
        {
            return (userStateToLifetime[taskId] == null);
        }

        // 非同期処理のキャンセル
        public void CancelAsync()
        {
            if (!this.IsBusy) return;
            if (lw != null)
            {
                lw.Invoke(new MethodInvoker(lw.Dispose));         // 別スレッドからの操作のため
            }

            AsyncOperation asyncOp = userStateToLifetime[this.taskId] as AsyncOperation;
            if (asyncOp != null)
            {
                lock (userStateToLifetime.SyncRoot)
                {
                    userStateToLifetime.Remove(taskId);
                }
            }

            this.taskId = null;
        }
        #endregion

        #region NFCカードアクセス定型処理
        /// <summary>
        /// NFCカードアクセスまでの前処理（初めに実行）
        /// </summary>
        /// <param name="port_name">オープン対象ポート名</param>
        /// <param name="target_device">デバイス情報</param>
        private void NfcSetUp(StringBuilder port_name, UInt32 target_device)
        {
            // ListenerWindow の設定
            lw = new ListenerWindow();

            // Windowメッセージの登録
            card_find_message = RegisterWindowMessage(msg_str_of_find);
            if (card_find_message == 0)
                throw new ApplicationException("Failed! : RegisterWindowMessage");
            card_enable_message = RegisterWindowMessage(msg_str_of_enable);
            if (card_enable_message == 0)
                throw new ApplicationException("Failed! : RegisterWindowMessage");

            // 監視するメッセージの登録
            if (lw.WatchMessage(card_find_message) == false)
                throw new ApplicationException("Failed! : WatchMessage");
            if (lw.WatchMessage(card_enable_message) == false)
                throw new ApplicationException("Failed! : WatchMessage");

            // イベントハンドラの追加
            lw.MessageReceived += new MessageReceivedEventHandler(lw_MessageReceived);

            // felicalib_nfc_initialize : ライブラリ初期化
            if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcInitialize())
            {
                throw new ApplicationException("Failed! : FeliCaLibNfcInitialize");
            }

            // felicalib_nfc_open : リーダ／ライタのオープン
            if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcOpen(port_name))
            {
                throw new ApplicationException("Failed! : FeliCaLibNfcOpen");
            }

            // felicalib_nfc_set_poll_callback_parameter : デバイス捕捉通知情報のセット
            if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcSetPollCallbackParameters(lw.Handle, msg_str_of_find, msg_str_of_enable))
            {
                throw new ApplicationException("Failed! : FeliCaLibNfcSetPollCallbackParameters");
            }

            // felicalib_nfc_start_poll_mode : デバイス捕捉処理の開始
            if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcStartPollMode(target_device))
            {
                throw new ApplicationException("Failed! : FeliCaLibNfcStartPollMode");
            }

            return;
        }
        /// <summary>
        /// NFCカードアクセスからの後処理（最後に実行）
        /// </summary>
        /// <param name="stop_mode">停止動作種別</param>
        private void NfcClosing(UInt32 stop_mode)
        {
            // felicalib_nfc_stop_dev_access : デバイス使用権の解放
            if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcStopDevAccess(stop_mode))
            {
                throw new ApplicationException("Failed! : FeliCaLibNfcStopDevAccess");
            }

            // felicalib_nfc_stop_poll_mode : デバイス捕捉処理の終了
            if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcStopPollMode())
            {
                throw new ApplicationException("Failed! : FeliCaLibNfcStopPollMode");
            }

            // felicalib_nfc_close : リーダ／ライタのクローズ
            if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcClose())
            {
                throw new ApplicationException("Failed! : FeliCaLibNfcClose");
            }

            // felicalib_nfc_uninitialize : ライブラリ終了化
            if (!FeliCaNfcDllWrapperClass.FeliCaLibNfcUninitialize())
            {
                throw new ApplicationException("Failed! : FeliCaLibNfcUninitialize");
            }

            return;
        }

        // エラー発生時処理
        private static void ErrorRoutine()
        {
            UInt32[] error_info = new UInt32[] { 0, 0 };
            FeliCaNfcDllWrapperClass.FeliCaLibNfcGetLastError(error_info);

            // ToDo : エラーメッセージ処理
            // 　　　 バイト列をメッセージに変換したり？

            MessageBox.Show(string.Format("0x{0:X8}, 0x{1:X8}", error_info[0], error_info[1]));

            // 後片付け
            FeliCaNfcDllWrapperClass.FeliCaLibNfcClose();
            FeliCaNfcDllWrapperClass.FeliCaLibNfcUninitialize();

            return;
        }
        #endregion
    }
}
