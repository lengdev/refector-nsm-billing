using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;

namespace MVC.BarCode
{
    /// <summary>
    /// Summary description for clsBarCode.
    /// </summary>
    public class clsBarCode
    {
        private const int ASCII_START128_A = 203;

        private const int ASCII_START128_B = 204;

        private const int ASCII_START128_C = 205;

        private const int ASCII_FNC1_128 = 202;

        private const int ASCII_STOP128 = 206;

        private const int ASCII_SWITCH_TO_C = 199;

        private const int ASCII_SWITCH_TO_A = 201;

        private const int ASCII_SWITCH_TO_B = 200;

        private const int ASCII_SPACE_CHAR = 194;

        private const char CODABAR_START_CHAR = 'A';

        private const char CODABAR_STOP_CHAR = 'B';

        private const int ASCII_START_I2OF5 = 203;

        private const int ASCII_STOP_I2OF5 = 204;

        private const string DOUBLE_ZEROES = "00";

        private const int CHECK_DIGIT_MOD = 103;

        private Font lclFont;

        private string TextToPrint;

        public clsBarCode()
        {
        }

        public string Codabar(string InputValue)
        {
            string GoodData = "";
            int LenOfData = InputValue.Length;
            int idx;
            for (idx = 0; idx < LenOfData; idx++)
            {
                string CurrentChar = InputValue.Substring(idx, 1);
                if (CurrentChar == "$" || CurrentChar == "+" || CurrentChar == "-" || CurrentChar == "/" || CurrentChar == ":" || this.IsNumber(CurrentChar))
                {
                    GoodData = string.Concat(GoodData, CurrentChar);
                }
            }
            return string.Concat('A', GoodData, 'B');
        }

        /// <summary>
        /// This method will automatically generate the correct version of Code 128 bar code
        /// add switch between the different character sets.  It will take the Input string,
        /// add the start character, stop character, and check digit character and return the 
        /// completed string 
        /// </summary>
        /// <param name="InputValue">The value to barcode</param>
        /// <param name="InputValue">whether or not tildes are processed as special escape characters</param>
        /// <returns></returns>
        public string Code128(string InputValue, bool applyTilde)
        {
            bool flag;
            bool flag1;
            bool flag2;
            bool flag3;
            bool flag4;
            string EncodedData = "";
            char OutStartCharacter = ' ';
            clsBarCode.enuEncodingType EncodingType = clsBarCode.enuEncodingType.EncodingA;
            string ActualInputValue = !applyTilde ? InputValue : Code128TildeProcessing(InputValue);
            int LenDataToEncode = ActualInputValue.Length;
            int AscCharacter = ActualInputValue.ToCharArray(0, 1)[0];
            if (AscCharacter < 32)
            {
                OutStartCharacter = 'Ë';
                EncodingType = clsBarCode.enuEncodingType.EncodingA;
            }
            if ((AscCharacter > 31 && AscCharacter < 127) || AscCharacter == 197)
            {
                OutStartCharacter = 'Ì';
                EncodingType = clsBarCode.enuEncodingType.EncodingB;
            }
            if (LenDataToEncode > 4 && this.IsNumber(ActualInputValue.Substring(0, 4)))
            {
                OutStartCharacter = 'Í';
                EncodingType = clsBarCode.enuEncodingType.EncodingC;
            }
            flag = AscCharacter != 202 && (AscCharacter <= 211 || AscCharacter >= 218);
            if (!flag)
            {
                OutStartCharacter = 'Í';
                EncodingType = clsBarCode.enuEncodingType.EncodingC;
            }
            char OutStopCharacter = 'Î';
            int idx;
            for (idx = 0; idx < LenDataToEncode; idx++)
            {
                bool AsciiFnc1Found = false;
                AscCharacter = ActualInputValue.ToCharArray(idx, 1)[0];
                flag1 = AscCharacter != 202 && (AscCharacter <= 211 || AscCharacter >= 218);
                if (!flag1)
                {
                    AsciiFnc1Found = true;
                    EncodedData = string.Concat(EncodedData, 'Ê');
                }
                flag2 = (idx > LenDataToEncode - 4 || !this.IsNumber(ActualInputValue.Substring(idx, 1)) || !this.IsNumber(ActualInputValue.Substring(idx + 1, 1)) || !this.IsNumber(ActualInputValue.Substring(idx, 4))) && (idx >= LenDataToEncode - 1 || !this.IsNumber(ActualInputValue.Substring(idx, 1)) || !this.IsNumber(ActualInputValue.Substring(idx + 1, 1)) || EncodingType != clsBarCode.enuEncodingType.EncodingC || AsciiFnc1Found);
                if (flag2)
                {
                    flag3 = (idx > LenDataToEncode || AscCharacter >= 31) && (EncodingType != clsBarCode.enuEncodingType.EncodingA || AscCharacter <= 32 || AscCharacter >= 96);
                    if (flag3)
                    {
                        flag4 = idx > LenDataToEncode || ((AscCharacter <= 31 || AscCharacter >= 127) && AscCharacter != 197);
                        if (!flag4)
                        {
                            if (EncodingType != clsBarCode.enuEncodingType.EncodingB)
                            {
                                EncodedData = string.Concat(EncodedData, 'È');
                            }
                            EncodingType = clsBarCode.enuEncodingType.EncodingB;
                            AscCharacter = ActualInputValue.ToCharArray(idx, 1)[0];
                            EncodedData = AscCharacter != 32 ? string.Concat(EncodedData, (char)AscCharacter) : string.Concat(EncodedData, 'Â');
                        }
                    }
                    else
                    {
                        if (EncodingType != clsBarCode.enuEncodingType.EncodingA)
                        {
                            EncodedData = string.Concat(EncodedData, 'É');
                        }
                        EncodingType = clsBarCode.enuEncodingType.EncodingA;
                        AscCharacter = ActualInputValue.ToCharArray(idx, 1)[0];
                        if (AscCharacter == 32)
                        {
                            EncodedData = string.Concat(EncodedData, 'Â');
                        }
                        else if (AscCharacter < 32)
                        {
                            AscCharacter = AscCharacter + 96;
                            EncodedData = string.Concat(EncodedData, (char)AscCharacter);
                        }
                        else if (AscCharacter > 32)
                        {
                            EncodedData = string.Concat(EncodedData, (char)AscCharacter);
                        }
                    }
                }
                else
                {
                    if (EncodingType != clsBarCode.enuEncodingType.EncodingC)
                    {
                        EncodedData = string.Concat(EncodedData, 'Ç');
                    }
                    EncodingType = clsBarCode.enuEncodingType.EncodingC;
                    string CurrentChars = ActualInputValue.Substring(idx, 2);
                    AscCharacter = !(CurrentChars == "00") ? Convert.ToInt32(CurrentChars) : 194;
                    if (!(AscCharacter >= 95 || AscCharacter <= 0))
                    {
                        AscCharacter = AscCharacter + 32;
                        EncodedData = string.Concat(EncodedData, (char)AscCharacter);
                    }
                    else if (AscCharacter > 94)
                    {
                        if (AscCharacter != 194)
                        {
                            AscCharacter = AscCharacter + 100;
                        }
                        EncodedData = string.Concat(EncodedData, (char)AscCharacter);
                    }
                    else if (AscCharacter == 0)
                    {
                        EncodedData = string.Concat(EncodedData, 'Â');
                    }
                    idx++;
                }
            }
            int WeightedTotal = OutStartCharacter - 100;
            LenDataToEncode = EncodedData.Length;
            for (idx = 0; idx < LenDataToEncode; idx++)
            {
                AscCharacter = EncodedData.ToCharArray(idx, 1)[0];
                if (AscCharacter >= 135)
                {
                    if (AscCharacter == 194)
                    {
                        AscCharacter = 0;
                    }
                    if (AscCharacter > 134)
                    {
                        AscCharacter = AscCharacter - 100;
                    }
                }
                else
                {
                    AscCharacter = AscCharacter - 32;
                }
                AscCharacter = AscCharacter * (idx + 1);
                WeightedTotal = WeightedTotal + AscCharacter;
            }
            int iCheckDigitValue = WeightedTotal % 103;
            if (!(iCheckDigitValue >= 95 || iCheckDigitValue <= 0))
            {
                iCheckDigitValue = iCheckDigitValue + 32;
            }
            else if (iCheckDigitValue > 94)
            {
                iCheckDigitValue = iCheckDigitValue + 100;
            }
            else if (iCheckDigitValue == 0)
            {
                iCheckDigitValue = 194;
            }
            char OutCheckCharacter = (char)iCheckDigitValue;
            object[] objArray = new object[] { OutStartCharacter, EncodedData, OutCheckCharacter, OutStopCharacter };
            EncodedData = string.Concat(objArray);
            return EncodedData;
        }

        public string Code128A(string InputValue)
        {
            int iCurrentChar;
            string EncodedData = "";
            char Code128AStartChar = 'Ë';
            char Code128AStopChar = 'Î';
            int WeightedTotal = 103;
            int LenDataToEncode = InputValue.Length;
            char[] InputData = InputValue.ToCharArray();
            int idx;
            for (idx = 0; idx < LenDataToEncode; idx++)
            {
                iCurrentChar = InputData[idx];
                if (iCurrentChar < 135)
                {
                    iCurrentChar = iCurrentChar - 32;
                }
                else if (iCurrentChar > 134)
                {
                    iCurrentChar = iCurrentChar - 100;
                }
                WeightedTotal = WeightedTotal + (iCurrentChar * (idx + 1));
            }
            iCurrentChar = WeightedTotal % 103;
            if (!(iCurrentChar >= 95 || iCurrentChar <= 0))
            {
                iCurrentChar = iCurrentChar + 32;
            }
            else if (iCurrentChar > 94)
            {
                iCurrentChar = iCurrentChar + 100;
            }
            else if (iCurrentChar == 0)
            {
                iCurrentChar = 194;
            }
            char Code128ACheckDigit = (char)iCurrentChar;
            for (idx = 0; idx < LenDataToEncode; idx++)
            {
                EncodedData = InputData[idx] != ' ' ? string.Concat(EncodedData, InputData[idx]) : string.Concat(EncodedData, 'Â');
            }
            object[] objArray = new object[] { Code128AStartChar, EncodedData, Code128ACheckDigit, Code128AStopChar };
            EncodedData = string.Concat(objArray);
            return EncodedData;
        }

