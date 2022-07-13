using System;
using System.Threading;
using System.Linq;
using System.IO;

using NdefLibrary.Ndef;
using PCSC;
using PCSC.Monitoring;
using PCSC.Exceptions;

using PcscSdk;
using PcscSdk.Common;
using Org.BouncyCastle.Security;

namespace FlagCarrierBase
{
    public class NfcHandlerException : Exception
    {
        public NfcHandlerException()
        {
        }

        public NfcHandlerException(string message)
            : base(message)
        {
        }

        public NfcHandlerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class NfcHandler : IDisposable
    {
        private static readonly IContextFactory contextFactory = ContextFactory.Instance;
        private volatile int blockCardEvents = 0;

        public static string[] GetReaderNames()
        {
            using (var ctx = contextFactory.Establish(SCardScope.System))
            {
                return ctx.GetReaders();
            }
        }

        public NfcHandler()
        {
        }

        ~NfcHandler()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (monitor != null)
            {
                monitor.Cancel();
                monitor.CardInserted -= Monitor_CardInserted;
                monitor.CardRemoved -= Monitor_CardRemoved;
                monitor.MonitorException -= Monitor_MonitorException;
                monitor.Dispose();
                monitor = null;
            }
        }

        public event Action<string> StatusMessage;
        public event Action<string> ErrorMessage;
        public event Action<string> CardAdded;
        public event Action<string> CardHandlingDone;
        public event Action<string> CardRemoved;
        public event Action<NdefMessage> ReceiveNdefMessage;
        public event Action<byte[]> NewTagUid;
        public event Action WriteSuccess;

        private byte[] ndefDataToWrite;
        private ISCardMonitor monitor;

        public void StartMonitoring(string[] readerNames = null)
        {
            try
            {
                if (monitor == null)
                {
                    var monitorFactory = MonitorFactory.Instance;
                    monitor = monitorFactory.Create(SCardScope.System);
                    monitor.MonitorException += Monitor_MonitorException;
                    monitor.CardInserted += Monitor_CardInserted;
                    monitor.CardRemoved += Monitor_CardRemoved;
                }

                monitor.Cancel();

                if (readerNames == null)
                    readerNames = GetReaderNames();

                foreach (string readerName in readerNames)
                    TryTurnOffBeep(readerName);

                if (readerNames.Length == 0)
                    return;

                monitor.Start(readerNames);
            }
            catch(PCSCException e)
            {
                throw new NfcHandlerException("Failed monitoring readers\n" + e.Message, e);
            }
        }

        public void WriteNdefMessage(NdefMessage msg)
        {
            if (msg != null)
                ndefDataToWrite = msg.ToByteArray();
            else
                ndefDataToWrite = null;
        }

        #region ACR122U Specifics

        private void TryTurnOffBeep(string readerName)
        {
            if (!readerName.Contains("ACR"))
                return;

            try
            {
                using (ISCardContext ctx = contextFactory.Establish(SCardScope.System))
                using (ICardReader reader = ctx.ConnectReader(readerName, SCardShareMode.Direct, SCardProtocol.Unset))
                {
                    var apdu = new Iso7816.ApduCommand(0xFF, 0x00, 0x52, 0x00, null, 0x00);
                    var res = reader.Control(apdu);
                    if (res.Succeeded)
                        StatusMessage?.Invoke("Turned off buzzer on " + reader.Name);
                }
            }
            catch(Exception)
            {
            }
        }

        public string GetACRReader()
        {
            string[] readerNames = GetReaderNames();
            foreach (string readerName in readerNames)
                if (readerName.Contains("ACR"))
                    return readerName;
            return null;
        }

        public class ACRReaderControl
        {
            public bool finalRed; // Final Red LED State (On/Off)
            public bool finalGreen; // Final Green LED State (On/Off)
            public bool redMask; // Red LED State Mask (Update the State/No change)
            public bool greenMask; // Green LED State Mask (Update the State/No change)
            public bool initRedBlink; // Initial Red LED Blinking State (On/Off)
            public bool initGreenBlink; // Initial Green LED Blinking State (On/Off)
            public bool redBlinkMask; // Red LED Blinking Mask (Blink/Not Blink)
            public bool greenBlinkMask; // Green LED Blinking Mask (Blink/Not Blink)

            public byte t1Duration; // T1 Duration Initial Blinking State, unit: 100ms
            public byte t2Duration; // T2 Duration Toggle Blinking State, unit: 100ms
            public byte reps; // Nuber of repetition
            public byte buzzer; // bitmask, 1 = on during T1, 2 = on during T2, 3 = on during both
        }

