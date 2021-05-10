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
            AdditionalData = new string[256];//128
        }
    }

    public class IncommingFile
    {
        BinaryReader br;
        string str_addition;
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
            //if (fs_input == null || br == null)
            //    return null;
            //List<classMessage> messages = new classMessage[1024];
            List<classMessage> messages = new List<classMessage>();
            int i = 0;
            try
            {
                while (fs_input != null && endOfFile == false)//???????????????????
                {
                    //messages[i++] = ReadOneMessageFromFileStream(filename);
                    messages.Add(ReadOneMessageFromFileStream(filename));
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
            switch (index)
            {
                case 0: //filename
                    message.DataElement[index] = filename;
                    break;
                case 1://DE2 PAN
                    string card_full = new string(br.ReadChars(int.Parse(new string(br.ReadChars(2)))));
                    message.DataElement[index] = card_full;
                    //2222222
                    //Datatable tbl_get_loc = new Datable();
                    //tbl_get_loc.Rows.Clear();
                    //get_loc = GetLOC_CardType_Branch(card_full);
                    //if (tbl_get_loc.Rows.Count > 0)
                    //{
                    //    foreach (DataRow row in tbl_get_loc.Rows)
                    //    {
                            
                    //    }

                    //}
                    //else
                    //{
                        
                    //}
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
                case 42://DE43 Card Acceptor Name/Location --hhhh
                    int leng = int.Parse(new string(br.ReadChars(2)));
                    string location =null;
                    for (int i = 0; i < leng; i++)
                    {
                        Byte temp = br.ReadByte();
                        UInt64 ascii = Convert.ToUInt64(temp);
                        if (ascii > 128)
                            location = location + " ";
                        else
                        {

                            location = location + Convert.ToChar(temp);
                        }
                    }
                   
                    message.DataElement[index] = location;
                    //hoannd change 15022017
                    //message.DataElement[index] = new string(br.ReadChars(int.Parse(new string(br.ReadChars(2)))));
                    break;
                case 47://DE48 Additional Data----------------field of header

                    //string s = new string(br.ReadChars(3));
                    //len = int.Parse(s);
                    //message.DataElement[index] = new string(br.ReadChars(len));


                    int length = int.Parse(new string(br.ReadChars(3)));
                    string Additional = null;
                    for (int i = 0; i < length; i++)
                    {
                        Byte temp = br.ReadByte();
                        UInt64 ascii = Convert.ToUInt64(temp);
                        if (ascii > 128)
                            Additional = Additional + " ";
                        else
                        {

                            Additional = Additional + Convert.ToChar(temp);
                        }
                    }            

                    message.DataElement[index] = Additional;
                    /////////////hhhh
                    if (message.MTI == "1240")
                    {
                        str_addition = message.DataElement[index];
                        ReadOneAdditionalDataByIndex(message, str_addition);
                    }
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
                    string s = new string(br.ReadChars(3));
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
        private bool ReadOneAdditionalDataByIndex(classMessage message, string s_message)
        {
            int len = 0;           
            int i = 0;
            while (i < s_message.Length)
            //while (i<82)
            {
                int s_index = int.Parse(s_message.Substring(i, 4));
                i=i+4;
                switch (s_index)
                {

                    case 0002://PDS 0002: GCMS Product
                        len=int.Parse(s_message.Substring(i, 3));
                        i=i+3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0003://PDS 0003: Licensed Product
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0005://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;

                    case 0006://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0023://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0025://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0026://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0042://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0043://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0044://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0052://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0056://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0057://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0058://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0059://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0071://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0072://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0080://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0105://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0110://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0122://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0137://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0140://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0141://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0145://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;                   
                    case 0146://hhh PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        string str_fee = s_message.Substring(i, len);
                        int deOrcr = int.Parse(str_fee.Substring(2, 2));
                        int temp = 2; //= int.Parse(str_fee.Substring(4, 2));
                        string cur_code = str_fee.Substring(6, 3);
                        string cur_code_Reconciliation = str_fee.Substring(21, 3);    
                        double amout_fee = double.Parse(str_fee.Substring(9, 12));
                        double amout_fee_Recon = double.Parse(str_fee.Substring(24, 12));

                        if (cur_code != "704")
                        {
                            amout_fee = double.Parse(str_fee.Substring(9, 12)) / Math.Pow(10, temp);
                            //amout_fee_Recon = double.Parse(str_fee.Substring(24, 12)) / Math.Pow(10, temp);
                        }
                        if (cur_code_Reconciliation != "704")
                        {
                            //amout_fee = double.Parse(str_fee.Substring(9, 12)) / Math.Pow(10, temp);
                            amout_fee_Recon = double.Parse(str_fee.Substring(24, 12)) / Math.Pow(10, temp);
                        }
                        string reversal_indicator = message.AdditionalData[0025];
                        if (reversal_indicator != null)
                        {
                            if (reversal_indicator.Substring(0,1) == "R")
                            {
                                if (deOrcr == 29)//29 la phi hoan tra (dau -), 19 la phi phai thu (dau +)
                                {
                                    amout_fee = amout_fee * -1;
                                    amout_fee_Recon = amout_fee_Recon * -1;
                                }

                            }
                            else
                            {
                                if (deOrcr == 19)//19 la phi hoan tra (dau -), 29 la phi phai thu (dau +)
                                {
                                    amout_fee = amout_fee * -1;
                                    amout_fee_Recon = amout_fee_Recon * -1;
                                }
                            }
                            //if (deOrcr == 29)//19 la phi hoan tra (dau -), 29 la phi phai thu (dau +)
                            //{
                            //    amout_fee = amout_fee * -1;
                            //    amout_fee_Recon = amout_fee_Recon * -1;
                            //}

                        }
                        else
                        {
                            if (deOrcr == 19)//19 la phi hoan tra (dau -), 29 la phi phai thu (dau +)
                            {
                                amout_fee = amout_fee * -1;
                                amout_fee_Recon = amout_fee_Recon * -1;
                            }
                        }


                        message.AdditionalData[150] = cur_code;//str_fee.Substring(6, 3);//lay PDS 150 chua currency code: 

                        message.AdditionalData[151] = amout_fee.ToString();//lay PDS 151 chua amount fee
                        //message.AdditionalData[151] = String.Format("{0:#,##0}", amout);

                        message.AdditionalData[152] = cur_code_Reconciliation;//lay PDS 152 chua currency code TQT: 

                        message.AdditionalData[153] = amout_fee_Recon.ToString();//lay PDS 153 chua amount fee TQT
                        //message.AdditionalData[153] = String.Format("{0:0,0.000}", amout2);

                        i = i + len;
                        break;
                    case 0147://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0148://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0149://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0157://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0158://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0159://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0160://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0164://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0165://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0170://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0171://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0172://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0173://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0174://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0175://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0176://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0177://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0178://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0179://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0180://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0181://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0188://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0189://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0190://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0191://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0192://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0194://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0195://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0196://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0197://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0198://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0199://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0200://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0202://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0204://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0205://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0206://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0207://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0208://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0209://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0210://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0211://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0212://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0213://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0214://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    case 0215://PDS
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;
                        message.AdditionalData[s_index] = s_message.Substring(i, len);
                        i = i + len;
                        break;
                    default:
                        len = int.Parse(s_message.Substring(i, 3));
                        i = i + 3;                        
                        i = i + len;
                        break;

                   
                    
                }
            }
            return true;
        }
        //private DataTable GetLOC_CardType_Branch(string pan_clear)
        //{
        //    DataTable cardData = new DataTable();
        //    OracleConnection conn = new OracleConnection();
        //    try
        //    {
        //        conn = OracleDBConnection.OpenConnection("CW_DW");
        //        OracleCommand cmd = new OracleCommand("Get_LOC_T112", conn);

        //        OracleParameter pan_p = new OracleParameter("pan", pan_clear);
        //        pan_p.Direction = ParameterDirection.Input;
        //        cmd.Parameters.Add(pan_p);

        //        cmd.CommandType = CommandType.StoredProcedure;
        //        OracleParameter sysCursor = new OracleParameter("sys_cursor", OracleType.Cursor);
        //        sysCursor.Direction = ParameterDirection.Output;
        //        cmd.Parameters.Add(sysCursor);

        //        OracleDataAdapter da = new OracleDataAdapter(cmd);
        //        da.Fill(cardData);
        //        conn.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        conn.Close();
        //        return null;
        //    }
        //    return cardData;
        //}
    }
}