        public string Code128B(string InputValue)
        {
            int iCurrentChar;
            string EncodedData = "";
            char Code128BStartChar = 'Ì';
            char Code128BStopChar = 'Î';
            int WeightedTotal = 104;
            int LenDataToEncode = InputValue.Length;
            char[] InputData = InputValue.ToCharArray();
            int idx;
            for (idx = 0; idx < LenDataToEncode; idx++)
            {
                iCurrentChar = InputData[idx];
                if (iCurrentChar < 135)
                {
                    iCurrentChar = iCurrentChar - 32;
                }
                else if (iCurrentChar > 134)
                {
                    iCurrentChar = iCurrentChar - 100;
                }
                WeightedTotal = WeightedTotal + (iCurrentChar * (idx + 1));
            }
            iCurrentChar = WeightedTotal % 103;
            if (!(iCurrentChar >= 95 || iCurrentChar <= 0))
            {
                iCurrentChar = iCurrentChar + 32;
            }
            else if (iCurrentChar > 94)
            {
                iCurrentChar = iCurrentChar + 100;
            }
            else if (iCurrentChar == 0)
            {
                iCurrentChar = 194;
            }
            char Code128BCheckDigit = (char)iCurrentChar;
            for (idx = 0; idx < LenDataToEncode; idx++)
            {
                EncodedData = InputData[idx] != ' ' ? string.Concat(EncodedData, InputData[idx]) : string.Concat(EncodedData, 'Â');
            }
            object[] objArray = new object[] { Code128BStartChar, EncodedData, Code128BCheckDigit, Code128BStopChar };
            EncodedData = string.Concat(objArray);
            return EncodedData;
        }

        public string Code128C(string InputValue)
        {
            int iCurrentChars;
            string EncodedData = "";
            string GoodData = "";
            char Code128CStartChar = 'Í';
            char Code128CStopChar = 'Î';
            int WeightedTotal = 105;
            int LenDataToEncode = InputValue.Length;
            char[] InputData = InputValue.ToCharArray();
            int idx;
            for (idx = 0; idx < LenDataToEncode; idx++)
            {
                if (char.IsNumber(InputData[idx]))
                {
                    GoodData = string.Concat(GoodData, InputData[idx]);
                }
                else if (InputData[idx] != '-')
                {
                }
            }
            if (GoodData.Length % 2 != 0)
            {
                GoodData = string.Concat("0", GoodData);
            }
            LenDataToEncode = GoodData.Length;
            int WeightFactor = 1;
            for (idx = 0; idx < LenDataToEncode; idx = idx + 2)
            {
                iCurrentChars = Convert.ToInt32(GoodData.Substring(idx, 2));
                WeightedTotal = WeightedTotal + (iCurrentChars * WeightFactor);
                WeightFactor++;
                if (!(iCurrentChars >= 95 || iCurrentChars <= 0))
                {
                    iCurrentChars = iCurrentChars + 32;
                }
                else if (iCurrentChars > 94)
                {
                    iCurrentChars = iCurrentChars + 100;
                }
                else if (iCurrentChars == 0)
                {
                    iCurrentChars = 194;
                }
                EncodedData = string.Concat(EncodedData, (char)iCurrentChars);
            }
            iCurrentChars = WeightedTotal % 103;
            if (!(iCurrentChars >= 95 || iCurrentChars <= 0))
            {
                iCurrentChars = iCurrentChars + 32;
            }
            else if (iCurrentChars > 94)
            {
                iCurrentChars = iCurrentChars + 100;
            }
            else if (iCurrentChars == 0)
            {
                iCurrentChars = 194;
            }
            char Code128CCheckDigit = (char)iCurrentChars;
            object[] objArray = new object[] { Code128CStartChar, EncodedData, Code128CCheckDigit, Code128CStopChar };
            EncodedData = string.Concat(objArray);
            return EncodedData;
        }

        /// <summary>
        /// This method will convert data encoded for Code128 and return the human readable form
        /// of the code 
        /// </summary>
        /// <param name="InputValue">The value that is to be barcoded</param>
        /// <param name="applyTilde">Determines if tilde characters should be interpreted as escape sequence characters</param>
        /// <returns></returns>
        public string Code128HR(string InputValue, bool applyTilde)
        {
            bool flag;
            bool flag1;
            bool flag2;
            StringBuilder sbReturnString = new StringBuilder();
            string ActualInputValue = !applyTilde ? InputValue : Code128TildeProcessing(InputValue);
            char[] charDataToEncode = ActualInputValue.ToCharArray();
            for (int i = 0; i < charDataToEncode.Length; i++)
            {
                bool FNCDone = false;
                if (!(i > ActualInputValue.Length - 3 || charDataToEncode[i] != 'Ô'))
                {
                    sbReturnString.Append(string.Concat(" (", ActualInputValue.Substring(i + 1, 2), ") "));
                    i = i + 2;
                }
                else if (!(i > ActualInputValue.Length - 4 || charDataToEncode[i] != 'Õ'))
                {
                    sbReturnString.Append(string.Concat(" (", ActualInputValue.Substring(i + 1, 3), ") "));
                    i = i + 3;
                }
                else if (!(i > ActualInputValue.Length - 5 || charDataToEncode[i] != 'Ö'))
                {
                    sbReturnString.Append(string.Concat(" (", ActualInputValue.Substring(i + 1, 4), ") "));
                    i = i + 4;
                }
                else if (!(i > ActualInputValue.Length - 6 || charDataToEncode[i] != '×'))
                {
                    sbReturnString.Append(string.Concat(" (", ActualInputValue.Substring(i + 1, 5), ") "));
                    i = i + 5;
                }
                else if (!(i > ActualInputValue.Length - 7 || charDataToEncode[i] != 'Ø'))
                {
                    sbReturnString.Append(string.Concat(" (", ActualInputValue.Substring(i + 1, 6), ") "));
                    i = i + 6;
                }
                else if (!(i > ActualInputValue.Length - 8 || charDataToEncode[i] != 'Ù'))
                {
                    sbReturnString.Append(string.Concat(" (", ActualInputValue.Substring(i + 1, 7), ") "));
                    i = i + 7;
                }
                else if (!(i > ActualInputValue.Length - 4 || charDataToEncode[i] != 'Ê' || !this.IsNumber(ActualInputValue.Substring(i + 1, 1)) || !this.IsNumber(ActualInputValue.Substring(i + 2, 1))))
                {
                    try
                    {
                        int Next2CharacterValue = Convert.ToInt32(ActualInputValue.Substring(i + 1, 2));
                        flag = Next2CharacterValue != 80 && Next2CharacterValue != 81 && (Next2CharacterValue > 34 || Next2CharacterValue < 31);
                        if (!flag)
                        {
                            sbReturnString.Append(string.Concat(" (", ActualInputValue.Substring(i + 1, 4), ") "));
                            i = i + 4;
                            FNCDone = true;
                        }
                        if (!FNCDone)
                        {
                            flag2 = (Next2CharacterValue > 49 || Next2CharacterValue < 40) && (Next2CharacterValue > 25 || Next2CharacterValue < 23);
                            if (!flag2)
                            {
                                sbReturnString.Append(string.Concat(" (", ActualInputValue.Substring(i + 1, 3), ") "));
                                i = i + 3;
                                FNCDone = true;
                            }
                        }
                        if (!FNCDone)
                        {
                            flag1 = (Next2CharacterValue > 30 || Next2CharacterValue < 0) && (Next2CharacterValue > 99 || Next2CharacterValue < 90);
                            if (!flag1)
                            {
                                sbReturnString.Append(string.Concat(" (", ActualInputValue.Substring(i + 1, 2), ") "));
                                i = i + 2;
                                FNCDone = true;
                            }
                        }
                        if (!FNCDone)
                        {
                            sbReturnString.Append(string.Concat(" (", ActualInputValue.Substring(i + 1, 4), ") "));
                            i = i + 4;
                            FNCDone = true;
                        }
                    }
                    catch
                    {
                        sbReturnString.Append(ActualInputValue.Substring(i, 1));
                    }
                }
                else if (!(charDataToEncode[i] < ' ' || charDataToEncode[i] > '~'))
                {
                    sbReturnString.Append(ActualInputValue.Substring(i, 1));
                }
                else if (charDataToEncode[i] < ' ')
                {
                    sbReturnString.Append(" ");
                }
            }
            return sbReturnString.ToString();
        }