        public Iso7816.ApduResponse SignalACR(string readerName, ACRReaderControl ctrl)
        {
            byte ledStateCtrl = 0;
            if (ctrl.finalRed)
                ledStateCtrl |= 1 << 0;
            if (ctrl.finalGreen)
                ledStateCtrl |= 1 << 1;
            if (ctrl.redMask)
                ledStateCtrl |= 1 << 2;
            if (ctrl.greenMask)
                ledStateCtrl |= 1 << 3;
            if (ctrl.initRedBlink)
                ledStateCtrl |= 1 << 4;
            if (ctrl.initGreenBlink)
                ledStateCtrl |= 1 << 5;
            if (ctrl.redBlinkMask)
                ledStateCtrl |= 1 << 6;
            if (ctrl.greenBlinkMask)
                ledStateCtrl |= 1 << 7;
            byte[] ctrlData = new byte[] { ctrl.t1Duration, ctrl.t2Duration, ctrl.reps, ctrl.buzzer };

            blockCardEvents += 1;

            try
            {
                using (ISCardContext ctx = contextFactory.Establish(SCardScope.System))
                using (ICardReader reader = ctx.ConnectReader(readerName, SCardShareMode.Direct, SCardProtocol.Unset))
                {
                    var apdu = new Iso7816.ApduCommand(0xFF, 0x00, 0x40, ledStateCtrl, ctrlData, null);
                    return reader.Control(apdu);
                }
            }
            finally
            {
                new Thread(() =>
                {
                    Thread.Sleep(((ctrl.t1Duration * 100) + (ctrl.t2Duration * 100)) * ctrl.reps);
                    blockCardEvents -= 1;
                }).Start();
            }

            // For all I'm aware, this always returns a failure APDU, but the reader does perform the specified action.
        }

        public void SignalSuccess(string readerName)
        {
            ACRReaderControl ctrl = new ACRReaderControl
            {
                finalRed = true,
                initGreenBlink = true,
                greenBlinkMask = true,
                redMask = true,
                greenMask = true,

                t1Duration = 4,
                t2Duration = 4,
                reps = 5
            };

            SignalACR(readerName, ctrl);
        }

        public void SignalFailure(string readerName)
        {
            ACRReaderControl ctrl = new ACRReaderControl
            {
                finalRed = true,
                initRedBlink = true,
                redBlinkMask = true,
                redMask = true,
                greenMask = true,

                t1Duration = 6,
                t2Duration = 4,
                reps = 3,
                buzzer = 1
            };

            SignalACR(readerName, ctrl);
        }

        #endregion

        private void Monitor_CardInserted(object sender, CardStatusEventArgs args)
        {
            if (blockCardEvents > 0)
                return;

            CardAdded?.Invoke(args.ReaderName);
            StatusMessage?.Invoke("Tag detected on " + args.ReaderName);

            try
            {
                HandleSmartCard(args.ReaderName);
            }
            catch (Exception e)
            {
                ErrorMessage?.Invoke("Error handling tag: " + e.Message);
            }

            CardHandlingDone?.Invoke(args.ReaderName);
        }

        private void Monitor_CardRemoved(object sender, CardStatusEventArgs args)
        {
            if (blockCardEvents > 0)
                return;

            StatusMessage?.Invoke("Tag removed on " + args.ReaderName);

            CardRemoved?.Invoke(args.ReaderName);
        }

        private void Monitor_MonitorException(object sender, PCSCException exception)
        {
            ErrorMessage?.Invoke("Monitoring Error: " + exception.Message);
        }

