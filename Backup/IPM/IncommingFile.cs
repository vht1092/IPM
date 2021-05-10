using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace IPM
{
    public class classMessage
    {
        public string MTI;              // vd: 1644, 1240
        public string BitMap;
        public string[] DataElement;    // chua cac file chinh, vd: DE24, DE48
        public string[] AdditionalData; // chua cac file additional, vd: p0105, p0122

        public classMessage()
        {
            MTI = "";
            BitMap = "";
            DataElement = new string[128];
            AdditionalData = new string[128];
        }
    }

    public class IncommingFile
    {
        BinaryReader br;
        bool endOfFile = false;
        int REPLACE_SIZE = 1012;
        static string EXTENTION = ".Replaced";
        //static string STANDARD = "510235";
        //static string GOLD = "545579";

    
        private bool ReplaceAllAscii64(string filename)
        {
            FileStream fs_input = new FileStream(filename, FileMode.Open);
            FileStream fs_output = new FileStream(filename + EXTENTION, FileMode.Create);
            br = new BinaryReader(fs_input);
            BinaryWriter wr = new BinaryWriter(fs_output);

            endOfFile = false;
            if (fs_input == null || br == null)
                return false;

            int fileSize = int.Parse(fs_input.Length.ToString());
            //string temp = fs_input.
            byte[] inputdata = new byte[fileSize];
            byte[] outputdata = new byte[fileSize];
            inputdata = br.ReadBytes(int.Parse(fileSize.ToString()));

            br.Close();
            fs_input.Close();

            int i = 0;
            int index = 0;

            while (index + REPLACE_SIZE <= fileSize)
            {
                AppendByteArrayToByteArrayAtIndex(inputdata, index + i * "@@".Length, outputdata, index, REPLACE_SIZE);
                i++;
                index = i * REPLACE_SIZE;
            }
            AppendByteArrayToByteArrayAtIndex(inputdata, index + i * "@@".Length, outputdata, index, fileSize - (index + i * "@@".Length));         

            wr.Write(outputdata);

            wr.Close();
            fs_output.Close();

            return true;
        }

        private void AppendByteArrayToByteArrayAtIndex(byte[] b1, int index1, byte[] b2, int index2, int size)
        {
            for (long i = 0; i < size; i++)
            {
                //char t= Char.IsLetter(b1[i].ToString()); ///hhh
                if (index1 + i < b1.Length)
                    b2[index2 + i] = b1[index1 + i];
            }
        }

        private List<classMessage> ParseMessageT112(string filename)
        {
            FileStream fs_input = new FileStream(filename, FileMode.Open);
            br = new BinaryReader(fs_input);
            endOfFile = false;
            if (fs_input == null || br == null)
                return null;
            List<classMessage> messages = new classMessage[40000];
            //List<classMessage> messages = new List<classMessage>();
            int i = 0;
            try
            {
                while (fs_input != null && endOfFile == false)//???????????????????
                {
                    messages[i++] = ReadOneMessageFromFileStream(filename);
                    //messages.Add(ReadOneMessageFromFileStream(filename));
                }
            }
            catch(Exception ex)
            {
                ex.Message.ToString();
                //messages = null;
            }
            br.Close();
            fs_input.Close();
            return messages;
        }

        //public classMessage[] ParseMessages(string fileName, string rootPath)
        //{
        //    if (fileName.IndexOf(EXTENTION) < 0)
        //    {
        //        ReplaceAllAscii64(fileName);
        //        fileName = fileName + EXTENTION;
        //    }
        //    classMessage[] messages = ParseMessageT112(fileName);
        //    return messages;
        //}

        public List<classMessage> ParseMessages(string fileName, string rootPath)
        {
            if (fileName.IndexOf(EXTENTION) < 0)
            {
                //ReplaceAscii(fileName);//hhh
                ReplaceAllAscii64(fileName);
                fileName = fileName + EXTENTION;
            }
            List<classMessage> messages = ParseMessageT112(fileName);
            return messages;
        }

        private classMessage ReadOneMessageFromFileStream(string filename)
        {
            classMessage message = new classMessage();
            br.ReadBytes(4); // bo qua 4 byte dau message
            message.MTI = ReadMTI();
            if (message.MTI.Length == 4 && message.MTI.IndexOf('@') < 0)
            {
                message.BitMap = ReadStringBitMap();
                ReadDataElement(filename, message);
            }
            else
                endOfFile = true;// ket thuc file, stop read file
            return message;
        }

        private string ReadMTI()
        {
            char[] MTI = new char[4];
            MTI = br.ReadChars(4);
            return new string(MTI);
        }

        private string ReadStringBitMap()
        {
            byte[] priBitMap = new byte[8];
            priBitMap = br.ReadBytes(8);

            byte[] secondBitMap = new byte[8];
            secondBitMap = br.ReadBytes(8);// (secondBitMap, 0, 8);

            string sbitmap = "";
            for (int i = 0; i < 8; i++)
                for (int j = 1; j <= 8; j++)
                    if (((priBitMap[i] >> (8 - j)) & 1) == 1)
                        sbitmap += "1";
                    else
                        sbitmap += "0";
            for (int i = 0; i < 8; i++)
                for (int j = 1; j <= 8; j++)
                    if (((secondBitMap[i] >> (8 - j)) & 1) == 1)
                        sbitmap += "1";
                    else
                        sbitmap += "0";
            //br.Close();
            return sbitmap;
        }

        private bool ReadDataElement(string filename, classMessage message)
        {
            string bitmap = message.BitMap;
            for (int i = 0; i < 128; i++)
                if (bitmap.Substring(i, 1) == "1")
                {
                    ReadOneDataElementByIndex(filename, message, i);
                }
            return true;
        }

        private bool ReadOneDataElementByIndex(string filename, classMessage message, int index)
        {
            int len = 0;
            //string detail1 = "";
            //string detail2 = "";
            switch (index)
            {
                case 0: //filename
                    message.DataElement[index] = filename;
                    break;
                case 1://DE2 PAN
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(2)))));
                    break;
                case 2://DE3 Processing Code(3 sub field)
                    message.DataElement[index] = new string(br.ReadChars(6));//????????????????????? sau nay se xu ly chi tiet tung sub field
                    break;
                case 3://DE4 Amount Transaction
                    message.DataElement[index] = new string(br.ReadChars(12));
                    break;
                case 4://DE5 Amount Reconciliation
                    message.DataElement[index] = new string(br.ReadChars(12));
                    break;
                case 5://DE6 Amount Cardholder Billing
                    message.DataElement[index] = new string(br.ReadChars(12));
                    break;
                case 8://DE9 Conversion Rate, Reconciliation
                    message.DataElement[index] = new string(br.ReadChars(8));
                    break;
                case 9://DE10 Conversion Rate, Cardholder Billing
                    message.DataElement[index] = new string(br.ReadChars(8));
                    break;
                case 11://DE12 Date and Time, Local Transaction
                    message.DataElement[index] = new string(br.ReadChars(6)) + "-" + new string(br.ReadChars(6));
                    break;
                case 13://DE14 Date, Expiration
                    message.DataElement[index] = new string(br.ReadChars(4));
                    break;
                case 21://DE22 Point of Service Data Code----------------????????????????????????
                    message.DataElement[index] = new string(br.ReadChars(12));
                    break;
                case 22://DE23 Card Sequence Number
                    message.DataElement[index] = new string(br.ReadChars(3));
                    break;
                case 23://DE24 Function Code----------------field of header
                    message.DataElement[index] = new string(br.ReadChars(3));
                    break;
                case 24://DE25 Message Reason Code
                    message.DataElement[index] = new string(br.ReadChars(4));
                    break;
                case 25://DE26 Card Acceptor Business Code (MCC)
                    message.DataElement[index] = new string(br.ReadChars(4));
                    break;
                case 29://DE30  Amounts, Original ----------------------------????????????????????
                    message.DataElement[index] = new string(br.ReadChars(24));
                    break;
                case 30://DE31  Acquirer Reference Data--------------------------??????????????????
                    len = int.Parse(new string(br.ReadChars(2)));
                    message.DataElement[index] = new string(br.ReadChars(len));
                    break;
                case 31://DE32 Acquiring Institution ID Code
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(2)))));
                    break;
                case 32://DE33 Forwarding Institution ID Code
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(2)))));
                    break;
                case 36://DE37  Retrieval Reference Number
                    message.DataElement[index] = new string(br.ReadChars(12));
                    break;
                case 37://DE38 Approval Code
                    message.DataElement[index] = new string(br.ReadChars(6));
                    break;
                case 39://DE40 Service Code
                    message.DataElement[index] = new string(br.ReadChars(3));
                    break;
                case 40://DE41 Card Acceptor Terminal ID
                    message.DataElement[index] = new string(br.ReadChars(8));
                    break;
                case 41://DE42 Card Acceptor ID Code
                    message.DataElement[index] = new string(br.ReadChars(15));
                    break;
                case 42://DE43 Card Acceptor Name/Location -----------------------------???????????????????????????
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(2)))));
                    break;
                case 47://DE48 Additional Data----------------field of header
                    string s = new string(br.ReadChars(3));
                    len = int.Parse(s);
                    message.DataElement[index] = new string(br.ReadChars(len));
                    break;
                case 48://DE49 Currency Code, Transaction
                    message.DataElement[index] = new string(br.ReadChars(3));
                    break;
                case 49://DE50 Currency Code, Reconciliation
                    message.DataElement[index] = new string(br.ReadChars(3));
                    break;
                case 50://DE51 Currency Code, Cardholder Billing
                    message.DataElement[index] = new string(br.ReadChars(3));
                    break;
                case 53://DE54 Amounts, Additional ---------------?????????????????????????
                    len = int.Parse(new string(br.ReadChars(3)));
                    message.DataElement[index] = new string(br.ReadChars(len));
                    break;
                case 54://DE55 Integrated Circuit Card (ICC) System-Related Data---------binary data----------
                    len = int.Parse(new string(br.ReadChars(3)));
                    //message.DataElement[index] = 
                    //br.ReadChars(int.Parse(new string(br.ReadChars(3))));//.ToString();
                    br.ReadBytes(len);//.ToString();
                    break;
                case 61://DE62 Additional Data 2
                    len = int.Parse(new string(br.ReadChars(3)));
                    message.DataElement[index] = new string(br.ReadChars(len));
                    break;
                case 62://DE63 Transaction Life Cycle ID-------------??????????????????????????
                    string s1 = new string(br.ReadChars(3));
                    len = int.Parse(s1);
                    //len = int.Parse("16");
                    message.DataElement[index] = new string(br.ReadChars(len));
                    break;
                case 70://DE71 Message Number----------------field of header
                    message.DataElement[index] = new string(br.ReadChars(8));
                    break;
                case 71://DE72 Data Record
                    len = int.Parse(new string(br.ReadChars(3)));
                    message.DataElement[index] = new string(br.ReadChars(len));
                    break;
                case 72://D73 Date, Action
                    message.DataElement[index] = new string(br.ReadChars(6));
                    break;
                case 92://DE93 Transaction Destination Institution ID Code
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(2)))));
                    break;
                case 93://DE94 Transaction Originator Institution ID Code
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(2)))));
                    break;
                case 94://DE95 Card Issuer Reference Data
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(2)))));
                    break;
                case 99://DE100 Receiving Institution ID Code
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(2)))));
                    break;
                case 110://DE111 Amount, Currency Conversion Assessment
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(3)))));
                    break;
                case 122://DE123 Additional Data 3
                    s = new string(br.ReadChars(3));
                    len = int.Parse(s);
                    message.DataElement[index] = new string(br.ReadChars(len));
                    break;
                case 123://DE124 Additional Data 4
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(3)))));
                    break;
                case 124://DE125 Additional Data 5
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(3)))));
                    break;
                case 126://DE127 Network Data
                    message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(3)))));
                    break;
            }
            return true;
        }
    }
}