        /// <summary>
        /// Interprets tilde characters for special processing.  
        /// </summary>
        /// <param name="InputValue"></param>
        /// <returns></returns>
        private string Code128TildeProcessing(string InputValue)
        {
            int tempValue;
            string ActualInputValue = "";
            int LenDataToEncode = InputValue.Length;
            for (int idx = 0; idx < LenDataToEncode; idx++)
            {
                if (!InputValue.Substring(idx, 1).Equals("~") || LenDataToEncode - idx <= 3)
                {
                    ActualInputValue = string.Concat(ActualInputValue, InputValue.Substring(idx, 1));
                }
                else if (InputValue.Substring(idx + 1, 1).Equals("m"))
                {
                    try
                    {
                        tempValue = Convert.ToInt32(InputValue.Substring(idx + 2, 2));
                        if (tempValue <= 0 || tempValue > idx)
                        {
                            ActualInputValue = string.Concat(ActualInputValue, InputValue.Substring(idx, 1));
                        }
                        else
                        {
                            string StringToCheck = InputValue.Substring(0, idx);
                            string StringToCalculate = "";
                            for (int jdx = StringToCheck.Length; jdx >= 0; jdx--)
                            {
                                if (this.IsNumber(StringToCheck.Substring(jdx - 1, 1)))
                                {
                                    StringToCalculate = string.Concat(StringToCalculate, StringToCheck.Substring(jdx - 1, 1));
                                    if (StringToCalculate.Length == tempValue)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (StringToCalculate.Length != tempValue)
                            {
                                ActualInputValue = string.Concat(ActualInputValue, InputValue.Substring(idx, 1));
                            }
                            else
                            {
                                StringBuilder revString = new StringBuilder();
                                for (int rev = StringToCalculate.Length; rev > 0; rev--)
                                {
                                    revString.Append(StringToCalculate.Substring(rev - 1, 1));
                                }
                                ActualInputValue = string.Concat(ActualInputValue, this.FindMod10Digit(revString.ToString()));
                                idx = idx + 3;
                            }
                        }
                    }
                    catch
                    {
                        ActualInputValue = string.Concat(ActualInputValue, InputValue.Substring(idx, 1));
                    }
                }
                else if (!InputValue.Substring(idx + 1, 1).Equals("~"))
                {
                    try
                    {
                        tempValue = Convert.ToInt32(InputValue.Substring(idx + 1, 3));
                        if (tempValue <= 0 || tempValue > 255)
                        {
                            ActualInputValue = string.Concat(ActualInputValue, InputValue.Substring(idx, 1));
                        }
                        else
                        {
                            char chr = (char)tempValue;
                            ActualInputValue = string.Concat(ActualInputValue, chr.ToString());
                            idx = idx + 3;
                        }
                    }
                    catch
                    {
                        ActualInputValue = string.Concat(ActualInputValue, InputValue.Substring(idx, 1));
                    }
                }
                else
                {
                    ActualInputValue = string.Concat(ActualInputValue, InputValue.Substring(idx, 1));
                    idx++;
                }
            }
            return ActualInputValue;
        }

        public string Code39(string InputValue)
        {
            int LenOfData = InputValue.Length;
            InputValue = InputValue.Replace(" ", "=");
            return string.Concat("!", InputValue, "!");
        }

        public string Code39mod43(string InputValue)
        {
            string OutData = "";
            int LenOfData = InputValue.Length;
            InputValue = InputValue.ToUpper();
            char[] cInputValue = InputValue.ToCharArray();
            int idx;
            int iCurrentChar;
            for (idx = 0; idx < LenOfData; idx++)
            {
                iCurrentChar = cInputValue[idx];
                if (!(iCurrentChar >= 58 || iCurrentChar <= 47))
                {
                    OutData = string.Concat(OutData, cInputValue[idx]);
                }
                else if (!(iCurrentChar >= 91 || iCurrentChar <= 64))
                {
                    OutData = string.Concat(OutData, cInputValue[idx]);
                }
                else if (iCurrentChar == 32)
                {
                    OutData = string.Concat(OutData, cInputValue[idx]);
                }
                else if (iCurrentChar == 45)
                {
                    OutData = string.Concat(OutData, cInputValue[idx]);
                }
                else if (iCurrentChar == 46)
                {
                    OutData = string.Concat(OutData, cInputValue[idx]);
                }
                else if (iCurrentChar == 36)
                {
                    OutData = string.Concat(OutData, cInputValue[idx]);
                }
                else if (iCurrentChar == 47)
                {
                    OutData = string.Concat(OutData, cInputValue[idx]);
                }
                else if (iCurrentChar == 43)
                {
                    OutData = string.Concat(OutData, cInputValue[idx]);
                }
                else if (iCurrentChar == 37)
                {
                    OutData = string.Concat(OutData, cInputValue[idx]);
                }
            }

            int WeightedTotal = 0;
            LenOfData = OutData.Length;
            cInputValue = OutData.ToCharArray();
            for (idx = 0; idx < LenOfData; idx++)
            {
                iCurrentChar = cInputValue[idx];
                if (!(iCurrentChar >= 58 || iCurrentChar <= 47))
                {
                    iCurrentChar = iCurrentChar - 48;
                }
                else if (!(iCurrentChar >= 91 || iCurrentChar <= 64))
                {
                    iCurrentChar = iCurrentChar - 55;
                }
                else if (iCurrentChar == 32)
                {
                    iCurrentChar = 38;
                }
                else if (iCurrentChar == 45)
                {
                    iCurrentChar = 36;
                }
                else if (iCurrentChar == 46)
                {
                    iCurrentChar = 37;
                }
                else if (iCurrentChar == 36)
                {
                    iCurrentChar = 39;
                }
                else if (iCurrentChar == 47)
                {
                    iCurrentChar = 40;
                }
                else if (iCurrentChar == 43)
                {
                    iCurrentChar = 41;
                }
                else if (iCurrentChar == 37)
                {
                    iCurrentChar = 42;
                }
                else if (iCurrentChar == 32)
                {
                    iCurrentChar = 61;
                }
                WeightedTotal = WeightedTotal + iCurrentChar;
            }

            int CheckDigit = WeightedTotal % 43;
            OutData = OutData.Replace(" ", "=");
            if (CheckDigit < 10)
            {
                CheckDigit = CheckDigit + 48;
            }
            else if (!(CheckDigit >= 36 || CheckDigit <= 9))
            {
                CheckDigit = CheckDigit + 55;
            }
            else if (CheckDigit == 38)
            {
                CheckDigit = 61;
            }
            else if (CheckDigit == 36)
            {
                CheckDigit = 45;
            }
            else if (CheckDigit == 37)
            {
                CheckDigit = 46;
            }
            else if (CheckDigit == 39)
            {
                CheckDigit = 36;
            }
            else if (CheckDigit == 40)
            {
                CheckDigit = 47;
            }
            else if (CheckDigit == 41)
            {
                CheckDigit = 43;
            }
            else if (CheckDigit == 42)
            {
                CheckDigit = 37;
            }
            object[] objArray = new object[] { "!", OutData, (char)CheckDigit, "!" };
            OutData = string.Concat(objArray);
            return OutData;
        }

        public string EAN13(string InputValue)
        {
            string OutData = "";
            string GoodData = "";
            string EAN5AddOn = "";
            string EAN2AddOn = "";
            string Encoding = "";
            string EANAddOnToPrint = "";
            int iEANaddchar = 0;
            int LenOfData = InputValue.Length;
            int idx;
            for (idx = 0; idx < LenOfData; idx++)
            {
                if (this.IsNumber(InputValue.Substring(idx, 1)))
                {
                    GoodData = string.Concat(GoodData, InputValue.Substring(idx, 1));
                }
            }
            LenOfData = GoodData.Length;
            if (LenOfData == 13)
            {
                GoodData = GoodData.Substring(0, 12);
            }
            else if (LenOfData == 15)
            {
                GoodData = string.Concat(GoodData.Substring(0, 12), GoodData.Substring(13, 2));
            }
            else if (LenOfData == 18)
            {
                GoodData = string.Concat(GoodData.Substring(0, 12), GoodData.Substring(13, 5));
            }
            if (GoodData.Length == 17)
            {
                EAN5AddOn = GoodData.Substring(12, 5);
            }
            else if (GoodData.Length == 14)
            {
                EAN2AddOn = GoodData.Substring(12, 2);
            }
            GoodData = GoodData.Substring(0, 12);
            int WeightFactor = 3;
            int WeightedTotal = 0;
            LenOfData = GoodData.Length;
            char[] cInputValue = GoodData.ToCharArray();
            int iCurrentChar;
            for (idx = LenOfData - 1; idx >= 0; idx--)
            {
                iCurrentChar = cInputValue[idx] - 48;
                WeightedTotal = WeightedTotal + (iCurrentChar * WeightFactor);
                WeightFactor = 4 - WeightFactor;
            }

            int CheckDigit = WeightedTotal % 10;
            CheckDigit = CheckDigit == 0 ? 0 : 10 - CheckDigit;
            cInputValue = GoodData.ToCharArray();
            int LeadingDigit = Convert.ToInt32(GoodData.Substring(0, 1));
            int num = LeadingDigit;
            switch (num)
            {
                case 0:
                    {
                        Encoding = "AAAAAACCCCCC";
                        break;
                    }
                case 1:
                    {
                        Encoding = "AABABBCCCCCC";
                        break;
                    }
                case 2:
                    {
                        Encoding = "AABBABCCCCCC";
                        break;
                    }
                case 3:
                    {
                        Encoding = "AABBBACCCCCC";
                        break;
                    }
                case 4:
                    {
                        Encoding = "ABAABBCCCCCC";
                        break;
                    }
                case 5:
                    {
                        Encoding = "ABBAABCCCCCC";
                        break;
                    }
                case 6:
                    {
                        Encoding = "ABBBAACCCCCC";
                        break;
                    }
                case 7:
                    {
                        Encoding = "ABABABCCCCCC";
                        break;
                    }
                case 8:
                    {
                        Encoding = "ABABBACCCCCC";
                        break;
                    }
                case 9:
                    {
                        Encoding = "ABBABACCCCCC";
                        break;
                    }
            }
            GoodData = string.Concat(GoodData.Substring(1, 11), CheckDigit.ToString());
            LenOfData = GoodData.Length;
            cInputValue = GoodData.ToCharArray();
            string CurrentEncoding;
            string temp;
            for (idx = 0; idx < LenOfData; idx++)
            {
                iCurrentChar = cInputValue[idx];
                CurrentEncoding = Encoding.Substring(idx, 1);
                string str = CurrentEncoding;
                if (str != null)
                {
                    if (str == "A")
                    {
                        OutData = string.Concat(OutData, cInputValue[idx]);
                        goto Label0;
                    }
                    else if (str == "B")
                    {
                        iCurrentChar = iCurrentChar + 17;
                        OutData = string.Concat(OutData, (char)iCurrentChar);
                        goto Label0;
                    }
                    else
                    {
                        if (str != "C")
                        {
                            goto Label0;
                        }
                        iCurrentChar = iCurrentChar + 27;
                        OutData = string.Concat(OutData, (char)iCurrentChar);
                        goto Label0;
                    }
                }
            Label0:
                num = idx;
                if (num != 0)
                {
                    if (num == 5)
                    {
                        OutData = string.Concat(OutData, "*");
                    }
                    else if (num == 11)
                    {
                        OutData = string.Concat(OutData, "(");
                    }
                }
                else if (LeadingDigit > 4)
                {
                    temp = LeadingDigit.ToString();
                    iCurrentChar = temp.ToCharArray()[0];
                    iCurrentChar = iCurrentChar + 64;
                    OutData = string.Concat((char)iCurrentChar, "(", OutData);
                }
                else if (LeadingDigit < 5)
                {
                    temp = LeadingDigit.ToString();
                    iCurrentChar = temp.ToCharArray()[0];
                    iCurrentChar = iCurrentChar + 37;
                    OutData = string.Concat((char)iCurrentChar, "(", OutData);
                }
            }
            if (EAN5AddOn.Length == 5)
            {
                WeightFactor = 3;
                WeightedTotal = 0;
                for (idx = EAN5AddOn.Length - 1; idx >= 0; idx--)
                {
                    iCurrentChar = Convert.ToInt32(EAN5AddOn.Substring(idx, 1));
                    if (WeightFactor == 3)
                    {
                        WeightedTotal = WeightedTotal + (iCurrentChar * 3);
                    }
                    if (WeightFactor == 1)
                    {
                        WeightedTotal = WeightedTotal + (iCurrentChar * 9);
                    }
                    WeightFactor = 4 - WeightFactor;
                }
                temp = WeightedTotal.ToString();
                CheckDigit = Convert.ToInt32(temp.Substring(temp.Length - 1, 1));
                num = CheckDigit;
                switch (num)
                {
                    case 0:
                        {
                            Encoding = "BBAAA";
                            break;
                        }
                    case 1:
                        {
                            Encoding = "BABAA";
                            break;
                        }
                    case 2:
                        {
                            Encoding = "BAABA";
                            break;
                        }
                    case 3:
                        {
                            Encoding = "BAAAB";
                            break;
                        }
                    case 4:
                        {
                            Encoding = "ABBAA";
                            break;
                        }
                    case 5:
                        {
                            Encoding = "AABBA";
                            break;
                        }
                    case 6:
                        {
                            Encoding = "AAABB";
                            break;
                        }
                    case 7:
                        {
                            Encoding = "ABABA";
                            break;
                        }
                    case 8:
                        {
                            Encoding = "ABAAB";
                            break;
                        }
                    case 9:
                        {
                            Encoding = "AABAB";
                            break;
                        }
                }
                LenOfData = EAN5AddOn.Length;
                EANAddOnToPrint = "";
                for (idx = 0; idx < LenOfData; idx++)
                {
                    iCurrentChar = Convert.ToInt32(EAN5AddOn.Substring(idx, 1));
                    CurrentEncoding = Encoding.Substring(idx, 1);
                    if (CurrentEncoding == "A")
                    {
                        num = iCurrentChar;
                        switch (num)
                        {
                            case 0:
                                {
                                    iEANaddchar = 34;
                                    break;
                                }
                            case 1:
                                {
                                    iEANaddchar = 35;
                                    break;
                                }
                            case 2:
                                {
                                    iEANaddchar = 36;
                                    break;
                                }
                            case 3:
                                {
                                    iEANaddchar = 37;
                                    break;
                                }
                            case 4:
                                {
                                    iEANaddchar = 38;
                                    break;
                                }
                            case 5:
                                {
                                    iEANaddchar = 44;
                                    break;
                                }
                            case 6:
                                {
                                    iEANaddchar = 46;
                                    break;
                                }
                            case 7:
                                {
                                    iEANaddchar = 47;
                                    break;
                                }
                            case 8:
                                {
                                    iEANaddchar = 58;
                                    break;
                                }
                            case 9:
                                {
                                    iEANaddchar = 59;
                                    break;
                                }
                        }
                        EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iEANaddchar);
                    }
                    else if (CurrentEncoding == "B")
                    {
                        num = iCurrentChar;
                        switch (num)
                        {
                            case 0:
                                {
                                    iEANaddchar = 122;
                                    break;
                                }
                            case 1:
                                {
                                    iEANaddchar = 61;
                                    break;
                                }
                            case 2:
                                {
                                    iEANaddchar = 63;
                                    break;
                                }
                            case 3:
                                {
                                    iEANaddchar = 64;
                                    break;
                                }
                            case 4:
                                {
                                    iEANaddchar = 91;
                                    break;
                                }
                            case 5:
                                {
                                    iEANaddchar = 92;
                                    break;
                                }
                            case 6:
                                {
                                    iEANaddchar = 93;
                                    break;
                                }
                            case 7:
                                {
                                    iEANaddchar = 95;
                                    break;
                                }
                            case 8:
                                {
                                    iEANaddchar = 123;
                                    break;
                                }
                            case 9:
                                {
                                    iEANaddchar = 125;
                                    break;
                                }
                        }
                        EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iEANaddchar);
                    }
                    num = idx;
                    switch (num)
                    {
                        case 0:
                            {
                                iEANaddchar = 43;
                                EANAddOnToPrint = string.Concat((char)iEANaddchar, EANAddOnToPrint);
                                iEANaddchar = 32;
                                EANAddOnToPrint = string.Concat((char)iEANaddchar, EANAddOnToPrint);
                                iEANaddchar = 33;
                                EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iEANaddchar);
                                break;
                            }
                        case 1:
                            {
                                iEANaddchar = 33;
                                EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iEANaddchar);
                                break;
                            }
                        case 2:
                            {
                                iEANaddchar = 33;
                                EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iEANaddchar);
                                break;
                            }
                        case 3:
                            {
                                iEANaddchar = 33;
                                EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iEANaddchar);
                                break;
                            }
                    }
                }
            }
            if (EAN2AddOn.Length == 2)
            {
                iEANaddchar = Convert.ToInt32(EAN2AddOn);
                for (idx = 0; idx <= 99; idx = idx + 4)
                {
                    if (iEANaddchar == idx)
                    {
                        Encoding = "AA";
                    }
                    else if (iEANaddchar == idx + 1)
                    {
                        Encoding = "AB";
                    }
                    else if (iEANaddchar == idx + 2)
                    {
                        Encoding = "BA";
                    }
                    else if (iEANaddchar == idx + 3)
                    {
                        Encoding = "BB";
                    }
                }
                LenOfData = EAN2AddOn.Length;
                for (idx = 0; idx < LenOfData; idx++)
                {
                    iCurrentChar = Convert.ToInt32(EAN2AddOn.Substring(idx, 1));
                    CurrentEncoding = Encoding.Substring(idx, 1);
                    if (CurrentEncoding == "A")
                    {
                        num = iCurrentChar;
                        switch (num)
                        {
                            case 0:
                                {
                                    iEANaddchar = 34;
                                    break;
                                }
                            case 1:
                                {
                                    iEANaddchar = 35;
                                    break;
                                }
                            case 2:
                                {
                                    iEANaddchar = 36;
                                    break;
                                }
                            case 3:
                                {
                                    iEANaddchar = 37;
                                    break;
                                }
                            case 4:
                                {
                                    iEANaddchar = 38;
                                    break;
                                }
                            case 5:
                                {
                                    iEANaddchar = 44;
                                    break;
                                }
                            case 6:
                                {
                                    iEANaddchar = 46;
                                    break;
                                }
                            case 7:
                                {
                                    iEANaddchar = 47;
                                    break;
                                }
                            case 8:
                                {
                                    iEANaddchar = 58;
                                    break;
                                }
                            case 9:
                                {
                                    iEANaddchar = 59;
                                    break;
                                }
                        }
                        EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iEANaddchar);
                    }
                    else if (CurrentEncoding == "B")
                    {
                        num = iCurrentChar;
                        switch (num)
                        {
                            case 0:
                                {
                                    iEANaddchar = 122;
                                    break;
                                }
                            case 1:
                                {
                                    iEANaddchar = 61;
                                    break;
                                }
                            case 2:
                                {
                                    iEANaddchar = 63;
                                    break;
                                }
                            case 3:
                                {
                                    iEANaddchar = 64;
                                    break;
                                }
                            case 4:
                                {
                                    iEANaddchar = 91;
                                    break;
                                }
                            case 5:
                                {
                                    iEANaddchar = 92;
                                    break;
                                }
                            case 6:
                                {
                                    iEANaddchar = 93;
                                    break;
                                }
                            case 7:
                                {
                                    iEANaddchar = 95;
                                    break;
                                }
                            case 8:
                                {
                                    iEANaddchar = 123;
                                    break;
                                }
                            case 9:
                                {
                                    iEANaddchar = 125;
                                    break;
                                }
                        }
                        EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iEANaddchar);
                    }
                    if (idx == 0)
                    {
                        iEANaddchar = 43;
                        EANAddOnToPrint = string.Concat((char)iEANaddchar, EANAddOnToPrint);
                        iEANaddchar = 33;
                        EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iEANaddchar);
                    }
                }
            }
            OutData = string.Concat(OutData, EANAddOnToPrint);
            return OutData;
        }

        public string EAN8(string InputValue)
        {
            string str;
            string OutData = "";
            string GoodData = "";
            int LenOfData = InputValue.Length;
            int idx;
            for (idx = 0; idx < LenOfData; idx++)
            {
                if (this.IsNumber(InputValue.Substring(idx, 1)))
                {
                    GoodData = string.Concat(GoodData, InputValue.Substring(idx, 1));
                }
            }
            if (GoodData.Length == 7)
            {
                int WeightFactor = 3;
                int WeightedTotal = 0;
                LenOfData = GoodData.Length;
                char[] cInputValue = GoodData.ToCharArray();
                int iCurrentChar;
                for (idx = LenOfData - 1; idx >= 0; idx--)
                {
                    iCurrentChar = cInputValue[idx];
                    WeightedTotal = WeightedTotal + (iCurrentChar * WeightFactor);
                    WeightFactor = 4 - WeightFactor;
                }

                int CheckDigit = WeightedTotal % 10;
                CheckDigit = CheckDigit == 0 ? 0 : 10 - CheckDigit;
                GoodData = string.Concat(GoodData, CheckDigit.ToString());
                LenOfData = GoodData.Length;
                for (idx = 0; idx < LenOfData; idx++)
                {
                    cInputValue = GoodData.ToCharArray();
                    iCurrentChar = cInputValue[idx];
                    switch (idx + 1)
                    {
                        case 1:
                            {
                                OutData = string.Concat(OutData, "(", (char)iCurrentChar);
                                break;
                            }
                        case 2:
                            {
                                OutData = string.Concat(OutData, (char)iCurrentChar);
                                break;
                            }
                        case 3:
                            {
                                OutData = string.Concat(OutData, (char)iCurrentChar);
                                break;
                            }
                        case 4:
                            {
                                OutData = string.Concat(OutData, (char)iCurrentChar, "*");
                                break;
                            }
                        case 5:
                            {
                                OutData = string.Concat(OutData, (char)(iCurrentChar + 27));
                                break;
                            }
                        case 6:
                            {
                                OutData = string.Concat(OutData, (char)(iCurrentChar + 27));
                                break;
                            }
                        case 7:
                            {
                                OutData = string.Concat(OutData, (char)(iCurrentChar + 27));
                                break;
                            }
                        case 8:
                            {
                                OutData = string.Concat(OutData, (char)(iCurrentChar + 27), "(");
                                break;
                            }
                    }
                }
                str = OutData;
            }
            else
            {
                GoodData = "Cannot process; you MUST enter a 7 digit NUMBER for this type of barcode. Do not use any spaces or dashes.";
                str = GoodData;
            }
            return str;
        }

        /// <summary>
        /// calculates a mod 10 check digit from the input string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string FindMod10Digit(string input)
        {
            string str;
            int M10Factor = 3;
            int M10WeightedTotal = 0;
            int M10I;
            for (M10I = input.Length; M10I > 0; M10I--)
            {
                M10WeightedTotal = M10WeightedTotal + (Convert.ToInt32(input.Substring(M10I - 1, 1)) * M10Factor);
                M10Factor = 4 - M10Factor;
            }
            int remainder = M10WeightedTotal % 10;
            str = remainder != 0 ? (10 - remainder).ToString() : "0";
            return str;
        }

        public string Interleaved2of5(string InputValue)
        {
            string GoodData = "";
            string OutData = "";
            int LenOfData = InputValue.Length;
            int idx;
            string CurrentChar;
            for (idx = 0; idx < LenOfData; idx++)
            {
                CurrentChar = InputValue.Substring(idx, 1);
                if (this.IsNumber(CurrentChar))
                {
                    GoodData = string.Concat(GoodData, CurrentChar);
                }
            }
            if (GoodData.Length % 2 != 0)
            {
                GoodData = string.Concat("0", GoodData);
            }
            LenOfData = GoodData.Length;
            for (idx = 0; idx < LenOfData; idx = idx + 2)
            {
                CurrentChar = GoodData.Substring(idx, 2);
                int iCurrentChar = Convert.ToInt32(CurrentChar);
                OutData = iCurrentChar >= 94 ? string.Concat(OutData, (char)(iCurrentChar + 103)) : string.Concat(OutData, (char)(iCurrentChar + 33));
            }
            OutData = string.Concat('Ë', OutData, 'Ì');
            return OutData;
        }

        public string Interleaved2of5Mod10(string InputValue)
        {
            string GoodData = "";
            string OutData = "";
            int WeightedTotal = 0;
            int LenOfData = InputValue.Length;
            int idx;
            string CurrentChar;
            for (idx = 0; idx < LenOfData; idx++)
            {
                CurrentChar = InputValue.Substring(idx, 1);
                if (this.IsNumber(CurrentChar))
                {
                    GoodData = string.Concat(GoodData, CurrentChar);
                }
            }
            if (GoodData.Length % 2 == 0)
            {
                GoodData = string.Concat("0", GoodData);
            }
            LenOfData = GoodData.Length;
            int WeightFactor = 3;
            int iCurrentChar;
            for (idx = LenOfData - 1; idx >= 0; idx--)
            {
                CurrentChar = GoodData.Substring(idx, 1);
                iCurrentChar = Convert.ToInt32(CurrentChar);
                WeightedTotal = WeightedTotal + (iCurrentChar * WeightFactor);
                WeightFactor = 4 - WeightFactor;
            }

            int CheckDigit = WeightedTotal % 10;
            CheckDigit = CheckDigit == 0 ? 0 : 10 - CheckDigit;
            GoodData = string.Concat(GoodData, CheckDigit.ToString());
            LenOfData = GoodData.Length;
            for (idx = 0; idx < LenOfData; idx = idx + 2)
            {
                CurrentChar = GoodData.Substring(idx, 2);
                iCurrentChar = Convert.ToInt32(CurrentChar);
                OutData = iCurrentChar >= 94 ? string.Concat(OutData, (char)(iCurrentChar + 103)) : string.Concat(OutData, (char)(iCurrentChar + 33));
            }
            OutData = string.Concat('Ë', OutData, 'Ì');
            return OutData;
        }

        private bool IsNumber(string InNumber)
        {
            int strLen = InNumber.Length;
            char[] InNumber2 = InNumber.ToCharArray();
            bool RtrnValue = true;
            if (strLen <= 0)
            {
                RtrnValue = false;
            }
            else
            {
                int idx;
                for (idx = 0; idx < strLen; idx++)
                {
                    if (!char.IsNumber(InNumber2[idx]))
                    {
                        RtrnValue = false;
                    }
                }
            }
            return RtrnValue;
        }

        public string MSI(string InputValue)
        {
            string OutData = "";
            string GoodData = "";
            string OddNumbers = "";
            int EvenNumbers = 0;
            int LenOfData = InputValue.Length;
            int idx;
            for (idx = 0; idx < LenOfData; idx++)
            {
                if (this.IsNumber(InputValue.Substring(idx, 1)))
                {
                    GoodData = string.Concat(GoodData, InputValue.Substring(idx, 1));
                }
            }
            //WeightedTotal = 0;
            LenOfData = GoodData.Length;
            for (idx = 0; idx < LenOfData; idx++)
            {
                if ((idx + 1) % 2 == 1)
                {
                    OddNumbers = string.Concat(OddNumbers, GoodData.Substring(idx, 1));
                }
                else if ((idx + 1) % 2 == 0)
                {
                    EvenNumbers = EvenNumbers + Convert.ToInt32(GoodData.Substring(idx, 1));
                }
            }
            int iOddNumbers = Convert.ToInt32(OddNumbers) * 2;
            OddNumbers = iOddNumbers.ToString();
            LenOfData = OddNumbers.Length;
            iOddNumbers = 0;
            for (idx = 0; idx < LenOfData; idx++)
            {
                iOddNumbers = iOddNumbers + Convert.ToInt32(OddNumbers.Substring(idx, 1));
            }
            iOddNumbers = (iOddNumbers + EvenNumbers) % 10;
            //int WeightedTotal = 0;
            int CheckDigit = iOddNumbers == 0 ? 0 : 10 - iOddNumbers;
            string[] str = new string[] { "(", OutData, GoodData, CheckDigit.ToString(), ")" };
            OutData = string.Concat(str);
            return OutData;
        }

        public string POSTNet(string InputValue)
        {
            string GoodData = "";
            int LenOfData = InputValue.Length;
            int idx;
            for (idx = 0; idx < LenOfData; idx++)
            {
                if (this.IsNumber(InputValue.Substring(idx, 1)))
                {
                    GoodData = string.Concat(GoodData, InputValue.Substring(idx, 1));
                }
            }

            int WeightedTotal = 0;
            LenOfData = GoodData.Length;
            for (idx = 0; idx < LenOfData; idx++)
            {
                int iCurrentChar = Convert.ToInt32(GoodData.Substring(idx, 1));
                WeightedTotal = WeightedTotal + iCurrentChar;
            }

            int CheckDigit = WeightedTotal % 10;
            CheckDigit = CheckDigit == 0 ? 0 : 10 - CheckDigit;
            string OutData = string.Concat("(", GoodData, CheckDigit.ToString(), ")");
            return OutData;
        }

        public void PrintBarCode(string TextFont, string EncodedText, float TextFontSize)
        {
            this.lclFont = new Font(TextFont, TextFontSize);
            this.TextToPrint = EncodedText;
            PrintDocument PrintDoc = new PrintDocument();
            PrintDoc.PrintPage += new PrintPageEventHandler(this.PrintDocHandler);
            PrintDoc.Print();
        }

        private void PrintDocHandler(object sender, PrintPageEventArgs ev)
        {
            float yPos = 100f;
            float xPos = 100f;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            yPos = yPos + this.lclFont.GetHeight(ev.Graphics);
            xPos = xPos + leftMargin;
            ev.Graphics.DrawString(this.TextToPrint, this.lclFont, Brushes.Black, xPos, yPos, new StringFormat());
        }

        private string UCC128(string InputValue)
        {
            string tempInput = InputValue.Substring(0, 1).Equals("Ê") ? InputValue : string.Concat("Ê", InputValue);
            return this.Code128(tempInput, true);
        }

        public string UPCA(string InputValue)
        {
            string GoodData = "";
            string EAN5AddOn = "";
            string EAN2AddOn = "";
            string Encoding = "";
            string EANAddOnToPrint = "";
            int EANToPrintAscii = 0;
            char[] cData = InputValue.ToCharArray();
            int LenOfData = InputValue.Length;
            int idx;
            for (idx = 0; idx < LenOfData; idx++)
            {
                if (char.IsNumber(cData[idx]))
                {
                    GoodData = string.Concat(GoodData, cData[idx]);
                }
            }
            int length = GoodData.Length;
            switch (length)
            {
                case 12:
                    {
                        GoodData = GoodData.Substring(0, 11);
                        goto case 13;
                    }
                case 13:
                    {
                        int WeightFactor = 3;
                        int WeightedTotal = 0;
                        LenOfData = GoodData.Length;
                        int iCharNumber;
                        for (idx = LenOfData - 1; idx >= 0; idx--)
                        {
                            iCharNumber = Convert.ToInt32(GoodData.Substring(idx, 1));
                            WeightedTotal = WeightedTotal + (iCharNumber * WeightFactor);
                            WeightFactor = 4 - WeightFactor;
                        }

                        int CheckDigit = WeightedTotal % 10;
                        CheckDigit = CheckDigit == 0 ? 0 : 10 - CheckDigit;
                        GoodData = string.Concat(GoodData, CheckDigit.ToString());
                        LenOfData = GoodData.Length;
                        cData = GoodData.ToCharArray();
                        string DataToPrint = "";
                        for (idx = 0; idx < LenOfData; idx++)
                        {
                            int iCurrentChar = cData[idx];
                            iCharNumber = Convert.ToInt32(GoodData.Substring(idx, 1));
                            length = idx;
                            if (length == 0)
                            {
                                if (iCharNumber > 4)
                                {
                                    DataToPrint = string.Concat((char)(iCurrentChar + 64), "(", (char)(iCurrentChar + 49));
                                }
                                else if (iCharNumber < 5)
                                {
                                    DataToPrint = string.Concat((char)(iCurrentChar + 37), "(", (char)(iCurrentChar + 49));
                                }
                            }
                            else if (length == 5)
                            {
                                DataToPrint = string.Concat(DataToPrint, cData[idx], "*");
                            }
                            else if (length == 11)
                            {
                                DataToPrint = string.Concat(DataToPrint, (char)(iCurrentChar + 59), "(");
                                if (iCharNumber > 4)
                                {
                                    DataToPrint = string.Concat(DataToPrint, (char)(iCurrentChar + 64));
                                }
                                else if (iCharNumber < 5)
                                {
                                    DataToPrint = string.Concat(DataToPrint, (char)(iCurrentChar + 37));
                                }
                            }
                            else if (!(idx <= 0 || idx >= 5))
                            {
                                DataToPrint = string.Concat(DataToPrint, cData[idx]);
                            }
                            else if (idx > 5 && idx < 11)
                            {
                                DataToPrint = string.Concat(DataToPrint, (char)(iCurrentChar + 27));
                            }
                        }

                        string CurrEncoding;
                        if (EAN5AddOn.Length == 5)
                        {
                            WeightFactor = 3;
                            WeightedTotal = 0;
                            LenOfData = EAN5AddOn.Length;
                            for (idx = LenOfData - 1; idx >= 0; idx--)
                            {
                                iCharNumber = Convert.ToInt32(EAN5AddOn.Substring(idx, 1));
                                if (WeightFactor == 3)
                                {
                                    WeightedTotal = WeightedTotal + (iCharNumber * 3);
                                }
                                else if (WeightFactor == 1)
                                {
                                    WeightedTotal = WeightedTotal + (iCharNumber * 9);
                                }
                                WeightFactor = 4 - WeightFactor;
                            }
                            string sTemp = WeightedTotal.ToString();
                            CheckDigit = Convert.ToInt32(sTemp.Substring(sTemp.Length - 1));
                            length = CheckDigit;
                            switch (length)
                            {
                                case 0:
                                    {
                                        Encoding = "BBAAA";
                                        break;
                                    }
                                case 1:
                                    {
                                        Encoding = "BABAA";
                                        break;
                                    }
                                case 2:
                                    {
                                        Encoding = "BAABA";
                                        break;
                                    }
                                case 3:
                                    {
                                        Encoding = "BAAAB";
                                        break;
                                    }
                                case 4:
                                    {
                                        Encoding = "ABBAA";
                                        break;
                                    }
                                case 5:
                                    {
                                        Encoding = "AABBA";
                                        break;
                                    }
                                case 6:
                                    {
                                        Encoding = "AAABB";
                                        break;
                                    }
                                case 7:
                                    {
                                        Encoding = "ABABA";
                                        break;
                                    }
                                case 8:
                                    {
                                        Encoding = "ABAAB";
                                        break;
                                    }
                                case 9:
                                    {
                                        Encoding = "AABAB";
                                        break;
                                    }
                            }
                            LenOfData = EAN5AddOn.Length;
                            for (idx = 0; idx < LenOfData; idx++)
                            {
                                iCharNumber = Convert.ToInt32(EAN5AddOn.Substring(idx, 1));
                                CurrEncoding = Encoding.Substring(idx, 1);
                                if (CurrEncoding == "A")
                                {
                                    length = iCharNumber;
                                    switch (length)
                                    {
                                        case 0:
                                            {
                                                EANToPrintAscii = 34;
                                                break;
                                            }
                                        case 1:
                                            {
                                                EANToPrintAscii = 35;
                                                break;
                                            }
                                        case 2:
                                            {
                                                EANToPrintAscii = 36;
                                                break;
                                            }
                                        case 3:
                                            {
                                                EANToPrintAscii = 37;
                                                break;
                                            }
                                        case 4:
                                            {
                                                EANToPrintAscii = 38;
                                                break;
                                            }
                                        case 5:
                                            {
                                                EANToPrintAscii = 44;
                                                break;
                                            }
                                        case 6:
                                            {
                                                EANToPrintAscii = 46;
                                                break;
                                            }
                                        case 7:
                                            {
                                                EANToPrintAscii = 47;
                                                break;
                                            }
                                        case 8:
                                            {
                                                EANToPrintAscii = 58;
                                                break;
                                            }
                                        case 9:
                                            {
                                                EANToPrintAscii = 59;
                                                break;
                                            }
                                    }
                                }
                                else if (CurrEncoding == "B")
                                {
                                    length = iCharNumber;
                                    switch (length)
                                    {
                                        case 0:
                                            {
                                                EANToPrintAscii = 122;
                                                break;
                                            }
                                        case 1:
                                            {
                                                EANToPrintAscii = 61;
                                                break;
                                            }
                                        case 2:
                                            {
                                                EANToPrintAscii = 63;
                                                break;
                                            }
                                        case 3:
                                            {
                                                EANToPrintAscii = 64;
                                                break;
                                            }
                                        case 4:
                                            {
                                                EANToPrintAscii = 91;
                                                break;
                                            }
                                        case 5:
                                            {
                                                EANToPrintAscii = 92;
                                                break;
                                            }
                                        case 6:
                                            {
                                                EANToPrintAscii = 93;
                                                break;
                                            }
                                        case 7:
                                            {
                                                EANToPrintAscii = 95;
                                                break;
                                            }
                                        case 8:
                                            {
                                                EANToPrintAscii = 123;
                                                break;
                                            }
                                        case 9:
                                            {
                                                EANToPrintAscii = 125;
                                                break;
                                            }
                                    }
                                }
                                EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)EANToPrintAscii);
                                length = idx;
                                switch (length)
                                {
                                    case 0:
                                        {
                                            iCharNumber = 43;
                                            EANAddOnToPrint = string.Concat((char)iCharNumber, EANAddOnToPrint);
                                            iCharNumber = 33;
                                            EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iCharNumber);
                                            break;
                                        }
                                    case 1:
                                        {
                                            iCharNumber = 33;
                                            EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iCharNumber);
                                            break;
                                        }
                                    case 2:
                                        {
                                            iCharNumber = 33;
                                            EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iCharNumber);
                                            break;
                                        }
                                    case 3:
                                        {
                                            iCharNumber = 33;
                                            EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iCharNumber);
                                            break;
                                        }
                                }
                            }
                        }
                        if (EAN2AddOn.Length == 2)
                        {
                            iCharNumber = Convert.ToInt32(EAN2AddOn);
                            for (idx = 0; idx <= 99; idx = idx + 4)
                            {
                                if (iCharNumber == idx)
                                {
                                    Encoding = "AA";
                                }
                                else if (iCharNumber == idx + 1)
                                {
                                    Encoding = "AB";
                                }
                                else if (iCharNumber == idx + 2)
                                {
                                    Encoding = "BA";
                                }
                                else if (iCharNumber == idx + 3)
                                {
                                    Encoding = "BB";
                                }
                            }
                            LenOfData = EAN2AddOn.Length;
                            for (idx = 0; idx < LenOfData; idx++)
                            {
                                iCharNumber = Convert.ToInt32(EAN2AddOn.Substring(idx, 1));
                                CurrEncoding = Encoding.Substring(idx, 1);
                                if (CurrEncoding == "A")
                                {
                                    length = iCharNumber;
                                    switch (length)
                                    {
                                        case 0:
                                            {
                                                EANToPrintAscii = 34;
                                                break;
                                            }
                                        case 1:
                                            {
                                                EANToPrintAscii = 35;
                                                break;
                                            }
                                        case 2:
                                            {
                                                EANToPrintAscii = 36;
                                                break;
                                            }
                                        case 3:
                                            {
                                                EANToPrintAscii = 37;
                                                break;
                                            }
                                        case 4:
                                            {
                                                EANToPrintAscii = 38;
                                                break;
                                            }
                                        case 5:
                                            {
                                                EANToPrintAscii = 44;
                                                break;
                                            }
                                        case 6:
                                            {
                                                EANToPrintAscii = 46;
                                                break;
                                            }
                                        case 7:
                                            {
                                                EANToPrintAscii = 47;
                                                break;
                                            }
                                        case 8:
                                            {
                                                EANToPrintAscii = 58;
                                                break;
                                            }
                                        case 9:
                                            {
                                                EANToPrintAscii = 59;
                                                break;
                                            }
                                    }
                                }
                                else if (CurrEncoding == "B")
                                {
                                    length = iCharNumber;
                                    switch (length)
                                    {
                                        case 0:
                                            {
                                                EANToPrintAscii = 122;
                                                break;
                                            }
                                        case 1:
                                            {
                                                EANToPrintAscii = 61;
                                                break;
                                            }
                                        case 2:
                                            {
                                                EANToPrintAscii = 63;
                                                break;
                                            }
                                        case 3:
                                            {
                                                EANToPrintAscii = 64;
                                                break;
                                            }
                                        case 4:
                                            {
                                                EANToPrintAscii = 91;
                                                break;
                                            }
                                        case 5:
                                            {
                                                EANToPrintAscii = 92;
                                                break;
                                            }
                                        case 6:
                                            {
                                                EANToPrintAscii = 93;
                                                break;
                                            }
                                        case 7:
                                            {
                                                EANToPrintAscii = 95;
                                                break;
                                            }
                                        case 8:
                                            {
                                                EANToPrintAscii = 123;
                                                break;
                                            }
                                        case 9:
                                            {
                                                EANToPrintAscii = 125;
                                                break;
                                            }
                                    }
                                }
                                EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)EANToPrintAscii);
                                if (idx == 0)
                                {
                                    iCharNumber = 43;
                                    EANAddOnToPrint = string.Concat((char)iCharNumber, EANAddOnToPrint);
                                    iCharNumber = 33;
                                    EANAddOnToPrint = string.Concat(EANAddOnToPrint, (char)iCharNumber);
                                }
                            }
                        }
                        DataToPrint = string.Concat(DataToPrint, EANAddOnToPrint);
                        return DataToPrint;
                    }
                case 14:
                    {
                        EAN2AddOn = GoodData.Substring(12);
                        GoodData = GoodData.Substring(0, 11);
                        goto case 13;
                    }
                default:
                    {
                        if (length == 17)
                        {
                            EAN5AddOn = GoodData.Substring(12);
                            GoodData = GoodData.Substring(0, 11);
                            goto case 13;
                        }
                        else
                        {
                            goto case 13;
                        }
                    }
            }
        }

        public string UPCe(string InputValue)
        {
            int I;
            int CurrentNumber;
            string Encoding;
            object[] objArray;
            int num;
            bool flag;
            bool flag1;
            bool flag2;
            StringBuilder sbOutput = new StringBuilder();
            StringBuilder EANAddOnToPrint = new StringBuilder();
            string ActualInputValue = "";
            string EAN2AddOn = "";
            string EAN5AddOn = "";
            int StringLength = InputValue.Length;
            int idx = 0;
            while (idx < InputValue.Length)
            {
                if (ActualInputValue.Length != 18)
                {
                    if (char.IsDigit(InputValue, idx))
                    {
                        ActualInputValue = string.Concat(ActualInputValue, InputValue.Substring(idx, 1));
                    }
                    idx++;
                }
                else
                {
                    break;
                }
            }
            if (ActualInputValue.Length < 11 || ActualInputValue.Length == 15 || ActualInputValue.Length > 18)
            {
                ActualInputValue = "00005000000";
            }
            if (ActualInputValue.Length == 12)
            {
                ActualInputValue = ActualInputValue.Substring(0, 11);
            }
            else if (ActualInputValue.Length == 13)
            {
                ActualInputValue = ActualInputValue.Substring(0, 11);
                EAN2AddOn = ActualInputValue.Substring(12, 2);
            }
            else if (ActualInputValue.Length == 14)
            {
                ActualInputValue = ActualInputValue.Substring(0, 11);
                EAN2AddOn = ActualInputValue.Substring(13, 2);
            }
            else if (ActualInputValue.Length == 16)
            {
                ActualInputValue = ActualInputValue.Substring(0, 11);
                EAN5AddOn = ActualInputValue.Substring(11, 5);
            }
            else if (ActualInputValue.Length == 17)
            {
                ActualInputValue = ActualInputValue.Substring(0, 11);
                EAN5AddOn = ActualInputValue.Substring(13, 5);
            }
            int Factor = 3;
            int WeightedTotal = 0;
            for (I = ActualInputValue.Length - 1; I >= 0; I--)
            {
                CurrentNumber = Convert.ToInt32(ActualInputValue.Substring(I, 1));
                WeightedTotal = WeightedTotal + (CurrentNumber * Factor);
                Factor = 4 - Factor;
            }
            int remainder = WeightedTotal % 10;
            if (remainder == 0)
            {
                ActualInputValue = string.Concat(ActualInputValue, "0");
            }
            else
            {
                remainder = 10 - remainder;
                ActualInputValue = string.Concat(ActualInputValue, remainder.ToString());
            }
            Convert.ToInt32(ActualInputValue.Substring(0, 1));
            int D2 = Convert.ToInt32(ActualInputValue.Substring(1, 1));
            int D3 = Convert.ToInt32(ActualInputValue.Substring(2, 1));
            int D4 = Convert.ToInt32(ActualInputValue.Substring(3, 1));
            int D5 = Convert.ToInt32(ActualInputValue.Substring(4, 1));
            int D6 = Convert.ToInt32(ActualInputValue.Substring(5, 1));
            int D7 = Convert.ToInt32(ActualInputValue.Substring(6, 1));
            int D8 = Convert.ToInt32(ActualInputValue.Substring(7, 1));
            int D9 = Convert.ToInt32(ActualInputValue.Substring(8, 1));
            int D10 = Convert.ToInt32(ActualInputValue.Substring(9, 1));
            int D11 = Convert.ToInt32(ActualInputValue.Substring(10, 1));
            int D12 = Convert.ToInt32(ActualInputValue.Substring(11, 1));
            flag = (D11 != 5 && D11 != 6 && D11 != 7 && D11 != 8 && D11 != 9) || D6 == 0 || D7 != 0 || D8 != 0 || D9 != 0 || D10 != 0;
            if (!flag)
            {
                objArray = new object[] { D2, D3, D4, D5, D6, D11 };
                ActualInputValue = string.Format("{0}{1}{2}{3}{4}{5}", objArray);
            }
            else if (D6 != 0 || D7 != 0 || D8 != 0 || D9 != 0 || D10 != 0 || D5 == 0)
            {
                flag1 = D5 != 0 || D6 != 0 || D7 != 0 || D8 != 0 || (D4 != 1 && D4 != 2 && D4 != 0);
                if (flag1)
                {
                    flag2 = D5 != 0 || D6 != 0 || D7 != 0 || D8 != 0 || D9 != 0 || D4 < 3 || D4 > 9;
                    if (flag2)
                    {
                        string unchangedValue = ActualInputValue;
                        ActualInputValue = unchangedValue;
                    }
                    else
                    {
                        objArray = new object[] { D2, D3, D4, D10, D11, "3" };
                        ActualInputValue = string.Format("{0}{1}{2}{3}{4}{5}", objArray);
                    }
                }
                else
                {
                    objArray = new object[] { D2, D3, D9, D10, D11, D4 };
                    ActualInputValue = string.Format("{0}{1}{2}{3}{4}{5}", objArray);
                }
            }
            else
            {
                objArray = new object[] { D2, D3, D4, D5, D11, "4" };
                ActualInputValue = string.Format("{0}{1}{2}{3}{4}{5}", objArray);
            }
            if (ActualInputValue.Length == 6)
            {
                Encoding = "";
                num = D12;
                switch (num)
                {
                    case 0:
                        {
                            Encoding = "BBBAAA";
                            break;
                        }
                    case 1:
                        {
                            Encoding = "BBABAA";
                            break;
                        }
                    case 2:
                        {
                            Encoding = "BBAABA";
                            break;
                        }
                    case 3:
                        {
                            Encoding = "BBAAAB";
                            break;
                        }
                    case 4:
                        {
                            Encoding = "BABBAA";
                            break;
                        }
                    case 5:
                        {
                            Encoding = "BAABBA";
                            break;
                        }
                    case 6:
                        {
                            Encoding = "BAAABB";
                            break;
                        }
                    case 7:
                        {
                            Encoding = "BABABA";
                            break;
                        }
                    case 8:
                        {
                            Encoding = "BABAAB";
                            break;
                        }
                    case 9:
                        {
                            Encoding = "BAABAB";
                            break;
                        }
                }
                for (I = 1; I <= ActualInputValue.Length; I++)
                {
                    if (I == 1)
                    {
                        sbOutput.Append('U');
                        sbOutput.Append("(");
                    }
                    CurrentNumber = ActualInputValue.ToCharArray()[I - 1];
                    switch (Encoding.ToCharArray()[I - 1])
                    {
                        case 'A':
                            {
                                sbOutput.Append((char)CurrentNumber);
                                break;
                            }
                        case 'B':
                            {
                                sbOutput.Append((char)(CurrentNumber + 17));
                                break;
                            }
                    }
                    num = I;
                    if (num != 1)
                    {
                        if (num == 6)
                        {
                            sbOutput.Append(")");
                            if (D12 > 4)
                            {
                                sbOutput.Append((char)(D12.ToString().ToCharArray()[0] + 64));
                            }
                            if (D12 < 5)
                            {
                                sbOutput.Append((char)(D12.ToString().ToCharArray()[0] + 37));
                            }
                        }
                    }
                }
            }
            if (ActualInputValue.Length != 6)
            {
                for (I = 1; I <= ActualInputValue.Length; I++)
                {
                    CurrentNumber = ActualInputValue.ToCharArray()[I - 1];
                    num = I;
                    switch (num)
                    {
                        case 1:
                            {
                                if (Convert.ToInt32(ActualInputValue.Substring(I - 1, 1)) <= 4)
                                {
                                    sbOutput.Append((char)(CurrentNumber + 37));
                                }
                                else
                                {
                                    sbOutput.Append((char)(CurrentNumber + 64));
                                }
                                sbOutput.Append("(");
                                sbOutput.Append((char)(CurrentNumber + 49));
                                break;
                            }
                        case 2:
                            {
                                sbOutput.Append(ActualInputValue.Substring(I - 1, 1));
                                break;
                            }
                        case 3:
                            {
                                sbOutput.Append(ActualInputValue.Substring(I - 1, 1));
                                break;
                            }
                        case 4:
                            {
                                sbOutput.Append(ActualInputValue.Substring(I - 1, 1));
                                break;
                            }
                        case 5:
                            {
                                sbOutput.Append(ActualInputValue.Substring(I - 1, 1));
                                break;
                            }
                        case 6:
                            {
                                sbOutput.Append(ActualInputValue.Substring(I - 1, 1));
                                sbOutput.Append("*");
                                break;
                            }
                        case 7:
                            {
                                sbOutput.Append((char)(CurrentNumber + 27));
                                break;
                            }
                        case 8:
                            {
                                sbOutput.Append((char)(CurrentNumber + 27));
                                break;
                            }
                        case 9:
                            {
                                sbOutput.Append((char)(CurrentNumber + 27));
                                break;
                            }
                        case 10:
                            {
                                sbOutput.Append((char)(CurrentNumber + 27));
                                break;
                            }
                        case 11:
                            {
                                sbOutput.Append((char)(CurrentNumber + 27));
                                break;
                            }
                        case 12:
                            {
                                sbOutput.Append((char)(CurrentNumber + 59));
                                sbOutput.Append(")");
                                if (Convert.ToInt32(ActualInputValue.Substring(I - 1, 1)) <= 4)
                                {
                                    sbOutput.Append((char)(CurrentNumber + 37));
                                }
                                else
                                {
                                    sbOutput.Append((char)(CurrentNumber + 64));
                                }
                                break;
                            }
                    }
                }
            }
            if (EAN5AddOn.Length == 5)
            {
                Factor = 3;
                WeightedTotal = 0;
                for (I = EAN5AddOn.Length - 1; I >= 0; I--)
                {
                    CurrentNumber = Convert.ToInt32(EAN5AddOn.Substring(I, 1));
                    if (Factor == 3)
                    {
                        WeightedTotal = WeightedTotal + (CurrentNumber * 3);
                    }
                    if (Factor == 1)
                    {
                        WeightedTotal = WeightedTotal + (CurrentNumber * 9);
                    }
                    Factor = 4 - Factor;
                }
                string sCheck = WeightedTotal.ToString();
                int CheckDigit = Convert.ToInt32(sCheck.Substring(sCheck.Length - 1, 1));
                Encoding = "";
                num = CheckDigit;
                switch (num)
                {
                    case 0:
                        {
                            Encoding = "BBAAA";
                            break;
                        }
                    case 1:
                        {
                            Encoding = "BABAA";
                            break;
                        }
                    case 2:
                        {
                            Encoding = "BAABA";
                            break;
                        }
                    case 3:
                        {
                            Encoding = "BAAAB";
                            break;
                        }
                    case 4:
                        {
                            Encoding = "ABBAA";
                            break;
                        }
                    case 5:
                        {
                            Encoding = "AABBA";
                            break;
                        }
                    case 6:
                        {
                            Encoding = "AAABB";
                            break;
                        }
                    case 7:
                        {
                            Encoding = "ABABA";
                            break;
                        }
                    case 8:
                        {
                            Encoding = "ABAAB";
                            break;
                        }
                    case 9:
                        {
                            Encoding = "AABAB";
                            break;
                        }
                }
                for (I = 1; I <= EAN5AddOn.Length; I++)
                {
                    CurrentNumber = Convert.ToInt32(EAN5AddOn.Substring(I - 1, 1));
                    switch (Encoding.ToCharArray()[I - 1])
                    {
                        case 'A':
                            {
                                if (CurrentNumber == 0)
                                {
                                    EANAddOnToPrint.Append('\"');
                                }
                                if (CurrentNumber == 1)
                                {
                                    EANAddOnToPrint.Append('#');
                                }
                                if (CurrentNumber == 2)
                                {
                                    EANAddOnToPrint.Append('$');
                                }
                                if (CurrentNumber == 3)
                                {
                                    EANAddOnToPrint.Append('%');
                                }
                                if (CurrentNumber == 4)
                                {
                                    EANAddOnToPrint.Append('&');
                                }
                                if (CurrentNumber == 5)
                                {
                                    EANAddOnToPrint.Append(',');
                                }
                                if (CurrentNumber == 6)
                                {
                                    EANAddOnToPrint.Append('.');
                                }
                                if (CurrentNumber == 7)
                                {
                                    EANAddOnToPrint.Append('/');
                                }
                                if (CurrentNumber == 8)
                                {
                                    EANAddOnToPrint.Append(':');
                                }
                                if (CurrentNumber == 9)
                                {
                                    EANAddOnToPrint.Append(';');
                                }
                                break;
                            }
                        case 'B':
                            {
                                if (CurrentNumber == 0)
                                {
                                    EANAddOnToPrint.Append('z');
                                }
                                if (CurrentNumber == 1)
                                {
                                    EANAddOnToPrint.Append('=');
                                }
                                if (CurrentNumber == 2)
                                {
                                    EANAddOnToPrint.Append('?');
                                }
                                if (CurrentNumber == 3)
                                {
                                    EANAddOnToPrint.Append('@');
                                }
                                if (CurrentNumber == 4)
                                {
                                    EANAddOnToPrint.Append('[');
                                }
                                if (CurrentNumber == 5)
                                {
                                    EANAddOnToPrint.Append('\\');
                                }
                                if (CurrentNumber == 6)
                                {
                                    EANAddOnToPrint.Append(']');
                                }
                                if (CurrentNumber == 7)
                                {
                                    EANAddOnToPrint.Append('\u005F');
                                }
                                if (CurrentNumber == 8)
                                {
                                    EANAddOnToPrint.Append('{');
                                }
                                if (CurrentNumber == 9)
                                {
                                    EANAddOnToPrint.Append('}');
                                }
                                break;
                            }
                    }
                    num = I;
                    switch (num)
                    {
                        case 1:
                            {
                                EANAddOnToPrint.Insert(0, '+');
                                EANAddOnToPrint.Append('!');
                                break;
                            }
                        case 2:
                            {
                                EANAddOnToPrint.Append('!');
                                break;
                            }
                        case 3:
                            {
                                EANAddOnToPrint.Append('!');
                                break;
                            }
                        case 4:
                            {
                                EANAddOnToPrint.Append('!');
                                break;
                            }
                    }
                }
            }
            if (EAN2AddOn.Length == 2)
            {
                Encoding = "";
                for (I = 0; I <= 99; I++)
                {
                    if (Convert.ToInt32(EAN2AddOn) == I)
                    {
                        Encoding = "AA";
                    }
                    if (Convert.ToInt32(EAN2AddOn) == I + 1)
                    {
                        Encoding = "AB";
                    }
                    if (Convert.ToInt32(EAN2AddOn) == I + 2)
                    {
                        Encoding = "BA";
                    }
                    if (Convert.ToInt32(EAN2AddOn) == I + 3)
                    {
                        Encoding = "BB";
                    }
                }
                for (I = 1; I <= EAN2AddOn.Length; I++)
                {
                    CurrentNumber = Convert.ToInt32(EAN5AddOn.Substring(I - 1, 1));
                    switch (Encoding.ToCharArray()[I - 1])
                    {
                        case 'A':
                            {
                                if (CurrentNumber == 0)
                                {
                                    EANAddOnToPrint.Append('\"');
                                }
                                if (CurrentNumber == 1)
                                {
                                    EANAddOnToPrint.Append('#');
                                }
                                if (CurrentNumber == 2)
                                {
                                    EANAddOnToPrint.Append('$');
                                }
                                if (CurrentNumber == 3)
                                {
                                    EANAddOnToPrint.Append('%');
                                }
                                if (CurrentNumber == 4)
                                {
                                    EANAddOnToPrint.Append('&');
                                }
                                if (CurrentNumber == 5)
                                {
                                    EANAddOnToPrint.Append(',');
                                }
                                if (CurrentNumber == 6)
                                {
                                    EANAddOnToPrint.Append('.');
                                }
                                if (CurrentNumber == 7)
                                {
                                    EANAddOnToPrint.Append('/');
                                }
                                if (CurrentNumber == 8)
                                {
                                    EANAddOnToPrint.Append(':');
                                }
                                if (CurrentNumber == 9)
                                {
                                    EANAddOnToPrint.Append(';');
                                }
                                break;
                            }
                        case 'B':
                            {
                                if (CurrentNumber == 0)
                                {
                                    EANAddOnToPrint.Append('z');
                                }
                                if (CurrentNumber == 1)
                                {
                                    EANAddOnToPrint.Append('=');
                                }
                                if (CurrentNumber == 2)
                                {
                                    EANAddOnToPrint.Append('?');
                                }
                                if (CurrentNumber == 3)
                                {
                                    EANAddOnToPrint.Append('@');
                                }
                                if (CurrentNumber == 4)
                                {
                                    EANAddOnToPrint.Append('[');
                                }
                                if (CurrentNumber == 5)
                                {
                                    EANAddOnToPrint.Append('\\');
                                }
                                if (CurrentNumber == 6)
                                {
                                    EANAddOnToPrint.Append(']');
                                }
                                if (CurrentNumber == 7)
                                {
                                    EANAddOnToPrint.Append('\u005F');
                                }
                                if (CurrentNumber == 8)
                                {
                                    EANAddOnToPrint.Append('{');
                                }
                                if (CurrentNumber == 9)
                                {
                                    EANAddOnToPrint.Append('}');
                                }
                                break;
                            }
                    }
                    num = I;
                    switch (num)
                    {
                        case 1:
                            {
                                EANAddOnToPrint.Insert(0, '+');
                                EANAddOnToPrint.Append('!');
                                break;
                            }
                    }
                }
            }
            sbOutput.Append(EANAddOnToPrint.ToString());
            return sbOutput.ToString();
        }

        private enum enuEncodingType : short
        {
            EncodingA = 1,
            EncodingB = 2,
            EncodingC = 3
        }
    }
}