        private void HandleSmartCard(String readerName)
        {
            using (ISCardContext ctx = contextFactory.Establish(SCardScope.System))
            using (ICardReader reader = ctx.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
            {
                StatusMessage?.Invoke("Connected to tag");

                IccDetection cardIdent = new IccDetection(reader);
                cardIdent.DetectCardType();

                StatusMessage?.Invoke("Device class: " + cardIdent.PcscDeviceClass.ToString());
                StatusMessage?.Invoke("Card name: " + cardIdent.PcscCardName.ToString());
                StatusMessage?.Invoke("ATR: " + BitConverter.ToString(cardIdent.Atr));

                if (cardIdent.PcscDeviceClass == DeviceClass.StorageClass &&
                    (cardIdent.PcscCardName == CardName.MifareUltralight
                    || cardIdent.PcscCardName == CardName.MifareUltralightC
                    || cardIdent.PcscCardName == CardName.MifareUltralightEV1))
                {
                    HandleMifareUL(reader);
                }
                else if (cardIdent.PcscDeviceClass == DeviceClass.StorageClass &&
                    (cardIdent.PcscCardName == CardName.MifareStandard1K
                    || cardIdent.PcscCardName == CardName.MifareStandard4K))
                {
                    HandleMifareStandard(reader);
                }
                else
                {
                    HandleHCEClient(reader);
                }
            }
        }

        #region Host Emulation Client

        public static readonly byte[] FLAGCARRIER_HCE_AID = new byte[] { 0xf0, 0x5a, 0x25, 0x58, 0x83, 0x6e, 0x09, 0x66, 0xae, 0xd5, 0x27, 0xce };
        private static readonly SecureRandom random = new SecureRandom();

        private void HandleHCEClient(ICardReader reader)
        {
            StatusMessage?.Invoke("Attemtping to talk to Android HCE device.");

            var selectCmd = new Iso7816.SelectCommand(FLAGCARRIER_HCE_AID, 0);
            var res = reader.Transceive(selectCmd);

            if (res.SW == 0x6a82)
            {
                ErrorMessage?.Invoke("Device has no idea who we are.");
                return;
            }
            else if(!res.Succeeded)
            {
                ErrorMessage?.Invoke("Failed communicating with device: " + res.ToString());
                return;
            }

            StatusMessage?.Invoke("Connected to FlagCarrier HCE device!");

            byte[] challengeToken = new byte[32];
            random.NextBytes(challengeToken, 0, challengeToken.Length);

            var updateCmd = new Iso7816.UpdateBinaryCommand(challengeToken);
            res = reader.Transceive(updateCmd);

            if (res.SW == 0x6A82)
            {
                ErrorMessage?.Invoke("HCE Device does not have any data for us.");
                return;
            }

            if (!res.Succeeded)
            {
                ErrorMessage?.Invoke("Failed sending challenge token: " + res.ToString());
                return;
            }

            StatusMessage?.Invoke("Sent challenge token.");

            byte[] ndefData = new byte[0];
            const int len = 250;
            int offset = 0;

            do
            {
                var readCmd = new Iso7816.ReadBinaryCommand(len, offset);
                res = reader.Transceive(readCmd);

                if (!res.Succeeded)
                {
                    ErrorMessage?.Invoke("Failed reading data at " + offset + ": " + res.ToString());
                    return;
                }

                if (res.ResponseData == null || res.ResponseData.Length == 0)
                    break;

                Array.Resize(ref ndefData, ndefData.Length + res.ResponseData.Length);
                res.ResponseData.CopyTo(ndefData, offset);
                offset += res.ResponseData.Length;

            } while (res.ResponseData.Length == len);

            StatusMessage?.Invoke("Read " + ndefData.Length + " bytes of ndef data from device.");

            NdefMessage msg = NdefMessage.FromByteArray(ndefData);

            NewTagUid?.Invoke(challengeToken);
            ReceiveNdefMessage?.Invoke(msg);

            var eraseCmd = new Iso7816.EraseBinaryCommand();
            res = reader.Transceive(eraseCmd);

            if (!res.Succeeded)
                ErrorMessage?.Invoke("Failed confirming transaction to device.");
        }

        #endregion

        #region Mifare Ultralight
        private void HandleMifareUL(ICardReader reader)
        {
            var mifare = new PcscSdk.MifareUltralight.AccessHandler(reader);

            StatusMessage?.Invoke("Handling as Mifare Ultralight");

            byte[] uid = mifare.GetUid();
            StatusMessage?.Invoke("UID: " + BitConverter.ToString(uid));
            Array.Resize(ref uid, uid.Length + 1);
            uid[uid.Length - 1] = 0xAA;
            NewTagUid?.Invoke(uid);

            byte[] infoData = mifare.Read(0);
            StatusMessage?.Invoke("CC: " + BitConverter.ToString(infoData.Skip(12).ToArray()));

            try
            {
                byte[] versionData = mifare.GetVersion();
                StatusMessage?.Invoke("Mifare Version: " + BitConverter.ToString(versionData));

                int capacity = versionData[6] >> 1;
                int capacityMax = capacity;
                if ((versionData[6] & 1) == 1)
                    capacityMax += 1;
                capacity = (int)Math.Pow(2, capacity);
                capacityMax = (int)Math.Pow(2, capacityMax);
                StatusMessage?.Invoke("Capacity is between " + capacity + " and " + capacityMax + " bytes");
            }
            catch(ApduFailedException e)
            {
                StatusMessage?.Invoke("Failed getting Mifare Version: " + e.Message);
            }

            byte identMagic = infoData[12];
            byte identVersion = infoData[13];
            int identCapacity = infoData[14] * 8;
            int major = identVersion >> 4;
            int minor = identVersion & 0x0F;

            if (identMagic != 0xE1 || identVersion < 0x10)
                throw new NfcHandlerException("Tag format is unsupported");

            StatusMessage?.Invoke("Found Type 2 Tag version " + major + "." + minor + " with " + identCapacity + " bytes capacity.");

            if(ndefDataToWrite != null)
            {
                WriteNdefToMifareUL(mifare, ndefDataToWrite);
                ndefDataToWrite = null;
            }
            else
            {
                byte[] data = DumpMifareUL(mifare);
                ParseTLVData(data);
            }
        }

        private void WriteNdefToMifareUL(PcscSdk.MifareUltralight.AccessHandler mifare, byte[] ndefData)
        {
            byte[] infoData = mifare.Read(3);
            int capacity = infoData[2] * 8;

            byte[] wrappedData = GenerateTLVData(ndefData);

            if (wrappedData.Length > capacity)
                throw new NfcHandlerException("Data size of " + wrappedData.Length + " bytes exceeds capacity of " + capacity + " bytes!");

            int data_length = wrappedData.Length;
            Array.Resize(ref wrappedData, (data_length / 4) * 4 + 4);

            for (byte pos = 4; (pos - 4) * 4 < wrappedData.Length; pos++)
            {
                mifare.Write(pos, wrappedData.Skip((pos - 4) * 4).Take(4).ToArray());
            }

            StatusMessage?.Invoke("Written " + data_length + " bytes of data. Ndef message length is " + ndefData.Length + " bytes.");
            WriteSuccess?.Invoke();
        }

        private byte[] DumpMifareUL(PcscSdk.MifareUltralight.AccessHandler mifare)
        {
            byte[] infoData = mifare.Read(3);
            int bytes_left = infoData[2] * 8;

            byte[] res = new byte[bytes_left];

            for (byte pos = 4; bytes_left > 0; pos += 4, bytes_left -= 16)
            {
                byte[] data = mifare.Read(pos);
                if (bytes_left < 16)
                    data = data.Take(bytes_left).ToArray();

                data.CopyTo(res, (pos - 4) * 4);
            }

            return res;
        }

        #endregion

        #region Mifare Standard

        private void HandleMifareStandard(ICardReader reader)
        {
            var mifare = new PcscSdk.MifareStandard.AccessHandler(reader);

            StatusMessage?.Invoke("Handling as Mifare Standard 1K");

            byte[] uid = mifare.GetUid();
            StatusMessage?.Invoke("UID: " + BitConverter.ToString(uid));
            Array.Resize(ref uid, uid.Length + 1);
            uid[uid.Length - 1] = 0xBB;
            NewTagUid?.Invoke(uid);

            byte gpByte = InitAndGetGPMifareStandard(mifare);

            bool usesMad = (gpByte & 0x80) != 0;
            bool multiApp = (gpByte & 0x40) != 0;
            int madVersion = gpByte & 0x03;

            StatusMessage?.Invoke("Uses MAD: " + usesMad + "; Multi App: " + multiApp + "; Version: " + madVersion);

            if (ndefDataToWrite != null)
            {
                WriteNdefToMifareStandard(mifare, ndefDataToWrite);
                ndefDataToWrite = null;
            }
            else
            {
                if (!usesMad)
                    throw new NfcHandlerException("No MAD in use");
                if (madVersion != 1)
                    throw new NfcHandlerException("Unsupported MAD version: " + madVersion + " (Only version 1 is supported)");

                byte[] data = DumpMifareStandard(mifare);
                ParseTLVData(data);
            }
        }

        private byte InitAndGetGPMifareStandard(PcscSdk.MifareStandard.AccessHandler mifare)
        {
            mifare.LoadKey(new byte[] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 }, 0);
            StatusMessage?.Invoke("Loaded public MAD key in slot 0.");

            mifare.LoadKey(new byte[] { 0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7 }, 1);
            StatusMessage?.Invoke("Loaded public NDEF key in slot 1.");

            byte[] infoData;
            try
            {
                infoData = mifare.Read(3, GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0);
            }
            catch (Exception)
            {
                StatusMessage?.Invoke("Failed reading with default key.");

                mifare.LoadKey(PcscSdk.MifareStandard.DefaultKeys.FactoryDefault, 0);
                StatusMessage?.Invoke("Loaded factory default key in slot 0.");

                infoData = mifare.Read(3, GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0);
                StatusMessage?.Invoke("Card uses factory default key!");
            }
            byte gpByte = infoData[9];
            StatusMessage?.Invoke("General purpose byte: " + BitConverter.ToString(new[] { gpByte }));

            return gpByte;
        }

        private void WriteNdefToMifareStandard(PcscSdk.MifareStandard.AccessHandler mifare, byte[] ndefData)
        {
            InitializeMifareStandard(mifare);

            const int capacity = 15 * 3 * 16;

            byte[] wrappedData = GenerateTLVData(ndefData);

            if (wrappedData.Length > capacity)
                throw new NfcHandlerException("Data size of " + wrappedData.Length + " bytes exceeds capacity of " + capacity + " bytes!");

            int data_length = wrappedData.Length;
            Array.Resize(ref wrappedData, (data_length / 16) * 16 + 16);

            byte writeBlock = 3;
            for (byte pos = 0; pos < wrappedData.Length; pos += 16)
            {
                if (++writeBlock % 4 == 3)
                    writeBlock++;
                mifare.Write(writeBlock, wrappedData.Skip(pos).Take(16).ToArray(), GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 1);
            }

            StatusMessage?.Invoke("Written " + data_length + " bytes of data. Ndef message length is " + ndefData.Length + " bytes.");
            WriteSuccess?.Invoke();
        }

        private static readonly byte[][] ndefMadData = new byte[][] {
                null,
                new byte[] { 0x14, 0x01, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 },
                new byte[] { 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1, 0x03, 0xE1 },
                new byte[] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0x78, 0x77, 0x88, 0xC1, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }
            };

        private static readonly byte[] ndefSectorIdent = new byte[] { 0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7, 0x7F, 0x07, 0x88, 0x40, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        private static readonly byte[] ndefComparator  = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0x07, 0x88, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        private void InitializeMifareStandard(PcscSdk.MifareStandard.AccessHandler mifare)
        {
            byte[] mad1 = mifare.Read(1, PcscSdk.MifareStandard.GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0);
            byte[] mad2 = mifare.Read(2, PcscSdk.MifareStandard.GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0);

            // One of my readers only has two key slots, another one does not allow overwriting them, but has at least 3. So here we go...
            // Also, the Microsoft IFD emulation(?) reports 0x0000 in response, but it still seems to have worked.
            byte factoryKeySlot = 0;
            try
            {
                mifare.LoadKey(PcscSdk.MifareStandard.DefaultKeys.FactoryDefault, 0);
                StatusMessage?.Invoke("Loaded factory default key in slot 0.");
            }
            catch(ApduFailedException e)
            {
                if (e.Response.SW == 0x0000 && e.Response.ResponseData.Length == 0 && mifare.CardReader.Name.Contains("Microsoft IFD"))
                {
                    StatusMessage?.Invoke("Loaded factory default key in slot 0. (MS Quirk)");
                }
                else
                {
                    StatusMessage?.Invoke("Could not re-load to slot 0, trying slot 2: " + e.Message);
                    mifare.LoadKey(PcscSdk.MifareStandard.DefaultKeys.FactoryDefault, 2);
                    StatusMessage?.Invoke("Loaded factory default key in slot 2.");
                    factoryKeySlot = 2;
                }
            }

            if (!mad1.SequenceEqual(ndefMadData[1]) || !mad2.SequenceEqual(ndefMadData[2]))
            {
                StatusMessage?.Invoke("Writing NDEF MAD block.");

                for (byte block = 1; block < 4; block++)
                {
                    StatusMessage?.Invoke("Writing MAD block " + block);
                    try
                    {
                        mifare.Write(block, ndefMadData[block], GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, factoryKeySlot);
                    }
                    catch(Exception)
                    {
                        mifare.Write(block, ndefMadData[block], GeneralAuthenticate.GeneralAuthenticateKeyType.PicoTagPassKeyB, factoryKeySlot);
                    }
                }
            }

            for (int sector = 1; sector < 16; sector++)
            {
                byte block = (byte)(sector * 4 + 3);
                try
                {
                    for (int i = 1; i < 4; i++)
                    {
                        var authRes = mifare.CardReader.Transceive(new PcscSdk.MifareStandard.GeneralAuthenticate((byte)(block - i), 1, GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA));
                        if (!authRes.Succeeded)
                        {
                            StatusMessage?.Invoke("NDEF read authentication failed, trying trailer rewrite.");
                            throw new Exception();
                        }
                    }

                    byte[] sectorIdent = mifare.Read(block, GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 1);
                    if (!sectorIdent.SequenceEqual(ndefComparator))
                        throw new Exception();
                }
                catch (Exception)
                {
                    StatusMessage?.Invoke("Writing NDEF trailer into sector " + sector);
                    try
                    {
                        mifare.Write(block, ndefSectorIdent, GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, factoryKeySlot);
                    }
                    catch(Exception)
                    {
                        mifare.Write(block, ndefSectorIdent, GeneralAuthenticate.GeneralAuthenticateKeyType.PicoTagPassKeyB, factoryKeySlot);
                    }
                }
            }

            StatusMessage?.Invoke("Tag is NDEF formated");
        }

        private byte[] DumpMifareStandard(PcscSdk.MifareStandard.AccessHandler mifare)
        {
            byte[] madData = new byte[32];
            mifare.Read(1, PcscSdk.MifareStandard.GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0).CopyTo(madData, 0);
            mifare.Read(2, PcscSdk.MifareStandard.GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 0).CopyTo(madData, 16);

            byte crc = madData[0];
            byte calcCrc = PcscSdk.MifareStandard.CRC8.Calc(madData.Skip(1).ToArray());

            if (crc != calcCrc)
                throw new NfcHandlerException("MAD CRC mismatch. 0x" + BitConverter.ToString(new[] { crc }) + " != 0x" + BitConverter.ToString(new[] { calcCrc }));

            StatusMessage?.Invoke("CRC 0x" + BitConverter.ToString(new[] { crc }) + " OK");

            using (MemoryStream mem = new MemoryStream())
            {
                for (int sec = 1; sec < 16; sec++)
                {
                    if (madData[sec * 2] != 0x03 || madData[sec * 2 + 1] != 0xE1)
                        continue;
                    for (int block = sec * 4; block < sec * 4 + 3; block++)
                    {
                        mem.Write(mifare.Read((ushort)block, PcscSdk.MifareStandard.GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA, 1), 0, 16);
                    }
                }

                return mem.ToArray();
            }
        }

        #endregion

        #region Helpers

        private byte[] GenerateTLVData(byte[] ndefData)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)0x03);
                    if (ndefData.Length >= 0xFF)
                    {
                        writer.Write((byte)0xFF);
                        byte[] lengthBytes = BitConverter.GetBytes((ushort)ndefData.Length);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(lengthBytes);
                        writer.Write(lengthBytes);
                    }
                    else
                    {
                        writer.Write((byte)ndefData.Length);
                    }
                    writer.Write(ndefData);

                    writer.Write((byte)0xFE);
                    writer.Write((byte)0x00);
                }

                return stream.ToArray();
            }
        }

        private void ParseTLVData(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while(stream.Position < stream.Length)
                {
                    byte tag = reader.ReadByte();
                    int length = 0;
                    byte[] val = null;
                    if (tag != 0x00 && tag != 0xFE)
                    {
                        length = reader.ReadByte();
                        if (length >= 0xFF)
                        {
                            byte[] lengthBytes = reader.ReadBytes(2);
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(lengthBytes);
                            length = BitConverter.ToUInt16(lengthBytes, 0);
                        }
                        val = length > 0 ? reader.ReadBytes(length) : null;
                    }

                    switch(tag)
                    {
                        case 0x00:
                            StatusMessage?.Invoke("Skipping NULL TLV");
                            break;
                        case 0x01:
                            StatusMessage?.Invoke("Skipping Lock Control TLV");
                            break;
                        case 0x02:
                            StatusMessage?.Invoke("Skipping Memory Control TLV");
                            break;
                        case 0xFE:
                            StatusMessage?.Invoke("Reached terminator TLV");
                            return;
                        case 0x03:
                            if (val != null)
                            {
                                StatusMessage?.Invoke("Found NDEF TLV (" + length + " bytes)");
                                NdefMessage msg = NdefMessage.FromByteArray(val);
                                ReceiveNdefMessage?.Invoke(msg);
                            }
                            else
                            {
                                StatusMessage?.Invoke("Found empty NDEF TLV");
                            }
                            break;
                        default:
                            StatusMessage?.Invoke("Skipping unknown TLV " + BitConverter.ToString(new byte[] { tag }));
                            break;
                    }
                }
            }
        }

        #endregion
    }
}
